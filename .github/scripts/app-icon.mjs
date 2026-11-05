#!/usr/bin/env node
// No npm packages. Writes the app icon, splash and README icon SVGs from one 32 × 32 pixel-art sprite.
// Colours come from src/WorkoutLog.App/Resources/AppIcon/palette.json, which AppIconColourTests also reads.
//
// Usage: app-icon.mjs
import { readFileSync, writeFileSync } from 'node:fs';
import { join, resolve, dirname } from 'node:path';
import { fileURLToPath } from 'node:url';

const repo = resolve(dirname(fileURLToPath(import.meta.url)), '../..');
const resources = 'src/WorkoutLog.App/Resources';
const palette = JSON.parse(readFileSync(join(repo, resources, 'AppIcon/palette.json'), 'utf8'));

// Sprite size, adaptive-icon canvas (108 units at 2 units per pixel), and where the sprite sits on it.
const N = 32;
const CANVAS = 54;
const OFF = 11;
// The 72-unit area a launcher shows, which the background gradient spans.
const WIN0 = 9;
const WIN = 36;
const UNIT = 2;

function sprite() {
  const g = Array.from({ length: N }, () => Array(N).fill(null));
  const s = {
    g,
    px(x, y, c) { if (x >= 0 && x < N && y >= 0 && y < N) g[y][x] = c; return s; },
    get(x, y) { return (x >= 0 && x < N && y >= 0 && y < N) ? g[y][x] : null; },
    rect(x, y, w, h, c) { for (let j = y; j < y + h; j++) for (let i = x; i < x + w; i++) s.px(i, j, c); return s; },
    hline(x1, x2, y, c) { return s.rect(x1, y, x2 - x1 + 1, 1, c); },
    vline(x, y1, y2, c) { return s.rect(x, y1, 1, y2 - y1 + 1, c); },
    each(fn) { for (let y = 0; y < N; y++) for (let x = 0; x < N; x++) fn(x, y, g[y][x]); return s; },
    outline(c) {
      const add = [];
      s.each((x, y, v) => { if (v === null && [[1, 0], [-1, 0], [0, 1], [0, -1]].some(([dx, dy]) => s.get(x + dx, y + dy) !== null)) add.push([x, y]); });
      add.forEach(([x, y]) => s.px(x, y, c));
      return s;
    },
    shadow(c) {
      const add = [];
      s.each((x, y, v) => { if (v !== null && s.get(x + 1, y + 1) === null) add.push([x + 1, y + 1]); });
      add.forEach(([x, y]) => s.px(x, y, c));
      return s;
    },
  };
  return s;
}

function notebook() {
  const s = sprite();
  s.rect(8, 4, 17, 23, 'P');
  s.vline(24, 5, 26, 'p'); s.hline(9, 24, 26, 'p');
  s.rect(8, 4, 17, 7, 'C'); s.hline(8, 24, 4, 'c');
  s.vline(10, 6, 8, 'D'); s.vline(11, 5, 9, 'D'); s.hline(12, 20, 7, 'D'); s.vline(21, 5, 9, 'D'); s.vline(22, 6, 8, 'D');
  [14, 17, 20, 23].forEach((y) => { s.hline(12, 22, y, 'L'); s.px(10, y, 'C'); });
  [6, 9, 12, 15, 18, 21, 24].forEach((y) => { s.px(6, y, 'G'); s.px(7, y, 'g'); });
  s.rect(20, 27, 2, 3, 'R'); s.vline(21, 27, 29, 'r'); s.px(21, 29, null);
  return s.outline('O').shadow('S').g;
}

const ROLES = {
  O: 'outline', P: 'page', p: 'page-shade', C: 'band', c: 'band-highlight', D: 'dumbbell',
  L: 'ruled-line', G: 'ring', g: 'ring-shade', R: 'ribbon', r: 'ribbon-shade',
};
// Android tints the monochrome layer as one shape, so these pixels become holes in it.
const CUT = new Set(['C', 'c', 'L', 'S']);

function colour(name) {
  const value = palette[name];
  if (!value) throw new Error(`palette.json has no "${name}"`);
  return value;
}

