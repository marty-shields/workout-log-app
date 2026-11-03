"""Usage: python3 -I coverage-summary.py [--baseline <tsv> | --tsv] <assembly>=<cobertura xml>...
Prints a Markdown table of line and branch coverage for each named assembly, read from the report of the test run that
targets it. Other assemblies in the report (what the run pulls in, such as Core in the UI run) are left out.
--baseline adds the committed line coverage (.github/coverage-baseline.tsv) to the table and exits 1 when an assembly's line
coverage is more than 1 point below it, or is missing from the file. --tsv prints the baseline file instead of the table."""
import math
import sys
import xml.etree.ElementTree as ET

TOLERANCE = 1.0
HEADER = "# Line coverage % per assembly, a gate for dev / build-and-test. Made by coverage-summary.py --tsv (docs/dev-setup-build.md)."


def package_rates(assembly, path):
    package = next((p for p in ET.parse(path).getroot().iter("package") if p.get("name") == assembly), None)
    if package is None:
        sys.exit(f"{assembly} is not in {path}")
    return rate(assembly, path, package, "line-rate"), rate(assembly, path, package, "branch-rate")


def rate(assembly, path, package, name):
    try:
        value = float(package.get(name)) * 100
    except (TypeError, ValueError):
        sys.exit(f"{assembly} has no readable {name} in {path}")
    if math.isnan(value) or math.isinf(value):
        sys.exit(f"{assembly} has a {name} of {package.get(name)} in {path}, not a number")
    return value


def read_baseline(path):
    rows = (line.split("\t") for line in open(path).read().splitlines() if line and not line.startswith("#"))
    return {assembly: float(lines) for assembly, lines in rows}


args = sys.argv[1:]
baseline = None
tsv = False
if args and args[0] == "--baseline":
    baseline = read_baseline(args[1])
    args = args[2:]
elif args and args[0] == "--tsv":
    tsv = True
    args = args[1:]

measured = {}
for argument in args:
    assembly, path = argument.split("=", 1)
    measured[assembly] = package_rates(assembly, path)

if tsv:
    print(HEADER)
    for assembly, (lines, _) in measured.items():
        print(f"{assembly}\t{lines:.1f}")
    sys.exit(0)

print("| Assembly | Lines | Branches |" + (" Baseline lines |" if baseline is not None else ""))
print("| --- | ---: | ---: |" + (" ---: |" if baseline is not None else ""))
failures = []
for assembly, (lines, branches) in measured.items():
    row = f"| {assembly} | {lines:.1f}% | {branches:.1f}% |"
    if baseline is not None:
        floor = baseline.get(assembly)
        row += f" {floor:.1f}% |" if floor is not None else " none |"
        if floor is None:
            failures.append(f"{assembly} has no row in the coverage baseline")
        elif round(lines, 1) < round(floor - TOLERANCE, 1):
            failures.append(f"{assembly} line coverage {lines:.1f}% is more than {TOLERANCE:g} point below its baseline {floor:.1f}%")
    print(row)
for failure in failures:
    print(f"\n**{failure}.**")
    print(f"::error::{failure}. Add tests, or refresh .github/coverage-baseline.tsv if the drop is intended.", file=sys.stderr)
sys.exit(1 if failures else 0)
