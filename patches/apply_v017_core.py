#!/usr/bin/env python3
"""Apply v0.17 core dens/feel files from gzip+b64 siblings in patches/."""
import base64, gzip, pathlib
root = pathlib.Path(__file__).resolve().parents[1]
core = root / "Assets/Wildbound/Core"
mapping = {
    "Movement.cs.gz.b64": "Movement.cs",
    "Enemies.cs.gz.b64": "Enemies.cs",
    "GameSession.cs.gz.b64": "GameSession.cs",
    "WorldRegions.cs.gz.b64": "WorldRegions.cs",
}
for src, dst in mapping.items():
    p = pathlib.Path(__file__).parent / src
    if not p.exists():
        print("skip missing", src)
        continue
    out = core / dst
    out.write_bytes(gzip.decompress(base64.b64decode(p.read_text().strip())))
    print("wrote", out, out.stat().st_size)
print("Done. Commit and push if needed.")
