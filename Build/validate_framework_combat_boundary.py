#!/usr/bin/env python3
"""Validate the static boundary of the pure .NET RPGFramework Combat slice."""

from __future__ import annotations

import json
import re
import sys
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]
COMBAT_RUNTIME = ROOT / "Assets" / "_Framework" / "Combat" / "Runtime"
COMBAT_TESTS = ROOT / "Assets" / "_Framework" / "Combat" / "Tests"


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


# Validate Combat Runtime assembly definition
combat_runtime_asmdef_path = COMBAT_RUNTIME / "RPGFramework.Combat.Runtime.asmdef"
combat_runtime_asmdef = read_json(combat_runtime_asmdef_path)

if combat_runtime_asmdef.get("name") != "RPGFramework.Combat.Runtime":
    fail("Combat runtime assembly name must be RPGFramework.Combat.Runtime")

expected_references = [
    "GUID:53b8d6d2c2c3435cb61ad1e7eaa55f8c",  # ECS.Runtime
    "GUID:9ab0b1bbfc7e4a4eaee3000000000003",  # Logging.Runtime
    "GUID:da40609a9472d8c4ea29b285a0de0ce9"   # Serialization.Runtime
]
actual_references = combat_runtime_asmdef.get("references", [])
if sorted(actual_references) != sorted(expected_references):
    fail(f"Combat runtime must reference ECS, Logging, and Serialization. Expected: {expected_references}, got: {actual_references}")

if combat_runtime_asmdef.get("noEngineReferences") is not True:
    fail("Framework Combat runtime must disable Unity engine references")

if combat_runtime_asmdef.get("autoReferenced") is not True:
    fail("Framework Combat runtime must be auto-referenced")

if combat_runtime_asmdef.get("allowUnsafeCode") is not True:
    fail("Framework Combat runtime must allow unsafe code for generic serialization")

# Validate Combat Tests assembly definition
combat_tests_asmdef_path = COMBAT_TESTS / "RPGFramework.Combat.Tests.asmdef"
combat_tests_asmdef = read_json(combat_tests_asmdef_path)

if combat_tests_asmdef.get("name") != "RPGFramework.Combat.Tests":
    fail("Combat test assembly name must be RPGFramework.Combat.Tests")

if "RPGFramework.Combat.Runtime" not in combat_tests_asmdef.get("references", []):
    fail("Combat test assembly must reference RPGFramework.Combat.Runtime")

# Validate no Unity/MythHunter dependencies in Combat Runtime
combat_runtime_sources = sorted(COMBAT_RUNTIME.rglob("*.cs"))
if not combat_runtime_sources:
    fail("no Framework Combat runtime C# files found")

for path in combat_runtime_sources:
    source = path.read_text(encoding="utf-8")
    for forbidden in ("using UnityEngine", "using UnityEditor", "using MythHunter.", "namespace MythHunter"):
        if forbidden in source:
            fail(f"{path.relative_to(ROOT)} contains forbidden dependency marker {forbidden!r}")

# Validate .meta files exist for all assets
for asset_root in (COMBAT_RUNTIME, COMBAT_TESTS):
    for path in sorted(asset_root.rglob("*")):
        if not path.is_file() or path.name.endswith(".meta"):
            continue
        if path.suffix in (".cs", ".asmdef", ".md") and not path.with_name(path.name + ".meta").exists():
            fail(f"missing Unity .meta file for {path.relative_to(ROOT)}")

# Validate GUID uniqueness within Combat module
combat_meta_paths = sorted(COMBAT_RUNTIME.rglob("*.meta")) + sorted(COMBAT_TESTS.rglob("*.meta"))
combat_guids: dict[str, Path] = {}
for path in combat_meta_paths:
    guid = get_guid(path)
    if guid in combat_guids:
        fail(f"duplicate Combat asset GUID {guid}: {combat_guids[guid].relative_to(ROOT)} and {path.relative_to(ROOT)}")
    combat_guids[guid] = path

# Validate Combat test sources exist
combat_test_sources = sorted(COMBAT_TESTS.rglob("*.cs"))
if not combat_test_sources:
    fail("no Framework Combat test source files found")

for path in combat_test_sources:
    if not path.with_suffix(path.suffix + ".meta").exists():
        fail(f"missing Unity .meta file for {path.relative_to(ROOT)}")

# Success
print("Framework Combat static boundary: OK")
print(f"Runtime C# files: {len(combat_runtime_sources)}")
print(f"Test C# files: {len(combat_test_sources)}")
print(f"Combat asset GUIDs checked: {len(combat_guids)}")
print("No UnityEngine, UnityEditor, or MythHunter references found in Framework Combat runtime.")
