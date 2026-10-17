#!/usr/bin/env python3
"""Validate the static boundary of the pure .NET RPGFramework Stats slice."""

from __future__ import annotations

import json
import re
import sys
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]
STATS_RUNTIME = ROOT / "Assets" / "_Framework" / "Stats" / "Runtime"
STATS_TESTS = ROOT / "Assets" / "_Framework" / "Stats" / "Tests"


def fail(message: str) -> None:
    print(f"BOUNDARY CHECK FAILED: {message}", file=sys.stderr)
    raise SystemExit(1)


def read_json(path: Path) -> dict:
    try:
        return json.loads(path.read_text(encoding="utf-8"))
    except Exception as exc:  # report parse errors with the asset path
        fail(f"{path.relative_to(ROOT)} is not valid JSON: {exc}")


def get_guid(meta_path: Path) -> str:
    if not meta_path.exists():
        fail(f"missing Unity metadata file: {meta_path.relative_to(ROOT)}")
    match = re.search(r"^guid:\s*([0-9a-fA-F]+)\s*$", meta_path.read_text(encoding="utf-8"), re.M)
    if not match:
        fail(f"missing GUID in {meta_path.relative_to(ROOT)}")
    return match.group(1).lower()


# Validate Stats Runtime assembly definition
stats_runtime_asmdef_path = STATS_RUNTIME / "RPGFramework.Stats.Runtime.asmdef"
stats_runtime_asmdef = read_json(stats_runtime_asmdef_path)

if stats_runtime_asmdef.get("name") != "RPGFramework.Stats.Runtime":
    fail("Stats runtime assembly name must be RPGFramework.Stats.Runtime")

expected_references = [
    "RPGFramework.ECS.Runtime",
    "RPGFramework.Logging.Runtime",
    "RPGFramework.Serialization.Runtime"
]
actual_references = stats_runtime_asmdef.get("references", [])
if sorted(actual_references) != sorted(expected_references):
    fail(f"Stats runtime must reference ECS, Logging, and Serialization. Expected: {expected_references}, got: {actual_references}")

if stats_runtime_asmdef.get("noEngineReferences") is not True:
    fail("Framework Stats runtime must disable Unity engine references")

if stats_runtime_asmdef.get("autoReferenced") is not True:
    fail("Framework Stats runtime must be auto-referenced")

if stats_runtime_asmdef.get("allowUnsafeCode") is not True:
    fail("Framework Stats runtime must allow unsafe code for generic serialization")

# Validate Stats Tests assembly definition
stats_tests_asmdef_path = STATS_TESTS / "RPGFramework.Stats.Tests.asmdef"
stats_tests_asmdef = read_json(stats_tests_asmdef_path)

if stats_tests_asmdef.get("name") != "RPGFramework.Stats.Tests":
    fail("Stats test assembly name must be RPGFramework.Stats.Tests")

if "RPGFramework.Stats.Runtime" not in stats_tests_asmdef.get("references", []):
    fail("Stats test assembly must reference RPGFramework.Stats.Runtime")

# Validate no Unity/MythHunter dependencies in Stats Runtime
stats_runtime_sources = sorted(STATS_RUNTIME.rglob("*.cs"))
if not stats_runtime_sources:
    fail("no Framework Stats runtime C# files found")

for path in stats_runtime_sources:
    source = path.read_text(encoding="utf-8")
    for forbidden in ("using UnityEngine", "using UnityEditor", "using MythHunter.", "namespace MythHunter"):
        if forbidden in source:
            fail(f"{path.relative_to(ROOT)} contains forbidden dependency marker {forbidden!r}")

# Validate .meta files exist for all assets
for asset_root in (STATS_RUNTIME, STATS_TESTS):
    for path in sorted(asset_root.rglob("*")):
        if not path.is_file() or path.name.endswith(".meta"):
            continue
        if path.suffix in (".cs", ".asmdef", ".md") and not path.with_name(path.name + ".meta").exists():
            fail(f"missing Unity .meta file for {path.relative_to(ROOT)}")

# Validate GUID uniqueness within Stats module
stats_meta_paths = sorted(STATS_RUNTIME.rglob("*.meta")) + sorted(STATS_TESTS.rglob("*.meta"))
stats_guids: dict[str, Path] = {}
for path in stats_meta_paths:
    guid = get_guid(path)
    if guid in stats_guids:
        fail(f"duplicate Stats asset GUID {guid}: {stats_guids[guid].relative_to(ROOT)} and {path.relative_to(ROOT)}")
    stats_guids[guid] = path

# Validate Stats test sources exist
stats_test_sources = sorted(STATS_TESTS.rglob("*.cs"))
if not stats_test_sources:
    fail("no Framework Stats test source files found")

for path in stats_test_sources:
    if not path.with_suffix(path.suffix + ".meta").exists():
        fail(f"missing Unity .meta file for {path.relative_to(ROOT)}")

# Success
print("Framework Stats static boundary: OK")
print(f"Runtime C# files: {len(stats_runtime_sources)}")
print(f"Test C# files: {len(stats_test_sources)}")
print(f"Stats asset GUIDs checked: {len(stats_guids)}")
print("No UnityEngine, UnityEditor, or MythHunter references found in Framework Stats runtime.")
