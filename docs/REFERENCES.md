# References and provenance

Inspected on 2026-09-02. Repository availability and package versions should be rechecked before future upgrades.

| Resource | Observed contribution | Use in Wildbound |
| --- | --- | --- |
| [btuhany / PixelAdventure-Unity2D](https://github.com/btuhany/PixelAdventure-Unity2D/tree/90e6e25f4f9f76b1b7b0a421fe16a6c05b677d59) | Two levels plus tutorial; fruit/lever gates; checkpoints, traps, and enemy types. Movement and wall-check source inspected. Repository code is MIT. | Reference for readable environmental lessons, checkpoints, and intentional exits. Wildbound makes collectibles optional to keep exploration open. No files copied. |
| [striderzz / 2D-Platformer-Unity](https://github.com/striderzz/2D-Platformer-Unity/tree/3b32db5cc8c44914ca218336e7f181fea0cdfb99) | Tilemap platformer structure; player animation, double jumping, pickups, respawn, mobile hooks. PlayerController source inspected. Repository code is MIT. | Reference for a self-contained Unity starter and feedback. Wildbound replaces the instant horizontal velocity model with acceleration, charged launch momentum, and wall kicks. No files copied. |
| [Matthew-J-Spencer / Ultimate-2D-Controller](https://github.com/Matthew-J-Spencer/Ultimate-2D-Controller) | README and public controller guidance emphasize coyote time and buffered actions. MIT repository. | Reference for forgiving movement principles. No source incorporated. |
| [Kenney Pixel Platformer](https://kenney.nl/assets/pixel-platformer) | Public page lists 200 assets, 18 × 18 tiles, CC0. | Candidate for a later tilemap experiment. Not downloaded or bundled. |
| [Unity Input System 1.14.2](https://docs.unity3d.com/Packages/com.unity.inputsystem@1.14/manual/Installation.html) | Installation, active input handling, and compatibility guidance. | Pinned package; new input backend enabled. |
| [Unity Web deployment guide](https://docs.unity3d.com/6000.3/Documentation/Manual/webgl-deploying.html) | Compression, decompression fallback, and server response requirements. | Initial gzip/fallback WebGL build configuration and hosting checklist. |

The MIT license in an example code repository should not be treated as evidence that every bundled third-party art pack has identical terms. Wildbound currently bundles none of those repositories' art, scenes, scripts, or audio. Its puma, scenery shapes, particles, and synthesized tones are original procedural implementations. Package and engine licenses remain with their owners.

If third-party assets are introduced, record the exact author, download URL, version/date, license text, attribution requirement, and asset paths here before committing them.
