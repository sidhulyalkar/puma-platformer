using System;
using System.Collections.Generic;
using Wildbound.Core;

namespace Wildbound.Tests
{
    public static class CinderCases
    {
        public static readonly Dictionary<string, Action> All = new Dictionary<string, Action>
        {
            { "Cinder Ravine authors as biome 3", CinderCreates },
            { "Cinder has heat vents and bark", CinderHasSystems },
            { "Ember bloom enables bridge only while glowing", EmberTimedBridge },
            { "Journey save accepts four biomes", SaveFourBiomes }
        };

        private static void Check(bool c, string why) { if (!c) throw new Exception(why); }

        private static void Tick(GameSession g, int n, PlayerInput input = default(PlayerInput))
        {
            for (int i = 0; i < n; i++)
            {
                g.Step(input);
                input.AttackPressed = input.JumpPressed = input.PouncePressed = input.PounceReleased = false;
            }
        }

        private static void CinderCreates()
        {
            var g = new GameSession(new JourneySave { Biome = 3 });
            Check(g.World.Biome == 3, "Biome not 3");
            Check(g.World.Name.Contains("CINDER"), "Wrong name");
            Check(g.World.Places.Count >= 2, "Missing wild places");
        }

        private static void CinderHasSystems()
        {
            var g = new GameSession(new JourneySave { Biome = 3 });
            int bark = 0, vents = 0, ember = 0;
            foreach (var p in g.World.Platforms) if (p.Surface == Surface.Bark) bark++;
            foreach (var f in g.World.WindFields) if (f.Velocity.Y > 3) vents++;
            foreach (var b in g.World.Blooms) if (b.Kind == BloomKind.Ember) ember++;
            Check(bark >= 2, "Need bark faces");
            Check(vents >= 2, "Need heat vents");
            Check(ember >= 1, "Need ember bloom");
        }

        private static void EmberTimedBridge()
        {
            var g = new GameSession();
            g.World.Platforms.Clear(); g.World.Enemies.Clear(); g.World.Blooms.Clear();
            g.World.Add(-20, -2, 60, 2);
            g.World.Blooms.Add(new Moonbloom(0, 1.2f, BloomKind.Ember));
            g.World.Platforms.Add(new Platform(new Box(2, 2, 3, .35f), Surface.Moonbridge) { LightSource = 0, Enabled = false });
            g.Player.Reset(new V2(0, 0)); Tick(g, 5);
            Check(!g.World.Platforms[1].Enabled, "Ember bridge solid before claw");
            for (int i = 0; i < 40; i++)
            {
                g.Step(new PlayerInput { AttackPressed = i == 0 });
                if ((g.Events & GameEvent.Bloom) != 0) break;
            }
            Check(g.World.Blooms[0].GlowTime > 0 && g.World.Platforms[1].Enabled, "Ember bridge not solid while glowing");
            Tick(g, 800);
            Check(g.World.Blooms[0].GlowTime <= 0 && !g.World.Platforms[1].Enabled, "Ember bridge should decay");
        }

        private static void SaveFourBiomes()
        {
            var save = new JourneySave { Biome = 3, FurthestBiome = 3 };
            save.Sanitize();
            Check(save.Biome == 3 && save.Collected.Length == 4 && save.Checkpoints.Length == 4, "Save schema");
            var g = new GameSession(save);
            Check(g.World.Biome == 3, "Load biome 3");
        }
    }
}
