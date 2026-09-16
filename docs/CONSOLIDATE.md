# v0.17 recover

**PR:** https://github.com/sidhulyalkar/puma-platformer/pull/16

## Apply complete dens/feel core (recommended)

```bash
git fetch origin && git checkout feature/v0.17-consolidated
python3 patches/apply_v017_core.py
# Expect 4/4 writes. Verify:
grep -n Prepare Assets/Wildbound/Core/Movement.cs
grep -n Prowler Assets/Wildbound/Core/Enemies.cs
grep -n HarvestSurfaceFeedback Assets/Wildbound/Core/GameSession.cs
grep -n BuildFrostglass Assets/Wildbound/Core/WorldRegions.cs
git add Assets/Wildbound/Core && git commit -m "feat(v0.17): apply dens/feel core from verified b64" && git push
```

## Already real source on remote
Encounter dens clear, Exploration Frostglass, WorldDefinition Ice/fragile, arcade docs.
