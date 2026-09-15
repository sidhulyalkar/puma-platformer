# v0.17 recover

## PR
https://github.com/sidhulyalkar/puma-platformer/pull/16

## On branch
Encounter dens clear, Exploration Frostglass, WorldDefinition Ice, Movement (restore if truncated), ARCADE docs.

## Restore full Movement if needed
```bash
python3 patches/restore_movement.py
git add Assets/Wildbound/Core/Movement.cs && git commit -m "fix: restore full Movement.cs"
git push
```

## Still to land
Enemies (Prowler), GameSession harvest, full WorldRegions Frostglass, Runtime view layer.
