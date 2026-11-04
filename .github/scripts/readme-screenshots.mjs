#!/usr/bin/env node
// No npm packages: drives headless Chrome over CDP with Node's built-in WebSocket.
// Retakes every README screenshot listed in the "README screenshots" table of docs/dev-setup-build.md.
//
// Usage: readme-screenshots.mjs <stories-url> <chrome-path> [--check]
//   writes docs/images/readme/<file>-light.png and -dark.png from each row's story;
//   --check writes nothing and exits 1 when a committed screenshot differs from its story.
import { spawn } from 'node:child_process';
import { readFileSync, writeFileSync, mkdtempSync, existsSync } from 'node:fs';
import { tmpdir } from 'node:os';
import { join, resolve, dirname } from 'node:path';
import { fileURLToPath } from 'node:url';

const [baseUrl, chromePath, ...flags] = process.argv.slice(2);
if (!baseUrl || !chromePath) {
  console.error('Usage: readme-screenshots.mjs <stories-url> <chrome-path> [--check]');
  process.exit(64);
}
const check = flags.includes('--check');
const repo = resolve(dirname(fileURLToPath(import.meta.url)), '../..');
const imageDir = join(repo, 'docs/images/readme');
const THEMES = ['light', 'dark'];
// Share of pixels that may differ before --check calls a screenshot stale; absorbs anti-aliasing between Chrome builds.
const STALE_RATIO = 0.002;

function rows() {
  const doc = readFileSync(join(repo, 'docs/dev-setup-build.md'), 'utf8');
  const section = doc.split('### README screenshots')[1]?.split('\n### ')[0] ?? '';
  return [...section.matchAll(/^\|\s*`([^`]+)`\s*\|\s*`([^`]+)`\s*\|/gm)].map((m) => ({ file: m[1], story: m[2] }));
}

const chrome = spawn(chromePath, [
  '--headless', '--no-sandbox', '--disable-gpu', '--hide-scrollbars', '--remote-debugging-port=0',
  `--user-data-dir=${mkdtempSync(join(tmpdir(), 'readme-shots-'))}`, 'about:blank',
], { stdio: ['ignore', 'ignore', 'pipe'] });

function die(message) {
  console.error(message);
  chrome.kill();
  process.exit(1);
}
setTimeout(() => die('Timed out'), 300_000).unref();

const endpoint = await new Promise((ok, fail) => {
  let log = '';
  chrome.stderr.on('data', (chunk) => {
    log += chunk;
    const match = /DevTools listening on (ws:\/\/\S+)/.exec(log);
    if (match) ok(match[1]);
  });
  chrome.on('exit', () => fail(new Error(`Chrome exited early:\n${log}`)));
}).catch((error) => die(error.message));

const socket = new WebSocket(endpoint);
await new Promise((ok) => { socket.onopen = ok; });
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
const send = (method, params = {}, sessionId) => new Promise((ok, fail) => {
  const id = ++next;
  pending.set(id, { resolve: ok, reject: fail });
  socket.send(JSON.stringify({ id, method, params, sessionId }));
});
const sleep = (ms) => new Promise((r) => setTimeout(r, ms));

async function page(fn) {
  const { targetId } = await send('Target.createTarget', { url: 'about:blank' });
  const { sessionId } = await send('Target.attachToTarget', { targetId, flatten: true });
  try {
    return await fn((method, params) => send(method, params, sessionId));
  } finally {
    await send('Target.closeTarget', { targetId }).catch(() => {});
  }
}

