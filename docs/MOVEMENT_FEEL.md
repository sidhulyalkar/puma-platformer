# Movement Feel: predator rhythm

## Target Fantasy

A low coil, a long leap, a committed sweep, a soft roll, claws catching a wall, and a tail balancing in flight.  
Movement and combat share the same language. Hunting creates routes and recovers momentum.

## Current Kit (v0.5)

| Verb | Purpose | Feel Target |
| --- | --- | --- |
| Variable jump + coyote/buffer | Forgiving, readable short hops | Immediate response, soft apex |
| Charged pounce (aimable) | Signature predator launch | Clear coil anticipation, satisfying arc |
| Wall slide + kick lock | Reliable vertical recovery | Kick must clear the wall |
| Falling rake rebound | Movement-combat synergy | Earned refresh, not free flight |
| Dash-claw / Moonfang | Closing + strike | Readable commitment |
| Ground roll | Timed defense + low profile | Vulnerable ends, clear dodge window |
| Spring flower | Route linking + resource refresh | Instant upward joy |
| Mantle / ledge grab | Vertical recovery & readability | Automatic when rising into a clear lip |
| **Soft tail-glide** | Brief float after full pounce / stomp | Hold jump; limited budget |
| **Air-control window** | Steering after recovery moves | Short boost to air accel |

**Priority**: human playtest and tune the above. Record hesitation, missed landings, and recovery clarity.

## Mantle Implementation

See prior notes. Detection via `TryFindLedge`; short locked rise; `GameEvent.Mantle`.

## Soft Tail-Glide + Air-Control Window

### Tail-glide contract

- **Grant:** full pounce (`Charge >= FullPounceCharge` ≈ 0.85) or upward recovery launch (stomp / falling-rake rebound, vertical > 8).
- **Use:** airborne, hold jump, budget remaining, not rolling/dashing/mantling.
- **Effect:** gravity scaled by `GlideGravityScale` (≈ 0.38); no extra fall multiplier while active.
- **Limit:** budget drains in real time (`GlideSeconds` ≈ 0.55; recovery grants ~70%); clears on ground.
- **Event:** `GameEvent.Glide` on first frame of engagement (audio/VFX).
- **Not** unlimited flight — miss the landing window and normal fall returns.

### Air-control window contract

- **Grant:** wall kick, mantle completion, spring, pounce, or any `Launch` / recovery.
- **Duration:** `AirControlSeconds` ≈ 0.32 s.
- **Effect:** air acceleration × `AirControlAccelMult` (≈ 2.15) while the timer is positive.
- Clears on ground.

### Tuning knobs

- `GlideSeconds`, `GlideGravityScale`, `FullPounceCharge`
- `AirControlSeconds`, `AirControlAccelMult`

### Regressions

- `FullPounceGrantsGlide` — full coil + hold jump engages glide and slows descent vs no hold
- `AirControlAfterWallKick` — post-kick lateral steering reaches farther than baseline air accel alone
- Glide budget expires (no infinite float)

## Later candidates

- Limited claw-climb on tagged surfaces
- One-way platforms / gentle slopes
- Mantle into immediate pounce or claw
- Wind fields (see NATURAL_SYSTEMS.md)

## Tuning Discipline

- Deterministic + regression-covered
- Additive short states, not new physics models
- Rectangular solver remains the source of truth
- Document every new constant in `MovementTuning`

## Feel Checklist (Playtest)

- [ ] New player deliberately pounces within 60 s
- [ ] Wall kick consistently clears the wall
- [ ] Falling rake rebound feels earned
- [ ] Roll recovery is readable under pressure
- [ ] Players can see the landing before committing to a long pounce
- [ ] Mantle recovers near-miss vertical approaches without feeling sticky
- [ ] Tail-glide softens a full pounce arc without removing commitment
- [ ] Air-control after kick/mantle feels responsive, not floaty forever
- [ ] Optional routes feel like rewards, not chores
