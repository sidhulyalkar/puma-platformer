using System;
using System.Collections.Generic;
using Wildbound.Core;

namespace Wildbound.Tests
{
    public static class CombatCases
    {
        public static readonly Dictionary<string, Action> All = new Dictionary<string, Action>
        {
            { "Claw windup does not deal early damage", Windup },
            { "A swing hits each target only once", SingleHit },
            { "Buffered claw input links all three combo moves", Combo },
            { "An expired combo restarts with the first claw", ComboReset },
            { "Roll cannot erase committed claw windup", Commitment },
            { "Roll can cancel claw recovery", RecoveryCancel },
            { "Dash claw damages while moving", DashAttack },
            { "Air dash requires a landing before a second use", AirDash },
            { "Dash and claws cannot pass through a solid wall", WallBlocks },
            { "Roll remains crouched beneath a low ceiling", LowCeiling },
            { "Roll has finite dodge frames and a cooldown", DodgeWindow },
            { "Airborne roll cannot provide free movement", NoAirRoll },
            { "Rolling does not protect against terrain hazards", HazardRoll },
            { "Damage grants temporary contact invulnerability", DamageGrace },
            { "A projectile is absorbed by a wall before reaching the puma", ShotWall },
            { "A fast projectile cannot skip across the puma", FastShot },
            { "Terrain behind the puma does not cancel a nearer projectile hit", NearestShot },
            { "A timed roll dodges a low projectile", DodgeShot },
            { "Projectiles expire and cannot grow without bound", ShotExpiry },
            { "Stalking lets the puma approach prey more closely", Stalk },
            { "An alerted hare telegraphs then leaps away", HareHop },
            { "A hunted hare restores a heart and traversal resources", HuntReward },
            { "Bristleback front armor blocks a claw", Armor },
            { "Bristleback rear is vulnerable", RearOpening },
            { "Bristleback recovery exposes its front", RecoveryOpening },
            { "Bristleback charge has a tell and locks its facing", ChargeTell },
            { "Thornling jumps only after its warning", ThornTell },
            { "Moth dive targets are locked during the warning", MothTell },
            { "Spitter fires a bounded three-shot volley", SpitterVolley },
            { "Walls block enemy target acquisition", EnemySight },
            { "Moonbloom awakens a bridge through an actual claw hit", Moonwake },
            { "Moonbridges remain solid after the light pulse fades", LastingBridge },
            { "Dormant moonbridges do not block movement", DormantBridge },
            { "Walls block strikes on moonblooms", BloomWall },
            { "Moonbloom light interrupts a nearby moth", Dazzle },
            { "Falling rake bounces only on a confirmed hit", DownRake },
            { "Pounce rake does not double-count a claw and body impact", PounceRakeImpact },
            { "Rising rake cannot repeatedly launch in the air", RisingRake },
            { "Three hunts fuel one empowered dash claw", Moonfang },
            { "Training post cannot farm hunt rewards", NoDummyReward },
            { "Pause freezes attacks, enemies, blooms and projectiles", PauseAll },
            { "Respawn clears projectiles and attack state", CombatRespawn },
            { "Combat stress play stays finite and collision-safe", CombatStress },
            { "Thin-gap LOS still allows a clear silhouette hit", ThinGapLos },
            { "Fully occluded target remains blocked by multi-point LOS", FullWallBlocksStrike },
            { "Front armor holds when slightly above the front face", ArmorEdgeSlightlyAbove },
            { "Active claw and descending body never double-hit", NoDoubleHitClawAndBody }
        };
        private static void Check(bool condition, string why) { if (!condition) throw new Exception(why); }
        private static GameSession Flat()
        {
            var g = new GameSession(); g.World.Platforms.Clear(); g.World.Enemies.Clear(); g.World.Pickups.Clear();
            g.World.Hazards.Clear(); g.World.Checkpoints.Clear(); g.World.Blooms.Clear();
            g.World.Add(-40, -2, 120, 2); g.Player.Reset(new V2(0, 0)); Tick(g, 125); return g;
        }
        private static void Tick(GameSession g, int n, PlayerInput input = default(PlayerInput))
        {
            for (int i = 0; i < n; i++)
            {
                g.Step(input);
                input.AttackPressed = input.DashPressed = input.RollPressed = input.JumpPressed = input.PouncePressed = input.PounceReleased = false;
            }
        }
        private static Enemy Target(GameSession g, EnemyKind kind = EnemyKind.ClawPost, float x = 1.2f, float y = 0)
        { var e = new Enemy(kind, x, y, 0); g.World.Enemies.Add(e); return e; }
        private static void Swing(GameSession g, float aim = 0) { Tick(g, 50, new PlayerInput { AttackPressed = true, AimY = aim, JumpHeld = true }); }
        private static void Windup()
        {
            var g = Flat(); var e = Target(g); Tick(g, 6, new PlayerInput { AttackPressed = true });
            Check(e.Health == e.MaxHealth, "Damage before claw appeared"); Tick(g, 8); Check(e.Health < e.MaxHealth, "No active strike");
        }
        private static void SingleHit() { var g = Flat(); var e = Target(g); Swing(g); Check(e.Health == e.MaxHealth - 1, "Repeated damage during one sweep"); }
        private static void Combo()
        {
            var g = Flat(); g.Step(new PlayerInput { AttackPressed = true }); Check(g.Combat.Move == ClawMove.Sweep, "First move");
            Tick(g, 30); g.Step(new PlayerInput { AttackPressed = true }); Tick(g, 15);
            Check(g.Combat.Move == ClawMove.Backhand, "Buffered second move");
            Tick(g, 25); g.Step(new PlayerInput { AttackPressed = true }); Tick(g, 20);
            Check(g.Combat.Move == ClawMove.Crescent, "Buffered finisher");
        }
        private static void ComboReset()
        {
            var g = Flat(); Swing(g); Tick(g, 150); g.Step(new PlayerInput { AttackPressed = true });
            Check(g.Combat.Move == ClawMove.Sweep, "Old combo never expired");
        }
        private static void Commitment()
        {
            var g = Flat(); g.Step(new PlayerInput { AttackPressed = true }); g.Step(new PlayerInput { RollPressed = true });
            Check(g.Combat.Busy && g.Player.RollTime == 0, "Roll canceled committed windup");
        }
        private static void RecoveryCancel()
        {
            var g = Flat(); Tick(g, 28, new PlayerInput { AttackPressed = true }); g.Step(new PlayerInput { RollPressed = true });
            Check(!g.Combat.Busy && g.Player.RollTime > 0, "Recovery was not cancelable");
        }
        private static void DashAttack()
        {
            var g = Flat(); var e = Target(g, EnemyKind.Thornling, 2); Tick(g, 15, new PlayerInput { DashPressed = true, Move = 1 });
            Check(!e.Alive && g.Player.Position.X > 1.5f && g.Deaths == 0, "Dash did not connect safely");
        }
        private static void AirDash()
        {
            var g = Flat(); g.Player.Reset(new V2(0, 30)); g.Step(new PlayerInput { DashPressed = true });
            Tick(g, 90); g.Step(new PlayerInput { DashPressed = true });
            Check(g.Player.DashTime == 0 && !g.Player.AirDashReady, "Infinite air dash");
        }
        private static void WallBlocks()
        {
            var g = Flat(); g.World.Add(.8f, 0, .13f, 6); var e = Target(g, EnemyKind.ClawPost, 1.4f);
            Tick(g, 30, new PlayerInput { DashPressed = true, Move = 1 });
            Check(g.Player.Bounds.Right <= .801f && e.Health == e.MaxHealth, "Dash or attack crossed a wall");
        }
        private static void LowCeiling()
        {
            var g = Flat(); g.World.Add(1, .76f, 6, .2f);
            Tick(g, 50, new PlayerInput { RollPressed = true, Move = 1 });
            Check(g.Player.Position.X > 3 && g.Player.LowProfile && g.Player.RollTime == 0, "Roll failed to travel under ceiling");
            Tick(g, 200, new PlayerInput { Move = 1 });
            Check(!g.Player.LowProfile && g.Player.Position.X > 7.5f, "Never stood up after leaving ceiling");
        }
        private static void DodgeWindow()
        {
            var g = Flat(); g.Step(new PlayerInput { RollPressed = true }); Check(!g.Player.Dodging, "No startup vulnerability");
            Tick(g, 10); Check(g.Player.Dodging, "Missing dodge window"); Tick(g, 25); Check(!g.Player.Dodging, "No recovery vulnerability");
            Tick(g, 10); g.Step(new PlayerInput { RollPressed = true }); Check(g.Player.RollTime == 0, "Roll cooldown bypassed");
        }
        private static void NoAirRoll() { var g = Flat(); g.Player.Reset(new V2(0, 5)); g.Step(new PlayerInput { RollPressed = true }); Check(g.Player.RollTime == 0, "Air roll started"); }
        private static void HazardRoll()
        {
            var g = Flat(); g.World.Hazards.Add(new Box(1, 0, .2f, .3f)); Tick(g, 20, new PlayerInput { RollPressed = true });
            Check(g.Deaths == 1, "Roll crossed fatal brambles");
        }
        private static void DamageGrace()
        {
            var g = Flat(); g.Combat.TakeDamage(g.Player, new V2(-1, 0)); g.Combat.TakeDamage(g.Player, new V2(-1, 0));
            Check(g.Combat.Health == 4, "Two hits in a single recovery window");
        }
        private static void ShotWall()
        {
            var g = Flat(); g.World.Add(1, 0, .15f, 3); g.Projectiles.Add(new Projectile(new V2(2, .4f), new V2(-100, 0)));
            g.Step(new PlayerInput(), .05f); Check(g.Combat.Health == 5 && g.Projectiles.Count == 0, "Shot crossed cover");
        }
        private static void FastShot()
        {
            var g = Flat(); g.Projectiles.Add(new Projectile(new V2(3, .4f), new V2(-100, 0)));
            g.Step(new PlayerInput(), .05f); Check(g.Combat.Health == 4, "Fast projectile skipped player");
        }
        private static void NearestShot()
        {
            var g = Flat(); g.World.Add(-1.5f, 0, .2f, 3);
            g.Projectiles.Add(new Projectile(new V2(3, .4f), new V2(-100, 0)));
            g.Step(new PlayerInput(), .05f); Check(g.Combat.Health == 4, "Distant wall incorrectly shielded player");
        }
        private static void DodgeShot()
        {
            var g = Flat(); Tick(g, 12, new PlayerInput { RollPressed = true });
            g.Projectiles.Add(new Projectile(g.Player.Position + new V2(1, .3f), new V2(-30, 0))); Tick(g, 5);
            Check(g.Combat.Health == 5 && g.Projectiles.Count == 0, "Roll did not dodge the shot");
        }
        private static void ShotExpiry()
        {
            var g = Flat(); g.Projectiles.Add(new Projectile(new V2(0, 20), new V2(0, 0))); Tick(g, 400);
            Check(g.Projectiles.Count == 0, "Expired projectile remained");
            var e = Target(g, EnemyKind.ReedSpitter, 7);
            var shots = new List<Projectile>();
            for (int i = 0; i < 6000; i++) e.Step(g.World, g.Player, shots, GameSession.StepSeconds);
            Check(shots.Count <= 24, "Projectile cap exceeded");
        }
        private static void Stalk()
        {
            var loud = Flat(); var quiet = Flat(); var a = Target(loud, EnemyKind.MossHare, 3); var b = Target(quiet, EnemyKind.MossHare, 3);
            loud.Step(new PlayerInput()); quiet.Step(new PlayerInput { StalkHeld = true });
            Check(a.Phase == EnemyPhase.Tell && b.Phase == EnemyPhase.Idle, "Stalking did not reduce prey detection");
        }
        private static void HareHop()
        {
            var g = Flat(); var e = new Enemy(EnemyKind.MossHare, 3, 0, 3); g.World.Enemies.Add(e);
            Tick(g, 12); Check(e.Phase == EnemyPhase.Tell && e.Position.Y < .02f, "Hare jumped before tell");
            Tick(g, 20); Check(e.Position.X > 3 && e.Position.Y > .2f, "Hare did not leap away");
        }
        private static void HuntReward()
        {
            var g = Flat(); g.Combat.TakeDamage(g.Player, new V2(1, 0)); g.Player.Reset(new V2(0, 0)); Tick(g, 2);
            var e = Target(g, EnemyKind.MossHare, 1.25f); Swing(g);
            Check(!e.Alive && g.Combat.Hunts == 1 && g.Combat.Health == 5 && g.Player.PounceReady, "Hunt rewards missing");
        }
        private static void Armor() { var g = Flat(); var e = Target(g, EnemyKind.Bristleback); Swing(g); Check(e.Health == 4, "Front armor ignored"); }
        private static void RearOpening()
        {
            var g = Flat(); var e = Target(g, EnemyKind.Bristleback); e.Facing = 1; e.Phase = EnemyPhase.Tell;
            Swing(g); Check(e.Health == 3, "Back armor wrongly blocked");
        }
        private static void RecoveryOpening()
        {
            var g = Flat(); var e = Target(g, EnemyKind.Bristleback); e.Phase = EnemyPhase.Recover;
            Swing(g); Check(e.Health == 3, "Recovery opening missing");
        }
        private static void ChargeTell()
        {
            var g = Flat(); var e = Target(g, EnemyKind.Bristleback, 5); Tick(g, 60);
            Check(e.Phase == EnemyPhase.Tell && e.Facing == -1, "Charge began too early");
            g.Player.Reset(new V2(9, 0)); Tick(g, 35);
            Check(e.Phase == EnemyPhase.Active && e.Facing == -1 && e.Velocity.X < 0, "Charge retargeted during tell");
        }
        private static void ThornTell()
        {
            var g = Flat(); var e = Target(g, EnemyKind.Thornling, 3.5f); Tick(g, 30);
            Check(e.Phase == EnemyPhase.Tell && e.Position.Y < .02f, "Thornling jumped without tell");
            Tick(g, 40); Check(e.Phase == EnemyPhase.Active && e.Position.Y > 0, "Thornling never jumped");
        }
        private static void MothTell()
        {
            var g = Flat(); var e = Target(g, EnemyKind.LanternMoth, 4, 4); Tick(g, 1); V2 locked = e.LockedTarget;
            g.Player.Reset(new V2(9, 0)); Tick(g, 50); Check(e.Phase == EnemyPhase.Tell, "Dive started too soon");
            Tick(g, 50); Check(e.LockedTarget.X == locked.X && e.Velocity.X < 0, "Dive did not preserve target");
        }
        private static void SpitterVolley()
        {
            var g = Flat(); var e = new Enemy(EnemyKind.ReedSpitter, 6, 0); var shots = new List<Projectile>();
            for (int i = 0; i < 165; i++) e.Step(g.World, g.Player, shots, GameSession.StepSeconds);
            Check(shots.Count == 3 && e.Phase == EnemyPhase.Active, "Volley count was not three");
            for (int i = 0; i < 40; i++) e.Step(g.World, g.Player, shots, GameSession.StepSeconds);
            Check(shots.Count == 3 && e.Phase == EnemyPhase.Recover, "Spitter did not recover");
        }
        private static void EnemySight()
        {
            var g = Flat(); g.World.Add(2, 0, .2f, 6); var e = Target(g, EnemyKind.ReedSpitter, 4); Tick(g, 200);
            Check(e.Phase == EnemyPhase.Idle && g.Projectiles.Count == 0, "Enemy acquired player through wall");
        }
        private static GameSession BloomScene()
        {
            var g = Flat(); g.World.Blooms.Add(new Moonbloom(1.2f, .6f));
            g.World.Platforms.Add(new Platform(new Box(3, 1, 3, .35f), Surface.Moonbridge) { LightSource = 0, Enabled = false }); return g;
        }
        private static void Moonwake()
        {
            var g = BloomScene(); Swing(g); Check(g.World.Blooms[0].Awakened && g.World.Platforms[1].Enabled, "Claw did not awaken bridge");
        }
        private static void LastingBridge()
        {
            var g = BloomScene(); Swing(g); Tick(g, 800); g.Player.Reset(new V2(4.5f, 3)); Tick(g, 120);
            Check(g.World.Blooms[0].GlowTime == 0 && g.Player.Grounded && g.Player.Position.Y > 1, "Bridge faded under player");
        }
        private static void DormantBridge()
        {
            var g = BloomScene(); g.Player.Reset(new V2(4.5f, 3)); Tick(g, 120);
            Check(g.Player.Grounded && g.Player.Position.Y < .01f, "Dormant bridge was solid");
        }
        private static void BloomWall()
        {
            var g = BloomScene(); g.World.Add(.6f, 0, .15f, 4); Swing(g);
            Check(!g.World.Blooms[0].Awakened, "Bloom activated through wall");
        }
        private static void Dazzle()
        {
            var g = BloomScene(); var e = Target(g, EnemyKind.LanternMoth, 3, 3); Swing(g);
            Check(e.Phase == EnemyPhase.Stunned && e.PhaseTime < 0, "Moonlight failed to interrupt moth");
            for (int biome = 1; biome < 3; biome++)
            {
                var authored = new GameSession(new JourneySave { Biome = biome });
                authored.Player.Reset(new V2(46.5f, 1)); Swing(authored);
                bool dazzled = false;
                foreach (var enemy in authored.World.Enemies)
                    dazzled |= enemy.Kind == EnemyKind.LanternMoth && enemy.Phase == EnemyPhase.Stunned;
                Check(authored.World.Blooms[1].Awakened && dazzled, "Authored bloom encounter cannot dazzle its moth in biome " + biome);
            }
        }
        private static void DownRake()
        {
            var hit = Flat(); var miss = Flat(); Target(hit, EnemyKind.Thornling, 0);
            hit.Player.Reset(new V2(0, 2.5f)); miss.Player.Reset(new V2(0, 2.5f));
            hit.Player.PounceReady = miss.Player.PounceReady = false;
            bool connected = false;
            for (int i = 0; i < 30; i++)
            {
                var input = new PlayerInput { AttackPressed = i == 0, AimY = -1 };
                hit.Step(input); miss.Step(input);
                if ((hit.Events & GameEvent.Hit) == 0) continue;
                connected = true;
                Check(hit.Player.Velocity.Y > 0 && hit.Player.PounceReady && miss.Player.Velocity.Y < 0, "Rake bounce did not require a hit");
                break;
            }
            Check(connected, "Falling rake never connected");
        }
        private static void PounceRakeImpact()
        {
            var g = Flat(); var e = Target(g, EnemyKind.ReedSpitter, 2);
            g.Player.PounceTime = .3f; g.Player.Velocity = new V2(20, 0);
            Tick(g, 9, new PlayerInput { AttackPressed = true });
            Check(e.Health == 1, "Two-damage pounce rake stacked a second body impact");
        }
        private static void RisingRake()
        {
            var g = Flat(); Tick(g, 20, new PlayerInput { AttackPressed = true, AimY = 1, JumpHeld = true });
            Check(g.Player.Position.Y > .1f, "Ground rising rake never lifted");
            g.Player.Reset(new V2(0, 10)); Tick(g, 100, new PlayerInput { AttackPressed = true, AimY = 1 });
            Check(g.Player.Position.Y < 10, "Rising attack created infinite lift");
        }
        private static void Moonfang()
        {
            var g = Flat();
            for (int i = 0; i < 3; i++) { Target(g, EnemyKind.MossHare, 1.25f); Swing(g); Tick(g, 30); }
            Check(g.Combat.Instinct == 3, "Hunts did not fill instinct");
            g.Step(new PlayerInput { DashPressed = true }); Check(g.Combat.Empowered && g.Combat.Instinct == 0, "Empowered dash did not consume instinct");
        }
        private static void NoDummyReward()
        {
            var g = Flat(); Target(g);
            for (int i = 0; i < 8; i++) { Swing(g); Tick(g, 35); }
            Check(g.Combat.Instinct == 0 && g.Combat.Hunts == 0 && g.Combat.Defeats == 0, "Dummy farmed rewards");
        }
        private static void PauseAll()
        {
            var g = BloomScene(); var e = Target(g, EnemyKind.LanternMoth, 6, 3); g.Step(new PlayerInput { AttackPressed = true });
            g.Projectiles.Add(new Projectile(new V2(10, 10), new V2(1, 0))); g.SetPaused(true);
            float age = g.Combat.Age, clock = e.Clock; Tick(g, 100);
            Check(g.Combat.Age == age && e.Clock == clock && g.Projectiles[0].Position.X == 10, "Paused world advanced");
        }
        private static void CombatRespawn()
        {
            var g = Flat(); g.Step(new PlayerInput { DashPressed = true }); g.Projectiles.Add(new Projectile(new V2(1, 1), new V2()));
            g.Respawn(); Check(!g.Combat.Busy && g.Player.DashTime == 0 && g.Projectiles.Count == 0 && g.Combat.Health == 5, "Combat state leaked through respawn");
        }
        private static void CombatStress()
        {
            for (int biome = 0; biome < 3; biome++)
            {
                var g = new GameSession(new JourneySave { Biome = biome }); var rng = new Random(621 + biome);
                for (int i = 0; i < 15000; i++)
                {
                    var input = new PlayerInput { Move = rng.Next(3) - 1, AimY = rng.Next(3) - 1, JumpHeld = i % 60 < 40,
                        JumpPressed = i % 60 == 0, AttackPressed = i % 23 == 0, DashPressed = i % 120 == 0,
                        RollPressed = i % 99 == 0, StalkHeld = i % 180 < 50, PouncePressed = i % 200 == 0,
                        PounceHeld = i % 200 < 80, PounceReleased = i % 200 == 80 };
                    g.Step(input);
                    Check(Scalar.Finite(g.Player.Position.X) && Scalar.Finite(g.Player.Position.Y), "Nonfinite combat position");
                    Check(g.Combat.Health > 0 && g.Combat.Health <= 5 && g.Projectiles.Count <= 24, "Combat state outside bounds");
                    foreach (var platform in g.World.Platforms)
                        Check(!platform.Enabled || !g.Player.Bounds.Overlaps(new Box(platform.Bounds.X + .002f, platform.Bounds.Y + .002f, platform.Bounds.W - .004f, platform.Bounds.H - .004f)), "Player embedded in terrain");
                }
            }
        }

