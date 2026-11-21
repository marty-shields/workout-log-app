// Run by screenshot-diff-test.sh (node --test).
import { test } from 'node:test';
import assert from 'node:assert/strict';
import { compare, isStale, PIXEL_DISTANCE } from '../screenshot-diff.mjs';

const width = 428;
const height = 931;
const total = width * height; // 398,468; STALE_RATIO 0.0001 allows 39 changed pixels, not 40
const blank = () => new Uint8ClampedArray(total * 4).fill(255);
const run = (image) => compare(blank(), image, PIXEL_DISTANCE);

// Lowers `pixels` pixels by `shift` in each channel of `channels` (0 red, 1 green, 2 blue, 3 alpha).
function shifted(pixels, shift, channels = [0, 1, 2]) {
  const copy = blank();
  for (let p = 0; p < pixels; p++) for (const c of channels) copy[p * 4 + c] -= shift;
  return copy;
}

test('identical images pass', () => {
  const result = run(blank());
  assert.equal(result.differing, 0);
  assert.equal(result.maxDistance, 0);
  assert.equal(isStale(result), false);
});

test('small uniform shift and a few changed pixels pass', () => {
  const everyPixel = run(shifted(total, 8)); // distance 24 on every pixel, not counted
  assert.equal(everyPixel.differing, 0);
  assert.equal(everyPixel.maxDistance, 24);
  assert.equal(isStale(everyPixel), false);
  const edges = run(shifted(15, 40)); // distance 120, but only 15 pixels
  assert.equal(edges.differing, 15);
  assert.equal(edges.maxDistance, 120);
  assert.equal(isStale(edges), false);
});

test('ratio boundary: 39 changed pixels pass, 40 fail', () => {
  assert.equal(isStale(run(shifted(39, 100))), false);
  const result = run(shifted(40, 100));
  assert.equal(result.differing, 40);
  assert.equal(isStale(result), true);
});

test('distance boundary: exactly 24 is not counted, 25 is', () => {
  const at = run(shifted(1000, 8)); // 3 * 8 = 24
  assert.equal(at.differing, 0);
  assert.equal(isStale(at), false);
  const over = run(shifted(1000, 25, [0])); // 25
  assert.equal(over.differing, 1000);
  assert.equal(over.maxDistance, 25);
  assert.equal(isStale(over), true);
});

test('a uniform shift of white to #f3f3f3 (distance 36) fails', () => {
  const result = run(shifted(total, 12));
  assert.equal(result.differing, total);
  assert.equal(isStale(result), true);
});

test('a single-channel change and an alpha-only change are counted exactly', () => {
  const green = run(shifted(100, 30, [1]));
  assert.equal(green.differing, 100);
  assert.equal(green.maxDistance, 30);
  const alpha = run(shifted(100, 30, [3]));
  assert.equal(alpha.differing, 100);
  assert.equal(alpha.maxDistance, 30);
});

test('a text-sized change fails', () => {
  const word = run(shifted(252, 100)); // the measured pixel count of a one-letter header change
  assert.equal(isStale(word), true);
});

test('compare runs from its source text in a clean scope, as readme-screenshots.mjs sends it', () => {
  const standalone = (0, eval)(`(${compare})`);
  assert.deepEqual(standalone(blank(), shifted(40, 100), PIXEL_DISTANCE), run(shifted(40, 100)));
});
