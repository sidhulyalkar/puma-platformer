# v0.17 recover

**PR:** https://github.com/sidhulyalkar/puma-platformer/pull/16

## Best path (from a machine with git push)

The sandbox has a complete local commit with full Movement/Enemies/GameSession/WorldRegions. Prefer:

```bash
git fetch origin
git checkout feature/v0.17-consolidated
# If you have the local workspace with complete files, push:
git push origin feature/v0.17-consolidated
```

## Apply from patches (partial)

```bash
python3 patches/apply_v017_core.py
# Expect: Movement, Enemies (if b64 present), GameSession, WorldRegions
grep Prepare Assets/Wildbound/Core/Movement.cs
grep Prowler Assets/Wildbound/Core/Enemies.cs
```

## What is already real source on remote
Encounter dens clear, Exploration Frostglass IDs, WorldDefinition Ice/fragile, arcade docs.
