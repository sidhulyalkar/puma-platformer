using System;
using System.Collections.Generic;
using Wildbound.Core;

namespace Wildbound.Tests
{
    public static class PracticeCases
    {
        public static readonly Dictionary<string, Action> All = new Dictionary<string, Action>
        {
            { "Practice extends old saves without replacing discoveries", SaveCompatibility },
            { "Invalid practice bits are sanitized and new journeys start clean", InvalidSave },
            { "Jump practice requires a launched jump and records once", Jump },
            { "Canceled or unavailable pounces do not complete practice", Pounce },
            { "Claw practice requires a connected strike", Claw },
            { "Armor blocks and body hits cannot complete claw practice", FalseClaw },
            { "Dash practice is separate from claw and respects windup", Dash },
            { "A ground roll practices while an airborne roll is rejected", Roll },
            { "Low ceilings keep blocked attacks and pounces unpracticed", Crawl },
            { "Scent practice requires visible undiscovered tracks", Scent },
            { "A saved waystone does not pretend a moonbloom was practiced", Bloom },
            { "Spring and wall kick practice use actual movement outcomes", Traversal },
            { "Introductory hints retire but route signs remain", RetireHints },
            { "Combined lessons accept either order", AnyOrder },
            { "Guidance respects proximity sight and pause", LocalHints },
            { "Pause and recovery cannot record practice or spend notice time", Frozen },
            { "Practice survives death travel and save reconstruction", Persistence },
            { "Trial practice keeps outside progress and suppresses introductory hints", Trial },
            { "Outside objectives lead to the arch with or without waystones", MainObjective },
            { "Trial objectives still follow the next mechanism", TrialObjective },
            { "Opening practice works from spawn using input with wildlife active", OpeningRoute }
        };
        private static void Check(bool ok, string message) { if (!ok) throw new Exception(message); }
        private static bool Has(GameSession g, PracticeSkill skill) { return PracticeGuide.Has(g.Save, skill); }
        private static void Tick(GameSession g, int count, PlayerInput input = default(PlayerInput))
        {
            for (int i = 0; i < count; i++)
            {
                g.Step(input);
                input.JumpPressed = input.PouncePressed = input.PounceReleased = input.AttackPressed = false;
                input.DashPressed = input.RollPressed = input.InteractPressed = false;
            }
        }
        private static GameSession Start() { var g = new GameSession(); Tick(g, 5); return g; }
        private static GameSession Flat()
        {
            var g = new GameSession(); g.World.Platforms.Clear(); g.World.Hazards.Clear();
            g.World.Enemies.Clear(); g.World.Pickups.Clear(); g.World.Blooms.Clear(); g.World.Places.Clear();
            g.World.Checkpoints.Clear(); g.World.Signs.Clear(); g.World.Add(-100, -2, 300, 2);
            g.Player.Reset(new V2(0, 0)); Tick(g, 5); return g;
        }
        private static void SaveCompatibility()
        {
            var save = new JourneySave { Biome = 1, FurthestBiome = 2, Waystones = 5, Discoveries = 3,
                Collected = new[] { 31, 3, 8191 }, Checkpoints = new[] { 1, 0, 1 }, Completed = true };
            var g = new GameSession(save);
            Check(save.Practiced == 0 && save.Version == 1 && save.Waystones == 5 && save.Discoveries == 3
                && save.Collected[2] == 8191 && save.Checkpoints[0] == 1 && save.Completed && g.Player.Position.X == 23, "Old journey changed");
        }
        private static void InvalidSave()
        {
            var save = new JourneySave { Practiced = -1 }; save.Sanitize(); Check(save.Practiced == 0, "Negative bits became practice");
            save.Practiced = 4095; save.Sanitize(); Check(save.Practiced == PracticeGuide.AllSkills, "Unknown bits survived");
            save.Version = 99; save.Sanitize(); Check(save.Practiced == 0, "Future version retained practice");
            Check(new GameSession().Save.Practiced == 0, "New journey inherited practice");
        }
        private static void Jump()
        {
            var g = Flat(); g.Player.Reset(new V2(0, 4));
            Tick(g, 5, new PlayerInput { JumpPressed = true }); Check(!Has(g, PracticeSkill.Jump), "Air press counted");
            Tick(g, 150); g.Step(new PlayerInput { JumpPressed = true, JumpHeld = true });
            Check(Has(g, PracticeSkill.Jump) && (g.Events & GameEvent.Practice) != 0 && g.Player.Velocity.Y > 0, "Actual jump missing");
            Tick(g, 200); g.Step(new PlayerInput { JumpPressed = true });
            Check((g.Events & GameEvent.Practice) == 0, "Repeat jump dirtied save");
        }
        private static void Pounce()
        {
            var g = Flat(); Tick(g, 78, new PlayerInput { PouncePressed = true, PounceHeld = true });
            Check(!Has(g, PracticeSkill.Pounce), "Holding counted as release");
            g.SetPaused(true); g.Step(new PlayerInput { PounceReleased = true }); g.SetPaused(false);
            g.Step(new PlayerInput { PounceReleased = true }); Check(!Has(g, PracticeSkill.Pounce), "Canceled coil counted");
            g.Player.Reset(new V2(0, 4)); g.Player.PounceReady = false;
            Tick(g, 3, new PlayerInput { PouncePressed = true, PounceHeld = true }); g.Step(new PlayerInput { PounceReleased = true });
            Check(!Has(g, PracticeSkill.Pounce), "Unavailable pounce counted");
            Tick(g, 180); Tick(g, 78, new PlayerInput { PouncePressed = true, PounceHeld = true });
            g.Step(new PlayerInput { PounceReleased = true, AimY = 1 });
            Check(Has(g, PracticeSkill.Pounce) && !g.Player.PounceReady, "Launched pounce missing");
        }
        private static void Claw()
        {
            var g = Flat(); Tick(g, 60, new PlayerInput { AttackPressed = true });
            Check(!Has(g, PracticeSkill.Claw), "Empty swing counted");
            g.World.Enemies.Add(new Enemy(EnemyKind.ClawPost, 1.1f, 0));
            Tick(g, 12, new PlayerInput { AttackPressed = true });
            Check(Has(g, PracticeSkill.Claw), "Scratch post hit not recorded");
            Tick(g, 310); Check(g.Practice.Recent == null && g.Practice.NoticeSeconds == 0, "Completion notice never expired");
        }
        private static void FalseClaw()
        {
            var g = Flat(); var boar = new Enemy(EnemyKind.Bristleback, 1, 0) { Facing = -1 };
            g.World.Enemies.Add(boar); Tick(g, 12, new PlayerInput { AttackPressed = true });
            Check(!Has(g, PracticeSkill.Claw) && boar.Health == boar.MaxHealth, "Armor block counted");
            g = Flat(); g.World.Enemies.Add(new Enemy(EnemyKind.Thornling, 0, 0)); g.Player.Reset(new V2(0, 2));
            g.Player.Velocity.Y = -10; Tick(g, 24);
            Check(!Has(g, PracticeSkill.Claw), "Body rebound counted as a claw");
        }
        private static void Dash()
        {
            var g = Flat(); g.Step(new PlayerInput { AttackPressed = true });
            g.Step(new PlayerInput { DashPressed = true }); Check(!Has(g, PracticeSkill.Dash), "Windup cancellation counted");
            Tick(g, 80); g.World.Enemies.Add(new Enemy(EnemyKind.ClawPost, 1, 0));
            g.Step(new PlayerInput { DashPressed = true });
            Check(Has(g, PracticeSkill.Dash) && !Has(g, PracticeSkill.Claw), "Dash granted ordinary claw practice");
        }
        private static void Roll()
        {
            var g = Flat(); g.Player.Reset(new V2(0, 4)); g.Step(new PlayerInput { RollPressed = true });
            Check(!Has(g, PracticeSkill.Roll), "Airborne roll counted");
            Tick(g, 160); g.Step(new PlayerInput { RollPressed = true });
            Check(Has(g, PracticeSkill.Roll) && g.Player.LowProfile, "Ground roll missing");
        }
        private static void Scent()
        {
            var g = Start(); g.Player.Reset(new V2(10.8f, 1)); Tick(g, 3);
            Check(!Has(g, PracticeSkill.Scent), "Proximity alone counted");
            g.World.Add(11.4f, 1, .2f, 3);
            // Remove the track under her paws so every remaining track is behind the wall.
            var place = g.World.Places[0]; g.World.Places.Clear();
            g.World.Places.Add(new WildPlace(place.Id, place.Name, place.Story, place.Hint, place.Position, new V2(12.8f, 1)));
            Tick(g, 3, new PlayerInput { StalkHeld = true }); Check(!Has(g, PracticeSkill.Scent), "Scent leaked through wall");
            g.World.Platforms.RemoveAt(g.World.Platforms.Count - 1);
            g.Step(new PlayerInput { StalkHeld = true }); Check(Has(g, PracticeSkill.Scent), "Visible local scent missing");
            g = new GameSession(new JourneySave { Discoveries = 63 }); g.Player.Reset(new V2(10.8f, 1));
            Tick(g, 5, new PlayerInput { StalkHeld = true }); Check(!Has(g, PracticeSkill.Scent), "Found destination masqueraded as a new scent");
            g = Flat(); Tick(g, 10, new PlayerInput { StalkHeld = true }); Check(!Has(g, PracticeSkill.Scent), "Stalking empty space counted");
        }
        private static void Crawl()
        {
            var g = Flat(); g.World.Add(1, .75f, 10, 1);
            Tick(g, 45, new PlayerInput { Move = 1, RollPressed = true });
            Check(g.Player.LowProfile && g.Player.RollTime == 0, "Did not enter a low passage");
            g.World.Signs.Add(new Sign(g.Player.Position.X, 0, "COIL", "Pounce", PracticeSkill.Pounce));
            Tick(g, 10, new PlayerInput { PouncePressed = true, PounceHeld = true, AttackPressed = true });
            g.Step(new PlayerInput { PounceReleased = true });
            Check(!Has(g, PracticeSkill.Pounce) && !Has(g, PracticeSkill.Claw) && g.Practice.NearbyLesson(g) == null,
                "Low passage suggested or awarded a blocked action");
        }
        private static void Bloom()
        {
            var g = new GameSession(new JourneySave { Waystones = 7 }); Tick(g, 5);
            Check(!Has(g, PracticeSkill.Moonwake), "Restored bridges faked an action");
            g = Flat(); g.World.Blooms.Add(new Moonbloom(1, .6f));
            Tick(g, 12, new PlayerInput { AttackPressed = true });
            Check(Has(g, PracticeSkill.Moonwake | PracticeSkill.Claw), "Connected bloom claw not recorded");
        }
        private static void Traversal()
        {
            var g = Flat(); g.World.Add(-1, 0, 2, .35f, Surface.Spring); g.Player.Reset(new V2(0, 1));
            Tick(g, 30); Check(Has(g, PracticeSkill.Spring), "Actual spring contact missing");
            g = Flat(); g.World.Add(1, 0, .3f, 10); g.Player.Reset(new V2(.55f, 4));
            Tick(g, 2, new PlayerInput { Move = 1 }); g.Step(new PlayerInput { Move = 1, JumpPressed = true });
            Check(Has(g, PracticeSkill.WallKick) && !Has(g, PracticeSkill.Jump), "Wall kick not distinguished from ground jump");
        }
        private static void RetireHints()
        {
            var g = Start(); Check(g.Practice.NearbyLesson(g).Skill == PracticeSkill.Jump, "Opening hint missing");
            g.Step(new PlayerInput { JumpPressed = true }); Tick(g, 200);
            Check(g.NearbySign().Skills == PracticeSkill.Claw, "Completed jump sign kept repeating");
            g.Save.Practiced = PracticeGuide.AllSkills;
            Check(g.NearbySign() == null, "Completed introductory signs remain");
            g.Player.Reset(new V2(23, 1)); Tick(g, 3);
            Check(g.NearbySign() != null && g.NearbySign().Skills == PracticeSkill.None, "Shelter route sign disappeared");
        }
        private static void AnyOrder()
        {
            var g = Start(); g.Player.Reset(new V2(10.8f, 1)); Tick(g, 3);
            Check(g.Practice.NearbyLesson(g).Skill == PracticeSkill.Scent, "First local option missing");
            g.Save.Practiced |= (int)PracticeSkill.Roll;
            Check(g.Practice.NearbyLesson(g).Skill == PracticeSkill.Scent, "Practicing roll first blocked scent");
            g.Save.Practiced = (int)PracticeSkill.Scent;
            Check(g.Practice.NearbyLesson(g).Skill == PracticeSkill.Roll, "Scent did not advance local suggestion");
            g.Save.Practiced |= (int)PracticeSkill.Roll;
            Check(g.Practice.NearbyLesson(g) == null, "Completed combined lesson repeats");
        }
        private static void LocalHints()
        {
            var g = Flat(); g.World.Signs.Add(new Sign(2, 0, "JUMP", "Jump here", PracticeSkill.Jump));
            Check(g.Practice.NearbyLesson(g) != null, "Nearby hint missing");
            g.World.Add(1, 0, .2f, 3); Check(g.NearbySign() == null, "Hint visible through wall");
            g.World.Platforms.RemoveAt(g.World.Platforms.Count - 1); g.Player.Reset(new V2(-5, 0)); Tick(g, 3);
            Check(g.Practice.NearbyLesson(g) == null, "Distant lesson leaked");
            g.Player.Reset(new V2(0, 0)); Tick(g, 3); g.SetPaused(true);
            Check(g.Practice.NearbyLesson(g) == null, "Paused lesson visible");
        }
        private static void Frozen()
        {
            var g = Flat(); g.Step(new PlayerInput { JumpPressed = true }); float notice = g.Practice.NoticeSeconds;
            int bits = g.Save.Practiced; g.SetPaused(true);
            Tick(g, 90, new PlayerInput { DashPressed = true, RollPressed = true, AttackPressed = true });
            Check(g.Save.Practiced == bits && g.Practice.NoticeSeconds == notice, "Pause changed practice");
            g.SetPaused(false); g.Respawn(); Tick(g, 10, new PlayerInput { DashPressed = true });
            Check(g.Save.Practiced == bits && g.Practice.Recent == null && g.Practice.NearbyLesson(g) == null, "Recovery accepted practice");
        }
        private static void Persistence()
        {
            var g = Start(); g.Step(new PlayerInput { JumpPressed = true }); int bits = g.Save.Practiced;
            g.Respawn(); Tick(g, 90); Check(g.Save.Practiced == bits, "Death lost practice");
            g.Save.FurthestBiome = 2; Check(g.TravelTo(2) && g.TravelTo(0) && g.Save.Practiced == bits, "Travel lost practice");
            var restored = new GameSession(new JourneySave { Practiced = bits, Collected = new[] { 3, 7, 1 }, Discoveries = 3, Waystones = 1 });
            Check(restored.Save.Practiced == bits && restored.Save.Collected[1] == 7 && restored.Save.Discoveries == 3
                && restored.Save.Waystones == 1 && restored.Practice.Recent == null, "Reconstruction lost progress or replayed notice");
        }
        private static void Trial()
        {
            var g = Start(); g.Player.Reset(Moontrial.Entrance); Tick(g, 3); Check(g.TryEnterTrial(), "Trial entry failed");
            Tick(g, 3); g.Step(new PlayerInput { JumpPressed = true });
            Check(Has(g, PracticeSkill.Jump) && g.Practice.NearbyLesson(g) == null && g.Save.Discoveries == 0 && g.Save.Waystones == 0, "Trial practice crossed progression boundary");
            Check(g.LeaveTrial() && Has(g, PracticeSkill.Jump) && g.Practice.Recent == null, "Return lost practice or retained wrong-room notice");
        }
        private static bool Same(V2 a, V2 b) { return (a - b).Length < .001f; }
        private static void MainObjective()
        {
            for (int biome = 0; biome < 3; biome++)
                foreach (int waystones in new[] { 0, 7 })
                {
                    var g = new GameSession(new JourneySave { Biome = biome, Waystones = waystones });
                    Check(Same(PracticeGuide.ObjectivePosition(g), g.World.Exit), "Optional trial replaced the main objective");
                    g.Save.Practiced = PracticeGuide.AllSkills;
                    Check(Same(PracticeGuide.ObjectivePosition(g), g.World.Exit), "Practice changed main destination");
                }
        }
        private static void TrialObjective()
        {
            var g = Start(); g.Player.Reset(Moontrial.Entrance); Tick(g, 3); Check(g.TryEnterTrial(), "Trial entry failed");
            Check(Same(PracticeGuide.ObjectivePosition(g), g.World.Trial.NextPosition(g.World)), "Mechanism guidance missing");
            g.World.Blooms[g.World.Trial.BloomIndex].Awakened = true;
            Check(Same(PracticeGuide.ObjectivePosition(g), g.World.Trial.NextPosition(g.World)), "Guidance did not advance with mechanism");
            g.LeaveTrial(); Check(Same(PracticeGuide.ObjectivePosition(g), g.World.Exit), "Outside guidance remained in trial");
        }
        private static void Walk(GameSession g, float x, bool roll = false)
        {
            for (int i = 0; i < 600; i++)
            {
                if (g.Player.Grounded && Math.Abs(g.Player.Position.X - x) < .15f && Math.Abs(g.Player.Velocity.X) < .5f) return;
                g.Step(new PlayerInput { Move = Scalar.Clamp((x - g.Player.Position.X) * 2.5f - g.Player.Velocity.X * .6f, -1, 1), RollPressed = roll && i == 0 });
            }
            throw new Exception("Opening walk stopped at " + g.Player.Position.X + "," + g.Player.Position.Y);
        }
        private static void OpeningRoute()
        {
            var g = Start(); Tick(g, 160, new PlayerInput { JumpPressed = true, JumpHeld = true });
            Walk(g, 3.8f); g.Step(new PlayerInput { Move = 1, AttackPressed = true }); Tick(g, 60);
            Walk(g, 10.8f); Tick(g, 3, new PlayerInput { StalkHeld = true });
            Walk(g, 16.5f, true); Walk(g, 21); g.Step(new PlayerInput { Move = -1, AttackPressed = true }); Tick(g, 70);
            Tick(g, 78, new PlayerInput { PouncePressed = true, PounceHeld = true });
            g.Step(new PlayerInput { PounceReleased = true, AimY = 1 });
            Check(Has(g, PracticeSkill.Jump | PracticeSkill.Claw | PracticeSkill.Scent | PracticeSkill.Roll | PracticeSkill.Pounce | PracticeSkill.Moonwake),
                "Opening route missed practice; bits=" + g.Save.Practiced);
            Check(g.Deaths == 0 && g.DiscoveryCount == 1 && g.Save.Waystones == 0, "Opening practice required a recovery or optional trial");
        }
    }
}
