using System;
using System.Collections.Generic;
using Wildbound.Core;

namespace Wildbound.Tests
{
    public static class ExplorationCases
    {
        public static readonly Dictionary<string, Action> All = new Dictionary<string, Action>
        {
            { "Old saves retain pickups checkpoints and waystones", OldSave },
            { "Malformed discovery bits cannot unlock arbitrary places", InvalidSave },
            { "Discoveries open only their linked return paths", LinkedPaths },
            { "Wild places survive a fall and a fresh session", Persistence },
            { "Finding a place twice does not repeat its reward", Once },
            { "Flying above a wild place does not discover it", MustLand },
            { "Pause and trials cannot collect outside discoveries", PauseAndTrial },
            { "Scent clues require stalking proximity and clear sight", Scent },
            { "Found trails stop advertising undiscovered destinations", FoundScent },
            { "Waystones and wild places retain separate rewards", SeparateRewards },
            { "Authored discoveries have safe landings and stable IDs", AuthoredPlaces },
            { "Canopy hollow memory and return route work through input", CanopyRoute },
            { "Grotto switchback memory and return route work through input", GrottoRoute },
            { "Sky spring memory and return route work through input", SkyRoute }
        };
        private static void Check(bool ok, string message) { if (!ok) throw new Exception(message); }
        private static void Tick(GameSession g, int count, PlayerInput input = default(PlayerInput))
        {
            for (int i = 0; i < count; i++)
            { g.Step(input); input.JumpPressed = input.PouncePressed = input.PounceReleased = input.RollPressed = input.AttackPressed = false; }
        }
        private static GameSession Start(int biome = 0) { var g = new GameSession(new JourneySave { Biome = biome }); Tick(g, 5); return g; }
        private static void Reach(GameSession g, WildPlace place)
        {
            g.Player.Reset(place.Position);
            if (place.Id == WildPlaceId.RootHollow) g.Player.LowProfile = true;
            Tick(g, 3);
        }
        private static void OldSave()
        {
            var s = new JourneySave { Biome = 1, FurthestBiome = 2, Waystones = 5, Collected = new[] { 37, 3, 8191 }, Checkpoints = new[] { 1, 0, 1 }, Completed = true };
            var g = new GameSession(s);
            Check(s.Version == 1 && s.Discoveries == 0 && s.Waystones == 5 && s.Collected[0] == 37 && s.Collected[2] == 8191 && s.Checkpoints[0] == 1 && s.Completed, "Existing progress changed");
            Check(g.Player.Position.X == 23 && g.World.Pickups[0].Collected, "Old checkpoint or memory lost");
        }
        private static void InvalidSave()
        {
            var s = new JourneySave { Discoveries = -1 }; s.Sanitize(); Check(s.Discoveries == 0, "Negative value became completion");
            s.Discoveries = 1023; s.Sanitize(); Check(s.Discoveries == 63, "Unknown bits survived");
            s.Version = 90; s.Sanitize(); Check(s.Discoveries == 0, "Future save leaked rewards");
        }
        private static void LinkedPaths()
        {
            var g = Start(); var place = g.World.Places[0]; Reach(g, place);
            Check(place.Found && g.Save.Discoveries == place.Mask && g.DiscoveryCount == 1, "Discovery missing");
            foreach (var p in g.World.Platforms)
                if (p.Surface == Surface.Trailbridge) Check(p.Enabled == (p.DiscoverySource == (int)place.Id), "Wrong path opened");
            Check(!g.Save.Completed && g.Save.FurthestBiome == 0 && g.Save.Waystones == 0, "Discovery advanced journey");
        }
        private static void Persistence()
        {
            for (int biome = 0; biome < 3; biome++)
            {
                var g = Start(biome); foreach (var place in g.World.Places) Reach(g, place);
                int bits = g.Save.Discoveries; g.Respawn(); Tick(g, 60);
                Check(g.Save.Discoveries == bits, "Fall lost discoveries");
                var restored = new GameSession(new JourneySave { Biome = biome, Discoveries = bits });
                foreach (var place in restored.World.Places) Check(place.Found, "Reconstruction lost place");
                foreach (var p in restored.World.Platforms) if (p.Surface == Surface.Trailbridge) Check(p.Enabled, "Reload closed a return path");
                Check(restored.TravelTo(0) && restored.TravelTo(biome), "Return travel failed");
            }
        }
        private static void Once()
        {
            var g = Start(); Reach(g, g.World.Places[0]); g.Step(new PlayerInput());
            Check((g.Events & GameEvent.Discovery) == 0 && g.DiscoveryCount == 1 && g.Combat.Instinct == 0, "Repeated reward or combat farming");
        }
        private static void MustLand()
        {
            var g = Start(); var place = g.World.Places[1]; g.Player.Reset(place.Position + new V2(0, .3f));
            g.Step(new PlayerInput()); Check(!place.Found, "A flyby counted as arrival");
            Tick(g, 120); Check(place.Found, "Landing did not discover place");
        }
        private static void PauseAndTrial()
        {
            var g = Start(); g.Player.Reset(g.World.Places[1].Position); g.SetPaused(true); Tick(g, 60);
            Check(g.DiscoveryCount == 0, "Pause collected a place"); g.SetPaused(false); g.Player.Reset(Moontrial.Entrance); Tick(g, 3);
            Check(g.TryEnterTrial(), "Could not enter trial"); Tick(g, 60); Check(g.DiscoveryCount == 0 && g.World.Places.Count == 0, "Trial leaked outside discoveries");
            g.LeaveTrial(); Check(g.World.Places.Count == 2, "Return lost outside places");
        }
        private static void Scent()
        {
            var g = Start(); var point = new V2(4, 1);
            Check(!WildPlace.ScentVisible(g.World, g.Player, point), "Scent visible without stalking");
            g.Step(new PlayerInput { StalkHeld = true }); Check(WildPlace.ScentVisible(g.World, g.Player, point), "Nearby scent missing");
            Check(!WildPlace.ScentVisible(g.World, g.Player, new V2(20, 1)), "Distant scent leaked");
            g.World.Add(3, 1, .2f, 3, Surface.Stone); Check(!WildPlace.ScentVisible(g.World, g.Player, point), "Scent revealed through wall");
        }
        private static void FoundScent()
        {
            var g = Start(); g.Player.Reset(new V2(10.8f, 1)); Tick(g, 3, new PlayerInput { StalkHeld = true });
            Check(g.NearbyTrail() == g.World.Places[0], "Local trail missing");
            Reach(g, g.World.Places[0]); g.Step(new PlayerInput { StalkHeld = true });
            Check(g.NearbyTrail() != g.World.Places[0], "Found trail still advertises itself");
        }
        private static void SeparateRewards()
        {
            var g = new GameSession(new JourneySave { Waystones = 7 });
            foreach (var p in g.World.Platforms)
                if (p.Surface == Surface.Trailbridge) Check(!p.Enabled, "Waystone granted unseen places");
                else if (p.Surface == Surface.Moonbridge) Check(p.Enabled, "Waystone stopped lighting moonbridges");
        }
        private static void AuthoredPlaces()
        {
            int ids = 0;
            for (int biome = 0; biome < 3; biome++)
            {
                var g = Start(biome); Check(g.World.Places.Count == 2 && g.World.Pickups.Count == 13 && g.World.Checkpoints.Count == 2, "Changed persistent list contract");
                foreach (var place in g.World.Places)
                {
                    Check((ids & place.Mask) == 0, "Duplicate permanent ID"); ids |= place.Mask;
                    Reach(g, place); Check(place.Found && g.Player.Grounded && !WorldCollision.OverlapsSolid(g.World, g.Player.Bounds), "Unsafe place " + place.Name);
                    int links = 0; foreach (var p in g.World.Platforms) if (p.DiscoverySource == (int)place.Id) links++;
                    Check(links > 0 && place.Tracks.Length > 1 && place.Position.Y < g.World.MapBounds.Top, "Missing path, clues, or map coverage");
                }
            }
            Check(ids == 63, "Missing permanent ID");
        }

