# World Expansion Plan

Expand only after the current three regions (Canopy, Grotto, Sky) feel excellent in human playtest. New biomes should teach one new verb or deepen an existing natural system—not dump content.

## Current biomes (v0.5)

| Biome | Traversal identity | Signature systems |
| --- | --- | --- |
| Amber Canopy | Root crawl + branching canopy | Scent trails, standard/updraft blooms |
| Lantern Grotto | Tall switchbacks | Vine bloom ladder, **Bark claw-climb**, moth dazzle |
| Sky Garden | Island chain + springs | Wind ribbons, wide-dazzle blooms |

## Candidate biomes

### 1. Cinder Ravine (priority A)

**Fantasy:** Warm volcanic cleft under a red moon. Embers drift; cooled basalt is climbable; live vents are hazards.

| Element | Design |
| --- | --- |
| Traversal | Long vertical Bark faces + short ember-jumps |
| New verb | **Heat shimmer** — brief upward gust near vents (reuse updraft sample) |
| Enemies | Ash hopper (hare variant), cinder spitters, armored basalt beetle (bristleback timing) |
| Blooms | Emberbloom — short timed bridge that *decays* unless re-clawed |
| Wild places | Charcoil Den, Ridge of Quiet Fire |
| Risk | Do not require climb stamina longer than `ClimbBudgetSeconds` |

### 2. Frostglass Ridge (priority A)

**Fantasy:** Thin ice sheets, brittle platforms, moonlit blue.

| Element | Design |
| --- | --- |
| Traversal | Low-friction ice (reduced brake), fragile ice plates |
| New verb | **Frost slide** — commit to a long slide; jump cancel |
| Enemies | Glass moth (dives on reflection), ice spitters |
| Blooms | Frostbloom — freezes a hazard corridor for GlowTime |
| Wild places | Mirror Shelf, Crown of Quiet |
| Risk | Keep rectangular solver; simulate ice as brake/accel multipliers, not slopes |

### 3. Tidal Hollow (priority B)

**Fantasy:** Flooded mangrove basin. Water line rises/falls on a readable timer.

| Element | Design |
| --- | --- |
| Traversal | Swim-lite (slow horizontal + soft buoyancy) in water AABBs |
| New verb | **Tide window** — platforms emerge/submerge on cycle |
| Enemies | Reed crabs, surface skimmers |
| Blooms | Tidebloom — locks water height for GlowTime |
| Wild places | Pearl Root, Low-Tide Crossing |

### 4. Whispering Mire (priority B)

**Fantasy:** Fog, soft ground, sound-based hunting.

| Element | Design |
| --- | --- |
| Traversal | Soft mud (speed penalty), firm root roads |
| New verb | **Echo stalk** — stalking reveals prey noise rings farther |
| Enemies | Silent hares, fog moths |
| Blooms | Fogbloom — clears local fog / reveals tracks |

### 5. Starfall Archive (endgame optional)

**Fantasy:** Ruined sky-library linking all waystones. Puzzle-light, low combat.

| Element | Design |
| --- | --- |
| Traversal | All prior verbs as a mastery exam |
| Reward | Epilogue vignette + gallery of discoveries |

## Expansion rules

1. One new biome at a time; ship with regression cases + a human feel pass.
2. Main exit remains open without the new verb (optional depth).
3. Reuse wind / bloom / climb / scent infrastructure—prefer new *content* over new *physics*.
4. Biome count in `WorldDefinition.Create` stays a single int switch until a registry is needed.
5. Story: each biome adds one memory vignette beat (see STORY.md).

## Suggested order

1. Finish Unity playtest of Canopy → Grotto → Sky
2. Cinder Ravine (climb + heat)
3. Frostglass Ridge (friction identity)
4. Tidal Hollow or Mire based on which fantasy plays better
5. Starfall Archive as epilogue
