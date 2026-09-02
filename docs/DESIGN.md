# Design: the night is hers

The fantasy is a curious, powerful female puma in a world worth exploring. Movement and combat share the same language: a low coil, a long leap, a committed sweep, a soft roll, claws catching a wall, and a tail balancing in flight. Hunting should create routes and recover momentum. Exploration never requires clearing an arena or grinding prey.

The nocturnal setting is part of play. Warm fur and pale terrain edges identify the puma and safe landings. Amber warnings identify committed attacks. Blue moonblooms react to her claws, revealing bridges and interrupting moths. Darkness supplies atmosphere without requiring players to navigate invisible hazards.

## Implemented in v0.3

Three regions share a tested route spine with different night palettes, scenery, and additional upper platforms. They establish mechanics and progression; they are not yet three fully bespoke levels. The new Waystone Trials add three separately authored objective rooms, entered from the region’s starting crescent. Clawing a bloom, balancing in wind, rebounding from a moonbell, and breaching roots create physical goals with permanent bridge rewards. See [Moontrail strategy and critique](MOONTRAIL.md).

All terrain is rectangular and solid except dormant moonbridges and breached root gates. Slopes, one-way platforms, ledge grabs, and arbitrary Unity physics interactions remain outside this version.

| Mechanic | Default | Purpose |
| --- | --- | --- |
| Simulation | 120 Hz | Consistent timing and small collision increments |
| Run speed | 8.5 units/s | Useful short jumps and controllable approaches |
| Coyote / jump buffer | 110 ms / 130 ms | Forgive slightly early or late inputs |
| Jump / gravity | 14.5 units/s / 34 units/s² | Compact arc; releasing reduces height |
| Full coil | 650 ms | Anticipation and a meaningful range tradeoff |
| Pounce launch | 13–24 units/s | Holding changes travel, with directional aiming |
| Wall slide / kick lock | 3 units/s / 160 ms | Time to read a wall and reliably clear it |
| Spring flower | 20 units/s upward | Refill pounce and aerial dash to link routes |
| Dash-claw | 21 units/s for 180 ms | Close distance and strike through an opening |
| Ground roll | 340 ms; middle 210 ms dodges | A timed defensive choice with vulnerable ends |
| Vitality / recovery | Five hearts; 300 ms checkpoint freeze | Recover quickly while keeping discoveries |
| Moonwake | Six-second flare; bridge lasts for the visit | A visible environmental response without an expiring platform trap |

These are starting values, not playtest-proven tuning. See [Night Hunt](NIGHT_HUNT.md) for attack, enemy, and resource rules.

## Teaching order

1. Safe ground introduces short/held jumps. The scratch post introduces a three-claw chain without danger.
2. The first hare introduces stalking and timing a strike around a hop. Capturing prey restores vitality and traversal resources.
3. The first moonbloom introduces a response to claws; its bridge connects upper ledges. The first gap teaches charged pounces.
4. A checkpoint keeps discoveries safe. A thornling's curl introduces a warning, a committed jump, and an opening to counter.
5. A spring flower refills movement. The second bloom in a wall-kick alcove connects light, moth interruption, and an upper route in the grotto and sky garden.
6. Reed spitters teach cover and volley timing; the final bristleback teaches jumping above armor or rolling behind a committed charge.
7. An arch introduces intentional world travel. The map allows return visits with collectible progress intact.

Movement tutorial text remains proximity-based. The trial objective card and journal now confirm actual mechanism activation, rather than simply displaying the same lesson forever. The field guide carries a lot of new actions; an input-aware teaching sequence should replace that burden after first-time playtests.

## Design boundaries and next milestones

1. **Unity qualification:** clean import, run EditMode and PlayMode, inspect the actual scene, try every move and counter, collect each memory, and build WebGL. Resolve real console and visual issues before extending scope.
2. **Combat and movement feel:** record claw timing, small jumps, full coils, air-dash chains, low-ceiling rolls, and enemy warnings. Check whether the falling rebound feels earned and whether roll recovery is readable. Tune from observation.
3. **One showcase night region:** replace the canopy's repeated route spine with a handcrafted loop: safe lower trail, fast hunting route, and a moonbloom detour that reconnects to a checkpoint. Make revealed paths visible from their activation point and give their destinations a reason to visit.
4. **Light identity:** assess how well scent rings, moonwake, moth reactions, wind-perch gauges, bell cues, and glowing warnings explain themselves. Only then consider light-carrying prey, optional darkness routes, or a light-reactive guardian. These are future ideas, not shipped mechanics.
5. **Distinct worlds:** give each region its own geometry and a new use of the same moves. Test bridge and hidden-memory reachability deliberately; the current exit tests do not prove every scenic route.
6. **Arcade release:** browser and controller qualification, measured download/frame-time/memory budgets, an actual-game trailer, and integration into sidhulyalkar.com.

The custom rectangular solver owns movement and must not be mixed with Rigidbody2D motion without redefining collision ownership. Art, audio cues, and move implementations are original. There is no background music, boss encounter, dynamic shadow simulation, mobile control scheme, or control remapping yet.

## First-playtest targets

These are prospective goals, not measured outcomes: a new player moves within 15 seconds, deliberately pounces within 60 seconds, understands a warning before taking repeated hits, and finds at least one optional route in five minutes. Ask them to demonstrate one claw chain, a safe roll, a moonbloom response, and one complete waystone trial. Check whether they can explain why a balance attempt failed and what restoring the waystone changed. Record hesitation and missed cues instead of coaching them through unclear instructions.
