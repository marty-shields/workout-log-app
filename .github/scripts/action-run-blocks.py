"""Usage: python3 -I action-run-blocks.py <folder> <action.yml>...
Writes each `run:` block of the given composite actions to <folder>/<action folder>-<file name>-<line>.sh, so shellcheck can
lint it (shellcheck and actionlint read only the workflows). The file is padded with blank lines so shellcheck's line numbers
are the action's. `${{ ... }}` becomes __EXPR__.
Exits 1 when a file has no run block (only composite actions are linted), or when its `shell:` keys and the run blocks read
differ in number: this is not a YAML parser, so a flow mapping or a multi-line plain scalar must be written as `run: |`."""
import pathlib
import re
import sys

KEY = re.compile(r"^(\s*)(?:- )?run:\s*(.*)$")


def indent(line):
    return len(line) - len(line.lstrip())


class Unreadable(Exception):
    pass


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
            if value[0] in "\"'":
                if len(value) < 2 or value[-1] != value[0]:
                    raise Unreadable(f"line {number}: a quoted run value must end on its line")
                value = value[1:-1]
            following = lines[number] if number < len(lines) else ""
            if following.strip() and indent(following) > key_indent:
                raise Unreadable(f"line {number}: a run value on several lines must be a block scalar")
            yield number, [value]


SHELL = re.compile(r"(?:^|[\s{,])shell:")


def shell_keys(lines, skipped):
    return sum(
        1
        for number, line in enumerate(lines, 1)
        if number not in skipped and not line.lstrip().startswith("#") and SHELL.search(line)
    )


def main(folder, paths):
    out = pathlib.Path(folder)
    for path in map(pathlib.Path, paths):
        lines = path.read_text().splitlines()
        found = 0
        skipped = set()
        try:
            for start, body in run_blocks(lines):
                text = re.sub(r"\$\{\{.*?\}\}", "__EXPR__", "\n".join(body))
                name = f"{path.parent.name}-{path.name}-{start}.sh"
                (out / name).write_text("\n" * (start - 1) + text + "\n")
                skipped.update(range(start, start + len(body)))
                found += 1
        except Unreadable as error:
            sys.exit(f"{path}: {error}.")
        if not found:
            sys.exit(f"{path} has no run block. Only composite actions (runs.using: composite) have run steps to lint.")
        shells = shell_keys(lines, skipped)
        if shells != found:
            sys.exit(
                f"{path} has {shells} `shell:` keys but {found} run blocks were read. The extractor does not read every "
                "YAML form: write each run step as `run: |` (or a one-line value) with `shell:` on its own line."
            )


if len(sys.argv) < 3:
    sys.exit(__doc__)
main(sys.argv[1], sys.argv[2:])
