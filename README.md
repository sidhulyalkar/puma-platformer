# Puma: Wildbound

**Quiet paws. A wild heart after dark.**

A Unity 2D exploration platformer about a female puma hunting beneath the moon. Stalk a moss hare, turn a pounce into a claw strike, read a bristleback's charge, and wake a hidden bridge with a moonbloom. Follow the trail from a nocturnal forest through a lantern grotto to a floating sky garden. The main trail stays open; the scenic route asks more of your paws.

This repository contains **v0.2: Night Hunt**, written for **Unity 6000.3.22f1**. The gameplay simulation has executable regression coverage. **Unity import, rendering, editor playtesting, and WebGL execution still need verification.** No browser release is published yet.

## Play in Unity

1. Install [Unity 6000.3.22f1](https://unity.com/releases/editor/whats-new/6000.3.22f1) through Unity Hub. On an M1 Mac, use the Apple silicon editor. Add **Web Build Support** if you want browser builds.
2. Clone this repository and add its root folder as a project in Unity Hub.
3. Let the pinned packages import. Choose **Wildbound → Play Wildbound** (or open `Assets/Scenes/Wildbound.unity` and press Play).
4. Start at **1280 × 720** in the Game view. Press Enter to begin; press C for the field guide.

The opening scene builds its own world, camera, puma, interface, and audio. No asset downloads, inspector wiring, API keys, or external services are required to play after Unity imports the packages.

## The first journey

| Region | Traversal character | Discovery |
| --- | --- | --- |
| Amber Canopy | Broad landings, low steps, first pounce gaps | A memory above the forest floor |
| Lantern Grotto | Additional wall-kick shafts and high ledges | A lantern perch above the main trail |
| Sky Garden | More flowers and floating platforms | A memory at the highest garden |

- Accelerated movement, variable jump height, coyote time, and buffered jumps.
- **Charged pounce:** hold to coil; release for an arcing leap. Charge changes range; up/down changes the angle. One aerial pounce until you land or touch a spring flower.
- Wall slides and wall kicks, with a brief steering lock so the kick actually clears the wall.
- A three-strike claw chain, rising rake, confirmed-hit falling rebound, pounce rake, dash-claw, and ground roll with a short dodge window. A scratch post makes practicing safe.
- **Hunting:** stalk to approach hares more closely; prey restores one heart and traversal resources. Three enemy defeats charge the next dash into **Moonfang Rush**. Damage clears that charge.
- **Distinct encounters:** leaping thornlings, armored charging bristlebacks, three-shot reed spitters, and moths that telegraph a locked dive path.
- **Moonwake:** claw blue moonblooms to reveal lasting bridges and dazzle nearby moths. Light marks terrain edges, warnings, scents, and attack arcs against dark scenery.
- Moving platforms, spring flowers, five-heart vitality, and quick checkpoint recovery. Discovered checkpoints fully heal; falling preserves collectibles.
- Twelve light motes and one hidden memory per region. **Discoveries are optional and survive falls.**
- Local saves, a trail map with return travel to discovered worlds, a clean title screen, contextual lessons, gamepad input, mute, and reduced-motion options.
- Original procedural cut-paper scenery, an articulated quadruped puma, pooled particles, and synthesized sound cues.

## Controls

| Action | Keyboard | Gamepad |
| --- | --- | --- |
| Move / aim pounce | A/D or arrows; W/S to aim | Left stick |
| Jump / wall kick | Space | South / A |
| Charge and release pounce | Left Shift | West / X |
| Claw / three-hit chain | J or left click | Right bumper |
| Rising / falling rake | W+J / airborne S+J | Stick up/down + right bumper |
| Pounce rake | J during a pounce | Right bumper during a pounce |
| Dash-claw / Moonfang Rush | K | Right trigger |
| Ground roll | L | East / B |
| Stalk / reveal scents | Hold Q | Hold left trigger |
| Enter arch | E | North / Y |
| Pause | Escape | Start |
| Field guide / map | C / Tab | Keyboard or mouse menu |
| Return to checkpoint / mute | R / M | Keyboard or mouse menu |

Controller labels use the Xbox layout. Touch controls are not implemented. Progress is stored on the current device/browser profile, not synced online.

## Verify

The headless runner compiles the **same C# core used by Unity**, with no test-package dependencies:

```bash
dotnet run --project tests/Wildbound.Tests.csproj --configuration Release
python3 tools/validate_project.py
```

**73 regression cases** cover movement, combat timing, enemy patterns, armor, projectile collision ordering, low-ceiling rolls, hunting rewards, moonblooms, saves, and actual input-driven routes to all three exits. They include 81,000 seeded movement/combat stress steps. The GitHub Actions workflow runs both commands. These are simulation and structural checks, not a substitute for Unity runtime qualification.

Run the same cases in **Window → General → Test Runner → EditMode**. On macOS, the command-line equivalents are:

```bash
export UNITY_EDITOR="/Applications/Unity/Hub/Editor/6000.3.22f1/Unity.app/Contents/MacOS/Unity"
bash tools/test-unity.sh
bash tools/test-unity.sh PlayMode
bash tools/build-webgl.sh
```

See [validation evidence and the playtest checklist](docs/VALIDATION.md) before treating the slice as release-ready.

## Browser arcade path

**Wildbound → Build WebGL** exports `Builds/WebGL` with a responsive loader, explicit start button, loading/error states, capped pixel density, and fullscreen. The build uses gzip with decompression fallback for a simple initial hosting path.

Serve the result over HTTP:

```bash
python3 -m http.server 8000 --directory Builds/WebGL
```

Open `http://localhost:8000`. The eventual `sidhulyalkar.com` arcade can embed this standalone build after browser qualification. See the [WebGL integration plan](docs/WEBGL.md).

## Development notes

- [Design direction and next milestones](docs/DESIGN.md)
- [Night Hunt moves, enemies, and light rules](docs/NIGHT_HUNT.md)
- [Architecture and editing the game](docs/ARCHITECTURE.md)
- [References and asset provenance](docs/REFERENCES.md)

Inspired by the readable puzzle progression of [PixelAdventure-Unity2D](https://github.com/btuhany/PixelAdventure-Unity2D) and the approachable Unity platformer structure of [2D-Platformer-Unity](https://github.com/striderzz/2D-Platformer-Unity). This is an original implementation; neither repository's code, scenes, nor bundled artwork is vendored here.

Original repository code and procedural art: [MIT](LICENSE). Unity and its packages retain their own licenses.
