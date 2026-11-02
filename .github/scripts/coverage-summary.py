"""Usage: python3 -I coverage-summary.py <cobertura xml>...
Prints a Markdown table of line and branch coverage per assembly, for the job summary. Test assemblies are left out; an
assembly that appears in several files (Core in the Core and UI runs) is listed once per file."""
import sys
import xml.etree.ElementTree as ET


def percent(rate):
    return f"{float(rate) * 100:.1f}%"


rows = []
for path in sys.argv[1:]:
    for package in ET.parse(path).getroot().iter("package"):
        name = package.get("name")
        if not name.endswith(".Tests"):
            rows.append((name, path, package.get("line-rate"), package.get("branch-rate")))

print("| Assembly | Lines | Branches | Report |")
print("| --- | ---: | ---: | --- |")
for name, path, lines, branches in sorted(rows):
    print(f"| {name} | {percent(lines)} | {percent(branches)} | {path.split('/')[-3]} |")
