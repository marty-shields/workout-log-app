#!/usr/bin/env node
// No npm packages. Writes the app icon, splash and README icon SVGs from one dumbbell outline.
// Colours come from src/WorkoutLog.App/Resources/AppIcon/palette.json, which AppIconColourTests also reads.
// The MAUI build rasterises these SVGs, so they use only plain paths, solid fills, linearGradient, fill-opacity and clipPath.
//
// Usage: app-icon.mjs
import { readFileSync, writeFileSync } from 'node:fs';
import { join, resolve, dirname } from 'node:path';
import { fileURLToPath } from 'node:url';

const repo = resolve(dirname(fileURLToPath(import.meta.url)), '../..');
const resources = 'src/WorkoutLog.App/Resources';
const palette = JSON.parse(readFileSync(join(repo, resources, 'AppIcon/palette.json'), 'utf8'));

function colour(name) {
  const value = palette[name];
  if (!value) throw new Error(`palette.json has no "${name}"`);
  return value;
}

// Android's adaptive-icon canvas. Launchers show the central 72 units; the safe circle is 66 across.
const CANVAS = 108;
const CENTRE = 54;
const CORNERS = 24;
// The dumbbell is drawn level, then turned by TILT degrees about the canvas centre and scaled so it clears the circular crop.
const TILT = -40;
const SCALE = 0.78;
const SHADOW = {
  'shadow-dark': { dy: 2.2, opacity: 0.5 },
  'shadow-light': { dy: 2.8, opacity: 0.32 },
};

// Parts in the level frame: [x, y, width, height, corner radius]. The bar runs 1 unit under each plate so no seam shows.
const SOLID = [
  [36, 50, 36, 8, 1.5],
  [26, 34, 11, 40, 4],
  [71, 34, 11, 40, 4],
  [21, 42, 5, 24, 2],
  [82, 42, 5, 24, 2],
];
// Slots in the two big plates; holes under the nonzero rule because they wind the other way.
const HOLES = [
  [29.5, 42, 3, 24, 1.5],
  [75.5, 42, 3, 24, 1.5],
];

const num = (v) => String(Math.round(v * 100) / 100);

const angle = (TILT * Math.PI) / 180;
const turn = ([x, y], dy) => {
  const dx = x - CENTRE;
  const ey = y - CENTRE;
  return [
    CENTRE + (dx * Math.cos(angle) - ey * Math.sin(angle)) * SCALE,
    CENTRE + (dx * Math.sin(angle) + ey * Math.cos(angle)) * SCALE + dy,
  ];
};

// A rounded rectangle as lines and circular arcs. Clockwise for solid parts, counter-clockwise for holes.
function rounded([x, y, w, h, r], clockwise, dy) {
  const X = x + w;
  const Y = y + h;
  const p = (px, py) => turn([px, py], dy).map(num).join(',');
  const a = num(r * SCALE);
  const sweep = clockwise ? 1 : 0;
  const arc = (px, py) => `A${a},${a} 0 0,${sweep} ${p(px, py)}`;
  return clockwise
    ? `M${p(x + r, y)} L${p(X - r, y)} ${arc(X, y + r)} L${p(X, Y - r)} ${arc(X - r, Y)} L${p(x + r, Y)} ${arc(x, Y - r)} L${p(x, y + r)} ${arc(x + r, y)} Z`
    : `M${p(x + r, y)} ${arc(x, y + r)} L${p(x, Y - r)} ${arc(x + r, Y)} L${p(X - r, Y)} ${arc(X, Y - r)} L${p(X, y + r)} ${arc(X - r, y)} Z`;
}

const dumbbell = (dy = 0) =>
  [...SOLID.map((r) => rounded(r, true, dy)), ...HOLES.map((r) => rounded(r, false, dy))].join(' ');

const gradient = (id, prefix) => [
  `  <linearGradient id="${id}" x1="0" y1="0" x2="0" y2="1">`,
  `    <stop offset="0" stop-color="${colour(`${prefix}-top`)}" />`,
  `    <stop offset="0.55" stop-color="${colour(`${prefix}-mid`)}" />`,
  `    <stop offset="1" stop-color="${colour(`${prefix}-bottom`)}" />`,
  '  </linearGradient>',
];

const shadowPath = (shadow, indent) =>
  `${indent}<path d="${dumbbell(SHADOW[shadow].dy)}" fill="${colour(shadow)}" fill-opacity="${SHADOW[shadow].opacity}" fill-rule="nonzero" />`;
const bodyPath = (id, indent) => `${indent}<path d="${dumbbell()}" fill="url(#${id})" fill-rule="nonzero" />`;
const background = (indent) => `${indent}<rect width="${CANVAS}" height="${CANVAS}" fill="${colour('icon-background')}" />`;

const OUTPUTS = [
  { file: `${resources}/AppIcon/appicon.svg`, lines: [background('  ')] },
  {
    file: `${resources}/AppIcon/appiconfg.svg`,
    lines: [...gradient('volt', 'volt'), shadowPath('shadow-dark', '  '), bodyPath('volt', '  ')],
  },
  {
    file: `${resources}/AppIcon/appicon_monochrome.svg`,
    lines: [`  <path d="${dumbbell()}" fill="${colour('monochrome')}" fill-rule="nonzero" />`],
  },
  {
    // The light page would swallow the bright top of the gradient, so the light splash has its own deeper one.
    file: `${resources}/Splash/splash.svg`,
    lines: [...gradient('volt', 'volt-deep'), shadowPath('shadow-light', '  '), bodyPath('volt', '  ')],
  },
  {
    file: 'docs/images/readme/icon.svg',
    lines: [
      ...gradient('volt', 'volt'),
      '  <clipPath id="corners">',
      `    <rect width="${CANVAS}" height="${CANVAS}" rx="${CORNERS}" />`,
      '  </clipPath>',
      '  <g clip-path="url(#corners)">',
      background('    '),
      shadowPath('shadow-dark', '    '),
      bodyPath('volt', '    '),
      '  </g>',
    ],
  },
];

const open = `<svg xmlns="http://www.w3.org/2000/svg" width="${CANVAS}" height="${CANVAS}" viewBox="0 0 ${CANVAS} ${CANVAS}">`;

for (const { file, lines } of OUTPUTS) {
  writeFileSync(join(repo, file), [open, ...lines, '</svg>', ''].join('\n'));
  console.log(`wrote ${file}`);
}