        // End-to-end routes below start at the real spawn, keep wildlife active, and use only input.
        private static float Steer(GameSession g, float x) { return Scalar.Clamp((x - g.Player.Position.X) * 2.5f - g.Player.Velocity.X * .6f, -1, 1); }
        private static string Pos(GameSession g) { return g.Player.Position.X.ToString("F2") + "," + g.Player.Position.Y.ToString("F2"); }
        private static void Walk(GameSession g, float x, bool roll = false)
        {
            for (int i = 0; i < 600; i++)
            {
                if (g.Player.Grounded && Math.Abs(g.Player.Position.X - x) < .2f && Math.Abs(g.Player.Velocity.X) < .6f) return;
                g.Step(new PlayerInput { Move = Steer(g, x), JumpHeld = true, RollPressed = roll && i == 0 });
            }
            Check(false, "Could not walk to " + x + ": " + Pos(g));
        }
        private static void Leap(GameSession g, float x, float y)
        {
            for (int attempt = 0; attempt < 3; attempt++)
            {
                float facing = Math.Sign(x - g.Player.Position.X);
                Tick(g, 78, new PlayerInput { PouncePressed = true, PounceHeld = true, JumpHeld = true });
                g.Step(new PlayerInput { Move = facing, PounceReleased = true, AimY = 1, JumpHeld = true });
                for (int i = 0; i < 360; i++)
                {
                    if (g.Player.Grounded && Math.Abs(g.Player.Position.Y - y) < .12f && Math.Abs(g.Player.Position.X - x) < 1.1f) { Walk(g, x); return; }
                    g.Step(new PlayerInput { Move = Steer(g, x), JumpHeld = true });
                }
            }
            Check(false, "Could not leap to " + x + "," + y + ": " + Pos(g));
        }
        private static void Drop(GameSession g, float x, float y)
        {
            for (int i = 0; i < 600; i++)
            {
                if (g.Player.Grounded && Math.Abs(g.Player.Position.Y - y) < .12f && Math.Abs(g.Player.Position.X - x) < .3f) return;
                g.Step(new PlayerInput { Move = Steer(g, x), JumpHeld = true });
            }
            Check(false, "Could not descend to " + x + "," + y + ": " + Pos(g));
        }
        private static void FinishRoute(GameSession g, int checkpoint)
        {
            Tick(g, 5);
            Check(g.DiscoveryCount == 2 && g.World.Pickups[0].Collected, "Route missed a wild place or memory");
            Check(g.Save.Checkpoints[g.Save.Biome] == checkpoint && g.Deaths == 0, "Route did not return safely to shelter: " + Pos(g));
            foreach (var p in g.World.Platforms) if (p.Surface == Surface.Trailbridge) Check(p.Enabled, "Return path stayed closed");
        }
        private static void CanopyRoute()
        {
            var g = Start(); Walk(g, 10.8f); Walk(g, 16.5f, true);
            Check(g.World.Places[0].Found && g.Player.LowProfile, "Root hollow did not require low traversal");
            Walk(g, 21.5f); Leap(g, 18, 2.95f); Leap(g, 25, 5.2f); Leap(g, 31.5f, 8.1f); Leap(g, 41, 11.1f);
            Drop(g, 31.5f, 10.65f); Drop(g, 28, 1); Walk(g, 23); FinishRoute(g, 0);
        }
        private static void GrottoRoute()
        {
            var g = Start(1); Walk(g, 8.5f); Leap(g, 14, 3.7f); Leap(g, 22, 6.7f); Leap(g, 15, 9.7f);
            Leap(g, 25, 12.7f); Leap(g, 34, 15.7f); Leap(g, 25, 18.7f); Leap(g, 37, 21.7f);
            Drop(g, 44, 15.35f); Drop(g, 51, 5); Drop(g, 58, 1); Walk(g, 60); FinishRoute(g, 1);
        }
        private static void SkyRoute()
        {
            var g = Start(2); Walk(g, 9); bool spring = false;
            for (int i = 0; i < 240 && !spring; i++) { g.Step(new PlayerInput { Move = Steer(g, 11), JumpPressed = i == 0, JumpHeld = true }); spring = (g.Events & GameEvent.Spring) != 0; }
            Check(spring, "Route did not use the spring");
            Drop(g, 17, 5.7f); Leap(g, 26, 9.7f); Leap(g, 38, 13.7f); Leap(g, 48, 17.7f);
            Leap(g, 52.5f, 18.6f); Leap(g, 61, 21.7f); Drop(g, 67, 18.85f); Drop(g, 73, 1); Walk(g, 60); FinishRoute(g, 1);
        }
    }
}
