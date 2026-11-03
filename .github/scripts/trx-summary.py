"""Usage: python3 -I trx-summary.py [--annotations] <label>=<folder with trx files>...
Prints a Markdown table of the passed, failed and skipped tests of each label, then the failed tests with their messages.
With --annotations it prints one GitHub ::error line per failed test instead. Exits 1 when a folder has no readable trx."""
import sys
import xml.etree.ElementTree as ET
from pathlib import Path

NS = {"t": "http://microsoft.com/schemas/VisualStudio/TeamTest/2010"}


def read_results(label, folder):
    files = sorted(Path(folder).rglob("*.trx"))
    if not files:
        sys.exit(f"{label} has no trx report in {folder}")
    try:
        return [r for file in files for r in ET.parse(file).getroot().findall(".//t:UnitTestResult", NS)]
    except ET.ParseError as error:
        sys.exit(f"{label} has an unreadable trx report in {folder}: {error}")


def message(result):
    return (result.findtext("t:Output/t:ErrorInfo/t:Message", "Failed", NS)).strip()


def escape(text):
    return text.replace("%", "%25").replace("\r", "%0D").replace("\n", "%0A").replace(":", "%3A").replace(",", "%2C")


args = sys.argv[1:]
reports = dict(arg.split("=", 1) for arg in args if arg != "--annotations")
results = {label: read_results(label, folder) for label, folder in reports.items()}
failed = [(label, r) for label, rs in results.items() for r in rs if r.get("outcome") == "Failed"]

if "--annotations" in args:
    for label, r in failed:
        print(f"::error title={escape(label + ': ' + r.get('testName'))}::{escape(message(r))}")
else:
    print("| Tests | Passed | Failed | Skipped |")
    print("| --- | ---: | ---: | ---: |")
    for label, rs in results.items():
        count = lambda outcome: sum(r.get("outcome") == outcome for r in rs)
        print(f"| {label} | {count('Passed')} | {count('Failed')} | {len(rs) - count('Passed') - count('Failed')} |")
    for label, r in failed:
        print(f"\n**{label}: {r.get('testName')}**\n\n```\n{message(r)}\n```")
