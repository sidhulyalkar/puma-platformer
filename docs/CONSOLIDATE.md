# v0.17 recover

## Apply complete core dens/feel files

```bash
git checkout feature/v0.17-consolidated
python3 patches/apply_v017_core.py
grep -n Prepare Assets/Wildbound/Core/Movement.cs
grep -n Prowler Assets/Wildbound/Core/Enemies.cs
git add Assets/Wildbound/Core && git commit -m "feat(v0.17): apply dens/feel core from patches" && git push
```

## PR
https://github.com/sidhulyalkar/puma-platformer/pull/16
