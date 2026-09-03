using System;
using System.Collections.Generic;
using Wildbound.Core;

namespace Wildbound.Tests
{
    public static class NaturalSystemsCases
    {
        public static readonly Dictionary<string, Action> All = new Dictionary<string, Action>
        {
            { "Sky Garden authors wind fields", SkyHasWind },
            { "Overlapping wind field drifts the puma", WindDriftsPlayer },
            { "Discovery raises a memory vignette", DiscoveryVignette },
            { "Memory pickup raises a biome vignette", MemoryPickupVignette },
            { "Hunting a hare leaves a scent mark", HuntDropsScent },
            { "Scent marks expire", ScentExpires }
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

        private static void SkyHasWind()
        {
            var g = new GameSession(new JourneySave { Biome = 2 });
            Check(g.World.WindFields.Count >= 2, "Sky Garden missing wind fields");
            foreach (var f in g.World.WindFields)
                Check(Math.Abs(f.Velocity.X) + Math.Abs(f.Velocity.Y) > 0, "Zero wind field");
        }

        private static void WindDriftsPlayer()
        {
            var g = new GameSession();
            g.World.Platforms.Clear(); g.World.Enemies.Clear(); g.World.Hazards.Clear();
            g.World.Add(-20, -2, 60, 2);
            g.World.WindFields.Clear();
            g.World.WindFields.Add(new WindField(-5, 0, 20, 8, 4f, 0));
            g.Player.Reset(new V2(0, 0));
            Tick(g, 10);
            float start = g.Player.Position.X;
            // Stand still in the field; wind should push east.
            Tick(g, 60);
            Check(g.Player.Position.X > start + .5f, "Wind did not drift a stationary puma");
        }

        private static void DiscoveryVignette()
        {
            var g = new GameSession();
            var place = g.World.Places[0];
            g.Player.Reset(place.Position);
            if (place.Id == WildPlaceId.RootHollow) g.Player.LowProfile = true;
            Tick(g, 5);
            Check(place.Found && (g.Events & GameEvent.Discovery) != 0 || g.LastDiscovery != null, "Discovery missing");
            Check(g.LastVignette != null && g.VignetteTime > 0, "Vignette not raised");
            Check(g.LastVignette.Title.Length > 0 && g.LastVignette.Body.Length > 0, "Empty vignette");
        }

        private static void MemoryPickupVignette()
        {
            var g = new GameSession();
            Pickup mem = null;
            foreach (var p in g.World.Pickups) if (p.Kind == PickupKind.Memory) { mem = p; break; }
            Check(mem != null, "No memory pickup");
            g.Player.Reset(mem.Position + new V2(0, -.4f));
            Tick(g, 5);
            Check(mem.Collected && g.LastVignette != null, "Memory vignette missing");
            Check(g.LastVignette.Biome == 0, "Wrong vignette biome");
        }

        private static void HuntDropsScent()
        {
            var g = new GameSession();
            g.World.Platforms.Clear(); g.World.Enemies.Clear(); g.World.Hazards.Clear();
            g.World.Add(-20, -2, 60, 2);
            g.Player.Reset(new V2(0, 0)); Tick(g, 5);
            g.World.Enemies.Add(new Enemy(EnemyKind.MossHare, 1.25f, 0, 0));
            int before = g.World.ScentMarks.Count;
            // Swing until hunt
            for (int i = 0; i < 80; i++)
            {
                g.Step(new PlayerInput { AttackPressed = i % 25 == 0 });
                if ((g.Events & GameEvent.Hunt) != 0) break;
            }
            Check(g.World.ScentMarks.Count > before, "Hunt did not drop scent mark");
        }

        private static void ScentExpires()
        {
            var g = new GameSession();
            g.World.ScentMarks.Add(new ScentMark(new V2(1, 1), .05f));
            Tick(g, 20);
            Check(g.World.ScentMarks.Count == 0, "Scent mark did not expire");
        }
    }
}
