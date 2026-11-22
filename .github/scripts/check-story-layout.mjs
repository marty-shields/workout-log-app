#!/usr/bin/env node
// No npm packages: drives headless Chrome over CDP with Node's built-in WebSocket.
import { spawn } from 'node:child_process';
import { readFileSync, readdirSync, statSync, mkdtempSync } from 'node:fs';
import { tmpdir } from 'node:os';
import { join, resolve, dirname } from 'node:path';
import { fileURLToPath } from 'node:url';
import { CARD_WIDTH, defaultScopes, frameFindings, measureFor, qualifyScope, rightEdge, storiesOf } from './story-layout-scopes.mjs';

const [baseUrl, chromePath, ...prefixes] = process.argv.slice(2);
if (!baseUrl || !chromePath) {
  console.error('Usage: check-story-layout.mjs <stories-url> <chrome-path> [title-prefix ...]  (a prefix without the story prefix gets it prepended, e.g. Screens/; default Components/Workout/)');
  process.exit(64);
}

// Exploration knob (Android font size scales rem); not run in CI.
const FONT_SCALE = Number(process.env.STORY_FONT_SCALE ?? 1);
const CONCURRENCY = 8;
const library = resolve(dirname(fileURLToPath(import.meta.url)), '../../src/WorkoutLog.UI.Stories');
const root = join(library, 'Stories');
const storyPrefix = /Prefix = "([^"]+)"/.exec(readFileSync(join(library, 'StoryTitles.cs'), 'utf8'))?.[1];
if (!storyPrefix) throw new Error('StoryTitles.Prefix not found in StoryTitles.cs');
const scopes = prefixes.length ? prefixes.map((p) => qualifyScope(p, storyPrefix)) : defaultScopes(storyPrefix);

function storyFiles(dir) {
  return readdirSync(dir).flatMap((name) => {
    const path = join(dir, name);
    return statSync(path).isDirectory() ? storyFiles(path) : name.endsWith('.stories.razor') ? [path] : [];
  });
}

function stories() {
  return storyFiles(root).flatMap((file) => storiesOf(readFileSync(file, 'utf8'), scopes, storyPrefix));
}

// Runs in the page. Returns one finding per problem. A component story is measured over the whole app against the page
// width; a screen story (phone: true) over the content inside the kit's phone frame against that frame's width.
function inspect(phone, rightEdge) {
  const PROSE = 'h1, h2, h3, [role=alert], .note, .error, [data-wrap-ok]';
  const findings = [];
  const label = (el) => {
    const id = el.closest('[data-testid]')?.getAttribute('data-testid');
    return `${el.tagName.toLowerCase()}${el.className && typeof el.className === 'string' ? '.' + el.className.trim().split(/\s+/)[0] : ''}${id ? `[${id}]` : ''}`;
  };
  const app = phone ? document.querySelector('[data-testid=phone] > .viewport') : document.querySelector('#app');
  if (!app) return ['no phone frame found'];
  const pageWidth = document.documentElement.clientWidth;
  const frame = app.getBoundingClientRect();
  const frameLeft = phone ? frame.left : 0;
  const viewport = phone ? frame.width : pageWidth;
  if (document.documentElement.scrollWidth > pageWidth) {
    findings.push(`page scrolls sideways (${document.documentElement.scrollWidth}px > ${pageWidth}px)`);
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
    const right = rightEdge(box.right, frameLeft);
    if (right > viewport + 1) {
      findings.push(`${label(el)} runs past the screen edge (${Math.round(right)}px > ${viewport}px)`);
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

async function check(story, measure) {
  const phone = measure.kind === 'screen';
  const { targetId } = await send('Target.createTarget', { url: 'about:blank' });
  const { sessionId } = await send('Target.attachToTarget', { targetId, flatten: true });
  const run = (method, params) => send(method, params, sessionId);
  try {
    await run('Emulation.setDeviceMetricsOverride', { width: measure.pageWidth, height: 900, deviceScaleFactor: 1, mobile: true });
    await run('Page.navigate', { url: `${baseUrl}/iframe.html?id=${story.id}&viewMode=story` });
    const ready = '(document.documentElement.classList.contains("_blazing_story_ready_for_visible") && document.querySelector("#app")?.children.length > 0)';
    for (let i = 0; ; i++) {
      const { result } = await run('Runtime.evaluate', { expression: ready, returnByValue: true });
      if (result.value) break;
      if (i > 150) return { findings: ['did not render within 30s'] };
      await sleep(200);
    }
    if (phone) {
      // The kit frame is 412px wide; narrow it to the narrowest phone so the screen lays out as there.
      await run('Runtime.evaluate', { expression: `document.documentElement.style.setProperty('--stories-phone-width', '${measure.phoneWidth}px')` });
      await sleep(300);
    }
    if (FONT_SCALE !== 1) {
      await run('Runtime.evaluate', { expression: `document.documentElement.style.fontSize = '${FONT_SCALE * 100}%'` });
      await sleep(300);
    }
    const { result, exceptionDetails } = await run('Runtime.evaluate', { expression: `(${inspect})(${phone}, ${rightEdge})`, returnByValue: true });
    if (exceptionDetails) return { findings: [`checker failed: ${exceptionDetails.text}`] };
    const cardScope = phone ? `document.querySelector('[data-testid=phone] > .viewport')` : 'document';
    const cardWidth = (await run('Runtime.evaluate', { expression: `Math.max(...[...${cardScope}.querySelectorAll('.card')].map((c) => c.getBoundingClientRect().width))`, returnByValue: true })).result.value;
    const frameWidth = phone ? (await run('Runtime.evaluate', { expression: `document.querySelector('[data-testid=phone] > .viewport')?.getBoundingClientRect().width`, returnByValue: true })).result.value : undefined;
    const findings = frameFindings(measure, frameWidth, result.value);
    return { findings, cardWidth, measure };
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
  const measures = new Set();
  const started = Date.now();
  let cursor = 0;
  await Promise.all(Array.from({ length: CONCURRENCY }, async () => {
    while (cursor < jobs.length) {
      const story = jobs[cursor++];
      const measure = measureFor(story.title, storyPrefix);
      const result = await check(story, measure);
      measures.add(measure.label);
      if (result.cardWidth !== undefined) cardWidths.push(result.cardWidth);
      if (result.findings.length) failures.push({ story, findings: [...new Set(result.findings)], label: measure.label });
    }
  }));

  console.log(`Checked ${jobs.length} stories at ${[...measures].join(' and ')} in ${((Date.now() - started) / 1000).toFixed(1)}s. Card width: ${[...new Set(cardWidths)].join(', ') || 'none found'}px.`);
  if (cardWidths.length === 0 || cardWidths.some((width) => width !== CARD_WIDTH)) {
    console.log(`::error title=Story layout::Card is not ${CARD_WIDTH}px wide at ${[...measures].join(' and ')}; the story preview offset changed, update the widths in story-layout-scopes.mjs.`);
    exitCode = 1;
  }
  for (const { story, findings, label } of failures.sort((a, b) => a.story.id.localeCompare(b.story.id))) {
    console.log(`\n${story.title} / ${story.name}`);
    for (const finding of findings) {
      console.log(`  ${finding}`);
      console.log(`::error title=Story layout::${story.title} / ${story.name} at ${label}: ${finding}`);
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