        // --- AABB precision cases (docs/COMBAT_PRECISION.md) ---

        private static void ThinGapLos()
        {
            // Thin pillar offset so center-to-center is blocked but upper/lower samples clear.
            var g = Flat();
            g.World.Add(.55f, .35f, .12f, .35f); // small mid-height blocker
            var e = Target(g, EnemyKind.Thornling, 1.35f);
            Swing(g);
            Check(e.Health < e.MaxHealth, "Multi-point LOS should still connect past a thin mid gap");
        }

        private static void FullWallBlocksStrike()
        {
            var g = Flat();
            g.World.Add(.7f, 0, .2f, 4); // full-height wall
            var e = Target(g, EnemyKind.ClawPost, 1.5f);
            Swing(g);
            Check(e.Health == e.MaxHealth, "Fully occluded target must remain blocked");
        }

        private static void ArmorEdgeSlightlyAbove()
        {
            var g = Flat();
            var e = Target(g, EnemyKind.Bristleback, 1.3f);
            // Place puma slightly above the front face mid-line while still overlapping horizontally.
            g.Player.Reset(new V2(0, .35f));
            Tick(g, 2);
            Swing(g);
            Check(e.Health == e.MaxHealth, "Front armor must hold when slightly above the front face");
        }

        private static void NoDoubleHitClawAndBody()
        {
            var g = Flat();
            var e = Target(g, EnemyKind.ReedSpitter, .9f);
            // Start a descending claw that will also overlap as a body hit.
            g.Player.Reset(new V2(0, 1.8f));
            g.Player.Velocity = new V2(4, -8);
            int startHealth = e.Health;
            Tick(g, 20, new PlayerInput { AttackPressed = true, AimY = -1 });
            // At most the claw damage (1 for down rake) should apply once; never claw+body stack.
            Check(e.Health >= startHealth - 1, "Claw and body hit stacked on the same enemy");
            Check(e.Health < startHealth, "Expected a single confirmed hit");
        }
    }
}
