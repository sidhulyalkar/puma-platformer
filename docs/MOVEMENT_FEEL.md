# Movement Feel: predator rhythm

## Target Fantasy

A low coil, a long leap, a committed sweep, a soft roll, claws catching a wall, and a tail balancing in flight.  
Movement and combat share the same language. Hunting creates routes and recovers momentum.

## Current Kit (v0.4 + mantle foundation)

| Verb | Purpose | Feel Target |
| --- | --- | --- |
| Variable jump + coyote/buffer | Forgiving, readable short hops | Immediate response, soft apex |
| Charged pounce (aimable) | Signature predator launch | Clear coil anticipation, satisfying arc |
| Wall slide + kick lock | Reliable vertical recovery | Kick must clear the wall |
| Falling rake rebound | Movement-combat synergy | Earned refresh, not free flight |
| Dash-claw / Moonfang | Closing + strike | Readable commitment |
| Ground roll | Timed defense + low profile | Vulnerable ends, clear dodge window |
| Spring flower | Route linking + resource refresh | Instant upward joy |
| **Mantle / ledge grab** | Vertical recovery & readability | Automatic when rising into a clear lip |

**Priority**: human playtest and tune the above before adding further verbs. Record hesitation, missed landings, and recovery clarity.

## Mantle Implementation (v0.5 foundation)

Inspired by common public 2D platformer patterns (ledge detection + short locked pull-up) found in open repositories such as:

- ta-david-yu/2D-Platformer-Hunter (ledge grabbing)
- Matthew-J-Spencer/Ultimate-2D-Controller (feel tricks & ledge concepts)
- Unity CharacterControllerSamples Platformer ledge-grab state notes

**No third-party code or assets were copied.** The system is implemented originally against Wildbound’s rectangular AABB solver.

### Contract

- Detection: `WorldCollision.TryFindLedge` looks for a solid platform top near chest height in the facing direction with clear standing space above it.
- Trigger: automatic while airborne, not low-profile, not rolling/dashing, and vertical velocity ≥ −2 (prefers rising / near-apex approaches so pure falls past a ledge do not auto-grab).
- State: short locked rise (`MantleSeconds` ≈ 0.22 s at `MantleSpeed`) then a small forward hop onto the surface.
- Event: `GameEvent.Mantle` for audio/VFX hooks.
- Does not create infinite vertical climb on plain walls; requires a real lip with clear top space.

### Tuning knobs (`MovementTuning`)

- `MantleSeconds`, `MantleSpeed`
- `MantleReachX`, `MantleReachY`

### Regression

`SimulationCases.MantleOntoLedge` verifies a rising approach onto a clear ledge produces `GameEvent.Mantle` and leaves the puma above the lip.

## Proposed Next Additions (Ordered)

### 1. Soft Tail Glide / Counterweight (Medium)
- Brief, controllable float or slight upward bias while holding a button after a full pounce or successful falling rake.
- Feels feline; limited duration so it does not become unlimited glide.

### 2. Directional Air Control Window
- Short window of increased air acceleration after wall kick, falling-rake rebound, or mantle.

### 3. Later Candidates
- Limited claw-climb on specifically tagged textured surfaces.
- One-way platforms and gentle slopes (requires careful collision ownership changes).
- Mantling into an immediate pounce or claw for advanced expression.

## Tuning Discipline

- All new movement must remain deterministic and covered by regression routes.
- Prefer additive flags and short locked states over continuous new physics models.
- Keep the rectangular solver as the single source of truth; do not mix Rigidbody2D motion.
- Document every new constant in `MovementTuning` and update the teaching order.

## Feel Checklist (Playtest)

- [ ] New player deliberately pounces within 60 s
- [ ] Wall kick consistently clears the wall
- [ ] Falling rake rebound feels earned
- [ ] Roll recovery is readable under pressure
- [ ] Players can see the landing before committing to a long pounce
- [ ] Mantle recovers near-miss vertical approaches without feeling sticky
- [ ] Optional routes feel like rewards, not chores
