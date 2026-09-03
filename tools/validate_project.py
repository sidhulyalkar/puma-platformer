#!/usr/bin/env python3
"""Structural gate; deliberately not presented as a Unity compilation test."""
import json
import re
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]
guids = {}
errors = []
for path in sorted((ROOT / "Assets").rglob("*")):
    if path.suffix == ".meta":
        continue
    meta = Path(str(path) + ".meta")
    if not meta.exists():
        errors.append(f"Missing metadata: {path.relative_to(ROOT)}")
        continue
    match = re.search(r"^guid: ([a-f0-9]{32})$", meta.read_text(), re.M)
    if not match:
        errors.append(f"Invalid GUID: {meta.relative_to(ROOT)}")
    elif match[1] in guids:
        errors.append(f"Duplicate GUID: {meta} and {guids[match[1]]}")
    else:
        guids[match[1]] = meta
    if path.suffix in (".asmdef", ".json"):
        json.loads(path.read_text())
manifest = json.loads((ROOT / "Packages/manifest.json").read_text())
assert manifest["dependencies"]["com.unity.inputsystem"] == "1.14.2"
scene = ROOT / "Assets/Scenes/Wildbound.unity"
assert scene.exists()
scene_guid = re.search(r"^guid: (.+)$", Path(str(scene) + ".meta").read_text(), re.M)[1]
assert scene_guid in (ROOT / "ProjectSettings/EditorBuildSettings.asset").read_text()
assert 'activeInputHandler: 1' in (ROOT / "ProjectSettings/ProjectSettings.asset").read_text()
template = (ROOT / "Assets/WebGLTemplates/Wildbound/index.html").read_text()
for name in ("LOADER_FILENAME", "DATA_FILENAME", "FRAMEWORK_FILENAME", "CODE_FILENAME"):
    assert "{{{ " + name + " }}}" in template
project_version = re.search(r"^  bundleVersion: (.+)$", (ROOT / "ProjectSettings/ProjectSettings.asset").read_text(), re.M)
template_version = re.search(r"productVersion:\s*'([^']+)'", template)
assert project_version and template_version and project_version[1] == template_version[1], "WebGL loader version differs from project"
for path in (ROOT / "Assets/Wildbound/Core").glob("*.cs"):
    assert "using UnityEngine" not in path.read_text(), f"Engine dependency leaked into core: {path}"
if errors:
    raise SystemExit("\n".join(errors))
print(f"PASS project structure: {len(guids)} unique Unity asset GUIDs; scene, input, assemblies, WebGL template")
