# Validation and release boundary

## Living Trails qualification — 2026-09-02

[Gameplay core run 33666605865](https://github.com/sidhulyalkar/puma-platformer/actions/runs/33666605865) passed at gameplay commit `833a964f0dab1810b37fb57548b1bc6673977188`. Subsequent release metadata and documentation changes use the same gameplay sources; check the PR's final commit status before merging.

- **112/112 simulation cases pass in GitHub Actions**, compiling the production C# core with warnings treated as errors. The existing 98 movement, combat, save, and Moontrail cases remain in the suite.
- **Fourteen exploration cases** cover additive save compatibility, stable discovery IDs, linked paths, repeated arrivals, airborne/paused/trial exclusions, local scent sight lines, separate waystone rewards, and authored site safety.
- All three exploration routes start at the real spawn and use simulated input with wildlife active. They find **all six wild places and all three memories**, then return to a checkpoint **without a death**. The Canopy route traverses the low root hollow; the Grotto route changes direction between shelves; the Sky route lands on a spring and a moving perch.
- The main-route test reaches all three exits without editing position or bypassing collision, within 3,600 ticks per region and at most one checkpoint recovery. All three existing trial routes also complete through input with zero deaths. The grotto trial's roll alternative remains covered.
- Deterministic replay and **99,000 seeded stress steps** exercise the revised outside regions and existing trials. The movement steps check finite state and world bounds; the combat/trial steps also reject embedding in solid terrain.
- Structural validation passes with **43 unique asset GUIDs**. CI parses **all 24 Assets C# files with zero syntax errors** using C# 9 and the pinned .NET 8 SDK. This is not Unity API or shader compilation.
- The coordinate schematic is generated from `WorldRegions.cs` and visually inspected for layout. It is design evidence, not a rendering of the game.

This container has no .NET SDK or activated Unity editor. Executable C# verification ran in GitHub Actions with the standard SDK commands. Local work includes structural validation, source review, whitespace checks, and schematic generation. The route policies use analog steering and retries; passing them does not establish first-time keyboard difficulty, play duration, or enjoyment. They deliberately cover the memories and named places, not every possible approach or every mote.

## Not verified in this environment

**No Unity editor is installed or activated here.** Unity imports, package resolution, Runtime/Editor assembly compilation, shader rendering, actual input hardware, audio output, and WebGL builds were not executed locally. The game has not yet received a visual or human playtest. There are no fabricated screenshots or browser-performance claims.

The Unity wrapper scripts and EditMode adapter are provided for an activated editor. CI validates the headless core, C# syntax, and repository structure; a green `Gameplay core` check must not be described as a green Unity build.

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

## Living Trails playtest

- [ ] Notice the first pawprints without coaching. Hold Q / LT and verify scent stays local and does not show through terrain.
- [ ] Roll into Root Hollow and crawl clear at both ends. Climb onto the roof and follow the branches to Amber Overlook.
- [ ] Discover each wild place and watch its golden path appear. Confirm the new crossing is useful and visible from the destination or clearly described in its field note.
- [ ] Climb the Grotto switchback. Check that camera framing makes each reversal and the eastern descent readable.
- [ ] Jump onto the Sky spring, reach Cloud Nest, cross the moving perch, and return past the lower spring without feeling trapped.
- [ ] Find all three memories and inspect the corresponding shelter stars. Collect every mote and record awkward landing requirements.
- [ ] Cycle Tab's map/objectives/wild-place pages. Check tall-room scaling, marker visibility, hint wrapping, and keyboard/gamepad use.
- [ ] Fall, enter/leave a trial, change regions, and reload after finding a place. Confirm golden paths remain and blue waystone rewards stay separate.
- [ ] Repeat with reduced motion. Physical tracks, spring petals, light bridges, and enemy tells must stay distinguishable.

## Browser qualification

- [ ] Produce a WebGL build and serve it over HTTP.
- [ ] Start/reload in Safari and Firefox on the M1 Mac, then Chromium.
- [ ] Check loading progress, retry errors, keyboard focus, scroll prevention, fullscreen, audio gesture behavior, tab switching, and controller reconnect.
- [ ] Test local save survival and storage-unavailable behavior.
- [ ] Test the eventual iframe with the website around it; keyboard input must not scroll the host page.
- [ ] Measure compressed transfer size, load time, steady-state frame time, and memory on the actual device. Establish budgets from observations.
- [ ] Confirm no missing shaders/textures, console errors, external asset fetches, or unexpected network calls.

Only after these checks should this become a portfolio arcade release.
