// Pixel comparison for readme-screenshots.mjs --check. Pure, so tests can call it and the script can run it inside Chrome
// (it is sent as source text: keep it free of imports and outer names).

// Colour distance (summed absolute RGBA channel difference) a pixel must exceed to count as changed.
export const PIXEL_DISTANCE = 24;
// Share of pixels that may change before --check calls a screenshot stale.
export const STALE_RATIO = 0.0001;

// True when a compare() result has more changed pixels than STALE_RATIO allows.
export const isStale = (result) => result.ratio > STALE_RATIO;

// compare(expected, actual, pixelDistance) -> { differing, total, ratio, maxDistance } for two RGBA byte arrays of equal length.
export function compare(expected, actual, pixelDistance) {
  let differing = 0;
  let maxDistance = 0;
  for (let i = 0; i < expected.length; i += 4) {
    const distance = Math.abs(expected[i] - actual[i]) + Math.abs(expected[i + 1] - actual[i + 1])
      + Math.abs(expected[i + 2] - actual[i + 2]) + Math.abs(expected[i + 3] - actual[i + 3]);
    if (distance > maxDistance) maxDistance = distance;
    if (distance > pixelDistance) differing++;
  }
  const total = expected.length / 4;
  return { differing, total, ratio: differing / total, maxDistance };
}
