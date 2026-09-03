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
| Moonbloom Standard | Claw → 6 s flare + lasting bridge | Visit / waystone |
| **Updraft** | Soft vertical lift while glowing | GlowTime |
| **WideDazzle** | Moth stun ≤8u / 1.6s | On activation |
| **Vine** | Timed climbable `Surface.Vine` AABBs | **Only while GlowTime > 0** |
| Wind fields | Additive velocity ribbons | Authored |
| Hare scent marks | Stalking + LOS | ~4.5 s |
| Memory vignettes | Discovery / memory payload | Display timer |

## Bloom variants

`BloomKind`: `Standard` | `Updraft` | `WideDazzle` | `Vine`

### Vine contract

- Linked platforms use `Surface.Vine` + `LightSource = bloomIndex`, start `Enabled = false`.
- While `GlowTime > 0`, those platforms are solid (full AABB, thickness ≥ 0.13).
- When glow expires, vines disable again (unlike moonbridges, which stay for the visit).
- Grotto first bloom is Vine with a short step ladder (optional climb).

## Later

- Soft platforms from dazzled moths
- Moon-phase ambient after three waystones
