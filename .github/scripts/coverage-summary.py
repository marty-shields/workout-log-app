"""Usage: python3 -I coverage-summary.py <assembly>=<cobertura xml>...
Prints a Markdown table of line and branch coverage for each named assembly, read from the report of the test run that
targets it. Other assemblies in the report (what the run pulls in, such as Core in the UI run) are left out."""
import sys
import xml.etree.ElementTree as ET


def percent(rate):
    return f"{float(rate) * 100:.1f}%"


print("| Assembly | Lines | Branches |")
print("| --- | ---: | ---: |")
for argument in sys.argv[1:]:
    assembly, path = argument.split("=", 1)
    package = next((p for p in ET.parse(path).getroot().iter("package") if p.get("name") == assembly), None)
    if package is None:
        sys.exit(f"{assembly} is not in {path}")
    print(f"| {assembly} | {percent(package.get('line-rate'))} | {percent(package.get('branch-rate'))} |")
