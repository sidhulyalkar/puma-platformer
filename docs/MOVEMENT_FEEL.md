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
| Soft tail-glide | Brief float after full pounce / stomp | Hold jump; limited budget |
| Air-control window | Steering after recovery moves | Short boost to air accel |
| **Claw-climb (Bark)** | Limited vertical on tagged faces | Stamina drain; jump kicks off |

## Claw-climb contract

- **Surface:** only `Surface.Bark` sets `WallClimbable`.
- **Enter:** airborne, press into wall, hold jump *or* aim up, `ClimbBudget > 0`.
- **While climbing:** up (jump held / aim up) or slow down (aim down); slight press into wall.
- **Drain:** full while moving; slower while clinging still. Regens on ground.
- **Exit:** budget empty, leave wall, roll/dash, or **Jump** → wall kick away.
- **Event:** `GameEvent.Climb` while actively climbing (VFX/audio).
- **Not** infinite wall run — budget ≈ 1.35 s at full climb speed.

### Tuning

- `ClimbSpeed`, `ClimbBudgetSeconds`, `ClimbRegenSeconds`

### Regressions

- Climb rises on Bark and not on Stone
- Budget expires and drops the puma
- Jump-from-climb issues WallKick

## Later candidates

- One-way platforms / gentle slopes
- Mantle into immediate pounce or claw

## Feel Checklist (Playtest)

- [ ] New player deliberately pounces within 60 s
- [ ] Wall kick consistently clears the wall
- [ ] Bark climb is discoverable and clearly limited
- [ ] Optional routes feel like rewards, not chores
