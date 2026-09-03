using System;
using System.Collections.Generic;

namespace Wildbound.Core
{
    /// <summary>
    /// Multi-enemy fight patterns. Simulation-only: staggers tells / roles so
    /// pressure is readable and skillful rather than simultaneous spam.
    /// </summary>
    public enum EncounterPattern
    {
        Independent = 0,
        StaggerTell = 1,
        Pincer = 2,
        TwinDive = 3,
        ShelfAmbush = 4
    }

    public sealed class EncounterPack
    {
        public readonly EncounterPattern Pattern;
        public readonly float TriggerRadius;
        public readonly List<int> Members = new List<int>();
        public bool Triggered;
        public float StaggerSeconds = .42f;
        public V2 Anchor;

        public EncounterPack(EncounterPattern pattern, float ax, float ay, float triggerRadius = 9f)
        {
            Pattern = pattern;
            Anchor = new V2(ax, ay);
            TriggerRadius = triggerRadius;
        }

        public EncounterPack Add(int enemyIndex)
        {
            Members.Add(enemyIndex);
            return this;
        }
    }

    public static class EncounterDirector
    {
        public static void Tick(WorldDefinition world, PumaMotor puma)
        {
            if (world.Encounters == null || world.Encounters.Count == 0) return;
            V2 center = puma.Bounds.Center;
            foreach (var pack in world.Encounters)
            {
                if (pack.Triggered || pack.Members.Count == 0) continue;
                if ((center - pack.Anchor).Length > pack.TriggerRadius) continue;
                Activate(world, pack, puma);
                pack.Triggered = true;
            }
        }

        public static void Reset(WorldDefinition world)
        {
            if (world.Encounters == null) return;
            foreach (var pack in world.Encounters) pack.Triggered = false;
        }

        private static void Activate(WorldDefinition world, EncounterPack pack, PumaMotor puma)
        {
            var list = world.Enemies;
            switch (pack.Pattern)
            {
                case EncounterPattern.StaggerTell:
                    for (int i = 0; i < pack.Members.Count; i++)
                    {
                        var e = EnemyAt(list, pack.Members[i]);
                        if (e == null || !e.Alive) continue;
                        e.Cooldown = i * pack.StaggerSeconds;
                        if (e.Phase == EnemyPhase.Idle) e.FaceToward(puma);
                    }
                    break;

                case EncounterPattern.Pincer:
                    for (int i = 0; i < pack.Members.Count; i++)
                    {
                        var e = EnemyAt(list, pack.Members[i]);
                        if (e == null || !e.Alive) continue;
                        e.FaceToward(puma);
                        if (i == 0) e.Cooldown = 0;
                        else if (e.Kind == EnemyKind.ReedSpitter || e.Kind == EnemyKind.LanternMoth)
                            e.Cooldown = pack.StaggerSeconds * 1.15f;
                        else e.Cooldown = pack.StaggerSeconds * .55f;
                    }
                    break;

                case EncounterPattern.TwinDive:
                    for (int i = 0; i < pack.Members.Count; i++)
                    {
                        var e = EnemyAt(list, pack.Members[i]);
                        if (e == null || !e.Alive) continue;
                        e.FaceToward(puma);
                        e.Cooldown = (i % 2) * pack.StaggerSeconds * 1.4f;
                        e.Clock = i * 1.7f;
                    }
                    break;

                case EncounterPattern.ShelfAmbush:
                    for (int i = 0; i < pack.Members.Count; i++)
                    {
                        var e = EnemyAt(list, pack.Members[i]);
                        if (e == null || !e.Alive) continue;
                        e.FaceToward(puma);
                        if (e.Kind == EnemyKind.LanternMoth) e.Cooldown = pack.StaggerSeconds * .3f;
                        else e.Cooldown = pack.StaggerSeconds * 1.1f;
                    }
                    break;
            }
        }

        private static Enemy EnemyAt(List<Enemy> list, int index)
        {
            if (index < 0 || index >= list.Count) return null;
            return list[index];
        }
    }
}
