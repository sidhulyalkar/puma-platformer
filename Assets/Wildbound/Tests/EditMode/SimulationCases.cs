using System;
using System.Collections.Generic;
using Wildbound.Core;

namespace Wildbound.Tests
{
    public static class SimulationCases
    {
        static SimulationCases()
        {
            foreach (var pair in CombatCases.All) All.Add(pair.Key, pair.Value);
            foreach (var pair in MoontrailCases.All) All.Add(pair.Key, pair.Value);
            foreach (var pair in ExplorationCases.All) All.Add(pair.Key, pair.Value);
        }
        public static readonly Dictionary<string, Action> All = new Dictionary<string, Action>
        {
            { "Spawn settles without falling", Spawn },
            { "Run accelerates and brakes", RunAndBrake },
            { "Holding jump reaches higher than tapping", VariableJump },
            { "Coyote jump works after an edge", Coyote },
            { "Expired coyote time cannot create an air jump", ExpiredCoyote },
            { "Buffered jump triggers on landing", BufferedJump },
            { "A full pounce travels farther than a tap", ChargedDistance },
            { "Pounce has one use before landing", AirPounceBudget },
            { "Landing restores the pounce", Recharge },
            { "Wall kick pushes away and resists immediate reversal", WallKick },
            { "Wall slide caps descent", WallSlide },
            { "Fast pounce cannot tunnel through a thin wall", ThinWall },
            { "Jump cannot pass through a ceiling", Ceiling },
            { "Flower bounces and restores pounce", Spring },
            { "Moving platform carries a resting player", PlatformCarry },
            { "Hazard returns to checkpoint and keeps discoveries", CheckpointRecovery },
            { "Collectible cannot be counted twice", CollectOnce },
            { "Side contact costs one heart and grants recovery time", CritterContact },
            { "Pounce staggers a thornling", CritterPounce },
            { "Pause freezes simulation and cancels charge", Pause },
            { "Save resumes in the same world with pickups", SaveRoundTrip },
            { "Malformed and future saves fall back safely", InvalidSave },
            { "Discovered worlds can be revisited without losing progress", ReturnTravel },
            { "Distant interaction cannot trigger a portal", PortalDistance },
            { "Three portals complete a journey without collectible gating", CompleteJourney },
            { "All spawns and checkpoints are on safe ground", SafeSpawns },
            { "Invalid timesteps and input are rejected", InvalidInput },
            { "Identical input replays produce identical state", Replay },
            { "Movement-only traversal reaches every world exit", TraverseWorlds },
            { "Seeded long play remains finite and inside world bounds", LongPlay },
            { "Rising into a clear ledge triggers mantle", MantleOntoLedge },
            { "Authored platforms respect minimum solid thickness", PlatformThicknessInvariant },
            { "Peak speeds still subdivide under MaxSubstep", SpeedSubstepBudget },
            { "SweepAABB reports TOI before tunneling a thin wall", SweepDetectsThinWall }
        };
        private static void Check(bool value, string message) { if (!value) throw new Exception(message); }
        private static bool Near(float a, float b, float epsilon = .02f) { return Math.Abs(a - b) < epsilon; }
        private static GameSession Flat()
        {
            var g = new GameSession(); g.World.Platforms.Clear(); g.World.Hazards.Clear(); g.World.Enemies.Clear(); g.World.Blooms.Clear(); g.World.Pickups.Clear();
            g.World.Checkpoints.Clear(); g.World.Add(-100, -2, 300, 2); g.Player.Reset(new V2(0, 0)); Tick(g, 125); return g;
        }
        private static void Tick(GameSession g, int count, PlayerInput input = default(PlayerInput))
        { for (int i = 0; i < count; i++) { g.Step(input); input.JumpPressed = input.PouncePressed = input.PounceReleased = input.InteractPressed = false; } }
        private static void Charge(GameSession g, int ticks, float aim = 0)
        {
            Tick(g, ticks, new PlayerInput { PouncePressed = true, PounceHeld = true });
            g.Step(new PlayerInput { PounceReleased = true, AimY = aim });
        }
        private static void Spawn() { var g = new GameSession(); Tick(g, 120); Check(g.Player.Grounded && Near(g.Player.Position.Y, 1), "Spawn fell or did not settle"); }
        private static void RunAndBrake()
        {
            var g = Flat(); Tick(g, 60, new PlayerInput { Move = 1 }); Check(Near(g.Player.Velocity.X, 8.5f), "Run speed");
            Tick(g, 30); Check(Near(g.Player.Velocity.X, 0), "Brake");
        }
        private static float JumpHeight(bool hold)
        {
            var g = Flat(); float max = 0;
            g.Step(new PlayerInput { JumpPressed = true, JumpHeld = true });
            for (int i = 0; i < 120; i++) { g.Step(new PlayerInput { JumpHeld = hold }); max = Math.Max(max, g.Player.Position.Y); }
            return max;
        }
        private static void VariableJump() { Check(JumpHeight(true) > JumpHeight(false) + 1, "Holding jump did not meaningfully change height"); }
        private static void Coyote()
        {
            var g = Flat(); g.World.Platforms.Clear(); Tick(g, 6);
            g.Step(new PlayerInput { JumpPressed = true, JumpHeld = true }); Check(g.Player.Velocity.Y > 10, "Coyote jump failed");
        }
        private static void ExpiredCoyote()
        {
            var g = Flat(); g.World.Platforms.Clear(); Tick(g, 24);
            g.Step(new PlayerInput { JumpPressed = true, JumpHeld = true }); Check(g.Player.Velocity.Y < 0, "Expired coyote jumped");
        }
        private static void BufferedJump()
        {
            var g = Flat(); g.Player.Reset(new V2(0, .4f)); g.Player.Velocity.Y = -6;
            g.Step(new PlayerInput { JumpPressed = true, JumpHeld = true }); bool jumped = false;
            for (int i = 0; i < 20; i++) { g.Step(new PlayerInput { JumpHeld = true }); jumped |= (g.Events & GameEvent.Jump) != 0; }
            Check(jumped && g.Player.Position.Y > .4f, "Landing buffer lost");
        }
        private static float PounceDistance(int ticks)
        {
            var g = Flat(); Charge(g, ticks); float start = g.Player.Position.X;
            for (int i = 0; i < 300 && !g.Player.Grounded; i++) g.Step(new PlayerInput { Move = 1, JumpHeld = true });
            return g.Player.Position.X - start;
        }
        private static void ChargedDistance() { Check(PounceDistance(80) > PounceDistance(1) * 1.5f, "Charging did not increase distance"); }
        private static void AirPounceBudget()
        {
            var g = Flat(); Charge(g, 80, 1); Tick(g, 8);
            g.Step(new PlayerInput { PouncePressed = true, PounceHeld = true }); Check(!g.Player.Charging && !g.Player.PounceReady, "Infinite air pounces");
        }
        private static void Recharge() { var g = Flat(); Charge(g, 80); Tick(g, 300); Check(g.Player.Grounded && g.Player.PounceReady, "Ground recharge failed"); }
        private static void WallKick()
        {
            var g = Flat(); g.World.Add(1, 0, .3f, 10, Surface.Stone); g.Player.Reset(new V2(.55f, 4));
            Tick(g, 2, new PlayerInput { Move = 1 }); g.Step(new PlayerInput { Move = 1, JumpPressed = true, JumpHeld = true });
            Check(g.Player.Velocity.X < -8 && g.Player.Velocity.Y > 10, "Kick did not push away");
            Tick(g, 8, new PlayerInput { Move = 1 }); Check(g.Player.Velocity.X < -8, "Kick immediately reversed");
        }
        private static void WallSlide()
        {
            var g = Flat(); g.World.Add(1, 0, 1, 20); g.Player.Reset(new V2(.55f, 15)); Tick(g, 100, new PlayerInput { Move = 1 });
            Check(g.Player.Velocity.Y >= -3.01f && g.Player.Wall == 1, "Slide too fast");
        }
        private static void ThinWall()
        {
            var g = Flat(); g.World.Add(3, 0, .13f, 25); Charge(g, 80); Tick(g, 90, new PlayerInput { Move = 1 });
            Check(g.Player.Bounds.Right <= 3.001f, "Tunneled through wall");
        }
        private static void Ceiling()
        {
            var g = Flat(); g.World.Add(-1, 2.5f, 3, .15f); g.Step(new PlayerInput { JumpPressed = true, JumpHeld = true });
            for (int i = 0; i < 90; i++) { g.Step(new PlayerInput { JumpHeld = true }); Check(g.Player.Bounds.Top <= 2.501f, "Ceiling penetration"); }
        }
        private static void Spring()
        {
            var g = Flat(); g.World.Add(-1, 0, 2, .3f, Surface.Spring); g.Player.Reset(new V2(0, 2)); g.Player.PounceReady = false;
            bool bounced = false;
            for (int i = 0; i < 100; i++) { g.Step(new PlayerInput()); if ((g.Events & GameEvent.Spring) != 0) { bounced = true; break; } }
            Check(bounced && g.Player.Velocity.Y >= 20 && g.Player.PounceReady, "Spring interaction failed");
        }
        private static void PlatformCarry()
        {
            var g = Flat(); g.World.Platforms.Clear(); g.World.Add(-2, 0, 4, .5f, Surface.Moving, 2); g.Player.Reset(new V2(0, .5f));
            Tick(g, 2); float relative = g.Player.Position.X - g.World.Platforms[0].Bounds.Center.X; Tick(g, 150);
            Check(g.Player.Grounded && Near(g.Player.Position.X - g.World.Platforms[0].Bounds.Center.X, relative, .03f), "Lost moving platform");
        }
        private static void CheckpointRecovery()
        {
            var g = new GameSession(); g.Player.Reset(new V2(23, 1)); Tick(g, 2); Check(g.Save.Checkpoints[0] == 0, "Checkpoint not recorded");
            g.World.Hazards.Add(new Box(30, 1, 2, .45f));
            g.Save.Collected[0] = 2; g.Player.Reset(new V2(30.5f, 1)); g.Step(new PlayerInput());
            Check(g.Deaths == 1 && Near(g.Player.Position.X, 23) && g.Save.Collected[0] == 2, "Recovery lost position or discoveries");
        }
        private static void CollectOnce()
        {
            var g = Flat(); g.World.Pickups.Add(new Pickup(0, .6f)); g.Step(new PlayerInput()); Check((g.Events & GameEvent.Collect) != 0, "Missing collect event");
            g.Step(new PlayerInput()); Check((g.Events & GameEvent.Collect) == 0 && g.Motes == 1, "Collected twice");
        }
        private static void CritterContact()
        {
            var g = Flat(); g.World.Enemies.Add(new Enemy(EnemyKind.Thornling, 0, 0, 0)); g.Step(new PlayerInput()); Check(g.Combat.Health == 4 && g.Deaths == 0, "Contact did not cost exactly one heart");
        }
        private static void CritterPounce()
        {
            var g = Flat(); g.World.Enemies.Add(new Enemy(EnemyKind.Thornling, 1.7f, 0, 0)); Charge(g, 1); Tick(g, 12, new PlayerInput { Move = 1 });
            Check(g.Deaths == 0 && g.World.Enemies[0].Health < g.World.Enemies[0].MaxHealth, "Pounce did not hit thornling");
        }
        private static void Pause()
        {
            var g = Flat(); Tick(g, 15, new PlayerInput { Move = 1, PouncePressed = true, PounceHeld = true });
            V2 position = g.Player.Position; float t = g.Time; g.SetPaused(true); Tick(g, 100, new PlayerInput { Move = 1 });
            Check(Near(g.Time, t) && Near(position.X, g.Player.Position.X) && !g.Player.Charging, "Pause leaked simulation or charge");
        }
        private static void SaveRoundTrip()
        {
            var save = new JourneySave { Biome = 1, Collected = new[] { 3, 5, 0 }, Checkpoints = new[] { 0, 1, -1 } };
            var g = new GameSession(save); Check(g.World.Biome == 1 && g.World.Pickups[0].Collected && g.World.Pickups[2].Collected && Near(g.Player.Position.X, 60), "Save did not restore");
        }
        private static void InvalidSave()
        {
            var g = new GameSession(new JourneySave { Biome = 99, Collected = null, Checkpoints = new[] { 888 } }); Check(g.World.Biome == 2 && Near(g.Player.Position.X, 2), "Malformed save failed");
            g = new GameSession(new JourneySave { Version = 90, Biome = 2, Completed = true }); Check(g.World.Biome == 0 && !g.Save.Completed, "Future version should reset");
        }
        private static void PortalDistance() { var g = new GameSession(); g.Step(new PlayerInput { InteractPressed = true }); Check(g.Save.Biome == 0, "Remote portal activated"); }
        private static void ReturnTravel()
        {
            var g = new GameSession(); Check(!g.TravelTo(2), "Undiscovered world unlocked");
            g.Save.Collected[0] = 3; g.Player.Reset(g.World.Exit); g.Step(new PlayerInput { InteractPressed = true });
            Check(g.TravelTo(0) && g.World.Pickups[0].Collected && g.Save.FurthestBiome == 1, "Return lost exploration");
            Check(g.TravelTo(1) && !g.TravelTo(2) && !g.TravelTo(-1), "Travel boundaries failed");
        }
        private static void CompleteJourney()
        {
            var g = new GameSession();
            for (int b = 0; b < 3; b++) { g.Player.Reset(g.World.Exit); g.Step(new PlayerInput { InteractPressed = true }); }
            Check(g.Save.Completed && g.Save.Biome == 2 && g.Memories == 0, "Optional pickups gated completion");
            g.Step(new PlayerInput { InteractPressed = true }); Check((g.Events & GameEvent.Portal) == 0, "Completion fired repeatedly");
        }
        private static void SafeSpawns()
        {
            for (int b = 0; b < 3; b++) for (int c = -1; c < 2; c++)
            {
                var save = new JourneySave { Biome = b }; save.Checkpoints[b] = c; var g = new GameSession(save); Tick(g, 180);
                Check(g.Deaths == 0 && g.Player.Grounded, "Unsafe spawn " + b + "/" + c);
                foreach (var platform in g.World.Platforms) Check(!platform.Enabled || !g.Player.Bounds.Overlaps(platform.Bounds), "Spawn in solid");
            }
        }
        private static void InvalidInput()
        {
            var g = Flat(); int errors = 0;
            foreach (float dt in new[] { 0, -.1f, .5f, float.NaN, float.PositiveInfinity }) try { g.Step(new PlayerInput(), dt); } catch (ArgumentOutOfRangeException) { errors++; }
            try { g.Step(new PlayerInput { Move = float.NaN }); } catch (ArgumentException) { errors++; }
            Check(errors == 6, "Accepted invalid simulation input");
        }
        private static PlayerInput RandomInput(Random random, int tick)
        { return new PlayerInput { Move = random.Next(3) - 1, AimY = random.Next(3) - 1, JumpHeld = tick % 60 < 40, JumpPressed = tick % 60 == 0, PounceHeld = tick % 200 < 80, PouncePressed = tick % 200 == 0, PounceReleased = tick % 200 == 80 }; }
        private static void Replay()
        {
            var a = new GameSession(); var b = new GameSession(); var random = new Random(991);
            for (int i = 0; i < 4000; i++) { var input = RandomInput(random, i); a.Step(input); b.Step(input); }
            Check(a.Player.Position.X == b.Player.Position.X && a.Player.Position.Y == b.Player.Position.Y && a.Deaths == b.Deaths, "Replay diverged");
        }
        private static void TraverseWorlds()
        {
            string failures = "";
            for (int biome = 0; biome < 3; biome++)
            {
                var g = new GameSession(new JourneySave { Biome = biome }); int chargeTicks = 0; bool reached = false;
                for (int tick = 0; tick < 3600; tick++)
                {
                    var p = g.Player;
                    var input = new PlayerInput { Move = p.Position.X < 74 ? 1 : p.Position.X > 76 ? -1 : 0, JumpHeld = true, AimY = 1, InteractPressed = (p.Position - g.World.Exit).Length < 2.2f };
                    if (p.Grounded && p.PounceReady && !p.Charging) { input.PouncePressed = true; chargeTicks = 0; }
                    if (p.Charging) { input.PounceHeld = true; if (++chargeTicks >= 80) { input.PounceHeld = false; input.PounceReleased = true; } }
                    if (p.Wall != 0 && p.Velocity.Y < 0) input.JumpPressed = true;
                    g.Step(input);
                    if (g.Save.Biome != biome || g.Save.Completed) { reached = true; break; }
                }
                if (!reached || g.Deaths > 1) failures += " biome " + biome + " at " + g.Player.Position.X.ToString("F2") + "," + g.Player.Position.Y.ToString("F2") + " deaths=" + g.Deaths + ";";
            }
            Check(failures.Length == 0, "Main route:" + failures);
        }
        private static void LongPlay()
        {
            for (int b = 0; b < 3; b++)
            {
                var g = new GameSession(new JourneySave { Biome = b }); var random = new Random(15 + b);
                for (int i = 0; i < 12000; i++)
                {
                    g.Step(RandomInput(random, i));
                    Check(Scalar.Finite(g.Player.Position.X) && Scalar.Finite(g.Player.Position.Y), "Nonfinite position");
                    Check(g.Player.Position.X > -6 && g.Player.Position.X < 82 && g.Player.Position.Y > -8.1f, "Escaped world bounds");
                }
            }
        }
        private static void MantleOntoLedge()
        {
            var g = Flat();
            g.World.Add(2, 0, 4, 2.2f);
            g.Player.Reset(new V2(1.4f, 1.1f));
            g.Player.Facing = 1;
            g.Player.Velocity = new V2(3, 6);
            bool mantled = false;
            for (int i = 0; i < 90; i++)
            {
                g.Step(new PlayerInput { Move = 1 });
                if ((g.Events & GameEvent.Mantle) != 0) mantled = true;
                if (g.Player.Grounded && g.Player.Position.Y >= 2.1f) break;
            }
            Check(mantled, "Mantle event never fired");
            Check(g.Player.Position.Y >= 2.0f, "Did not finish above the ledge lip");
        }

