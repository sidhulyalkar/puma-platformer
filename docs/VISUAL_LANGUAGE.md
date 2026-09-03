# Visual Language: the night is readable

## Core Style

Procedural cut-paper diorama. Distinctive, performant, and fitting for a nocturnal nature game.

**Non-negotiables**
- Strong silhouettes first (especially the articulated puma).
- Pale, slightly luminous platform lips so landings are always legible.
- Warm fur + amber enemy warnings.
- Cool blue moonwake / bridges.
- Cyan scent rings.
- Deep but never pure-black backgrounds with subtle parallax.

## Palette Discipline

| Role | Color Language |
| --- | --- |
| Puma | Warm fur accents, readable against all three night palettes |
| Safe landings | Pale lips / edges |
| Committed attacks | Amber warnings |
| Moon systems | Blue / cyan |
| Scent | Soft cyan rings, local only |
| Discovery / golden paths | Warm gold / starflower bloom |
| Danger / contact | Clear red or high-contrast flash (keep short) |

## Animation Priorities

1. Charge coil compression and release — weighty and anticipatory.
2. Gait, tail counterbalance, head tracking.
3. Distinct roll and stalk poses.
4. Claw arc and impact feedback that does not obscure the next input window.
5. Landing squash and particle that still leaves the next jump readable.

## Feedback Hierarchy

1. **Critical** (must be readable at speed): enemy telegraph, platform edge, pounce charge state, dodge grace.
2. **Important**: moonbloom flare, scent ring, wind ribbon, objective direction.
3. **Atmospheric**: parallax, distant lanterns, ambient particles, memory vignettes.

Never let atmospheric effects compete with critical information.

## Interaction Feedback Standards

- Moonbloom: flare + particles + sound + bridge outline → solid.
- Wind perch: visible ribbon + fill gauge readable under pressure.
- Discovery: starflower bloom + golden path materialization (rewarding, not flashy).
- Distant destinations: soft glow or atmospheric perspective so players can plan before committing.

## Reduced Motion

Already supported. Protect it. Prefer still scent cues and limited particles over removing information.

## Future Art Passes

- Stronger puma silhouette and secondary motion.
- Region-specific parallax and micro-animations (leaves, mist, lantern sway).
- Memory sequences as short environmental reconstructions rather than full cutscenes.
- Reactive environment particles tied to moonwake and discovery.
- Background music that respects the quiet nocturnal tone (reactive to region and combat intensity).

## Implementation Note

`WorldView` and the combat/exploration/trial view partials already own the visual layer. New effects should stay in the pooled particle / sprite systems and never drive simulation state.
