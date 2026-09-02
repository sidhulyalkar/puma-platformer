# Validation and release boundary

## Verified locally on 2026-09-02

- The production core and the shared test cases compile with the .NET 8 C# compiler with warnings treated as errors.
- **98/98 simulation cases pass.** The original 30 cover movement, collision, recovery, saves, and progression. The 43 combat cases cover attack timing, buffering and recovery cancels, once-per-target hits, armor, directed rakes, dash limits, low-ceiling rolls, dodge windows, damage grace, stalking, prey rewards, enemy warnings and committed targets, projectile cover and nearest-hit ordering, moonblooms, and pause/respawn cleanup.
- Another 25 cases cover trial entry/return, mechanism eligibility, keyboard countersteering, directional attack chaining, reward persistence, old saves, and authored room traversal. All three trial routes complete through simulated input with zero deaths and no position/activation overrides. The grotto roll passage is separately exercised.
- The input-driven route test reaches all three exits without editing player position or bypassing collision, within 3,600 ticks per region and at most one checkpoint recovery. This is a reachability check, not a human difficulty estimate.
- Deterministic replay and 36,000 seeded movement stress steps across the regions stay finite and within world boundaries. Another 45,000 combat stress steps and 18,000 trial stress steps check for nonfinite state and embedding in solid terrain (99,000 seeded stress steps total).
- The moonbloom interruption test exercises both an isolated fixture and the authored grotto/sky encounters, confirming a real claw strike can dazzle their nearby moths.
- The structural validator passes with 39 unique committed asset GUIDs, the entry scene/build settings, input mode, assembly JSON, and WebGL loader placeholders. The loader passes JavaScript syntax checking. All 20 C# files under Assets parse without syntax errors; this does not check Unity API references or replace an editor compilation.

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
- [ ] Practice all three claws on the scratch post. Check windup visibility, queued follow-ups, rising/falling rakes, dash-claws, and recovery cancels with both input devices.
- [ ] Compare drawn claw arcs with the deliberately forgiving strike boxes. Confirm damage and contact feedback feel fair at their edges.
- [ ] Roll under a ceiling, stay crouched until clear, dodge a projectile, and confirm a mistimed roll can still take damage. Verify the blue protection halo matches the dodge/grace window.
- [ ] Stalk and chase hares, take damage, hunt to restore a heart, and spend three instinct on an empowered dash.
- [ ] Read each enemy warning without relying only on color: thornling curl, boar charge direction, spitter three-shot rhythm, and moth dive line. Test armor from front, behind, and above.
- [ ] Strike both moonblooms; follow the revealed bridges. Dazzle the moth above the second bloom in the grotto and sky garden. Verify the bridge remains solid after its flare ends and after a fall.
- [ ] Enter and leave each trial with E / Y and through the map. Confirm the world art, camera limits, objective card, and journal rebuild correctly while staying in the same region.
- [ ] Complete all three waystones. Check balance feedback, moonbell arc/hit alignment, root-gate disappearance, and the grotto’s low roll passage. Try the alternative upper route.
- [ ] Fall after lighting a mechanism; confirm it stays lit. Leave/re-enter to reset an attempt. Reload after a completed trial and confirm the main-world bridges remain active.
- [ ] Verify the objective panel does not hide critical landings; the balance gauge and pounce charge bar must not overlap. Check both pages of the field guide and the journal at the target resolution.
- [ ] Test input at 30/60/120 FPS, rapid press/release, pause while charging, and focus loss while moving.
- [ ] Verify leg/tail animation, material tinting, terrain collision alignment, camera look-ahead, text contrast, and UI at 16:9 and resized windows.
- [ ] Complete the main trail and deliberately collect each hidden memory. Record any inaccessible or unclear approach.
- [ ] Fall onto hazards, exhaust vitality, and verify checkpoint recovery clears attacks/projectiles while retaining discoveries. Check that dead enemies stay down until leaving the region.
- [ ] Reload after collecting, changing regions, and choosing a checkpoint. Revisit earlier worlds through the map.
- [ ] Confirm audio stays muted when requested, reduced motion disables shake, and New Journey requires the second click.
- [ ] Inspect night contrast on the actual display: the puma, landable edges, brambles, enemies, dormant bridges, and small projectiles must remain distinguishable. Stylized glow is not a real-time shadow system.
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
