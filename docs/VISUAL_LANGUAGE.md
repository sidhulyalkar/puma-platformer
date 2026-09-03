# Visual Language: the night is readable

## Core Style

**Procedural cut-paper diorama.** Distinctive, performant, nocturnal. Silhouette-first shapes in `WorldView` — not a drop-in pixel tileset.

**Non-negotiables**
- Strong silhouettes first (articulated puma).
- Pale luminous platform lips for landings.
- Warm fur + amber enemy warnings.
- Cool blue moonwake / bridges.
- Cyan scent rings.
- Deep but never pure-black backgrounds with subtle parallax.

## Palette Discipline

| Role | Color Language |
| --- | --- |
| Puma | Warm fur accents |
| Safe landings | Pale lips / edges |
| Committed attacks | Amber warnings |
| Moon systems | Blue / cyan |
| Scent | Soft cyan rings |
| Discovery | Warm gold |
| Danger | Short red / high-contrast flash |
| Cinder heat | Ember orange on vents (never hide lips) |
| Bark climb | Subtle grain / claw-scratch on climbable faces |

## Region scene recipes

| Biome | Atmosphere | Micro-motion | Landmark |
| --- | --- | --- | --- |
| **Amber Canopy** | Layered leaf cut-outs, warm dust | Slow leaf drift | Moonbloom through canopy gaps |
| **Lantern Grotto** | Stacked shelves, soft god-rays | Lantern sway, moth dust | Vine rim while active |
| **Sky Garden** | Island silhouettes, thin clouds | Wind ribbon streaks | Spring pulse, dazzle burst |
| **Cinder Ravine** | Basalt slabs, ember haze | Vent shimmer columns | Emberbloom orange while burning |

## Public graphics references (inspiration, not imports)

Art stays **original procedural** to match the simulation-first core. These **CC0 / public** sources are approved **mood and silhouette** references:

| Source | Licence | Use for |
| --- | --- | --- |
| [Kenney Game Assets](https://kenney.nl/assets) | CC0 | Silhouette clarity, UI spareness |
| [Deep Night (VEXED)](https://v3x3d.itch.io/deep-night) | CC0 | Nocturnal value structure |
| [SunnyLand Forest (ansimuz)](https://ansimuz.itch.io/sunnyland-forest) | CC0 | Parallax layering lessons |
| [OGA nature tiles](https://opengameart.org/content/2d-nature-platformer-tileset-16x16) | CC0 | Prop density studies |
| [RavenTale free sprites](https://www.raventalestudio.com/free_tileset) | Public domain | Soft diorama props |
| [jam-ready-assets](https://github.com/series-ai/jam-ready-assets) | Mostly CC0 | Curated pack index |

**Do not** paste pixel tiles over the cut-paper solver without a deliberate direction change. Prefer silhouette study, palette sampling into `Hex(...)`, and optional CC0 particle sprites in pooled VFX only.

## Feedback Hierarchy

1. **Critical:** enemy Tell + amber, platform edge, pounce charge, dodge grace, bark climbability.
2. **Important:** bloom flare, scent, wind/heat ribbon, second threat entering Tell.
3. **Atmospheric:** parallax, lanterns, ember haze, vignettes.

## Encounter VFX

- StaggerTell: sequential amber pips
- TwinDive: mirrored wing-flash offsets
- Pincer: frontliner dust; support thin aim line
- Only one full-screen flash at a time

## Implementation

Visuals never drive simulation. Encounter timing lives in `EncounterDirector` (headless).
