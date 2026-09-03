# Multi-Enemy Encounters

## Design goal

Fights should feel like **choreography**, not spawn spam. Two or three threats are enough when their **timing is readable**.

Inspiration (mechanics, not assets): Hollow Knight / Silksong arena rhythm, Ori combat punctuation, Haiku the Robot encounter framing.

## Patterns

| Pattern | Read | Player skill |
| --- | --- | --- |
| **StaggerTell** | Leap pack attacks one-after-another | Keep moving; punish recovery frames |
| **Pincer** | Front charge first, ranged/support delayed | Side-step charge, close on spitter |
| **TwinDive** | Two moths with offset clocks | Claw bloom / roll between dives |
| **ShelfAmbush** | Air threat early, ground guard late | Vertical space management |

## Authoring

```csharp
w.Encounters.Add(new EncounterPack(EncounterPattern.Pincer, 71, 1, 10f).Add(4).Add(6));
```

- **Anchor** + **TriggerRadius** gate activation (once per visit).
- Respawn / biome load resets packs via `EncounterDirector.Reset`.
- Packs never deal damage — only shape cooldown / facing / clock seeds.

## Fairness rules

1. Never stack identical Active frames on the same X band without stagger.
2. One armored frontliner + support is fine; two armored frontliners is not (yet).
3. Optional routes can bypass densest packs.
4. Bloom dazzle remains a valid multi-moth answer.

## Region density (v0.6)

| Biome | Packs |
| --- | --- |
| Canopy | StaggerTell thorn pair · Pincer bristle + spit |
| Grotto | TwinDive moths · ShelfAmbush moth + thorn |
| Sky | StaggerTell ground · TwinDive high moths |
| Cinder | StaggerTell · Pincer · TwinDive |

## Next

- Soft cue when a pack is cleared
- Trial rooms with scripted sequences
- One elite duelist for single-target mastery
