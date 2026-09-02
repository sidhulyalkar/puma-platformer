# Design: make curiosity feel good in the hands

The fantasy is a small, powerful animal in a world worth exploring. Movement should look and feel feline: a low coil, a long arcing leap, claws catching a wall, a tail balancing in flight, and a soft landing. There is no timer, life counter, or required currency grind. The reward for competence is another route and another view.

## What is implemented

The first slice has three regions built from a shared, tested route spine, with different palettes, scenery, and additional upper platforms. This establishes the mechanics and progression; it does **not** yet provide three fully bespoke levels. Authored terrain is rectangular and solid. Slopes, one-way platforms, ledge grabs, and arbitrary Unity physics interactions are outside this version.

| Mechanic | Default | Design reason |
| --- | --- | --- |
| Simulation step | 120 Hz | Fine collision increments and consistent simulation inputs |
| Run speed | 8.5 units/s | Enough ground speed to make short jumps useful |
| Coyote time / jump buffer | 110 ms / 130 ms | Forgive slightly early or late inputs |
| Jump launch / gravity | 14.5 units/s / 34 units/s² | A readable, compact arc; releasing reduces height |
| Full coil | 650 ms | A visible anticipation pose with a meaningful timing tradeoff |
| Pounce launch | 13–24 units/s | Holding changes travel, not just an animation |
| Pounce aim | Shallow forward, steep upward, downward in air | Create choices without mouse aiming |
| Wall slide / kick lock | 3 units/s / 160 ms | Let players assess a wall and clear it when kicking |
| Spring flower | 20 units/s upward + pounce refresh | Connect terrain and movement into chains |
| Recovery | 300 ms freeze at checkpoint | Reset the attempt quickly while keeping discoveries |

These are starting values, **not playtest-proven tuning**. The independent core makes it cheap to preserve regression coverage while changing them.

## Teaching order

1. Safe ground introduces movement and short/held jumps.
2. The first gap introduces coiling, release, and aiming.
3. A checkpoint establishes that exploration is forgiving; an upper trail tempts a detour.
4. A spring flower demonstrates that the environment can replenish a pounce.
5. An alcove introduces sliding and wall kicks. Thornlings can be avoided, bounced on, or pounced through.
6. A clear arch introduces intentional travel to another world. The trail map allows revisiting discovered regions.

The contextual text is proximity-based in v0.1. It does not yet verify that someone performed a lesson. A short input-aware tutorial should replace repeated hints after watching first-time players.

## Critique of this slice

- The shared route spine is useful for regression coverage but too repetitive for a showcase release. Build bespoke spaces once the movement tuning is stable.
- Charged pouncing is the strongest differentiator. Validate whether 650 ms feels satisfying or interrupts flow; compare shorter coils with longer anticipatory poses at high charge.
- The puma and cut-paper environment are original and lightweight, but the presentation has not been seen in a running Unity editor in this development environment. Camera framing, silhouette readability, landing poses, and color contrast are unqualified.
- The three exit routes are exercised by input simulation. Hidden-memory routes and casual-player discoverability still need dedicated route verification and human playtests.
- A custom rectangular solver is deliberately small and reproducible. It must not be mixed with Rigidbody2D movement without defining ownership of collision and movement.
- No music, authored sprite animation, mobile controls, control remapping, or accessibility narration is claimed.

## Next milestones, in order

1. **Unity qualification:** clean import, EditMode tests, play through the opening tutorial, inspect the puma silhouette, collect each memory, and build WebGL. Resolve actual console errors and visual issues before adding scope.
2. **Movement laboratory:** collect short recordings of tap/hold jumps, minimum/maximum coils, wall kicks, and flower chains. Tune camera lead and deceleration. Add regression cases only for observed or high-risk failures.
3. **One showcase region:** replace the canopy's shared spine with a handcrafted loop: safe lower trail, satisfying fast route, and a surprising upper route that reconnects to the checkpoint. Add landmarks visible before they become reachable.
4. **Exploration identity:** prototype scent trails that briefly reveal nearby hidden life, then test whether they help players notice routes without telling them exactly where to go. Consider perch interactions and wildlife responses before adding conventional combat.
5. **Distinct worlds:** give the grotto light-reactive plants and the sky garden wind-assisted traversal only after the base controls are stable. Each new behavior needs a safe introduction and a route that uses it creatively.
6. **Arcade release:** browser qualification, gamepad/menu pass, download/CPU/memory measurements, a short trailer captured from the actual game, and integration into the personal site.

## Playtest acceptance targets

These are prospective goals, not measured outcomes: a new player starts moving within 15 seconds, performs a deliberate pounce within 60 seconds, identifies the checkpoint without an explanation, and finds at least one optional route in a five-minute session. Record where they hesitate; don't coach them past confusing instructions.