        private static void PlatformThicknessInvariant()
        {
            Check(WorldCollision.MaxSubstep < WorldCollision.MinSolidThickness, "MaxSubstep must stay below MinSolidThickness");
            for (int biome = 0; biome < 3; biome++)
            {
                var g = new GameSession(new JourneySave { Biome = biome });
                foreach (var p in g.World.Platforms)
                {
                    if (!p.Enabled) continue;
                    float thin = Math.Min(p.Bounds.W, p.Bounds.H);
                    Check(thin + 1e-4f >= WorldCollision.MinSolidThickness,
                        "Thin platform in biome " + biome + " size " + p.Bounds.W + "x" + p.Bounds.H);
                }
            }
        }

        private static void SpeedSubstepBudget()
        {
            var tuning = new MovementTuning();
            float peak = Math.Max(tuning.PounceMaxSpeed, Math.Max(tuning.DashSpeed, tuning.MaxFall));
            float perTick = peak * GameSession.StepSeconds;
            int steps = Math.Max(1, (int)Math.Ceiling(perTick / WorldCollision.MaxSubstep));
            Check(perTick / steps <= WorldCollision.MaxSubstep + 1e-5f, "Peak speed exceeds sub-step budget");
            Check(steps >= 1, "Sub-step count collapsed");
        }

        private static void SweepDetectsThinWall()
        {
            // Wall thinner than MaxSubstep: discrete single step could miss; sweep must still report TOI.
            var mover = new Box(0, 0, .9f, 1.05f);
            var wall = new Box(2, -1, .08f, 4);
            float toi; int axis;
            bool hit = WorldCollision.SweepAABB(mover, new V2(5, 0), wall, out toi, out axis);
            Check(hit, "Sweep missed thin wall");
            Check(toi > 0 && toi < 1, "TOI not in open unit interval");
            Check(axis == 0, "Expected horizontal dominant axis");
            // Clear path should miss.
            Check(!WorldCollision.SweepAABB(mover, new V2(0, 3), wall, out toi, out axis), "False positive on clear vertical path");
        }
    }
}
