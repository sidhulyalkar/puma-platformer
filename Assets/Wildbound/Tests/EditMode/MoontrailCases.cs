using System;
using System.Collections.Generic;
using Wildbound.Core;

namespace Wildbound.Tests
{
    public static class MoontrailCases
    {
        public static readonly Dictionary<string, Action> All = new Dictionary<string, Action>
        {
            { "Trials require deliberate nearby grounded interaction", Entry },
            { "Trial return preserves outside discoveries and checkpoint", Return },
            { "Trial death stays in the trial and preserves lit mechanisms", Retry },
            { "An unfinished waystone cannot grant its reward", LockedReward },
            { "Restoring a waystone persists light bridges without unlocking regions", Reward },
            { "Old and malformed waystone saves sanitize safely", SaveCompatibility },
            { "Reloading during a trial resumes the outside trail", MidTrialSave },
            { "Wind prevents idle camping from balancing a perch", IdleBalance },
            { "Keyboard countersteering can stabilize the moving perch", KeyboardBalance },
            { "Charging or attacking cannot also attune a perch", BalanceCommitment },
            { "A moonbell needs a real downward rake from above", BellDirection },
            { "Terrain blocks moonbell rebounds", BellCover },
            { "Moonbells refresh traversal without farming health or instinct", BellReward },
            { "Downward intent can redirect an active pounce into a rake", PounceRedirect },
            { "Pounce cancels recovery but not committed attack frames", PounceCancel },
            { "Root gates require dash claws and cannot be breached through cover", GateRules },
            { "A dash from an upper ledge can hit the visible gate surface", HighGate },
            { "The grotto low route is traversable with a grounded roll", GrottoRoll },
            { "Pause freezes trial wind and mechanisms", Pause },
            { "Map travel abandons a trial without leaving stale return state", Abandon },
            { "Trial scenes have safe starts and valid mechanism links", Layouts },
            { "Canopy waystone completes through real input", CanopyRoute },
            { "Grotto waystone completes through real input", GrottoRoute },
            { "Sky waystone completes through real input", SkyRoute },
            { "Seeded trial combat remains finite and collision-safe", Stress }
        };
        private static void Check(bool ok, string why) { if (!ok) throw new Exception(why); }
        private static void Tick(GameSession g, int count, PlayerInput input = default(PlayerInput))
        {
            for (int i = 0; i < count; i++)
            {
                g.Step(input);
                input.AttackPressed = input.DashPressed = input.RollPressed = input.JumpPressed = input.PouncePressed = input.PounceReleased = input.InteractPressed = false;
            }
        }
        private static GameSession Trial(int biome = 0)
        {
            var g = new GameSession(new JourneySave { Biome = biome }); Tick(g, 3);
            g.Step(new PlayerInput { InteractPressed = true }); Check(g.InTrial, "Trial entrance failed"); Tick(g, 3); return g;
        }
        private static void Entry()
        {
            var g = new GameSession(); Tick(g, 180); Check(!g.InTrial, "Walking near entrance opened trial");
            g.Player.Reset(new V2(20, 1)); Tick(g, 3); Check(!g.TryEnterTrial(), "Remote entrance accepted");
            g.Player.Reset(Moontrial.Entrance); Check(!g.TryEnterTrial(), "Airborne entrance accepted");
            Tick(g, 3); g.SetPaused(true); Check(!g.TryEnterTrial(), "Paused entrance accepted");
            g.SetPaused(false); g.Step(new PlayerInput { InteractPressed = true }); Check(g.InTrial, "Nearby E failed");
        }
        private static void Return()
        {
            var g = new GameSession(); var outside = g.World; g.Save.Collected[0] = 3; outside.Pickups[0].Collected = true; g.Save.Checkpoints[0] = 1;
            Tick(g, 3); g.Step(new PlayerInput { InteractPressed = true }); g.Projectiles.Add(new Projectile(new V2(5, 5), new V2(1, 0)));
            Check(g.LeaveTrial() && object.ReferenceEquals(outside, g.World), "Outside world was reconstructed or lost");
            Check(g.Save.Collected[0] == 3 && g.Save.Checkpoints[0] == 1 && g.World.Pickups[0].Collected && g.Projectiles.Count == 0, "Return leaked state");
        }
        private static void Retry()
        {
            var g = Trial(); g.Save.Checkpoints[0] = 1; g.World.Blooms[0].Awakened = true; g.World.Trial.Balance.Charge = .5f;
            g.Player.Reset(new V2(30, -9)); g.Step(new PlayerInput());
            Check(g.InTrial && g.Player.Position.X == g.World.Spawn.X && g.World.Blooms[0].Awakened && g.World.Trial.Balance.Charge == 0, "Trial retry lost mechanisms or used outside checkpoint");
        }
        private static void LockedReward()
        {
            var g = Trial(); g.Player.Reset(g.World.Trial.Sanctuary); Tick(g, 3); g.Step(new PlayerInput { InteractPressed = true });
            Check(g.InTrial && g.Save.Waystones == 0 && (g.Events & GameEvent.ObjectiveBlocked) != 0, "Unfinished trial paid out");
        }
        private static void Reward()
        {
            var g = Trial(); g.World.Blooms[0].Awakened = true; g.World.Trial.Balance.Attuned = true;
            g.Player.Reset(g.World.Trial.Sanctuary); Tick(g, 3); g.Step(new PlayerInput { InteractPressed = true });
            Check(!g.InTrial && g.Save.Waystones == 1 && g.WaystoneCount == 1 && g.Save.FurthestBiome == 0 && !g.Save.Completed, "Reward changed unrelated progression");
            var restored = new GameSession(g.Save);
            foreach (var p in restored.World.Platforms) if (p.Surface == Surface.Moonbridge) Check(p.Enabled, "Saved reward did not restore bridge");
            Check(!restored.TravelTo(1), "Waystone bypassed region travel");
        }
        private static void SaveCompatibility()
        {
            var old = new JourneySave { Collected = new[] { 7, 0, 0 } }; old.Sanitize(); Check(old.Waystones == 0 && old.Collected[0] == 7, "Old save changed");
            var invalid = new JourneySave { Waystones = -1 }; invalid.Sanitize(); Check(invalid.Waystones == 0, "Negative flags unlocked rewards");
            invalid.Waystones = 511; invalid.Sanitize(); Check(invalid.Waystones == 7, "Unknown bits survived");
            invalid.Version = 88; invalid.Sanitize(); Check(invalid.Waystones == 0, "Future schema retained rewards");
        }
        private static void MidTrialSave()
        {
            var g = Trial(1); g.World.Blooms[0].Awakened = true; g.Save.Checkpoints[1] = 1;
            var resumed = new GameSession(g.Save); Check(!resumed.InTrial && resumed.Save.Biome == 1 && resumed.Player.Position.X == 60 && resumed.Save.Waystones == 0, "Trial state polluted persistent location");
        }
        private static void OnPerch(GameSession g)
        {
            var b = g.World.Platforms[g.World.Trial.Balance.PlatformIndex].Bounds;
            g.Player.Reset(new V2(b.Center.X, b.Top)); Tick(g, 2);
        }
        private static void IdleBalance()
        {
            var g = Trial(); OnPerch(g); Tick(g, 300, new PlayerInput { StalkHeld = true });
            Check(!g.World.Trial.Balance.Attuned, "Holding still solved the wind perch");
        }
        private static PlayerInput Countersteer(GameSession g)
        {
            var b = g.World.Trial.Balance; float offset = g.Player.Position.X - g.World.Platforms[b.PlatformIndex].Bounds.Center.X;
            // Keyboard inputs only: short taps against the ribbon, then release to drift back.
            return new PlayerInput { StalkHeld = true, Move = offset > -.08f ? -1 : 0 };
        }
        private static void KeyboardBalance()
        {
            var g = Trial(); OnPerch(g);
            for (int i = 0; i < 600 && !g.World.Trial.Balance.Attuned; i++) g.Step(Countersteer(g));
            var b = g.World.Trial.Balance;
            Check(b.Attuned && g.World.Platforms[b.BridgeIndex].Enabled, "Countersteering cannot attune perch");
        }
        private static void BalanceCommitment()
        {
            var g = Trial(); OnPerch(g);
            for (int i = 0; i < 400; i++)
            {
                var input = Countersteer(g); input.PouncePressed = i == 0; input.PounceHeld = true; g.Step(input);
            }
            Check(!g.World.Trial.Balance.Attuned, "Charging also balanced");
        }
        private static void BellDirection()
        {
            var g = Trial(1); var bell = g.World.Trial.Bell;
            g.Player.Reset(bell.Position + new V2(-1, 0)); Tick(g, 30, new PlayerInput { AttackPressed = true });
            Check(!bell.Rung, "Side swipe rang downward bell");
            g.Player.Reset(bell.Position + new V2(0, 2)); Tick(g, 35, new PlayerInput { AttackPressed = true, AimY = -1, JumpHeld = true });
            Check(bell.Rung, "Downward rake missed bell");
        }
        private static void BellCover()
        {
            var g = Trial(1); var bell = g.World.Trial.Bell; g.World.Add(bell.Position.X - 1, bell.Position.Y + .45f, 2, .15f);
            g.Player.Reset(bell.Position + new V2(0, 1)); Tick(g, 35, new PlayerInput { AttackPressed = true, AimY = -1 });
            Check(!bell.Rung, "Bell rang through solid ceiling");
        }
        private static void BellReward()
        {
            var g = Trial(1); var bell = g.World.Trial.Bell; g.Player.Reset(bell.Position + new V2(0, 2));
            g.Player.PounceReady = g.Player.AirDashReady = false; bool hit = false;
            for (int i = 0; i < 40; i++)
            {
                g.Step(new PlayerInput { AttackPressed = i == 0, AimY = -1, JumpHeld = true });
                if ((g.Events & GameEvent.Moonbell) == 0) continue;
                hit = true; Check(g.Player.Velocity.Y > 0 && g.Player.PounceReady && g.Player.AirDashReady, "Bell did not refresh traversal"); break;
            }
            Check(hit && g.Combat.Instinct == 0 && g.Combat.Hunts == 0 && g.Combat.Health == 5, "Bell granted hunt resources");
        }
        private static void PounceRedirect()
        {
            var g = Trial(); g.Player.Reset(new V2(6, 5)); g.Player.PounceTime = .2f;
            g.Step(new PlayerInput { AttackPressed = true, AimY = -1 }); Check(g.Combat.Move == ClawMove.DownRake, "Pounce swallowed directional intent");
        }
        private static void PounceCancel()
        {
            var g = Trial(); g.Step(new PlayerInput { AttackPressed = true });
            g.Step(new PlayerInput { PouncePressed = true, PounceHeld = true }); Check(!g.Player.Charging && g.Combat.Busy, "Pounce erased windup");
            Tick(g, 25); g.Step(new PlayerInput { PouncePressed = true, PounceHeld = true });
            Check(g.Player.Charging && !g.Combat.Busy, "Pounce could not chain out of recovery");
        }
        private static void GateRules()
        {
            var g = Trial(2); var gate = g.World.Trial.Gate; g.Player.Reset(new V2(34.8f, -2)); Tick(g, 3);
            Tick(g, 45, new PlayerInput { AttackPressed = true }); Check(!gate.Broken, "Basic claw broke reinforced roots");
            int shield = g.World.Platforms.Count; g.World.Add(35.4f, -2, .15f, 14);
            Tick(g, 70, new PlayerInput { DashPressed = true }); Check(!gate.Broken, "Dash breached through a nearer wall");
            g.World.Platforms.RemoveAt(shield); Tick(g, 40, new PlayerInput { DashPressed = true });
            Check(gate.Broken && !g.World.Platforms[gate.PlatformIndex].Enabled, "Dash did not break gate");
        }
        private static void Pause()
        {
            var g = Trial(2); OnPerch(g); g.World.Trial.Bell.Cooldown = .4f; float t = g.Time; var position = g.Player.Position;
            g.SetPaused(true); Tick(g, 300, new PlayerInput { StalkHeld = true });
            Check(g.Time == t && g.Player.Position.X == position.X && g.World.Trial.Bell.Cooldown == .4f && g.World.Trial.Balance.Charge == 0, "Paused mechanisms advanced");
        }
        private static void HighGate()
        {
            var g = Trial(2); g.Player.Reset(new V2(34.8f, 10)); Tick(g, 3);
            Tick(g, 35, new PlayerInput { Move = 1, DashPressed = true });
            Check(g.World.Trial.Gate.Broken, "Ledge incorrectly occluded a visible upper gate surface");
            Check(g.World.Trial.LastImpact.Y > 9.5f, "Upper gate impact feedback was placed below the strike");
        }
        private static void GrottoRoll()
        {
            var g = Trial(1); g.Player.Reset(new V2(37, 1)); Tick(g, 3);
            g.Step(new PlayerInput { Move = 1, RollPressed = true });
            for (int i = 0; i < 75; i++)
            {
                g.Step(new PlayerInput { Move = 1 });
                Check(!WorldCollision.OverlapsSolid(g.World, g.Player.Bounds), "Low route embedded the puma");
            }
            Check(g.Player.Position.X > 41.7f && !g.Player.LowProfile, "Grotto roll route could not clear the arch");
        }
        private static void Abandon()
        {
            var g = Trial(1); Check(g.TravelTo(0) && !g.InTrial && !g.LeaveTrial() && g.Save.Waystones == 0, "Abandoned trial left a stale return");
        }
        private static void Layouts()
        {
            for (int biome = 0; biome < 3; biome++)
            {
                var g = Trial(biome); var t = g.World.Trial;
                Check(g.Player.Grounded && !WorldCollision.OverlapsSolid(g.World, g.Player.Bounds), "Unsafe trial spawn");
                Check(t.GoalCount >= 2 && g.World.CameraMaxX < 54, "Trial goals or camera missing");
                if (t.Balance != null) Check(g.World.Platforms[t.Balance.PlatformIndex].Surface == Surface.Balance && !g.World.Platforms[t.Balance.BridgeIndex].Enabled, "Invalid perch link");
                if (t.Gate != null) Check(g.World.Platforms[t.Gate.PlatformIndex].Surface == Surface.RootGate, "Invalid gate link");
            }
        }
        private static void CanopyRoute() { RunRoute(0); }
        private static void GrottoRoute() { RunRoute(1); }
        private static void SkyRoute() { RunRoute(2); }
        public static void RunRoute(int biome)
        {
            // Filled by the actual input route policy below; no position edits after entry.
            var g = Trial(biome);
            Walk(g, 6.8f);
            if (biome < 2)
            {
                g.Step(new PlayerInput { AttackPressed = true, Move = 1 }); Tick(g, 49);
                Check(g.World.Blooms[0].Awakened, "Route did not claw the bloom");
            }
            Walk(g, 10.5f); Leap(g, 14.5f, 2.9f, false);
            if (g.World.Trial.Balance != null)
            {
                Leap(g, g.World.Platforms[g.World.Trial.Balance.PlatformIndex].Bounds.Center.X, biome == 0 ? 4.5f : 5.5f, true);
                for (int i = 0; i < 600 && !g.World.Trial.Balance.Attuned; i++) g.Step(Countersteer(g));
                Check(g.World.Trial.Balance.Attuned, "Route failed to balance: " + Pos(g));
            }
            if (biome == 1) Leap(g, 23.5f, 8, true);
            if (biome == 2) Leap(g, 29, 7.4f, true);
            if (biome > 0)
            {
                var bell = g.World.Trial.Bell;
                g.Step(new PlayerInput { JumpPressed = true, JumpHeld = true });
                for (int i = 0; i < 240 && !bell.Rung; i++)
                {
                    var p = g.Player; float dx = bell.Position.X - p.Position.X;
                    g.Step(new PlayerInput { Move = Steer(g, bell.Position.X), JumpHeld = true, AimY = -1,
                        AttackPressed = !g.Combat.Busy && Math.Abs(dx) < .6f && p.Position.Y > bell.Position.Y + .6f && p.Position.Y < bell.Position.Y + 3.5f });
                }
                Check(bell.Rung, "Route missed bell: " + Pos(g));
            }
            if (biome == 2)
            {
                var gate = g.World.Trial.Gate;
                for (int i = 0; i < 500 && !gate.Broken; i++)
                {
                    var p = g.Player; float center = g.World.Platforms[gate.PlatformIndex].Bounds.Center.X;
                    bool close = Math.Abs(center - p.Position.X) < 2 && p.Position.Y < 10.3f;
                    g.Step(new PlayerInput { Move = close ? Math.Sign(center - p.Position.X) : Steer(g, 34.8f), JumpHeld = true,
                        DashPressed = close && !g.Combat.Busy && p.DashCooldown <= 0 });
                }
                Check(gate.Broken, "Route missed root gate: " + Pos(g));
            }
            // Descend along the far side, then dash the gate (sky) or cross the encounter.
            for (int i = 0; i < 900 && g.Player.Position.X < 46.8f; i++)
            {
                var p = g.Player; var t = g.World.Trial;
                g.Step(new PlayerInput { Move = 1, JumpHeld = true,
                    DashPressed = !g.Combat.Busy && p.DashCooldown <= 0 && (t.Gate == null || p.Position.X > 33),
                    JumpPressed = p.Grounded && (biome != 2 || t.Gate.Broken),
                    RollPressed = biome == 1 && p.Grounded && p.Position.X > 37 && p.Position.X < 42 });
            }
            Walk(g, 48);
            Tick(g, 3); g.Step(new PlayerInput { InteractPressed = true });
            Check(!g.InTrial && g.WaystoneRestored(biome), "Route failed to restore waystone " + biome + ": " + Pos(g));
            Check(g.Deaths == 0, "Input route required a death");
        }
        private static string Pos(GameSession g) { return g.Player.Position.X.ToString("F2") + "," + g.Player.Position.Y.ToString("F2"); }
        private static float Steer(GameSession g, float x) { return Scalar.Clamp((x - g.Player.Position.X) * 2.5f - g.Player.Velocity.X * .6f, -1, 1); }
        private static void Walk(GameSession g, float x)
        {
            for (int i = 0; i < 600; i++)
            {
                if (Math.Abs(g.Player.Position.X - x) < .18f && g.Player.Grounded && Math.Abs(g.Player.Velocity.X) < .6f) return;
                g.Step(new PlayerInput { Move = Steer(g, x), JumpHeld = true });
            }
            Check(false, "Could not settle at " + x + ": " + Pos(g));
        }
        private static void Leap(GameSession g, float x, float y, bool pounce)
        {
            if (pounce)
            {
                Tick(g, 60, new PlayerInput { PouncePressed = true, PounceHeld = true, JumpHeld = true });
                g.Step(new PlayerInput { Move = 1, PounceReleased = true, AimY = 1, JumpHeld = true });
            }
            else g.Step(new PlayerInput { JumpPressed = true, JumpHeld = true, Move = 1 });
            for (int i = 0; i < 360; i++)
            {
                if (g.Player.Grounded && Math.Abs(g.Player.Position.Y - y) < .1f && Math.Abs(g.Player.Position.X - x) < 2) return;
                g.Step(new PlayerInput { Move = Steer(g, x), JumpHeld = true });
            }
            Check(false, "Could not leap to " + x + "," + y + ": " + Pos(g));
        }
        private static void Stress()
        {
            for (int biome = 0; biome < 3; biome++)
            {
                var g = Trial(biome); var random = new Random(930 + biome);
                for (int i = 0; i < 6000; i++)
                {
                    g.Step(new PlayerInput { Move = random.Next(3) - 1, AimY = random.Next(3) - 1, JumpPressed = i % 45 == 0,
                        JumpHeld = true, PouncePressed = i % 200 == 0, PounceHeld = i % 200 < 75, PounceReleased = i % 200 == 75,
                        AttackPressed = i % 41 == 0, DashPressed = i % 130 == 0, RollPressed = i % 99 == 0, StalkHeld = i % 200 < 50 });
                    Check(Scalar.Finite(g.Player.Position.X) && Scalar.Finite(g.Player.Position.Y) && g.Player.Position.X > -6 && g.Player.Position.X < 55, "Trial escaped bounds");
                    Check(!WorldCollision.OverlapsSolid(g.World, g.Player.Bounds), "Trial embedded puma in terrain");
                }
            }
        }
    }
}
