#!/usr/bin/env python3
"""Usage: check-filler-comments.py [file or folder ...]   (default: src tests)

Fails on an XML doc <summary> or <param> in a .cs or .razor file that only restates the name of its member, parameter,
type or enclosing type, such as "Exercise id." on ExerciseState.Id. Such a comment adds nothing, so it is deleted
rather than reworded. A comment that adds a unit, a range, a why or any other word is left alone. A doc block that
contains an XML comment with `filler-ok` (for example `/// <!-- filler-ok: shown on the stories Docs page -->`) is
skipped, and so is a component `[Parameter]`, whose summary the stories site shows. Prints each finding with its file
and line and exits 1 if there are any.
"""
import os
import pathlib
import re
import sys

STOPWORDS = {
    'a', 'an', 'the', 'of', 'for', 'to', 'is', 'are', 'this', 'that', 'in', 'on', 'and', 'or', 'by', 'with', 'as',
    'be', 'it', 'its', 'get', 'set', 'return', 'value', 'whether', 'when',
}
MODIFIERS = {
    'public', 'private', 'protected', 'internal', 'static', 'sealed', 'abstract', 'virtual', 'override', 'readonly',
    'partial', 'async', 'required', 'new', 'extern', 'unsafe', 'const', 'file', 'volatile', 'ref',
}
TYPE_DECL = re.compile(r'\b(?:class|struct|interface|enum|record(?:\s+(?:class|struct))?)\s+(\w+)')
DOC_LINE = re.compile(r'^\s*///\s?(.*)$')
ELEMENT = re.compile(r'<(summary|param)\b([^>]*)>(.*?)</\1>', re.S)
NAME_ATTR = re.compile(r'name\s*=\s*"(\w+)"')
SUFFIXES = ('ing', 'ed', 'es', 's', 'e')


def words(text):
    spaced = re.sub(r'([a-z0-9])([A-Z])', r'\1 \2', text)
    spaced = re.sub(r'([A-Z]+)([A-Z][a-z])', r'\1 \2', spaced)
    return [stem(w) for w in re.findall(r'[A-Za-z]+', spaced)]


def stem(word):
    word = word.lower()
    for suffix in SUFFIXES:
        if word.endswith(suffix) and len(word) > len(suffix) + 2:
            return word[:-len(suffix)]
    return word


def declaration_head(lines, start):
    """The declaration text from lines[start] up to its body, attributes skipped, and the line index it starts on."""
    index = start
    while index < len(lines) and lines[index].lstrip().startswith('['):
        index += 1
    if index >= len(lines):
        return None, index
    text = ''
    for line in lines[index:index + 12]:
        text += ' ' + line.strip()
        if text.count('(') == text.count(')') and re.search(r'[{;,]|=>', line):
            break
    return re.split(r'\{|=>|;|,(?![^(]*\))', text)[0].strip(), index


def declaration_words(head):
    """Words of the member name, its type and its parameter names and types, plus the parameter names by identifier."""
    head = re.sub(r'<[^<>()]*(?:<[^<>]*>[^<>]*)*>', ' ', head)
    open_paren = head.find('(')
    prefix = head if open_paren < 0 else head[:open_paren]
    tokens = [t for t in re.findall(r'[\w.]+', prefix) if t not in MODIFIERS]
    pool = []
    for token in tokens:
        pool += words(token)
    if open_paren >= 0:
        pool += words(head[open_paren:])
    return set(pool)


def blocks(lines):
    index = 0
    while index < len(lines):
        if DOC_LINE.match(lines[index]):
            start = index
            while index < len(lines) and DOC_LINE.match(lines[index]):
                index += 1
            yield start, index
        else:
            index += 1


def indent_of(line):
    return len(line) - len(line.lstrip())


def check_file(path):
    findings = []
    lines = path.read_text(encoding='utf-8-sig').splitlines()
    enclosing = []
    scanned_to = 0
    for start, end in blocks(lines):
        for number in range(scanned_to, start):
            declared = TYPE_DECL.search(lines[number])
            if declared and not lines[number].lstrip().startswith(('//', '*', '@')):
                enclosing = [(indent_of(lines[number]), declared.group(1))] + [e for e in enclosing if e[0] < indent_of(lines[number])]
        scanned_to = end
        text = '\n'.join(DOC_LINE.match(l).group(1) for l in lines[start:end])
        if 'filler-ok' in text:
            continue
        head, decl_line = declaration_head(lines, end)
        if not head or any('[Parameter' in line for line in lines[end:decl_line]):
            continue
        member_indent = indent_of(lines[decl_line])
        known = declaration_words(head)
        declared = TYPE_DECL.search(head)
        outer = [name for level, name in enclosing if level < member_indent]
        if declared:
            outer = [name for level, name in enclosing if level < member_indent and name != declared.group(1)]
        for name in outer[:1]:
            known |= set(words(name))
        for element in ELEMENT.finditer(text):
            kind, attributes, body = element.groups()
            if re.search(r'<(?!/?(?:para|c)\b)', body):
                continue
            body_words = [w for w in words(re.sub(r'</?\w+[^>]*>', ' ', body)) if w not in STOPWORDS]
            if not body_words:
                continue
            allowed = set(known)
            if kind == 'param':
                named = NAME_ATTR.search(attributes)
                if not named:
                    continue
                allowed |= set(words(named.group(1)))
            if len(body_words) <= 6 and all(w in allowed for w in body_words):
                line = start + text[:element.start()].count('\n') + 1
                label = f'<param name="{named.group(1)}">' if kind == 'param' else '<summary>'
                findings.append((line, f'{label} only restates the name: "{" ".join(body.split())}"'))
    return findings


def source_files(arguments):
    for argument in arguments:
        path = pathlib.Path(argument)
        if path.is_file():
            yield path
            continue
        for root, folders, names in os.walk(path):
            folders[:] = [f for f in folders if f not in ('bin', 'obj', 'node_modules', '.git')]
            for name in sorted(names):
                if name.endswith(('.cs', '.razor')):
                    yield pathlib.Path(root) / name


def main(arguments):
    failed = 0
    for path in source_files(arguments or ['src', 'tests']):
        for line, message in check_file(path):
            failed += 1
            print(f'{path}:{line}: {message}')
            if os.environ.get('GITHUB_ACTIONS'):
                print(f'::error file={path},line={line},title=Filler comment::{message}')
    if failed:
        print(f'{failed} filler doc comment(s). Delete them (the member name already says it), say something the name '
              f'does not, or mark a real exception with /// <!-- filler-ok: why -->.', file=sys.stderr)
    return 1 if failed else 0


if __name__ == '__main__':
    sys.exit(main(sys.argv[1:]))
