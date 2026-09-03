# Natural Systems: light, scent, wind, growth

## Design Rules

1. **Readable first** — every natural reaction has a clear visual + audio cue.
2. **Local and finite** — scent, wind, and flares never penetrate solid terrain or last forever without a clear reset rule.
3. **Movement-compatible** — no system should fight the pounce / wall-kick / roll language.
4. **Optional depth** — main exits stay open; natural systems enrich scenic routes and trials.
5. **Deterministic** — every interaction must be regression-testable.

## Current Systems (v0.5+)

| System | Behavior | Persistence |
| --- | --- | --- |
| Moonbloom / Moonwake | Claw activates 6 s flare + lasting bridge for the visit | Temporary unless waystone restored |
| **Updraft bloom** | While glowing, soft vertical lift in radius | Timed with GlowTime |
| **Wide-dazzle bloom** | Larger moth interrupt radius + longer stun | On activation |
| Scent rings / hare marks | Stalking + LOS | Transient |
| Wind fields (Sky Garden) | Bounded additive velocity | Authored |
| Memory vignettes | Discovery / memory pickup payload | Display timer |
| Springs, moonbell, golden paths | As before | — |

## Bloom variants

`BloomKind`: `Standard` | `Updraft` | `WideDazzle`

- **Standard** — bridge enable + moth stun within 5 units for 1.2 s.
- **Updraft** — same + while `GlowTime > 0`, `SampleUpdraft` applies vertical accel (stronger near center, radius 2.4). Does **not** grant free flight; limited by glow duration.
- **WideDazzle** — moth stun within 8 units for 1.6 s.

Authored placement:
- Amber Canopy second bloom → Updraft
- Sky Garden second bloom → WideDazzle

## Later

- Vine bloom (temporary climbable AABB for GlowTime only)
- Soft platforms from dazzled moths
- Moon-phase ambient after three waystones
