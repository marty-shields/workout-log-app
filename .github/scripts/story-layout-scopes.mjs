// Pure helpers of check-story-layout.mjs, split out so node tests can call them.

// The narrowest phone the app supports (360dp) less 16px side padding leaves a 328px card.
export const CARD_WIDTH = 328;
export const PHONE_WIDTH = 360;
// Component stories: the preview adds 96px around the card, so 424 shows a 328px card.
export const COMPONENT_WIDTH = 424;
// Screen stories render the kit's phone frame (bezel included) plus preview padding, which a 424px page cannot hold.
export const SCREEN_PAGE_WIDTH = 480;

export const slug = (text) => text.toLowerCase().replace(/[^a-z0-9]+/g, '-').replace(/^-+|-+$/g, '');

// Adds the story prefix to a scope given without it ("Screens/" -> "Workout Log/Screens/").
export const qualifyScope = (scope, storyPrefix) => (scope.startsWith(`${storyPrefix}/`) ? scope : `${storyPrefix}/${scope}`);

export const defaultScopes = (storyPrefix) => [`${storyPrefix}/Components/Workout/`];

// storiesOf(source, scopes, storyPrefix) -> [{ title, name, id }] for the stories a *.stories.razor file declares inside a scope.
export function storiesOf(source, scopes, storyPrefix) {
  const rest = /\[Stories\(StoryTitles\.Prefix \+ "\/([^"]+)"\)\]/.exec(source)?.[1];
  const title = rest && `${storyPrefix}/${rest}`;
  if (!title || !scopes.some((scope) => title.startsWith(scope))) return [];
  return [...source.matchAll(/<Story\s+Name="([^"]+)"/g)].map((m) => ({ title, name: m[1], id: `${slug(title)}--${slug(m[1])}` }));
}

// How a story is measured. Screen stories are measured inside the phone frame, narrowed to PHONE_WIDTH;
// everything else is measured on the whole page at COMPONENT_WIDTH.
export function measureFor(title, storyPrefix) {
  if (title.startsWith(`${storyPrefix}/Screens/`)) {
    return { kind: 'screen', pageWidth: SCREEN_PAGE_WIDTH, phoneWidth: PHONE_WIDTH, label: `${PHONE_WIDTH}px phone` };
  }
  return { kind: 'component', pageWidth: COMPONENT_WIDTH, phoneWidth: null, label: `${COMPONENT_WIDTH}px` };
}

// Right edge of an element relative to the left edge of the frame it is measured in (0 for the whole page).
export const rightEdge = (boxRight, frameLeft) => boxRight - frameLeft;

// The findings to report for a story: the page's own findings, unless the phone frame is not as wide as the measure expects.
// A missing frame is already a finding of the page check, so only a defined, different width replaces them.
export function frameFindings(measure, frameWidth, findings) {
  if (measure.phoneWidth === null || frameWidth === undefined || frameWidth === measure.phoneWidth) return findings;
  return [`phone frame is ${frameWidth}px wide, not ${measure.phoneWidth}px; the kit's --stories-phone-width changed`];
}
