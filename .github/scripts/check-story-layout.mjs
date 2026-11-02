#!/usr/bin/env node
// No npm packages: drives headless Chrome over CDP with Node's built-in WebSocket.
import { spawn } from 'node:child_process';
import { readFileSync, readdirSync, statSync, mkdtempSync } from 'node:fs';
import { tmpdir } from 'node:os';
import { join, resolve, dirname } from 'node:path';
import { fileURLToPath } from 'node:url';

const [baseUrl, chromePath, ...prefixes] = process.argv.slice(2);
if (!baseUrl || !chromePath) {
  console.error('Usage: check-story-layout.mjs <stories-url> <chrome-path> [title-prefix ...]');
  process.exit(64);
}

// The story preview adds 96px around the card, so 424 shows a 328px card: a 360dp phone less 16px side padding.
const WIDTH = 424;
const CARD_WIDTH = 328;
// Exploration knob (Android font size scales rem); not run in CI.
const FONT_SCALE = Number(process.env.STORY_FONT_SCALE ?? 1);
const CONCURRENCY = 8;
const root = resolve(dirname(fileURLToPath(import.meta.url)), '../../src/WorkoutLog.UI.Stories/Stories');
const scopes = prefixes.length ? prefixes : ['Components/Workout/'];

const slug = (text) => text.toLowerCase().replace(/[^a-z0-9]+/g, '-').replace(/^-+|-+$/g, '');

function storyFiles(dir) {
  return readdirSync(dir).flatMap((name) => {
    const path = join(dir, name);
    return statSync(path).isDirectory() ? storyFiles(path) : name.endsWith('.stories.razor') ? [path] : [];
  });
}

function stories() {
  return storyFiles(root).flatMap((file) => {
    const source = readFileSync(file, 'utf8');
    const title = /\[Stories\("([^"]+)"\)\]/.exec(source)?.[1];
    if (!title || !scopes.some((scope) => title.startsWith(scope))) return [];
    return [...source.matchAll(/<Story\s+Name="([^"]+)"/g)].map((m) => ({ title, name: m[1], id: `${slug(title)}--${slug(m[1])}` }));
  });
}

// Runs in the page. Returns one finding per problem.
function inspect() {
  const PROSE = 'h1, h2, h3, [role=alert], .note, .error, [data-wrap-ok]';
  const findings = [];
  const label = (el) => {
    const id = el.closest('[data-testid]')?.getAttribute('data-testid');
    return `${el.tagName.toLowerCase()}${el.className && typeof el.className === 'string' ? '.' + el.className.trim().split(/\s+/)[0] : ''}${id ? `[${id}]` : ''}`;
  };
  const app = document.querySelector('#app');
  const viewport = document.documentElement.clientWidth;
  if (document.documentElement.scrollWidth > viewport) {
    findings.push(`page scrolls sideways (${document.documentElement.scrollWidth}px > ${viewport}px)`);
  }
  for (const el of app.querySelectorAll('*')) {
    const style = getComputedStyle(el);
    if (style.display === 'none' || el.closest('.visually-hidden')) continue;
    const box = el.getBoundingClientRect();
    if (box.width === 0 || box.height === 0) continue;
    if (el.scrollWidth > el.clientWidth + 1 && style.textOverflow !== 'ellipsis') {
      if (el.tagName === 'INPUT' || style.overflowX !== 'visible') {
        findings.push(`${label(el)} clips its content (${el.scrollWidth}px of ${el.clientWidth}px)`);
      } else if (style.whiteSpace === 'nowrap') {
        findings.push(`${label(el)} text overflows its box (${el.scrollWidth}px in ${el.clientWidth}px)`);
      }
    }
    if (el.tagName === 'INPUT' && el.placeholder && !el.value) {
      // Placeholder text never counts toward scrollWidth, so it needs its own measurement.
      const probe = document.createElement('span');
      probe.style.cssText = 'position:absolute;visibility:hidden;white-space:pre';
      probe.style.font = style.font;
      probe.style.fontSize = getComputedStyle(el, '::placeholder').fontSize;
      probe.textContent = el.placeholder;
      document.body.append(probe);
      const needed = probe.getBoundingClientRect().width;
      probe.remove();
      const room = el.clientWidth - parseFloat(style.paddingLeft) - parseFloat(style.paddingRight);
      if (needed > room + 0.5) findings.push(`${label(el)} placeholder is cut off (${Math.round(needed)}px in ${Math.round(room)}px)`);
    }
    if (box.right > viewport + 1) {
      findings.push(`${label(el)} runs past the screen edge (${Math.round(box.right)}px > ${viewport}px)`);
    }
    if (el.childElementCount === 0 && el.textContent.trim() && !el.closest(PROSE) && style.whiteSpace !== 'nowrap') {
      const range = document.createRange();
      range.selectNodeContents(el);
      const lines = new Set([...range.getClientRects()].map((r) => Math.round(r.top)));
      if (lines.size > 1) findings.push(`${label(el)} wraps onto ${lines.size} lines: "${el.textContent.trim().slice(0, 60)}"`);
    }
  }
  return findings;
}

const reachable = await fetch(`${baseUrl}/iframe.html`).then((r) => r.ok, () => false);
if (!reachable) {
  console.error(`Stories not reachable at ${baseUrl}`);
  process.exit(1);
}

