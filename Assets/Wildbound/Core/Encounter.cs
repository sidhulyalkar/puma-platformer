using System;
using System.Collections.Generic;

namespace Wildbound.Core
{
    /// <summary>
    /// Readable multi-enemy choreography. Timing only — never damage.
    /// Clear = glide + air-control + hunt clarity + scent (one reward path).
    /// </summary>
    public enum EncounterPattern
    {
        Independent = 0, StaggerTell = 1, Pincer = 2, TwinDive = 3, ShelfAmbush = 4,
        CascadeDive = 5, GuardRelay = 6, Crossfire = 7, Duelist = 8
    }

    public sealed class EncounterPack
    {
        public readonly EncounterPattern Pattern;
        public readonly float TriggerRadius;
        public readonly List<int> Members = new List<int>();
        public bool Triggered, Cleared;
        public float StaggerSeconds = .42f;
        public V2 Anchor;

        public EncounterPack(EncounterPattern pattern, float ax, float ay, float triggerRadius = 9f)
        {
            Pattern = pattern; Anchor = new V2(ax, ay); TriggerRadius = triggerRadius;
        }
        public EncounterPack Add(int enemyIndex) { Members.Add(enemyIndex); return this; }

        public bool AllDefeated(List<Enemy> list)
        {
            if (Members.Count == 0) return false;
            for (int i = 0; i < Members.Count; i++)
            {
                int idx = Members[i];
                if (idx < 0 || idx >= list.Count) continue;
                if (list[idx] != null && list[idx].Alive) return false;
            }
            return true;
        }
    }

    public static class EncounterDirector
    {
        public static bool JustClearedPack { get; private set; }
        public static V2 LastClearAnchor { get; private set; }

        public static void Tick(WorldDefinition world, PumaMotor puma)
        {
            JustClearedPack = false;
            if (world.Encounters == null || world.Encounters.Count == 0) return;
            V2 center = puma.Bounds.Center;
            for (int p = 0; p < world.Encounters.Count; p++)
            {
                var pack = world.Encounters[p];
                if (!pack.Triggered && pack.Members.Count > 0
                    && (center - pack.Anchor).Length <= pack.TriggerRadius)
                {
                    Activate(world.Enemies, pack, puma);
                    pack.Triggered = true;
                }
                if (pack.Triggered && !pack.Cleared && pack.AllDefeated(world.Enemies))
                {
                    pack.Cleared = true;
                    JustClearedPack = true;
                    LastClearAnchor = pack.Anchor;
                    puma.GrantGlideFromRecovery();
                    puma.GrantAirControl();
                    puma.HuntClarityTime = Math.Max(puma.HuntClarityTime, PumaMotor.Tuning.HuntClaritySeconds);
                    if (world.ScentMarks != null)
                        world.ScentMarks.Add(new ScentMark(new V2(pack.Anchor.X, pack.Anchor.Y + .4f), 6.2f));
                }
            }
        }

        public static void Reset(WorldDefinition world)
        {
            JustClearedPack = false;
            if (world.Encounters == null) return;
            for (int i = 0; i < world.Encounters.Count; i++)
            {
                world.Encounters[i].Triggered = false;
                world.Encounters[i].Cleared = false;
            }
        }

        private static void Activate(List<Enemy> list, EncounterPack pack, PumaMotor puma)
        {
            for (int i = 0; i < pack.Members.Count; i++)
            {
                var e = At(list, pack.Members[i]);
                if (e == null || !e.Alive) continue;
                e.FaceToward(puma);
                switch (pack.Pattern)
                {
                    case EncounterPattern.StaggerTell:
                        e.Cooldown = i * pack.StaggerSeconds; break;
                    case EncounterPattern.Pincer:
                        if (i == 0) e.Cooldown = 0;
                        else if (e.Kind == EnemyKind.ReedSpitter || e.Kind == EnemyKind.LanternMoth)
                            e.Cooldown = pack.StaggerSeconds * 1.15f;
                        else e.Cooldown = pack.StaggerSeconds * .55f;
                        break;
                    case EncounterPattern.TwinDive:
                        e.Cooldown = (i % 2) * pack.StaggerSeconds * 1.4f; e.Clock = i * 1.7f; break;
                    case EncounterPattern.ShelfAmbush:
                        e.Cooldown = e.Kind == EnemyKind.LanternMoth ? pack.StaggerSeconds * .3f : pack.StaggerSeconds * 1.1f; break;
                    case EncounterPattern.CascadeDive:
                        e.Cooldown = i * pack.StaggerSeconds * 1.05f; e.Clock = i * 2.1f; break;
                    case EncounterPattern.GuardRelay:
                        e.Cooldown = i == 0 ? 0 : pack.StaggerSeconds * (1.8f + i * .55f); break;
                    case EncounterPattern.Crossfire:
                        e.Cooldown = (i % 2) * pack.StaggerSeconds * 1.25f; e.Clock = (i % 2) * 1.4f;
                        if (i % 2 == 1) e.Facing = -e.Facing; break;
                    case EncounterPattern.Duelist:
                        if (i == 0) { e.Cooldown = 0; e.Clock = 0; } else e.Cooldown = 99f; break;
                }
            }
        }

        private static Enemy At(List<Enemy> list, int index)
        {
            if (index < 0 || index >= list.Count) return null;
            return list[index];
        }
    }
}
