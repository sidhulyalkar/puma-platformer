using System;
using System.Collections.Generic;
using Wildbound.Core;

namespace Wildbound.Tests
{
    public static class EncounterCases
    {
        public static readonly Dictionary<string, Action> All = new Dictionary<string, Action>
        {
            { "StaggerTell offsets pack cooldowns", StaggerOffsets },
            { "Pincer delays support after frontliner", PincerRoles },
            { "TwinDive offsets moth clocks", TwinDiveOffsets },
            { "Encounter triggers once then stays triggered", TriggerOnce },
            { "Canopy authors skirmish and pincer packs", CanopyHasPacks }
        };

        private static void Check(bool c, string why) { if (!c) throw new Exception(why); }

        private static void Tick(GameSession g, int n, PlayerInput input = default(PlayerInput))
        {
            for (int i = 0; i < n; i++)
            {
                g.Step(input);
                input.AttackPressed = input.JumpPressed = false;
            }
        }

        private static void StaggerOffsets()
        {
            var g = new GameSession();
            g.World.Platforms.Clear(); g.World.Enemies.Clear(); g.World.Encounters.Clear();
            g.World.Add(-40, -2, 100, 2);
            g.World.Enemies.Add(new Enemy(EnemyKind.Thornling, 2, 0, 1));
            g.World.Enemies.Add(new Enemy(EnemyKind.Thornling, 4, 0, 1));
            g.World.Encounters.Add(new EncounterPack(EncounterPattern.StaggerTell, 3, 0, 12f).Add(0).Add(1));
            g.Player.Reset(new V2(3, 0));
            Tick(g, 5);
            Check(g.World.Encounters[0].Triggered, "Pack should trigger");
            Check(g.World.Enemies[1].Cooldown > g.World.Enemies[0].Cooldown + .2f, "Second member should be staggered");
        }

        private static void PincerRoles()
        {
            var g = new GameSession();
            g.World.Platforms.Clear(); g.World.Enemies.Clear(); g.World.Encounters.Clear();
            g.World.Add(-40, -2, 100, 2);
            g.World.Enemies.Add(new Enemy(EnemyKind.Bristleback, 2, 0, 1));
            g.World.Enemies.Add(new Enemy(EnemyKind.ReedSpitter, 6, 0));
            g.World.Encounters.Add(new EncounterPack(EncounterPattern.Pincer, 3, 0, 12f).Add(0).Add(1));
            g.Player.Reset(new V2(3, 0));
            Tick(g, 5);
            Check(g.World.Enemies[0].Cooldown <= 0.05f, "Frontliner ready");
            Check(g.World.Enemies[1].Cooldown > 0.3f, "Support delayed");
        }

        private static void TwinDiveOffsets()
        {
            var g = new GameSession();
            g.World.Platforms.Clear(); g.World.Enemies.Clear(); g.World.Encounters.Clear();
            g.World.Add(-40, -2, 100, 2);
            g.World.Enemies.Add(new Enemy(EnemyKind.LanternMoth, 2, 2, 1));
            g.World.Enemies.Add(new Enemy(EnemyKind.LanternMoth, 5, 2, 1));
            g.World.Encounters.Add(new EncounterPack(EncounterPattern.TwinDive, 3.5f, 2, 12f).Add(0).Add(1));
            g.Player.Reset(new V2(3.5f, 0));
            Tick(g, 5);
            Check(Math.Abs(g.World.Enemies[0].Cooldown - g.World.Enemies[1].Cooldown) > 0.3f, "Dive offsets");
        }

        private static void TriggerOnce()
        {
            var g = new GameSession();
            g.World.Platforms.Clear(); g.World.Enemies.Clear(); g.World.Encounters.Clear();
            g.World.Add(-40, -2, 100, 2);
            g.World.Enemies.Add(new Enemy(EnemyKind.Thornling, 2, 0, 1));
            g.World.Encounters.Add(new EncounterPack(EncounterPattern.StaggerTell, 2, 0, 12f).Add(0));
            g.Player.Reset(new V2(2, 0));
            Tick(g, 5);
            Check(g.World.Encounters[0].Triggered, "Triggered");
            g.World.Enemies[0].Cooldown = 9f;
            Tick(g, 5);
            Check(g.World.Enemies[0].Cooldown > 8f, "Should not re-apply stagger after trigger");
        }

        private static void CanopyHasPacks()
        {
            var g = new GameSession(new JourneySave { Biome = 0 });
            Check(g.World.Encounters.Count >= 2, "Canopy missing encounter packs");
            int multi = 0;
            foreach (var e in g.World.Encounters) if (e.Members.Count >= 2) multi++;
            Check(multi >= 2, "Need multi-member packs");
        }
    }
}
