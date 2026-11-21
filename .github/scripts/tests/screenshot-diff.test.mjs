// Run by screenshot-diff-test.sh (node --test).
import { test } from 'node:test';
import assert from 'node:assert/strict';
import { compare, PIXEL_DISTANCE, STALE_RATIO } from '../screenshot-diff.mjs';

const width = 428;
const height = 931;
const blank = () => new Uint8ClampedArray(width * height * 4).fill(255);
const stale = (a, b) => compare(a, b, PIXEL_DISTANCE).ratio > STALE_RATIO;

// Moves pixels by `shift` in every colour channel (alpha untouched).
function shifted(image, pixels, shift) {
  const copy = image.slice();
  for (let p = 0; p < pixels; p++) for (let c = 0; c < 3; c++) copy[p * 4 + c] -= shift;
  return copy;
}

test('identical images pass', () => {
  const result = compare(blank(), blank(), PIXEL_DISTANCE);
  assert.equal(result.differing, 0);
  assert.equal(stale(blank(), blank()), false);
});

test('anti-aliasing noise passes: every pixel slightly off, and a few edges moderately off', () => {
  const everyPixel = shifted(blank(), width * height, 10); // distance 30, under PIXEL_DISTANCE
  assert.equal(stale(blank(), everyPixel), false);
  const edges = shifted(blank(), 15, 40); // distance 120, but only 15 pixels (under STALE_RATIO)
  assert.equal(stale(blank(), edges), false);
});

test('a text-sized change fails', () => {
  const word = shifted(blank(), 218, 100); // the measured pixel count of a one-letter header change
  assert.equal(stale(blank(), word), true);
});
