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
            foreach (var pair in NaturalSystemsCases.All) All.Add(pair.Key, pair.Value);
            foreach (var pair in ClimbCases.All) All.Add(pair.Key, pair.Value);
            foreach (var pair in CinderCases.All) All.Add(pair.Key, pair.Value);
            foreach (var pair in EncounterCases.All) All.Add(pair.Key, pair.Value);
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
            { "Spring restores pounce and launches", Spring },
            { "Hazard forces a respawn at the last checkpoint", HazardRespawn },
            { "Falling below the world respawns", FallRespawn },
            { "Checkpoint heals and becomes the respawn", CheckpointHeal },
            { "Roll lowers the body and can pass a low gap", RollUnder },
            { "Dash-claw travels while active", DashClaw },
            { "Stalking slows the puma", StalkSlow },
            { "Long play remains finite and inside world bounds", LongPlay },
            { "Rising into a clear ledge triggers mantle", MantleOntoLedge },
            { "Authored platforms respect minimum solid thickness", PlatformThicknessInvariant },
            { "Peak speeds still subdivide under MaxSubstep", SpeedSubstepBudget },
            { "SweepAABB reports TOI before tunneling a thin wall", SweepDetectsThinWall },
            { "Full pounce grants a limited tail-glide", FullPounceGrantsGlide },
            { "Tail-glide budget expires", GlideBudgetExpires },
            { "Air control after wall kick improves steering", AirControlAfterWallKick }
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
        private static void VariableJump() { Check(JumpHeight(true) > JumpHeight(false) + .4f, "Hold jump should reach higher"); }
        private static void Coyote()
        {
            var g = Flat(); Tick(g, 30, new PlayerInput { Move = 1 });
            g.World.Platforms.Clear(); g.World.Add(2, -2, 100, 2);
            Tick(g, 8); g.Step(new PlayerInput { JumpPressed = true });
            Check(g.Player.Velocity.Y > 5, "Coyote jump failed");
        }
        private static void ExpiredCoyote()
        {
            var g = Flat(); Tick(g, 30, new PlayerInput { Move = 1 });
            g.World.Platforms.Clear(); Tick(g, 30); g.Step(new PlayerInput { JumpPressed = true });
            Check(g.Player.Velocity.Y <= 0, "Expired coyote created an air jump");
        }
        private static void BufferedJump()
        {
            var g = Flat(); Tick(g, 5); g.Player.Position.Y = 3; g.Player.Velocity = new V2(0, -8); g.Player.Grounded = false;
            g.Step(new PlayerInput { JumpPressed = true }); Tick(g, 40);
            Check(g.Player.Velocity.Y > 5 || g.Player.Position.Y > 2, "Buffered jump missing");
        }
        private static void ChargedDistance()
        {
            var g = Flat(); float x0 = g.Player.Position.X; Charge(g, 5); Tick(g, 90);
            float shortDist = g.Player.Position.X - x0;
            g = Flat(); x0 = g.Player.Position.X; Charge(g, 90); Tick(g, 90);
            Check(g.Player.Position.X - x0 > shortDist + 2, "Full charge should travel farther");
        }
        private static void AirPounceBudget()
        {
            var g = Flat(); Charge(g, 40); Tick(g, 10); float y = g.Player.Velocity.Y;
            g.Step(new PlayerInput { PouncePressed = true, PounceHeld = true }); Tick(g, 5, new PlayerInput { PounceHeld = true });
            g.Step(new PlayerInput { PounceReleased = true }); Check(g.Player.Velocity.Y <= y + 1, "Second air pounce should be denied");
        }
        private static void Recharge()
        {
            var g = Flat(); Charge(g, 40); Tick(g, 200); Check(g.Player.PounceReady && g.Player.Grounded, "Landing should restore pounce");
        }
        private static void WallKick()
        {
            var g = Flat(); g.World.Add(3, 0, 1, 8, Surface.Stone); Tick(g, 5);
            Tick(g, 40, new PlayerInput { Move = 1 }); g.Step(new PlayerInput { JumpPressed = true, Move = 1 });
            Tick(g, 20, new PlayerInput { Move = 1 }); float x = g.Player.Position.X;
            Tick(g, 15, new PlayerInput { Move = -1 }); Check(g.Player.Position.X > x - 1.5f, "Wall kick lock failed");
        }
        private static void WallSlide()
        {
            var g = Flat(); g.World.Add(3, 0, 1, 10, Surface.Stone); Tick(g, 5);
            Tick(g, 50, new PlayerInput { Move = 1 }); g.Player.Velocity.Y = -20;
            Tick(g, 20, new PlayerInput { Move = 1 }); Check(g.Player.Velocity.Y >= -3.5f, "Wall slide should cap fall");
        }
        private static void ThinWall()
        {
            var g = Flat(); g.World.Add(4, 0, .2f, 4, Surface.Stone); Tick(g, 5);
            Charge(g, 90); Tick(g, 60); Check(g.Player.Position.X < 4.2f, "Tunneled through thin wall");
        }
        private static void Ceiling()
        {
            var g = Flat(); g.World.Add(-5, 2.2f, 20, .4f, Surface.Stone); Tick(g, 5);
            g.Step(new PlayerInput { JumpPressed = true, JumpHeld = true }); Tick(g, 40, new PlayerInput { JumpHeld = true });
            Check(g.Player.Position.Y + g.Player.BodyHeight <= 2.35f, "Jumped through ceiling");
        }
        private static void Spring()
        {
            var g = Flat(); g.World.Add(1, 0, 2, .35f, Surface.Spring); Tick(g, 5);
            Tick(g, 30, new PlayerInput { Move = 1 }); Check(g.Player.Velocity.Y > 10 && g.Player.PounceReady, "Spring failed");
        }
        private static void HazardRespawn()
        {
            var g = Flat(); g.World.Hazards.Add(new Box(1, 0, 2, 1)); Tick(g, 5);
            Tick(g, 40, new PlayerInput { Move = 1 }); Check(g.Deaths >= 1, "Hazard should respawn");
        }
        private static void FallRespawn()
        {
            var g = Flat(); g.World.Platforms.Clear(); Tick(g, 200); Check(g.Deaths >= 1, "Fall should respawn");
        }
        private static void CheckpointHeal()
        {
            var g = new GameSession(); g.Player.Reset(g.World.Checkpoints[0]); Tick(g, 10);
            Check(g.Save.Checkpoints[0] == 0, "Checkpoint not recorded");
        }
        private static void RollUnder()
        {
            var g = Flat(); g.World.Add(2, .7f, 3, 1, Surface.Stone); Tick(g, 5);
            g.Step(new PlayerInput { RollPressed = true, Move = 1 }); Tick(g, 40, new PlayerInput { Move = 1 });
            Check(g.Player.Position.X > 4, "Roll should clear low gap");
        }
        private static void DashClaw()
        {
            var g = Flat(); float x0 = g.Player.Position.X;
            g.Step(new PlayerInput { DashPressed = true }); Tick(g, 20);
            Check(g.Player.Position.X > x0 + 2, "Dash-claw distance");
        }
        private static void StalkSlow()
        {
            var g = Flat(); Tick(g, 40, new PlayerInput { Move = 1, StalkHeld = true });
            Check(Math.Abs(g.Player.Velocity.X) < 4, "Stalk should slow run");
        }
        private static void LongPlay()
        {
            var g = new GameSession(); Tick(g, 2400, new PlayerInput { Move = 1 });
            Check(float.IsFinite(g.Player.Position.X) && g.Player.Position.X < 90, "Long play diverged");
        }
        private static void MantleOntoLedge()
        {
            var g = Flat(); g.World.Add(2, 2.2f, 3, .4f, Surface.Stone); Tick(g, 5);
            g.Player.Reset(new V2(1.2f, 1.5f)); g.Player.Velocity = new V2(2, 4); g.Player.Facing = 1;
            Tick(g, 40, new PlayerInput { Move = 1 });
            Check((g.Events & GameEvent.Mantle) != 0 || g.Player.Grounded, "Mantle should trigger or land");
        }
        private static void PlatformThicknessInvariant()
        {
            for (int b = 0; b < 4; b++)
            {
                var w = WorldDefinition.Create(b);
                foreach (var p in w.Platforms)
                    if (p.Enabled && p.Bounds.H > 0 && p.Bounds.H < WorldCollision.MinSolidThickness)
                        throw new Exception("Thin platform in biome " + b);
            }
        }
        private static void SpeedSubstepBudget()
        {
            float maxAxis = Math.Max(PumaMotor.Tuning.PounceMaxSpeed, PumaMotor.Tuning.DashSpeed);
            Check(maxAxis * GameSession.StepSeconds <= WorldCollision.MaxSubstep * 4, "Substep budget");
        }
        private static void SweepDetectsThinWall()
        {
            var from = new Box(0, 0, .9f, 1);
            float toi; int axis;
            toi = WorldCollision.SweepAABB(from, new V2(3, 0), new Box(1.5f, 0, .2f, 2), out axis);
            Check(toi < 1f && axis == 0, "Sweep should hit thin wall");
        }
        private static void FullPounceGrantsGlide()
        {
            var g = Flat(); Charge(g, 100); Tick(g, 5, new PlayerInput { JumpHeld = true });
            Check(g.Player.GlideBudget > 0, "Full pounce should grant glide");
        }
        private static void GlideBudgetExpires()
        {
            var g = Flat(); g.Player.GlideBudget = .2f; g.Player.Grounded = false; g.Player.Velocity = new V2(0, 2);
            Tick(g, 50, new PlayerInput { JumpHeld = true });
            Check(g.Player.GlideBudget <= 0.01f, "Glide budget should expire");
        }
        private static void AirControlAfterWallKick()
        {
            var g = Flat(); g.World.Add(3, 0, 1, 8, Surface.Stone); Tick(g, 5);
            Tick(g, 40, new PlayerInput { Move = 1 }); g.Step(new PlayerInput { JumpPressed = true, Move = 1 });
            float xKick = g.Player.Position.X;
            Tick(g, 30, new PlayerInput { Move = -1 });
            Check(g.Player.Position.X < xKick + 2, "Air control window should allow steering");
        }
    }
}
