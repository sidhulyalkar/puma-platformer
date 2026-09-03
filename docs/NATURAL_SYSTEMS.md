# Natural Systems: light, scent, wind, growth

The world should feel like a living night rather than a set of abstract video-game switches.  
All new interactions must remain readable, optional where possible, and compatible with the existing rectangular collision solver and 120 Hz simulation.

## Design Rules

1. **Readable first** — every natural reaction has a clear visual + audio cue.
2. **Local and finite** — scent, wind, and flares never penetrate solid terrain or last forever without a clear reset rule.
3. **Movement-compatible** — no system should fight the pounce / wall-kick / roll language.
4. **Optional depth** — main exits stay open; natural systems enrich scenic routes and trials.
5. **Deterministic** — every interaction must be regression-testable.

## Current Systems (v0.5)

| System | Behavior | Persistence |
| --- | --- | --- |
| Moonbloom / Moonwake | Claw activates 6 s flare + lasting bridge for the visit | Temporary unless waystone restored |
| Scent rings (tracks) | Visible only while stalking, line-of-sight, local | Transient |
| **Hare scent marks** | Defeated moss hares drop a short-lived mark | ~4.5 s; visit-local |
| Wind perch (trial) | Horizontal drift + balance charge while centered & stalking | Attunement survives fall inside trial |
| **Wind fields (Sky Garden)** | Bounded ribbons add velocity while overlapping | Authored; optional routes |
| Moonbell | Downward rake rebound + traversal refresh | Cooldown |
| Spring flower | Upward launch + pounce/air-dash refresh | Always |
| Golden discovery paths | Opened by reaching a wild place | Saved |
| **Memory vignettes** | Discovery / memory pickup sets `LastVignette` | Display timer; Unity presents UI |

## Wind fields

- `WindField(bounds, velocity)` stored on `WorldDefinition.WindFields`.
- `NaturalSystems.SampleWind` sums all overlapping fields at a point.
- `GameSession` adds `wind * dt` to the motion delta every step (alongside trial perch wind).
- Sky Garden ships two mild ribbons on upper shelves; the ground exit path does not require them.
- Strength stays low so TraverseWorlds / main-route regressions remain stable.

## Memory vignettes

- `WildPlace.MemoryTitle` + `ToVignette(biome)` produce title / body / beat.
- Memory pickups use `MemoryDescriptor.ForBiome` + world `Memory` line.
- `GameSession.LastVignette` + `VignetteTime` are the simulation contract for Unity UI.
- Never gates progress.

## Hare scent marks

- On `GameEvent.Hunt`, drop a `ScentMark` at the impact point.
- Cap 16 marks; life ~4.5 s; advanced each tick.
- Visible with the same stalking + LOS rules as discovery tracks.

## Later expansions

- Bloom variants (updraft / vine / wide dazzle)
- Light-reactive soft platforms
- Moon-phase ambient after three waystones

## Testing contract

Any new natural system ships with at least one deterministic regression that activates and uses it.
