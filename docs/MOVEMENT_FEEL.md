# Movement Feel: predator rhythm

## Target Fantasy

A low coil, a long leap, a committed sweep, a soft roll, claws catching a wall, and a tail balancing in flight.  
Movement and combat share the same language. Hunting creates routes and recovers momentum.

## Current Kit (v0.4) — Keep & Polish First

| Verb | Purpose | Feel Target |
| --- | --- | --- |
| Variable jump + coyote/buffer | Forgiving, readable short hops | Immediate response, soft apex |
| Charged pounce (aimable) | Signature predator launch | Clear coil anticipation, satisfying arc |
| Wall slide + kick lock | Reliable vertical recovery | Kick must clear the wall |
| Falling rake rebound | Movement-combat synergy | Earned refresh, not free flight |
| Dash-claw / Moonfang | Closing + strike | Readable commitment |
| Ground roll | Timed defense + low profile | Vulnerable ends, clear dodge window |
| Spring flower | Route linking + resource refresh | Instant upward joy |

**Priority**: human playtest and tune the above before adding new verbs. Record hesitation, missed landings, and recovery clarity.

## Proposed Additions (Ordered)

### 1. Mantle / Ledge Grab (High Priority)
- When the puma’s upper body reaches a ledge edge with upward or near-zero vertical velocity, automatically (or near-automatically) pull up.
- Gives recovery and vertical readability without changing the core collision solver drastically.
- Implementation sketch: probe a small ledge box above and slightly in front of the feet; if clear landing exists and velocity is favorable, trigger a short locked pull-up animation + position snap.
- Must respect LowProfile and not create infinite climb on vertical walls.

### 2. Soft Tail Glide / Counterweight (Medium)
- Brief, controllable float or slight upward bias while holding a button after a full pounce or successful falling rake.
- Feels feline; limited duration and resource so it does not become Ori-style unlimited glide.
- Can be gated behind a memory or waystone if desired, but prefer early availability for feel.

### 3. Directional Air Control Window
- Short window of increased air acceleration after wall kick or falling-rake rebound.
- Rewards chaining without rewriting base air physics.

### 4. Later Candidates
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
- [ ] Optional routes feel like rewards, not chores
