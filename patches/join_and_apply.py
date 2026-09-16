#!/usr/bin/env python3
"""Join split b64 parts (*.p0, *.p1, ...) then run apply_v017_core.py."""
from pathlib import Path
import re, subprocess, sys
p = Path(__file__).parent
for name in ("GameSession", "WorldRegions"):
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
sys.exit(subprocess.call([sys.executable, str(p / "apply_v017_core.py")]))