function hex(h) { return [1, 3, 5].map((i) => parseInt(h.slice(i, i + 2), 16)); }
function toHex(a) { return '#' + a.map((v) => Math.round(v).toString(16).padStart(2, '0')).join(''); }
function lerp(a, b, t) { const A = hex(a), B = hex(b); return toHex(A.map((v, i) => v + (B[i] - v) * t)); }

const ramp = Object.keys(palette).filter((name) => name.startsWith('background-')).map(colour);
const steps = ramp.length;
ramp.forEach((c, i) => {
  const expected = lerp(ramp[0], ramp[steps - 1], i / (steps - 1));
  if (c !== expected) throw new Error(`background-${i + 1} is ${c}; the ramp from its ends gives ${expected}`);
});

const bayer = [[0, 8, 2, 10], [12, 4, 14, 6], [3, 11, 1, 9], [15, 7, 13, 5]].map((r) => r.map((v) => (v + 0.5) / 16));
function dither(x, y) {
  const f = (y - WIN0) / (WIN - 1) * (steps - 1);
  const ff = Math.max(0, Math.min(steps - 1, f));
  const i = Math.min(Math.floor(ff), steps - 2);
  return (ff - i) > bayer[y % 4][x % 4] ? ramp[i + 1] : ramp[i];
}

const grid = notebook();
const background = { origin: 0, size: CANVAS, colourAt: dither };
const foreground = (shadow) => ({
  origin: OFF,
  size: N,
  colourAt: (x, y) => grid[y][x] && colour(grid[y][x] === 'S' ? shadow : ROLES[grid[y][x]]),
});
const monochrome = {
  origin: OFF,
  size: N,
  colourAt: (x, y) => grid[y][x] && !CUT.has(grid[y][x]) && colour('monochrome'),
};

const OUTPUTS = [
  { file: `${resources}/AppIcon/appicon.svg`, layers: [background] },
  { file: `${resources}/AppIcon/appiconfg.svg`, layers: [foreground('shadow-dark')] },
  { file: `${resources}/AppIcon/appicon_monochrome.svg`, layers: [monochrome] },
  { file: `${resources}/Splash/splash.svg`, layers: [foreground('shadow-light')] },
  { file: 'docs/images/readme/icon.svg', layers: [background, foreground('shadow-dark')], corners: 24 },
];

// Horizontal runs of one colour, merged downwards when the row below repeats the run.
function rects({ origin, size, colourAt }) {
  const out = [];
  let open = new Map();
  for (let y = 0; y < size; y++) {
    const next = new Map();
    for (let x = 0; x < size;) {
      const fill = colourAt(x, y) || null;
      let w = 1;
      while (x + w < size && (colourAt(x + w, y) || null) === fill) w++;
      if (fill) {
        const key = `${x},${w},${fill}`;
        let r = open.get(key);
        if (!r) { r = { x, y, w, h: 0, fill }; out.push(r); }
        r.h++;
        next.set(key, r);
      }
      x += w;
    }
    open = next;
  }
  return out.map((r) => ({ ...r, x: (origin + r.x) * UNIT, y: (origin + r.y) * UNIT, w: r.w * UNIT, h: r.h * UNIT }));
}

function svg({ layers, corners }) {
  const side = CANVAS * UNIT;
  const indent = corners ? '    ' : '  ';
  const body = layers.flatMap(rects)
    .map((r) => `${indent}<rect x="${r.x}" y="${r.y}" width="${r.w}" height="${r.h}" fill="${r.fill}" />`);
  const content = corners
    ? [
      '  <clipPath id="corners">',
      `    <rect width="${side}" height="${side}" rx="${corners}" />`,
      '  </clipPath>',
      '  <g clip-path="url(#corners)">',
      ...body,
      '  </g>',
    ]
    : body;
  return [
    `<svg xmlns="http://www.w3.org/2000/svg" width="${side}" height="${side}" viewBox="0 0 ${side} ${side}" shape-rendering="crispEdges">`,
    ...content,
    '</svg>',
    '',
  ].join('\n');
}

for (const output of OUTPUTS) {
  writeFileSync(join(repo, output.file), svg(output));
  console.log(`wrote ${output.file}`);
}
