#!/usr/bin/env bash
set -euo pipefail
cd "$(dirname "$0")/.."
: "${UNITY_EDITOR:?Set UNITY_EDITOR to the Unity 6000.3.22f1 executable}"
mkdir -p TestResults
"$UNITY_EDITOR" -batchmode -quit -projectPath "$PWD" -buildTarget WebGL -executeMethod Wildbound.Editor.WildboundBuild.WebGL -logFile "$PWD/TestResults/webgl-build.log"
