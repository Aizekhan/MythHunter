#!/usr/bin/env python3
"""
Boundary Validation: RPG Framework Progression Module

Ensures the Progression module maintains zero dependencies on:
- Unity Engine
- MythHunter game-specific code
- Other RPG Framework modules

This enforces the standalone, reusable nature of the Progression module.
"""

import os
import re
import sys
from pathlib import Path

# Forbidden imports that violate boundary rules
FORBIDDEN_IMPORTS = [
    r'using\s+UnityEngine',
    r'using\s+UnityEditor',
    r'using\s+Unity\.',
    r'using\s+MythHunter',
    r'using\s+RPGFramework\.(?!Progression)',  # No other Framework modules
]

def check_file(file_path: Path) -> list[str]:
    """Check a single C# file for boundary violations."""
    violations = []

    with open(file_path, 'r', encoding='utf-8') as f:
        content = f.read()

    for line_num, line in enumerate(content.splitlines(), start=1):
        for pattern in FORBIDDEN_IMPORTS:
            if re.search(pattern, line):
                violations.append(
                    f"{file_path}:{line_num}: Forbidden import detected: {line.strip()}"
                )

    return violations

def main():
    # Root path for Progression module
    progression_root = Path(__file__).parent.parent / "Assets" / "_Framework" / "Progression"

    if not progression_root.exists():
        print(f"[ERROR] Progression module not found at {progression_root}")
        return 1

    # Scan all C# files
    cs_files = list(progression_root.rglob("*.cs"))

    if not cs_files:
        print(f"[ERROR] No C# files found in {progression_root}")
        return 1

    print(f"[*] Scanning {len(cs_files)} C# files for boundary violations...")

    all_violations = []
    for cs_file in cs_files:
        violations = check_file(cs_file)
        all_violations.extend(violations)

    if all_violations:
        print(f"\n[FAILED] Found {len(all_violations)} boundary violation(s):\n")
        for violation in all_violations:
            print(f"  {violation}")
        return 1
    else:
        print(f"[PASSED] All {len(cs_files)} files respect module boundaries")
        return 0

if __name__ == "__main__":
    sys.exit(main())
