"""Usage: python3 -I action-run-blocks.py <folder> <action.yml>...
Writes each `run:` block of the given composite actions to <folder>/<action folder>-<line>.sh, so shellcheck can lint it
(shellcheck and actionlint read only the workflows). The file is padded with blank lines so shellcheck's line numbers are the
action.yml's. `${{ ... }}` becomes __EXPR__. Exits 1 when a file has no run block."""
import pathlib
import re
import sys

KEY = re.compile(r"^(\s*)(?:- )?run:\s*(.*)$")


def indent(line):
    return len(line) - len(line.lstrip())


def run_blocks(lines):
    for number, line in enumerate(lines, 1):
        match = KEY.match(line)
        if not match:
            continue
        value = match.group(2)
        key_indent = len(match.group(1)) + (2 if line.lstrip().startswith("- ") else 0)
        if re.fullmatch(r"[|>][+-]?\d*", value):
            body = []
            for rest in lines[number:]:
                if rest.strip() and indent(rest) <= key_indent:
                    break
                body.append(rest)
            while body and not body[-1].strip():
                body.pop()
            margin = min((indent(b) for b in body if b.strip()), default=0)
            yield number + 1, [b[margin:] for b in body]
        elif value and not value.startswith("#"):
            if len(value) > 1 and value[0] == value[-1] and value[0] in "\"'":
                value = value[1:-1]
            yield number, [value]


def main(folder, paths):
    out = pathlib.Path(folder)
    for path in map(pathlib.Path, paths):
        found = 0
        for start, body in run_blocks(path.read_text().splitlines()):
            text = re.sub(r"\$\{\{.*?\}\}", "__EXPR__", "\n".join(body))
            (out / f"{path.parent.name}-{start}.sh").write_text("\n" * (start - 1) + text + "\n")
            found += 1
        if not found:
            sys.exit(f"{path} has no run block")


if len(sys.argv) < 3:
    sys.exit(__doc__)
main(sys.argv[1], sys.argv[2:])
