// Run by story-layout-scopes-test.sh (node --test).
import { test } from 'node:test';
import assert from 'node:assert/strict';
import {
  CARD_WIDTH, COMPONENT_WIDTH, PHONE_WIDTH, SCREEN_PAGE_WIDTH, defaultScopes, measureFor, qualifyScope, slug, storiesOf,
} from '../story-layout-scopes.mjs';

const prefix = 'Workout Log';
const source = (title) => `@attribute [Stories(StoryTitles.Prefix + "/${title}")]\n<Story Name="Mid week" />\n<Story Name="Empty (new)"/>`;

test('slug lowercases and joins words with single dashes', () => {
  assert.equal(slug('Workout Log/This Week'), 'workout-log-this-week');
  assert.equal(slug(' Empty (new) '), 'empty-new');
});

test('a scope without the story prefix gets it prepended', () => {
  assert.equal(qualifyScope('Screens/', prefix), 'Workout Log/Screens/');
  assert.equal(qualifyScope('Workout Log/Screens/', prefix), 'Workout Log/Screens/');
});

test('the default scope is the workout components', () => {
  assert.deepEqual(defaultScopes(prefix), ['Workout Log/Components/Workout/']);
});

test('storiesOf lists every story of a file inside a scope with its id', () => {
  const found = storiesOf(source('Screens/This Week'), ['Workout Log/Screens/'], prefix);
  assert.deepEqual(found.map((s) => s.id), ['workout-log-screens-this-week--mid-week', 'workout-log-screens-this-week--empty-new']);
  assert.equal(found[0].title, 'Workout Log/Screens/This Week');
});

test('storiesOf skips a file outside every scope or without a title', () => {
  assert.deepEqual(storiesOf(source('Components/Workout/Card'), ['Workout Log/Screens/'], prefix), []);
  assert.deepEqual(storiesOf('<Story Name="Lonely" />', ['Workout Log/Screens/'], prefix), []);
});

test('a screen story is measured inside a phone narrowed to the narrowest phone', () => {
  const measure = measureFor('Workout Log/Screens/This Week', prefix);
  assert.equal(measure.kind, 'screen');
  assert.equal(measure.phoneWidth, PHONE_WIDTH);
  assert.equal(PHONE_WIDTH - 32, CARD_WIDTH);
  assert.ok(measure.pageWidth >= 6 * 2 + PHONE_WIDTH + 2 * 48, 'the page holds the phone frame and preview padding');
  assert.equal(measure.pageWidth, SCREEN_PAGE_WIDTH);
});

test('a component story is measured on the whole page at the component width', () => {
  const measure = measureFor('Workout Log/Components/Workout/Card', prefix);
  assert.equal(measure.kind, 'component');
  assert.equal(measure.phoneWidth, null);
  assert.equal(measure.pageWidth, COMPONENT_WIDTH);
});
