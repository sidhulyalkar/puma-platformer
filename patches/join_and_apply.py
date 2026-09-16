#!/usr/bin/env python3
"""Join split b64 parts then run apply_v017_core.py."""
from pathlib import Path
import subprocess, sys
p = Path(__file__).parent
for name in ("GameSession", "WorldRegions"):
    parts = sorted(p.glob(f"{name}.cs.gz.b64.p*"), key=lambda x: x.name)
    if not parts:
        print("no parts for", name); continue
    out = p / f"{name}.cs.gz.b64"
    out.write_text("".join(x.read_text().strip() for x in parts))
    print("joined", out, out.stat().st_size)
sys.exit(subprocess.call([sys.executable, str(p / "apply_v017_core.py")]))
