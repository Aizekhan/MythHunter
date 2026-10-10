#!/usr/bin/env python3
"""Validate the static boundary of the pure .NET RPGFramework ECS slice."""

from __future__ import annotations

import json
import re
import sys
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]
RUNTIME = ROOT / "Assets" / "_Framework" / "ECS" / "Runtime"
TESTS = ROOT / "Assets" / "_Framework" / "ECS" / "Tests"


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


runtime_asmdef_path = RUNTIME / "RPGFramework.ECS.Runtime.asmdef"
test_asmdef_path = TESTS / "RPGFramework.ECS.Runtime.Tests.asmdef"
runtime_asmdef = read_json(runtime_asmdef_path)
test_asmdef = read_json(test_asmdef_path)

if runtime_asmdef.get("name") != "RPGFramework.ECS.Runtime":
    fail("runtime assembly name must be RPGFramework.ECS.Runtime")
if runtime_asmdef.get("references") != []:
    fail("Framework ECS runtime must not reference other assemblies")
if runtime_asmdef.get("noEngineReferences") is not True:
    fail("Framework ECS runtime must disable Unity engine references")
if runtime_asmdef.get("autoReferenced") is not True:
    fail("Framework ECS runtime must be auto-referenced during this migration stage")

if "RPGFramework.ECS.Runtime" not in test_asmdef.get("references", []):
    fail("test assembly must reference RPGFramework.ECS.Runtime")
if "TestAssemblies" not in test_asmdef.get("optionalUnityReferences", []):
    fail("test assembly must enable Unity Test Framework references")

runtime_sources = sorted(RUNTIME.rglob("*.cs"))
if not runtime_sources:
    fail("no Framework ECS runtime C# files found")

for path in runtime_sources:
    source = path.read_text(encoding="utf-8")
    for forbidden in ("using UnityEngine", "using UnityEditor", "using MythHunter.", "namespace MythHunter"):
        if forbidden in source:
            fail(f"{path.relative_to(ROOT)} contains forbidden dependency marker {forbidden!r}")
    if not path.with_suffix(path.suffix + ".meta").exists():
        fail(f"missing Unity .meta file for {path.relative_to(ROOT)}")

new_meta_paths = sorted(RUNTIME.rglob("*.meta")) + sorted(TESTS.rglob("*.meta"))
new_guids: dict[str, Path] = {}
for path in new_meta_paths:
    guid = get_guid(path)
    if guid in new_guids:
        fail(f"duplicate new asset GUID {guid}: {new_guids[guid].relative_to(ROOT)} and {path.relative_to(ROOT)}")
    new_guids[guid] = path

legacy_meta_paths = [
    ROOT / "Assets" / "_MythHunter" / "Code" / "Core" / "ECS" / "IComponent.cs.meta",
    ROOT / "Assets" / "_MythHunter" / "Code" / "Core" / "ECS" / "IEntityManager.cs.meta",
]
for path in legacy_meta_paths:
    guid = get_guid(path)
    if guid in new_guids:
        fail(f"Framework assets reuse legacy MythHunter GUID {guid}")

test_sources = sorted(TESTS.rglob("*.cs"))
if not test_sources:
    fail("no Framework ECS test source files found")
for path in test_sources:
    if not path.with_suffix(path.suffix + ".meta").exists():
        fail(f"missing Unity .meta file for {path.relative_to(ROOT)}")

print("Framework ECS static boundary: OK")
print(f"Runtime C# files: {len(runtime_sources)}")
print(f"Test C# files: {len(test_sources)}")
print(f"Framework/Test asset GUIDs checked: {len(new_guids)}")
print("No UnityEngine, UnityEditor, or MythHunter references found in runtime sources.")
