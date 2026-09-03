# First Paws

The first few minutes should explain what the puma can do while leaving the choice of route to the player. Earlier introductory signs repeated after successful actions, and the outside objective pointed back toward an optional trial. First Paws addresses those two problems within the existing regions.

## What the player sees

- Nearby introductory hints ask for one action. A real result briefly changes the card to **PRACTICED**, then the known lesson retires. If a sign suggests two actions, either can be practiced first.
- **C → Practice notes** shows nine remembered actions. These notes are records of something tried, not a mastery score or a checklist required for travel.
- A roll that ends beneath a ceiling gets a **KEEP YOUR HEAD LOW** reminder until there is room to stand. Coiling or clawing while crouched does not falsely complete a note.
- The outside objective points toward the arch. The map marks the main arch with a larger violet square and the optional trial entrance with a smaller one. Near the crescent, the card explains E / Y trial entry. Inside a trial, guidance still follows its next mechanism.
- Notes survive falls, region changes, trial visits, and saved-session reconstruction. The short confirmation notice does not replay on loading. New Journey clears notes along with the rest of that journey.

Menus now use a single screen state. Opening the guide from Pause and closing it keeps the game paused; swapping between map and guide preserves that origin. Escape/Start also leaves the ending cleanly. New Journey opens a separate dialog with **Keep my trail** and **Start fresh**. Canceling or switching away from the app clears that confirmation, including before a later menu visit.

## Recognition rules

| Note | What records it |
| --- | --- |
| Jump | A real ground/coyote/buffered jump launches. A rejected air press does not. |
| Claw | An ordinary claw or rake connects with an enemy, scratch post, moonbloom, or moonbell. Empty swings, armor blocks, body rebounds, and dash-claws do not count. |
| Scent | Grounded stalking reveals nearby tracks for an undiscovered wild place with clear sight. Stalking empty ground or a completed trail does not count. |
| Roll | A ground roll starts. |
| Pounce | A charged or short coil is released into a real launch. Holding or canceling a coil does not count. |
| Moonwake | An attack actually wakes a moonbloom. Loading a restored waystone does not count. |
| Dash | A dash-claw starts; input rejected during another attack's windup does not count. |
| Spring | Contact with a spring launches the puma. |
| Wall kick | A wall kick launches away from the wall. It does not also count as an ordinary jump. |

The signs only introduce a subset of these actions locally; the field guide carries the complete reference. Scent clues stop advertising already discovered places, so an older journey that found every place without using scent can retain an untried scent note. No reward or completion depends on filling these notes.

## Focused Unity playtest

Use Unity 6000.3.22f1 at 1280 × 720. Preserve a copy of any existing journey you want to keep before deliberately testing New Journey.

1. Begin a fresh journey. Check that movement/jump instructions appear at the start. Jump once, observe the short acknowledgment, and verify the old jump prompt stops repeating.
2. Face away from the scratch post and claw empty air: the note should stay untried. Face the post and connect a claw: the note should become tried. Also try attacking an armored front and using a dash on the post.
3. Approach the root tracks. Hold Q / LT to reveal scent, roll into the hollow, stop beneath the ceiling, then crawl fully clear before attacking or coiling. Verify the crawl reminder explains why those actions are unavailable.
4. Claw the blue bloom, then hold/release a pounce toward the canopy. Cancel a separate coil by opening the guide; closing it must not launch or award a new pounce.
5. Open **C → Practice notes**. Inspect all nine rows and the footer for wrapping at 1280 × 720 and in a resized window. Cycle to trial strategy and back to controls. Confirm overlays pause play and retain practice progress.
6. Fall, return through the map, enter/leave a trial, and reload the project. Notes should persist, while notices should not replay and outside collectibles/waystones should remain intact.
7. Follow the objective through an arch without completing any trial. Enter a trial separately and confirm its mechanism guidance still advances. Check both map marker sizes are understandable without coaching.
8. From Pause, open New Journey, cancel, resume, and open New Journey again. The second visit must still show a dialog and retain your progress. Repeat with focus loss while the dialog is open. Only select Start fresh when you intend to replace that journey.
9. Open the guide from the title and verify no title button activates behind it. Open it from Pause, switch to the map, then close it: the game must remain paused. Repeat while playing, with a focus change before closing, and from the ending screen. Verify no menu click also starts a claw.

Record hesitation, missed targets, confusing direction changes, and obscured landings. The regression route is not evidence of enjoyment, readable UI, or controller usability. Complete the broader [Unity and browser qualification](VALIDATION.md) before publishing an arcade build.
