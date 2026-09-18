# Feel & playability pass (v0.17)

## Critique (theme / gameplay / enjoyment)

### Strengths
- **120 Hz headless core** with coyote + jump buffer keeps input honest under variable frame rates.
- **Optional depth**: dens clear, Prowler duel, Frostglass ice are opt-in skill checks, not mandatory walls.
- **Readable combat grammar**: tells → active → recover; feint is a taught exception.
- **Natural systems** (wind, scent, ice) interweave with movement instead of pure combat gates.

### Gaps addressed this pass
1. **Crash**: WorldView palette only indexed 3 biomes → Cinder/Frostglass hard crash. Fixed to 5.
2. **Ice readability**: ice looked like rock; added pale sheen + crack progress fade.
3. **Ice control**: friction 0.36 felt punitive on first contact → **0.42** (still slides, still commits).
4. **Landing forgiveness**: coyote **0.14s**, buffer **0.15s** (was 0.11 / 0.13).
5. **Post-maneuver air control**: **0.38s** @ 2.25× accel (mantle/pounce feel).
6. **Gravity slam**: fall mult 1.45 → **1.38**, max fall 24 → **22** (less rubber-band on long drops).
7. **Wall-kick lock**: 0.16 → **0.12s** so direction recovers faster.
8. **Silent systems**: Ambush / Hunt Clarity / Ice crack-break / Feint now toast + spark.

### Smoothness contract
| Layer | Rule |
|---|---|
| Simulation | Fixed 1/120 s; Unity `fixedDeltaTime` matched; `maximumDeltaTime` 0.067 |
| Input | Edge flags OR'd in Update, consumed in FixedUpdate (no lost presses) |
| Camera | Soft follow; reduced-motion kills bob/shake |
| Feedback | One toast line per event family; no spam stacking beyond existing toast replace |

### Playability checklist
- [x] First canopy dens clear teaches reward without requiring it
- [x] Prowler feint is announced once, then visual/tell only
- [x] Ice plates crack before break (progress visible)
- [x] JourneySave v2 migrates 4→5 biomes
- [ ] WebGL input latency pass on mid-tier devices (next)
- [ ] Prowler tell ring in WorldCombatView (next)

### Non-goals
- Not matching Celeste frame-perfect tech; prefer **readable commitment** over pixel precision.
- Not auto-balancing every pack; dens remain optional skill islands.
