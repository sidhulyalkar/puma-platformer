# Combat Design: predator fights

## Fantasy

You are not a tank. You are a **hunter** — read a tell, spend a claw, create space, use the land (bloom, bark, vent, roll).

## Single-target (baseline)

| Enemy | Answer |
| --- | --- |
| Moss hare | Stalk + ambush or simple claw |
| Thornling | Jump the leap; punish recover |
| Bristleback | Vault / roll / rear hits; no front armor |
| Reed spitter | Close during reload; use cover |
| Lantern moth | Bloom dazzle or roll the dive |

## Multi-enemy (v0.6)

See [ENCOUNTERS.md](ENCOUNTERS.md). Core idea: **stagger pressure** so the player always has a “next safe verb.”

### Example fight loops

**Canopy skirmish (StaggerTell)**  
Thorn A leaps → roll or jump → claw recover → Thorn B leaps → pounce gap.

**Pincer**  
Bristle tell → vault or bark climb → spit volley → dash-claw or cover → finish from rear.

**TwinDive**  
Moth A marks → bloom or open ground → claw/roll → Moth B on offset clock.

**ShelfAmbush**  
Respect the dive first, then clear the ground guard — or drop off the shelf.

## Tools that scale in packs

- **Roll** — invulnerable window vs seeds and contact
- **Bloom** — multi-moth interrupt
- **Bark climb** — leave the floor fight briefly
- **Heat vent / wind** — reposition without spending pounce
- **Stomp rebound** — refresh tools mid-pack

## What we avoid

- Three armored enemies overlapping Active frames
- Invisible spawn-in during a tell
- A single pixel-perfect claw as the only solution

## Future elites

- **Ash hopper** (Cinder): hare that leaves a short hazard puddle
- **Glass moth** (Frostglass): dive reflects if clawed head-on
- **Duelist bristle**: longer recover but feint charge
