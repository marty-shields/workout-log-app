# Reads a trx file and builds a dotnet test filter from failed tests. Sourced by e2e-run.sh.

# The full name of each failed test in a trx file, one per line.
failed_tests() {
  awk 'BEGIN { RS = "<" }
    function attr(name,   s, i) {
      if (!match($0, " " name "=\"[^\"]*\"")) return ""
      s = substr($0, RSTART + length(name) + 3, RLENGTH - length(name) - 4)
      gsub(/&quot;/, "\"", s); gsub(/&apos;/, "\047", s); gsub(/&lt;/, "<", s); gsub(/&gt;/, ">", s)
      while ((i = index(s, "&amp;")) > 0) s = substr(s, 1, i) substr(s, i + 5)
      return s
    }
    /^UnitTestResult / && attr("outcome") == "Failed" { failed[attr("testId")] = 1 }
    /^UnitTest / { id = attr("id") }
    /^TestMethod / { full[id] = attr("className") "." attr("name") }
    END { for (id in failed) print full[id] }' "$1"
}

# A filter for these full names: Class.Method for a Scenario Outline (all its rows), the exact name otherwise. No "(" in
# it: the NUnit adapter's filter parser runs out of memory on an unbalanced one, so no retry would run.
filter_for() {
  awk '{ i = index($0, "("); print (i ? "FullyQualifiedName~" substr($0, 1, i - 1) : "FullyQualifiedName=" $0) }' \
    | sort -u | paste -sd '|'
}
