#!/usr/bin/env python3
from pathlib import Path
p = Path(__file__).parent
(p/"GameSession.cs.gz.b64").write_text((p/"GameSession.cs.gz.b64.part1").read_text() + (p/"GameSession.cs.gz.b64.part2").read_text())
print("joined GameSession.cs.gz.b64", (p/"GameSession.cs.gz.b64").stat().st_size)
