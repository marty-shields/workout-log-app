#!/usr/bin/env python3
"""Usage: check-docs.py [repo root]   (default: the current folder)

Exits 1 and prints file:line for a relative link, image or `#anchor` in a tracked *.md that does not resolve, and for a
docs/images/readme screenshot with no row in the README screenshots table of docs/dev-setup-build.md or a row whose
image is missing. A `?query` on a link, as on the stories pages' `./?path=/story/...`, is not checked.
"""
import pathlib
import re
import subprocess
import sys
import urllib.parse

INLINE_LINK = re.compile(r'\]\(\s*<?([^)\s>]+)>?(?:\s+"[^"]*")?\s*\)')
REFERENCE_LINK = re.compile(r'^\s{0,3}\[[^\]]+\]:\s*<?(\S+?)>?(?:\s+.*)?$')
HTML_LINK = re.compile(r'\b(?:href|src|srcset)\s*=\s*"\s*([^"\s,]+)')
HEADING = re.compile(r'^\s{0,3}#{1,6}\s+(.*?)\s*#*\s*$')
HTML_ANCHOR = re.compile(r'\b(?:id|name)\s*=\s*"([^"]+)"')
FENCE = re.compile(r'^\s*(```|~~~)')
INLINE_CODE = re.compile(r'`+[^`\n]*`+')
SCHEME = re.compile(r'^[A-Za-z][A-Za-z0-9+.-]*:')
SCREENSHOT_ROW = re.compile(r'^\| `([a-z-]+)` \| `[^`]+` \|\s*$')
SCREENSHOT_DIR = pathlib.PurePosixPath('docs/images/readme')
SCREENSHOT_DOC = pathlib.PurePosixPath('docs/dev-setup-build.md')


def prose_lines(text):
    fence = None
    for number, line in enumerate(text.splitlines(), 1):
        match = FENCE.match(line)
        if match:
            fence = None if fence == match.group(1) else fence or match.group(1)
            continue
        if fence is None:
            yield number, line


def links(text):
    for number, line in prose_lines(text):
        line = INLINE_CODE.sub('', line)
        for pattern in (INLINE_LINK, REFERENCE_LINK, HTML_LINK):
            for match in pattern.finditer(line):
                yield number, match.group(1)


def slug(heading):
    text = re.sub(r'!?\[([^\]]*)\]\([^)]*\)', r'\1', heading)
    text = re.sub(r'<[^>]+>|[`*]', '', text).strip().lower()
    return re.sub(r'[^\w\- ]', '', text).replace(' ', '-')


def anchors(text):
    found, seen = set(), {}
    for _, line in prose_lines(text):
        heading = HEADING.match(line)
        if heading:
            base = slug(heading.group(1))
            count = seen.get(base, 0)
            seen[base] = count + 1
            found.add(base if count == 0 else f'{base}-{count}')
        found.update(HTML_ANCHOR.findall(line))
    return found


def check_links(root, files):
    findings, cache = [], {}

    def anchors_of(path):
        if path not in cache:
            cache[path] = anchors(path.read_text(encoding='utf-8'))
        return cache[path]

    for file in files:
        source = root / file
        for number, target in links(source.read_text(encoding='utf-8')):
            if SCHEME.match(target):
                continue
            path_part, _, fragment = urllib.parse.unquote(target).partition('#')
            path_part = path_part.partition('?')[0]
            if not path_part:
                base = source
            elif path_part.startswith('/'):
                base = root / path_part.lstrip('/')
            else:
                base = source.parent / path_part
            resolved = base.resolve()
            if not resolved.is_relative_to(root.resolve()) or not resolved.exists():
                findings.append(f'{file}:{number}: link to a missing path: {target}')
            elif fragment and resolved.suffix == '.md' and fragment.lower() not in anchors_of(resolved):
                findings.append(f'{file}:{number}: no heading or anchor "#{fragment}" in {resolved.relative_to(root.resolve()).as_posix()}')
    return findings


def check_screenshots(root):
    doc = root / SCREENSHOT_DOC
    if not doc.exists():
        return []
    rows = {}
    for number, line in enumerate(doc.read_text(encoding='utf-8').splitlines(), 1):
        match = SCREENSHOT_ROW.match(line)
        if match:
            rows[match.group(1)] = number
    images = {p.name[:-len('-light.png')] for p in (root / SCREENSHOT_DIR).glob('*-light.png')}
    findings = [f'{SCREENSHOT_DOC}: no row for {SCREENSHOT_DIR}/{name}-light.png' for name in sorted(images - rows.keys())]
    for name, number in rows.items():
        for theme in ('light', 'dark'):
            if not (root / SCREENSHOT_DIR / f'{name}-{theme}.png').exists():
                findings.append(f'{SCREENSHOT_DOC}:{number}: row `{name}` has no {SCREENSHOT_DIR}/{name}-{theme}.png')
    return findings


def main():
    root = pathlib.Path(sys.argv[1] if len(sys.argv) > 1 else '.')
    tracked = subprocess.run(['git', '-C', str(root), 'ls-files', '*.md'], capture_output=True, text=True, check=True)
    files = tracked.stdout.splitlines()
    findings = check_links(root, files) + check_screenshots(root)
    if findings:
        print('\n'.join(findings))
    return 1 if findings else 0


if __name__ == '__main__':
    sys.exit(main())
