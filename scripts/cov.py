#!/usr/bin/env python3
"""Report line coverage for ONE PodcastSync src assembly from the latest cobertura run.

Usage:
  scripts/cov.py <tests-project-dir>            # measure the matching src assembly
  scripts/cov.py --all                          # aggregate across every src assembly

A test project tests/PodcastSync.<X>.Tests measures src assembly PodcastSync.<X>.
Exits 1 if any line in the measured scope is uncovered (100% gate).
"""
import sys, glob, os, re, xml.etree.ElementTree as ET

SRC = "src"

def latest_cobertura():
    candidates = sorted(
        glob.glob("tests/*/TestResults/**/coverage.cobertura.xml", recursive=True),
        key=os.path.getmtime,
    )
    if not candidates:
        print("NO COVERAGE FILE — run the test project with --collect first", file=sys.stderr)
        sys.exit(2)
    return candidates[-1]

def subject_from_tests(tests_dir):
    base = os.path.basename(os.path.normpath(tests_dir))  # PodcastSync.Storage.Tests
    m = re.match(r"(.+)\.Tests$", base)
    return m.group(1) if m else base

def main():
    if len(sys.argv) == 2 and sys.argv[1] == "--all":
        cov = latest_cobertura()
        subjects = sorted(d for d in os.listdir(SRC) if os.path.isdir(os.path.join(SRC, d)))
    elif len(sys.argv) == 2:
        tests_dir = sys.argv[1]
        cov = latest_cobertura()
        subjects = [subject_from_tests(tests_dir)]
    else:
        print(__doc__, file=sys.stderr); sys.exit(2)

    tree = ET.parse(cov).getroot()
    print(f"coverage file: {cov}")

    overall_fail = False
    for subj in subjects:
        ns_prefix = subj + "."
        uncov_lines = []
        measured_classes = 0
        for cls in tree.iter("class"):
            cname = cls.get("name", "")
            if not (cname == subj or cname.startswith(ns_prefix)):
                continue
            measured_classes += 1
            fn = cls.get("filename", "")
            for ln in cls.findall("lines/line"):
                if ln.get("hits") == "0":
                    uncov_lines.append((f"{subj}/{fn}", ln.get("number")))

        if uncov_lines:
            overall_fail = True
            print(f"  {subj}: NOT 100% ({len(uncov_lines)} uncovered)")
            for fn, num in uncov_lines:
                print(f"      {fn}:{num}")
        else:
            print(f"  {subj}: 100% ✓  ({measured_classes} class(es))")

    if overall_fail:
        sys.exit(1)
    print("RESULT: 100% line coverage across measured assemblies ✓")
    sys.exit(0)

if __name__ == "__main__":
    main()
