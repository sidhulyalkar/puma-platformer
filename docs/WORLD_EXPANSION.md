# World Expansion Plan

## Current biomes

| # | Biome | Status | Signature systems |
| --- | --- | --- | --- |
| 0 | Amber Canopy | Shipped | Scent, standard/updraft blooms |
| 1 | Lantern Grotto | Shipped | Vine ladder, bark climb, moth dazzle |
| 2 | Sky Garden | Shipped | Wind ribbons, wide-dazzle |
| 3 | **Cinder Ravine** | **Scaffolded** | Heat vents, ember timed bridges, bark climbs |

## Cinder Ravine (biome 3)

**Fantasy:** Warm volcanic cleft under a red moon. Embers drift; cooled basalt is climbable; live vents are hazards and lifts.

| Element | Implementation |
| --- | --- |
| Heat shimmer | `WindField` with strong +Y (reuse sampler) |
| Bark climb | `Surface.Bark` faces (claw-climb budget) |
| Emberbloom | `BloomKind.Ember` — linked moonbridges solid **only while GlowTime > 0**; re-claw when `GlowTime ≤ 5` |
| Wild places | Charcoil Den, Quiet Fire Ridge |
| Floor hazards | Ember pits (hazard AABBs) |
| Enemies | Reused thornling / bristleback / spitter / moth |

Portal chain: Canopy → Grotto → Sky → **Cinder** → journey complete.

JourneySave holds 4 biomes (Collected / Checkpoints length 4; Waystones mask 4 bits; Discoveries 8 place bits).

## Still planned

### Frostglass Ridge (priority A)
Low-friction ice (brake/accel multipliers), fragile plates, frostbloom hazard freeze.

### Tidal Hollow (priority B)
Water AABBs with tide cycle; tidebloom locks water height.

### Whispering Mire (priority B)
Mud speed penalty; echo-stalk extends scent range.

### Starfall Archive (endgame)
Mastery exam of all verbs; epilogue gallery.

## Expansion rules

1. One new biome at a time; ship with regression cases + human feel pass.
2. Main exit open without the new verb.
3. Reuse wind / bloom / climb / scent — prefer new content over new physics.
4. Tune Cinder in Unity before authoring Frostglass.
