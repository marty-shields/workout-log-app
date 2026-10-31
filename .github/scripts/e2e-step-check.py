#!/usr/bin/env python3
"""Usage: e2e-step-check.py <E2E project dir>

Matches every Gherkin step in the project's Features against every step definition regex in its Steps, the way
Reqnroll does at run time: the keyword type (Given, When, Then; And and But take the previous one) and a full match
of the text, with Scenario Outline rows expanded. Reports each step that matches no definition or more than one,
with its file and line; exits 1 if there are any.
"""
import os
import pathlib
import re
import sys

STRING = r'@"(?:[^"]|"")*"|"(?:[^"\\]|\\.)*"'
ATTRIBUTE = r'\[(Given|When|Then|StepDefinition)\('
BLOCK = re.compile(r'\s*(Feature|Rule|Background|Scenario Outline|Scenario Template|Scenario|Example)\s*:')
EXAMPLES = re.compile(r'\s*(Examples|Scenarios)\s*:')
STEP = re.compile(r'\s*(Given|When|Then|And|But|\*) (.*)$')


def unquote(token):
    if token.startswith('@'):
        return token[2:-1].replace('""', '"')
    return re.sub(r'\\(u[0-9a-fA-F]{4}|.)',
                  lambda e: chr(int(e.group(1)[1:], 16)) if len(e.group(1)) == 5 else e.group(1), token[1:-1])


def evaluate(expr, consts):
    parts = []
    for token in re.findall(STRING + r'|\w+', expr):
        parts.append(unquote(token) if token.startswith(('"', '@')) else consts[token])
    return ''.join(parts)


def to_python(pattern):
    return re.sub(r'\(\?<(?=[A-Za-z])', '(?P<', pattern)


def read_bindings(project):
    bindings = []
    attributes = 0
    for cs in sorted((project / 'Steps').rglob('*.cs')):
        text = cs.read_text(encoding='utf-8-sig')
        attributes += len(re.findall(ATTRIBUTE, text))
        consts = {m.group(1): unquote(m.group(2))
                  for m in re.finditer(r'const string (\w+) = (' + STRING + r');', text)}
        for m in re.finditer(ATTRIBUTE + r'((?:' + STRING + r'|\s|\+|\w)+)\)\]', text):
            kind, expr = m.groups()
            method = re.compile(r'public\s+[\w<>]+\s+(\w+)').search(text, m.end()).group(1)
            bindings.append((kind, re.compile(to_python(evaluate(expr, consts))), f'{cs.stem}.{method}'))
    return bindings, attributes


def read_steps(feature):
    """Yields (line number, keyword type, step text) for each step, with Scenario Outline rows expanded."""
    block = []
    examples = []
    in_examples = False
    kind = None

    def flush():
        for n, step_kind, text in block:
            names = set(re.findall(r'<([^>]+)>', text))
            texts = {re.sub(r'<([^>]+)>', lambda g: row[g.group(1)], text) for row in examples if names <= row.keys()}
            for expanded in sorted(texts or {text}):
                yield n, step_kind, expanded
        block.clear()
        examples.clear()

    lines = feature.read_text(encoding='utf-8-sig').splitlines()
    for n, line in enumerate(lines, 1):
        if BLOCK.match(line):
            yield from flush()
            in_examples = False
        elif EXAMPLES.match(line):
            in_examples = True
            rows = []
            for row in lines[n:]:
                if not row.strip().startswith('|'):
                    break
                rows.append([c.strip() for c in row.strip().strip('|').split('|')])
            if rows:
                examples.extend(dict(zip(rows[0], r)) for r in rows[1:])
        elif not in_examples and (m := STEP.match(line)):
            keyword, text = m.groups()
            if keyword in ('Given', 'When', 'Then'):
                kind = keyword
            block.append((n, kind, text))
    yield from flush()


def main(project):
    bindings, attributes = read_bindings(project)
    if not bindings or len(bindings) != attributes:
        print(f'Read {len(bindings)} step definitions from {attributes} attributes under {project / "Steps"}: '
              'the attribute pattern in e2e-step-check.py needs updating.', file=sys.stderr)
        return 2

    actions = os.environ.get('GITHUB_ACTIONS') == 'true'
    steps = problems = 0
    for feature in sorted((project / 'Features').rglob('*.feature')):
        for n, kind, text in read_steps(feature):
            steps += 1
            hits = [name for k, rx, name in bindings if k in (kind, 'StepDefinition') and rx.fullmatch(text)]
            if len(hits) == 1:
                continue
            problems += 1
            what = 'matches no step definition' if not hits else f'matches {len(hits)} step definitions: {", ".join(hits)}'
            message = f'step "{kind} {text}" {what}'
            path = feature.as_posix()
            print(f'::error file={path},line={n},title=E2E step binding::{message}' if actions
                  else f'{path}:{n}: {message}')

    if not steps:
        print(f'No steps found under {project / "Features"}.', file=sys.stderr)
        return 2
    print(f'{len(bindings)} step definitions, {steps} steps, {problems} problems')
    return 1 if problems else 0


if __name__ == '__main__':
    sys.stdout.reconfigure(encoding='utf-8')
    sys.exit(main(pathlib.Path(sys.argv[1])))
