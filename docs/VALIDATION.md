# Validation and release boundary

## Verified locally on 2026-09-02

- The production core and the shared test cases compile with the .NET 8 C# compiler with warnings treated as errors.
- **30/30 simulation cases pass.** Coverage includes jump height, coyote/buffering windows, charge range, air-pounce limits, wall behavior, high-speed collisions, ceilings, springs, moving-platform carry, hazards, thornlings, checkpoints, save sanitization, return travel, pause, and progression.
- The input-driven route test reaches all three exits without editing player position or bypassing collision. With its fixed policy, canopy/grotto complete with no recoveries and the sky garden with one. This is a reachability check, not a human difficulty estimate.
- Deterministic replay and 36,000 seeded stress steps across the regions stay finite and within world boundaries.
- The structural validator checks unique committed asset GUIDs, the entry scene/build settings, input mode, assembly JSON, and WebGL loader placeholders.

The normal SDK CLI cannot start in this container because its process-information query is unsupported. Local verification therefore invoked the installed .NET 8 Roslyn compiler and runtime directly; GitHub CI uses the standard `dotnet run` command. Both compile the same source files.

## Not verified in this environment

**No Unity editor is installed or activated here.** Unity imports, package resolution, Runtime/Editor assembly compilation, shader rendering, actual input hardware, audio output, and WebGL builds were not executed locally. The game has not yet received a visual or human playtest. There are no fabricated screenshots or browser-performance claims.

The Unity wrapper scripts and EditMode adapter are provided for an activated editor. CI currently validates only the headless core and repository structure; a green `Gameplay core` check must not be described as a green Unity build.

## First Unity session

- [ ] Import with Unity 6000.3.22f1 and confirm zero compile errors.
- [ ] Run all EditMode cases.
- [ ] Run the PlayMode scene smoke test; it checks world construction, one puma, renderers, shader presence, and pause. This test is supplied but has not been executed here.
- [ ] Open the entry scene through the Wildbound menu, start with Enter, and confirm a single recognizable quadruped puma.
- [ ] Confirm keyboard and gamepad movement, tap/held jumps, full/minimum pounces, and aim directions.
- [ ] Test input at 30/60/120 FPS, rapid press/release, pause while charging, and focus loss while moving.
- [ ] Verify leg/tail animation, material tinting, terrain collision alignment, camera look-ahead, text contrast, and UI at 16:9 and resized windows.
- [ ] Complete the main trail and deliberately collect each hidden memory. Record any inaccessible or unclear approach.
- [ ] Fall onto hazards, step into thornlings, stomp and pounce through them, and verify fair recovery.
- [ ] Reload after collecting, changing regions, and choosing a checkpoint. Revisit earlier worlds through the map.
- [ ] Confirm audio stays muted when requested, reduced motion disables shake, and New Journey requires the second click.
- [ ] Capture a short actual-game recording for the next movement/visual critique.

## Browser qualification

- [ ] Produce a WebGL build and serve it over HTTP.
- [ ] Start/reload in Safari and Firefox on the M1 Mac, then Chromium.
- [ ] Check loading progress, retry errors, keyboard focus, scroll prevention, fullscreen, audio gesture behavior, tab switching, and controller reconnect.
- [ ] Test local save survival and storage-unavailable behavior.
- [ ] Test the eventual iframe with the website around it; keyboard input must not scroll the host page.
- [ ] Measure compressed transfer size, load time, steady-state frame time, and memory on the actual device. Establish budgets from observations.
- [ ] Confirm no missing shaders/textures, console errors, external asset fetches, or unexpected network calls.

Only after these checks should this become a portfolio arcade release.