async function shoot(story, theme) {
  return page(async (run) => {
    await run('Emulation.setDeviceMetricsOverride', { width: 520, height: 1100, deviceScaleFactor: 1, mobile: false });
    await run('Emulation.setEmulatedMedia', { features: [{ name: 'prefers-color-scheme', value: theme }, { name: 'prefers-reduced-motion', value: 'reduce' }] });
    await run('Emulation.setDefaultBackgroundColorOverride', { color: { r: 0, g: 0, b: 0, a: 0 } });
    await run('Page.navigate', { url: `${baseUrl}/iframe.html?id=${story}&viewMode=story` });
    for (let i = 0; ; i++) {
      const { result } = await run('Runtime.evaluate', { expression: '!!document.querySelector(".phone .viewport")?.children.length', returnByValue: true });
      if (result.value) break;
      if (i > 150) throw new Error(`${story} did not render within 30s`);
      await sleep(200);
    }
    await run('Runtime.evaluate', { expression: 'document.documentElement.style.background = document.body.style.background = "transparent"' });
    await sleep(1000);
    const { result } = await run('Runtime.evaluate', { expression: 'JSON.stringify(document.querySelector(".phone").getBoundingClientRect())', returnByValue: true });
    const box = JSON.parse(result.value);
    const { data } = await run('Page.captureScreenshot', { format: 'png', clip: { x: Math.floor(box.x), y: Math.floor(box.y), width: Math.ceil(box.width), height: Math.ceil(box.height), scale: 1 } });
    return Buffer.from(data, 'base64');
  });
}

// Share of differing pixels, decoded by Chrome itself so no PNG library is needed.
async function difference(a, b) {
  return page(async (run) => {
    const expression = `(async () => {
      const load = (src) => new Promise((ok) => { const i = new Image(); i.onload = () => ok(i); i.src = src; });
      const [x, y] = await Promise.all([load('data:image/png;base64,${a.toString('base64')}'), load('data:image/png;base64,${b.toString('base64')}')]);
      if (x.width !== y.width || x.height !== y.height) return 'is ' + x.width + 'x' + x.height + ', the story ' + y.width + 'x' + y.height;
      const pixels = (img) => { const c = new OffscreenCanvas(img.width, img.height); const g = c.getContext('2d'); g.drawImage(img, 0, 0); return g.getImageData(0, 0, img.width, img.height).data; };
      const [p, q] = [pixels(x), pixels(y)];
      let differing = 0;
      for (let i = 0; i < p.length; i += 4) {
        if (Math.abs(p[i] - q[i]) + Math.abs(p[i + 1] - q[i + 1]) + Math.abs(p[i + 2] - q[i + 2]) + Math.abs(p[i + 3] - q[i + 3]) > 24) differing++;
      }
      return differing / (p.length / 4);
    })()`;
    const { result } = await run('Runtime.evaluate', { expression, awaitPromise: true, returnByValue: true });
    return result.value;
  });
}

const table = rows();
if (table.length === 0) die('No rows found under "### README screenshots" in docs/dev-setup-build.md.');

const stale = [];
try {
  for (const { file, story } of table) {
    for (const theme of THEMES) {
      const path = join(imageDir, `${file}-${theme}.png`);
      const fresh = await shoot(story, theme);
      if (!check) {
        writeFileSync(path, fresh);
        console.log(`wrote ${file}-${theme}.png from ${story}`);
        continue;
      }
      if (!existsSync(path)) {
        stale.push(`${file}-${theme}.png is missing (${story})`);
        continue;
      }
      const result = await difference(readFileSync(path), fresh);
      if (typeof result === 'string') stale.push(`${file}-${theme}.png ${result} (${story})`);
      else if (result > STALE_RATIO) stale.push(`${file}-${theme}.png (${(result * 100).toFixed(1)}% of pixels differ from ${story})`);
    }
  }
} catch (error) {
  die(error.message);
}

chrome.kill();
if (check) {
  if (stale.length) {
    console.log('README screenshots out of date; retake them with .github/scripts/readme-screenshots.mjs:');
    for (const line of stale) console.log(`::error title=README screenshot::${line}`);
    process.exit(1);
  }
  console.log(`All ${table.length * THEMES.length} README screenshots match their stories.`);
}
process.exit(0);
