#!/usr/bin/env python3
"""
Static boundary validation for RPGFramework.Inventory.Runtime.
Ensures zero Unity dependencies and zero MythHunter coupling.
"""

import os
import re
import sys

# Framework Inventory module path
FRAMEWORK_PATH = "Assets/_Framework/Inventory/Runtime"

# Forbidden dependencies
FORBIDDEN_UNITY = [
    "UnityEngine",
    "UnityEditor",
    "Unity.",
]

FORBIDDEN_GAME = [
    "MythHunter",
    "using MythHunter",
]

def check_file(filepath):
    """Check a single C# file for forbidden dependencies."""
    violations = []

    with open(filepath, 'r', encoding='utf-8') as f:
        content = f.read()

        # Check for Unity dependencies
        for forbidden in FORBIDDEN_UNITY:
            if re.search(rf'\b{re.escape(forbidden)}', content):
                violations.append(f"UNITY DEPENDENCY: {filepath} references {forbidden}")

        # Check for MythHunter dependencies
        for forbidden in FORBIDDEN_GAME:
            if forbidden in content:
                violations.append(f"GAME COUPLING: {filepath} references {forbidden}")

    return violations

def main():
    """Main validation entry point."""
    if not os.path.exists(FRAMEWORK_PATH):
        print(f"ERROR: Framework path not found: {FRAMEWORK_PATH}")
        return 1

    all_violations = []

    # Scan all C# files in Framework Inventory module
    for root, dirs, files in os.walk(FRAMEWORK_PATH):
        for file in files:
            if file.endswith('.cs'):
                filepath = os.path.join(root, file)
                violations = check_file(filepath)
                all_violations.extend(violations)

    if all_violations:
        print("BOUNDARY VALIDATION FAILED:")
        for violation in all_violations:
            print(f"  [FAIL] {violation}")
        return 1
    else:
        print("[PASS] BOUNDARY VALIDATION PASSED")
        print(f"   Checked {FRAMEWORK_PATH}")
        print("   Zero Unity dependencies")
        print("   Zero MythHunter coupling")
        return 0

if __name__ == "__main__":
    sys.exit(main())
