# Natural Systems: light, scent, wind, growth

The world should feel like a living night rather than a set of abstract video-game switches.  
All new interactions must remain readable, optional where possible, and compatible with the existing rectangular collision solver and 120 Hz simulation.

## Design Rules

1. **Readable first** — every natural reaction has a clear visual + audio cue.
2. **Local and finite** — scent, wind, and flares never penetrate solid terrain or last forever without a clear reset rule.
3. **Movement-compatible** — no system should fight the pounce / wall-kick / roll language.
4. **Optional depth** — main exits stay open; natural systems enrich scenic routes and trials.
5. **Deterministic** — every interaction must be regression-testable.

## Current Systems (v0.4)

| System | Behavior | Persistence |
| --- | --- | --- |
| Moonbloom / Moonwake | Claw activates 6 s flare + lasting bridge for the visit | Temporary unless waystone restored |
| Scent rings | Visible only while stalking, line-of-sight, local | Transient |
| Wind perch | Horizontal drift + balance charge while centered & stalking | Attunement survives fall inside trial |
| Moonbell | Downward rake rebound + traversal refresh | Cooldown |
| Spring flower | Upward launch + pounce/air-dash refresh | Always |
| Golden discovery paths | Opened by reaching a wild place | Saved |

## Proposed Expansions (Priority Order)

### 1. Wind Fields (Sky Garden first)
- Bounded horizontal or slight vertical drift zones marked by subtle particle ribbons.
- Affects free fall and pounce arcs predictably.
- Can be used to extend a pounce or must be countered on perches.
- Implementation: additive velocity term inside the existing sub-step solver (same pattern as trial wind).

### 2. Scent Persistence & Herding
- Killed or startled hares leave a short-lived scent trail that can be followed.
- Optional: a hare that can be gently herded toward an enemy as a distraction (advanced, keep simple first).
- Never required for progress.

### 3. Bloom Variants
- **Updraft bloom**: short vertical lift after activation.
- **Vine bloom**: temporary climbable surface (still rectangular AABB) for a few seconds.
- **Wide dazzle**: larger moth interrupt radius.
- All variants keep the same “claw → visible response → lasting or timed geometry” contract.

### 4. Light-Reactive Prey / Soft Platforms
- Certain prey or moths, when dazzled, leave a brief soft platform or slow-fall zone.
- Gives skilled players extra route options without punishing others.

### 5. Moon-Phase Ambient (Post-Waystone)
- After all three waystones are restored, subtle palette and particle shifts.
- New optional scent trails or golden micro-paths can appear.
- No new required content; purely atmospheric + discovery reward.

## Rules for New Geometry

- All temporary platforms remain full solid AABBs while active.
- Thickness ≥ 0.13 units.
- Activation must be visible before the player is expected to commit.
- Reset rules must be explicit (visit end, trial leave, or timed).

## Testing Contract

Any new natural system must ship with:
- At least one deterministic regression route that activates and uses it.
- A clear “why it failed” state that the objective / journal can surface if used in a trial.
- No change to the existing 112+ case surface without explicit expansion of the test suite.