const chromeDir = mkdtempSync(join(tmpdir(), 'story-layout-'));
const chrome = spawn(chromePath, [
  '--headless=new', '--no-sandbox', '--disable-gpu', '--remote-debugging-port=0', `--user-data-dir=${chromeDir}`, 'about:blank',
], { stdio: ['ignore', 'ignore', 'pipe'] });
chrome.on('error', (error) => die(`Cannot start Chrome: ${error.message}`));

function die(message) {
  console.error(message);
  chrome.kill();
  process.exit(1);
}

setTimeout(() => die('Timed out'), 300_000).unref();

let endpointResolved = false;
const endpoint = await new Promise((resolveEndpoint, reject) => {
  let log = '';
  chrome.stderr.on('data', (chunk) => {
    log += chunk;
    const match = /DevTools listening on (ws:\/\/\S+)/.exec(log);
    if (match) { endpointResolved = true; resolveEndpoint(match[1]); }
  });
  chrome.on('exit', () => { if (!endpointResolved) reject(new Error(`Chrome exited early:\n${log}`)); });
}).catch((error) => die(error.message));

const socket = new WebSocket(endpoint);
socket.onclose = () => die('Chrome connection lost');
await new Promise((ok, fail) => { socket.onopen = ok; socket.onerror = () => fail(new Error('Cannot connect to Chrome')); }).catch((error) => die(error.message));
let next = 0;
const pending = new Map();
socket.onmessage = ({ data }) => {
  const message = JSON.parse(data);
  const waiter = pending.get(message.id);
  if (waiter) {
    pending.delete(message.id);
    message.error ? waiter.reject(new Error(message.error.message)) : waiter.resolve(message.result);
  }
};
const send = (method, params = {}, sessionId) => new Promise((resolve, reject) => {
  const id = ++next;
  pending.set(id, { resolve, reject });
  socket.send(JSON.stringify({ id, method, params, sessionId }));
});

const sleep = (ms) => new Promise((r) => setTimeout(r, ms));

async function check(story) {
  const { targetId } = await send('Target.createTarget', { url: 'about:blank' });
  const { sessionId } = await send('Target.attachToTarget', { targetId, flatten: true });
  const run = (method, params) => send(method, params, sessionId);
  try {
    await run('Emulation.setDeviceMetricsOverride', { width: WIDTH, height: 900, deviceScaleFactor: 1, mobile: true });
    await run('Page.navigate', { url: `${baseUrl}/iframe.html?id=${story.id}&viewMode=story` });
    const ready = '(document.documentElement.classList.contains("_blazing_story_ready_for_visible") && document.querySelector("#app")?.children.length > 0)';
    for (let i = 0; ; i++) {
      const { result } = await run('Runtime.evaluate', { expression: ready, returnByValue: true });
      if (result.value) break;
      if (i > 150) return { findings: ['did not render within 30s'] };
      await sleep(200);
    }
    if (FONT_SCALE !== 1) {
      await run('Runtime.evaluate', { expression: `document.documentElement.style.fontSize = '${FONT_SCALE * 100}%'` });
      await sleep(300);
    }
    const { result, exceptionDetails } = await run('Runtime.evaluate', { expression: `(${inspect})()`, returnByValue: true });
    if (exceptionDetails) return { findings: [`checker failed: ${exceptionDetails.text}`] };
    const cardWidth = (await run('Runtime.evaluate', { expression: `document.querySelector('.card')?.getBoundingClientRect().width`, returnByValue: true })).result.value;
    return { findings: result.value, cardWidth };
  } finally {
    await send('Target.closeTarget', { targetId }).catch(() => {});
  }
}

const jobs = stories();
if (jobs.length === 0) die(`No stories match ${scopes.join(', ')}.`);

let exitCode = 0;
try {
  const failures = [];
  const cardWidths = [];
  const started = Date.now();
  let cursor = 0;
  await Promise.all(Array.from({ length: CONCURRENCY }, async () => {
    while (cursor < jobs.length) {
      const story = jobs[cursor++];
      const result = await check(story);
      if (result.cardWidth !== undefined) cardWidths.push(result.cardWidth);
      if (result.findings.length) failures.push({ story, findings: [...new Set(result.findings)] });
    }
  }));

  console.log(`Checked ${jobs.length} stories at ${WIDTH}px in ${((Date.now() - started) / 1000).toFixed(1)}s. Card width: ${[...new Set(cardWidths)].join(', ') || 'none found'}px.`);
  if (cardWidths.length === 0 || cardWidths.some((width) => width !== CARD_WIDTH)) {
    console.log(`::error title=Story layout::Card is not ${CARD_WIDTH}px wide at ${WIDTH}px; the story preview offset changed, update WIDTH.`);
    exitCode = 1;
  }
  for (const { story, findings } of failures.sort((a, b) => a.story.id.localeCompare(b.story.id))) {
    console.log(`\n${story.title} / ${story.name}`);
    for (const finding of findings) {
      console.log(`  ${finding}`);
      console.log(`::error title=Story layout::${story.title} / ${story.name} at ${WIDTH}px: ${finding}`);
    }
    exitCode = 1;
  }
} catch (error) {
  console.error(error);
  exitCode = 1;
} finally {
  socket.onclose = null;
  chrome.kill();
}
process.exit(exitCode);
