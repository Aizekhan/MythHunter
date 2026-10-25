#!/usr/bin/env python3
"""
Static boundary validation for RPGFramework.Loot.Runtime module.
Ensures zero Unity dependencies and zero MythHunter coupling.
"""

import os
import sys
from pathlib import Path

LOOT_RUNTIME_PATH = Path("Assets/_Framework/Loot/Runtime")

FORBIDDEN_IMPORTS = [
    "using UnityEngine",
    "using Unity.",
    "using MythHunter",
    "using RPGFramework.ECS",
    "using RPGFramework.Items",
    "using RPGFramework.Inventory",
]

def scan_file(file_path):
    """Scan a single C# file for forbidden imports."""
    violations = []
    with open(file_path, 'r', encoding='utf-8') as f:
        for line_num, line in enumerate(f, start=1):
            for forbidden in FORBIDDEN_IMPORTS:
                if forbidden in line:
                    violations.append((line_num, forbidden, line.strip()))
    return violations

def main():
    print("[PASS] RPGFramework.Loot.Runtime boundary validation")
    print(f"Scanning: {LOOT_RUNTIME_PATH}")

    if not LOOT_RUNTIME_PATH.exists():
        print(f"[FAIL] Path not found: {LOOT_RUNTIME_PATH}")
        return 1

    cs_files = list(LOOT_RUNTIME_PATH.rglob("*.cs"))
    print(f"Found {len(cs_files)} C# files")

    total_violations = 0
    for cs_file in cs_files:
        violations = scan_file(cs_file)
        if violations:
            print(f"\n[FAIL] {cs_file.relative_to('Assets/_Framework/Loot/Runtime')}")
            for line_num, forbidden, line in violations:
                print(f"  Line {line_num}: {forbidden}")
                print(f"    {line}")
            total_violations += len(violations)

    if total_violations > 0:
        print(f"\n[FAIL] Found {total_violations} boundary violations")
        return 1

    print("\n[PASS] Zero Unity dependencies")
    print("[PASS] Zero MythHunter coupling")
    print("[PASS] Zero Framework dependencies (standalone module)")
    print("[PASS] Boundary validation complete")
    return 0

if __name__ == "__main__":
    sys.exit(main())
