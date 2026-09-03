# Combat Precision: AABB edge cases

Fights must feel fair and readable. Every hit, block, and miss is resolved against axis-aligned boxes at 120 Hz with sub-step motion. This document inventories the edge cases that matter and the contracts the simulation now enforces.

## Core geometry

| Actor | Bounds origin | Size notes |
| --- | --- | --- |
| Puma | feet center | 0.9 × 1.05 (0.58 tall while rolled) |
| Thornling / moth | feet center | ~0.85 × 0.7 |
| Bristleback | feet center | 1.2 × 0.9 |
| Reed spitter | feet center | ~0.85 × 1.2 |
| Moss hare | feet center | 0.65 × 0.7 |
| Claw post | feet center | ~0.85 × 1.6 |
| Projectile | center | radius 0.12 (swept segment vs expanded AABB) |

Strike boxes are move-specific AABBs relative to the puma’s feet center. They never drive physics; they only query overlaps + line-of-sight.

## Edge-case inventory & contracts

### 1. Strike line-of-sight through thin gaps / corners
**Risk:** Center-to-center `ClearLine` fails when a thin pillar sits between centers even though the strike box clearly overlaps the enemy silhouette.

**Contract:** `WorldCollision.StrikeClear` samples three rays (center, upper third, lower third of the strike box toward the enemy center). A strike is valid if **any** sample is clear. Terrain still blocks when the entire silhouette is occluded.

### 2. Armor / front determination
**Risk:** `player.Position.Y < enemy.Bounds.Top` plus a simple facing product can mis-classify a puma that is slightly above or overlapping the front face.

**Contract:** Front is determined by horizontal overlap of the strike (or player) relative to the enemy’s vertical mid-line and the enemy’s current facing. A hit is blocked only when:
- the enemy is armored,
- the strike approaches from the front half,
- and the strike is not a confirmed downward rake.

### 3. Strike + body-hit double counting
**Risk:** The same sub-step can register both an active claw and a descending body overlap, double-damaging or double-refreshing.

**Contract:**
- `hitEnemies` (per attack Sequence) owns claw impacts.
- `bodyHits` owns pounce / descending stomps.
- If a claw already recorded an enemy index this Sequence, body resolution skips it.
- Body resolution also skips while the claw is Active on that same index (existing rule, now asserted in tests).

### 4. Descending stomp threshold
**Risk:** A horizontal brush at the top of an enemy can be treated as a stomp, or a true stomp can be rejected when the enemy is moving.

**Contract:** A body hit counts as descending only when `dy < 0` and the previous feet Y was at or above `enemy.Bounds.Top - 0.10`. The tolerance is intentionally tight so side scrapes do not become free stomps.

### 5. Projectile vs player vs terrain ordering
**Risk:** A fast shot can tunnel, or a wall behind the player can incorrectly “shield” them.

**Contract (already present, re-asserted):** Swept segment vs expanded AABBs; the **nearest** intersection wins. Walls behind the player never cancel a nearer player hit. Global projectile cap remains 24; lifetime 3 s.

### 6. Contact damage while clawing / dodging
**Risk:** Contact damage during the active frames of a claw that already hit the same enemy, or during the dodge window of a roll.

**Contract:**
- `ContactDanger` is false while the enemy is Stunned.
- Player contact damage is suppressed while `Invulnerable > 0` or `player.Dodging`.
- Active claw that already recorded the enemy does not also apply body contact in the same resolve path.

### 7. Wall-blocked strikes and blooms
**Risk:** A claw arc visually crosses a pillar but still damages an enemy or awakens a bloom behind it.

**Contract:** Both enemy strikes and bloom activation require `StrikeClear` (multi-point). Authored tests cover a thin wall between puma and target.

### 8. Sub-step ordering
**Risk:** At high pounce speeds the order of “move → strike → body → contact” can change outcomes frame-to-frame.

**Contract:** Every motion sub-step (≤ 0.12 units) still runs strike then body then contact. Hit sets are Sequence-scoped so the first successful resolve owns the enemy for that attack. Deterministic replay remains a hard requirement.

### 9. Mantle interaction with combat
**Risk:** Entering a mantle mid-claw or mid-contact could leave inconsistent state.

**Contract:** Mantle is blocked while rolling/dashing/low-profile; claw recovery can still be cancelled by movement events that already clear `bodyHits`. Mantle itself does not deal damage and does not clear hit sets mid-Sequence.

## Implementation map

| Concern | Primary code |
| --- | --- |
| Strike boxes | `PumaCombat.StrikeBox` |
| Strike resolve + LOS | `PumaCombat.ResolveStrike`, `WorldCollision.StrikeClear` |
| Body / stomp | `PumaCombat.ResolveBodyHit` |
| Armor / front | `PumaCombat.IsFrontApproach` |
| Projectiles | `GameSession.Step` projectile loop |
| Enemy contact | `Enemy.ContactDanger`, `PumaCombat.TakeDamage` |

## Regression surface

New / strengthened cases in `CombatCases`:
- Thin-gap LOS still allows a clear silhouette hit
- Wall still fully blocks when all samples are occluded
- Armor edge when puma is slightly above the front face
- Simultaneous active claw + descending body does not double-hit
- Projectile nearest-intersection under an active claw remains stable

Run the full suite after any change to strike geometry, LOS, or sub-step order:

```bash
dotnet run --project tests/Wildbound.Tests.csproj -c Release
```

## Future hardening (not in this pass)

- Swept strike boxes for extremely fast dash-claws (currently discrete per sub-step)
- One-way / slope surfaces once those geometry types exist
- Explicit parry window if a defensive tool is added later
