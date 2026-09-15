#!/usr/bin/env python3
import base64, gzip, pathlib
root = pathlib.Path(__file__).resolve().parents[1]
raw = (pathlib.Path(__file__).parent / "Movement.cs.gz.b64").read_text()
out = root / "Assets/Wildbound/Core/Movement.cs"
out.write_bytes(gzip.decompress(base64.b64decode(raw)))
print("Restored", out, out.stat().st_size)
