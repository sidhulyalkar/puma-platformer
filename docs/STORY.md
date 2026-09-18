# Story: Quiet paws. A wild heart after dark.

## Core Fantasy

You are a young female puma whose territory has been fractured by a fading Moontrail.  
The night once connected forest, grotto, and sky gardens through living light.  
Now the bridges sleep, the prey is restless, and memories of the old path linger in hidden places.

Restore the waystones. Follow the scent of what was lost. Decide whether the restored trail belongs only to you or to the whole wild.

**Tone**: quiet, curious, powerful, slightly melancholic, ultimately hopeful.  
Avoid pure power fantasy or pure survival horror. Emphasize the joy of competent movement through a living night.

## Narrative Structure

### Act 1 — Amber Canopy (Awakening)
- First hunts and the discovery that the Moontrail is broken.
- Introduce basic movement, stalking, and one simple moonbloom.
- Memory: a fragmented recollection of running the trail with a sibling or mother under a brighter moon.

### Act 2 — Lantern Grotto (Descent)
- Deeper darkness, verticality, moths as living light thieves.
- First real multi-step trial that combines bloom, balance, and rebound.
- Memory: the older guardian who once maintained the waystones, and the cost of their absence.

### Act 3 — Sky Garden (Ascent)
- Open air, wind, springs, higher stakes.
- Final waystone and a quiet revelation about the origin of the trail.
- Memory: a choice or understanding — the trail is not owned; it is tended.

### Epilogue / Return Visits
- After all three waystones are restored, the night cycle softens.
- Prey behavior shifts slightly; golden paths remain; new scent trails may appear.
- Optional “true night” layer or New Game+ hooks can be added later without rewriting the core save.

## Memory System

Each region’s memory (pickup bit 0) is a short, mostly non-verbal vignette.

**Design rules**
- Triggered at the wild place or near a restored waystone.
- Prefer visual reconstruction, scent trails that briefly reconstruct a past route, and subtle audio over long text.
- Memories are optional. Collecting them deepens emotional investment and can unlock small ambient changes, never required progression.
- Persist across falls and region travel (already supported by the save schema).

**Content seeds**

| Region | Memory Title | Core Image | Emotional Beat |
| --- | --- | --- | --- |
| Amber Canopy | First Pawprints | Two sets of prints side-by-side under amber leaves | Belonging / loss |
| Lantern Grotto | The Keeper’s Lantern | A larger silhouette lighting blooms that no longer respond | Responsibility |
| Sky Garden | Starflower Crown | The puma alone on the highest island as the moon rises | Acceptance / agency |

Expand these into short environmental sequences (10–25 seconds) once Unity feel is solid.

## Lore Hooks (Non-Gating)

- Pawprints that subtly change after a waystone is restored.
- Prey that becomes slightly braver or more wary.
- Seasonal or moon-phase visual shifts after full restoration.
- Shelter stars that glow brighter with each memory collected.

## Voice & Text Guidelines

- Field notes and wild-place stories stay short, observational, and feline in perspective.
- Avoid exposition dumps. Prefer “the scent is older here” over “an ancient civilization built this.”
- Journal and objective text remains functional and clear.
- Title and completion screens can carry a single strong line that echoes the tagline.

## Implementation Notes

- `WildPlace.Story` and `WildPlace.Hint` already exist and are the correct extension points.
- Memory pickups already fire `GameEvent.Secret`.
- Future: a lightweight `MemorySequence` or ambient state flag driven by `Save.Collected` and `Save.Waystones` without expanding the save version until necessary.
