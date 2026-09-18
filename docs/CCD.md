# Continuous Collision Detection

## Current strategy (v0.4–v0.5)

Wildbound uses a **custom rectangular kinematic solver**, not Unity Rigidbody continuous modes.

| Layer | Method | Notes |
| --- | --- | --- |
| Player / enemy vs terrain | Discrete AABB + **sub-steps ≤ 0.12** | Bound is below `MinSolidThickness` (0.13) |
| Projectiles | Swept segment vs expanded AABB | True continuous for point-like shots |
| Combat strikes | Discrete per sub-step + multi-point LOS | See [COMBAT_PRECISION.md](COMBAT_PRECISION.md) |

**Invariant:** every enabled solid platform must satisfy `min(W, H) ≥ 0.13`.  
If a collider is thinner than the sub-step bound, discrete motion can tunnel.

Simulation runs at **120 Hz**. Peak speeds (full pounce ~24 u/s, dash 21 u/s) yield per-tick travel on the order of 0.2 units, which is split into multiple sub-steps.

## Why not full CCD everywhere?

- The solver is intentionally simple, deterministic, and regression-heavy.
- Sub-stepping already prevents tunneling when the thickness invariant holds.
- Full physics CCD (Box2D / Unity Continuous Dynamic) would replace ownership of collision and break the architecture boundary in [ARCHITECTURE.md](ARCHITECTURE.md).

## Swept AABB foundation

`WorldCollision.SweepAABB` implements the standard **moving AABB vs static AABB** continuous test:

1. Inflate the static obstacle by the moving box size (Minkowski sum in 2D).
2. Segment-cast the moving box’s center from start → start+delta against the inflated box.
3. Return the earliest time-of-impact fraction in `[0, 1]` and a dominant axis hint.

This matches the pattern already used for projectiles and is available for:

- Targeted high-speed player moves (optional future path)
- Validation / debug tools
- Regression proofs against intentionally thin geometry

Player gameplay motion remains on the sub-step path until a playtest or content need requires switching specific verbs to swept resolution.

## Contracts

| Name | Value | Role |
| --- | --- | --- |
| `MaxSubstep` | 0.12 | Max axis travel per discrete sub-step |
| `MinSolidThickness` | 0.13 | Minimum enabled solid extent |
| `StepSeconds` | 1/120 | Fixed simulation step |

Regression cases assert:

- All authored biome platforms respect `MinSolidThickness`
- Peak configured speeds still subdivide under `MaxSubstep`
- `SweepAABB` reports a TOI before the far side of a thin wall under large deltas

## Future hardening (ordered)

1. Keep enforcing thickness in content tools and tests.
2. If max speeds rise, lower `MaxSubstep` or raise tick rate before rewriting motion.
3. Optionally route **pounce / dash only** through swept resolution against static platforms.
4. Speculative contacts or full engine CCD only if the kinematic model is abandoned.
