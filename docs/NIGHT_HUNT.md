# Night Hunt: movement, combat, and moonlight

The Night Hunt moveset gives the puma a hunting rhythm: approach quietly, read a tell, commit to a strike, and spend the opening on movement. Broad claw sweeps and deliberate recovery reflect the requested Reaper-style inspiration; the feline attacks, prey rewards, and light interactions are original.

**Precision contracts** for AABB strikes, armor, stomps, projectiles, and double-hit exclusion live in [COMBAT_PRECISION.md](COMBAT_PRECISION.md).

## Moves

| Action | Keyboard | Behavior |
| --- | --- | --- |
| Claw chain | J or left click, then tap near the end of each strike | Sweep → backhand → crescent; damage 1 / 1 / 2. Each swing hits a target once. |
| Rising rake | W + J | Upward coverage; launches only when started on the ground. Repeating it in midair cannot create flight. |
| Falling rake | S + J while airborne | Descends quickly; a confirmed hit rebounds and refreshes pounce/aerial dash. A miss keeps falling. |
| Pounce rake | J during a charged pounce | A two-damage sweep that retains launch momentum. |
| Dash-claw | K | Fast horizontal strike, two damage, 0.55-second cooldown. One aerial use before a movement refresh. It does not grant invulnerability. |
| Moonfang Rush | K with three instinct | Consumes three instinct for a three-damage dash with extra reach and a blue-white arc. |
| Roll | L on the ground | 0.34-second low roll, 0.65-second cooldown. Dodges after the first 0.04 seconds until the final 0.09 seconds. Remains crouched if a low ceiling blocks standing. |
| Stalk | Hold Q on the ground | Moves at 35% run speed, reveals nearby scent rings, and reduces hare detection distance. Starting a strike while stalking adds one damage against an idle target. |

The attack buffer lasts 0.2 seconds. A combo expires 0.6 seconds after its move ends. Windup and active frames commit the claw; roll/dash and a ready pounce can cancel its recovery. All timing is simulation time, so pause freezes it.

| Claw | Windup | Active | Recovery |
| --- | --- | --- | --- |
| Sweep | 0.07 s | 0.12 s | 0.16 s |
| Backhand | 0.06 s | 0.13 s | 0.17 s |
| Crescent | 0.12 s | 0.18 s | 0.23 s |
| Rising rake | 0.09 s | 0.19 s | 0.22 s |
| Falling rake | 0.09 s | 0.22 s | 0.21 s |
| Pounce rake | 0.025 s | 0.22 s | 0.20 s |
| Dash-claw | Immediate | 0.20 s | 0.22 s |

Down + claw can interrupt an active pounce into a falling rake. Trial moonbells also provide a confirmed-hit rebound; see [Moontrail strategy](MOONTRAIL.md).

Controller equivalents are in the README and in-game field guide. Charge pounces, variable jumps, wall kicks, coyote time, buffered jumps, moving platforms, and spring flowers remain available alongside these moves.

## Wildlife and counters

| Creature | Pattern | Player response |
| --- | --- | --- |
| Moss hare — 1 heart | Brief alert, then a fleeing hop; detects a running puma within 5.5 units versus 1.65 while stalking. Harmless. | Approach quietly or intercept the landing. A capture restores one heart and traversal resources. |
| Thornling — 2 hearts | Curls for 0.48 seconds, leaps forward, then rests for 0.75 seconds. | Give the leap space; counter the landing or rake from above. |
| Bristleback — 4 hearts | Armored front, 0.75-second warning, locked-direction charge, 1.2-second recovery. | Jump above, roll through with correct timing, or strike the back. Front armor opens during recovery/stun. |
| Reed spitter — 3 hearts | Locks aim during a 0.8-second tell, fires three seeds 0.22 seconds apart, then recovers. | Break sight with terrain, time a roll, or close during the volley recovery. |
| Lantern moth — 2 hearts | Glows and marks its target line for 0.8 seconds, dives along that line, then returns home. | Leave the marked line or strike a nearby moonbloom to interrupt it. |
| Scratch post — 6 hearts | Does not attack; reforms after being struck down. | Practice combos. It grants no healing or instinct. |

Enemies need a clear terrain sight line to acquire the puma. Projectiles hit the nearest terrain/player intersection even at speed. Ordinary contact with hostile creatures costs one heart; stunned creatures cannot deal contact damage. Damage grants 0.85 seconds of grace and clears instinct. Terrain hazards still send the puma to her checkpoint during a roll.

Any wildlife defeat refreshes pounce/aerial dash and adds one instinct, capped at three. Only prey restores a heart. The scratch post cannot farm rewards. Traversal refreshes restore availability; they do not erase an ongoing dash cooldown.

## Moonwake and night readability

- Claw a blue moonbloom with an actual unobstructed strike. It flares for six seconds and activates its linked moonbridge.
- The bridge stays solid after the flare and through checkpoint recovery. Re-entering the region resets its blooms and bridges unless its waystone has been restored. [Waystone trials](MOONTRAIL.md) make that light permanent.
- The flare stuns visible moths within five units for 1.2 seconds. The grotto and sky garden place one directly above the second bloom so this interaction can be used in the level.
- Repeated bloom activation is limited to once per attack sequence and at least one second between pulses.
- Pale platform lips distinguish landable terrain. Warm puma accents, amber enemy warnings, cyan scent rings, glowing seeds, and claw arcs guide attention.
- A blue halo around the puma indicates a dodge or post-damage/spawn grace interval. Reduced motion removes camera shake and reduces impact particles.

These are lightweight sprite effects, not a dynamic lighting/shadow system. There is no light meter, exposure-based stealth, day/night cycle, or hidden terrain outside the dormant bridges. Actual darkness, contrast, telegraph legibility, and animation feel need Unity playtesting.

## Recovery and saves

The puma has five hearts. Activating a different checkpoint heals fully; reaching zero hearts or falling returns her to the remembered checkpoint with full health and one second of grace. Attacks, projectiles, and instinct clear. Surviving enemies return home; already-defeated wildlife and activated bridges stay changed during the visit. Region travel recreates wildlife and moonblooms; a restored waystone immediately awakens its region’s moonbridges.

Collectibles, discovered regions, checkpoints, and journey completion use the existing local save. Combat state and individual moonwake activations are temporary; restored waystones are saved. Hunt/defeat counts belong to the current session only. No mandatory enemy-clear gate blocks travel to the next world.

## Verification boundary

The shared core has executable regression cases covering combat timing, armor, projectiles, and the new AABB precision suite (see [COMBAT_PRECISION.md](COMBAT_PRECISION.md)). Unity rendering, Runtime/Editor API compilation, hardware input, audio, and WebGL execution remain unverified in the development container. Tune these starting values after watching actual play.
