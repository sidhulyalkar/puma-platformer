#!/usr/bin/env bash
set -euo pipefail
cd "$(dirname "$0")/.."
: "${UNITY_EDITOR:?Set UNITY_EDITOR to the Unity 6000.3.22f1 executable}"
mkdir -p TestResults
test_mode="${1:-EditMode}"
case "$test_mode" in
  EditMode) graphics=(-nographics) ;;
  PlayMode) graphics=() ;;
  *) echo "Usage: bash tools/test-unity.sh [EditMode|PlayMode]" >&2; exit 2 ;;
esac
"$UNITY_EDITOR" -batchmode "${graphics[@]}" -projectPath "$PWD" -runTests -testPlatform "$test_mode" -testResults "$PWD/TestResults/$test_mode.xml" -logFile "$PWD/TestResults/$test_mode.log"
