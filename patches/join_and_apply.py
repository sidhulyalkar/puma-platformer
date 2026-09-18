#!/usr/bin/env python3
"""Join split b64 parts (*.p0, *.p1, ...) then run apply_v017_core.py."""
from pathlib import Path
import re, subprocess, sys, base64
p = Path(__file__).parent
for name in ("Movement", "GameSession", "WorldRegions"):
    parts = sorted(
        [x for x in p.glob(f"{name}.cs.gz.b64.p*") if re.search(r"\.p\d+$", x.name)],
        key=lambda x: int(re.search(r"\.p(\d+)$", x.name).group(1)),
    )
    if not parts:
        print("no parts for", name); continue
    out = p / f"{name}.cs.gz.b64"
    data = "".join(x.read_text().strip() for x in parts)
    out.write_text(data)
    print("joined", out, len(data), "from", [x.name for x in parts])
# Runtime WorldView (feel pass)
rt = p.parent / "Assets/Wildbound/Runtime"
wv_parts = sorted(
    [x for x in p.glob("WorldView.cs.gz.b64.p*") if re.search(r"\.p\d+$", x.name)],
    key=lambda x: int(re.search(r"\.p(\d+)$", x.name).group(1)),
)
if wv_parts:
    data = "".join(x.read_text().strip() for x in wv_parts)
    out = p / "WorldView.cs.gz.b64"
    out.write_text(data)
    try:
        raw = base64.b64decode(data)
        import gzip as gz
        (rt / "WorldView.cs").write_bytes(gz.decompress(raw))
        print("wrote Runtime WorldView", (rt / "WorldView.cs").stat().st_size)
    except Exception as e:
        print("WorldView apply FAIL", e)
sys.exit(subprocess.call([sys.executable, str(p / "apply_v017_core.py")]))
