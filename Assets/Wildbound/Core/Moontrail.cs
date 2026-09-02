using System;

namespace Wildbound.Core
{
    public sealed class BalancePerch
    {
        public const float SettleSeconds = 1.6f, CenterRadius = .65f;
        public readonly int PlatformIndex, BridgeIndex;
        public float Charge;
        public bool Attuned;
        public BalancePerch(int platform, int bridge) { PlatformIndex = platform; BridgeIndex = bridge; }
        public float Wind(float time) { return 1.15f + .35f * (float)Math.Sin(time * 1.4f); }
        public GameEvent Step(WorldDefinition world, PumaMotor player, PumaCombat combat, float time, float dt)
        {
            if (Attuned) return GameEvent.None;
            var platform = world.Platforms[PlatformIndex];
            bool centered = player.Grounded && player.GroundIndex == PlatformIndex && player.Stalking
                && !player.Charging && !combat.Busy && Math.Abs(player.Position.X - platform.Bounds.Center.X) < CenterRadius
                && Math.Abs(player.Velocity.X + Wind(time)) < 1.6f;
            Charge = Scalar.Clamp(Charge + (centered ? dt : -dt * 1.7f), 0, SettleSeconds);
            if (Charge < SettleSeconds) return GameEvent.None;
            Attuned = true; world.Platforms[BridgeIndex].Enabled = true;
            return GameEvent.Balance;
        }
    }

    public sealed class Moonbell
    {
        public readonly V2 Position;
        public bool Rung;
        public int LastAttack = -1;
        public float Cooldown;
        public Moonbell(float x, float y) { Position = new V2(x, y); }
        public Box Bounds { get { return new Box(Position.X - .5f, Position.Y - .3f, 1, .6f); } }
    }

    public sealed class RootGate
    {
        public readonly int PlatformIndex;
        public bool Broken;
        public RootGate(int platform) { PlatformIndex = platform; }
    }

    /// <summary>A small authored room whose mechanisms, rather than input checklists, define its goals.</summary>
    public sealed class Moontrial
    {
        public static readonly V2 Entrance = new V2(3, 1);
        private static readonly string[] titles = { "WEIGHT OF MOONLIGHT", "ECHO UNDER STONE", "CROWN OF THE NIGHT" };
        public static string Title(int biome) { return titles[biome]; }
        public readonly int Biome;
        public int BloomIndex = -1;
        public BalancePerch Balance;
        public Moonbell Bell;
        public RootGate Gate;
        public V2 LastImpact { get; private set; }
        public readonly V2 Sanctuary = new V2(48, 1);
        public Moontrial(int biome) { Biome = biome; }
        public int GoalCount { get { return (BloomIndex >= 0 ? 1 : 0) + (Balance != null ? 1 : 0) + (Bell != null ? 1 : 0) + (Gate != null ? 1 : 0); } }
        public int FinishedGoals(WorldDefinition world)
        {
            return (BloomIndex >= 0 && world.Blooms[BloomIndex].Awakened ? 1 : 0)
                + (Balance != null && Balance.Attuned ? 1 : 0) + (Bell != null && Bell.Rung ? 1 : 0) + (Gate != null && Gate.Broken ? 1 : 0);
        }
        public bool Ready(WorldDefinition world) { return FinishedGoals(world) == GoalCount; }
        public string NextGoal(WorldDefinition world)
        {
            if (BloomIndex >= 0 && !world.Blooms[BloomIndex].Awakened) return "Claw the moonbloom to reveal the crossing.";
            if (Balance != null && !Balance.Attuned) return "Hold Q / LT on the perch; steer against the wind inside its center ring.";
            if (Bell != null && !Bell.Rung) return "Get above the moonbell. Use a falling rake (down + claw) to rebound.";
            if (Gate != null && !Gate.Broken) return "Dash-claw (K / RT) through the braided root gate.";
            return "Reach the far waystone and press E / Y to restore it.";
        }
        public V2 NextPosition(WorldDefinition world)
        {
            if (BloomIndex >= 0 && !world.Blooms[BloomIndex].Awakened) return world.Blooms[BloomIndex].Position;
            if (Balance != null && !Balance.Attuned) return world.Platforms[Balance.PlatformIndex].Bounds.Center;
            if (Bell != null && !Bell.Rung) return Bell.Position;
            if (Gate != null && !Gate.Broken) return world.Platforms[Gate.PlatformIndex].Bounds.Center;
            return Sanctuary;
        }
        public float WindDrift(PumaMotor player, float time)
        { return Balance != null && !Balance.Attuned && player.Grounded && player.GroundIndex == Balance.PlatformIndex ? Balance.Wind(time) : 0; }
        public void Advance(float dt) { if (Bell != null) Bell.Cooldown = Math.Max(0, Bell.Cooldown - dt); }
        public GameEvent ResolveStrike(WorldDefinition world, PumaMotor player, PumaCombat combat)
        {
            if (!combat.Active) return GameEvent.None;
            var strike = combat.StrikeBox(player);
            if (Bell != null && combat.Move == ClawMove.DownRake && !player.Grounded && player.Position.Y > Bell.Position.Y
                && Bell.Cooldown <= 0 && Bell.LastAttack != combat.Sequence && strike.Overlaps(Bell.Bounds)
                && WorldCollision.ClearLine(world, player.Bounds.Center, Bell.Position))
            {
                Bell.Rung = true; Bell.LastAttack = combat.Sequence; Bell.Cooldown = .4f;
                LastImpact = Bell.Position;
                player.Launch(player.Velocity.X, 14); player.PounceTime = 0;
                player.PounceReady = player.AirDashReady = true;
                return GameEvent.Moonbell;
            }
            if (Gate != null && !Gate.Broken && combat.Move == ClawMove.DashClaw)
            {
                var platform = world.Platforms[Gate.PlatformIndex];
                V2 from = player.Bounds.Center;
                V2 surface = new V2(Scalar.Clamp(from.X, platform.Bounds.X, platform.Bounds.Right), Scalar.Clamp(from.Y, platform.Bounds.Y, platform.Bounds.Top));
                if (strike.Overlaps(platform.Bounds) && WorldCollision.ClearLine(world, from, surface, Gate.PlatformIndex))
                {
                    Gate.Broken = true; platform.Enabled = false; LastImpact = surface;
                    return GameEvent.Breach;
                }
            }
            return GameEvent.None;
        }

        public static WorldDefinition Create(int biome)
        {
            if (biome < 0 || biome > 2) throw new ArgumentOutOfRangeException("biome");
            var w = new WorldDefinition { Biome = biome, Name = Title(biome), Subtitle = "Restore the waystone. Every lit mechanism survives a fall.",
                Memory = "A small light can change a whole trail.", CameraMaxX = 46, Trial = new Moontrial(biome) };
            var trial = w.Trial;
            w.Add(-5, -3, 17, 4); w.Add(12, -3, 26, 1); w.Add(38, -3, 16, 4);
            w.Add(-6, -3, 1, 25, Surface.Stone); w.Add(54, -3, 1, 25, Surface.Stone);
            w.Signs.Add(new Sign(3, 1, "A WAYSTONE WORTH REACHING", "Follow the objective above. E / Y at this crescent returns to your trail. TAB shows your journal."));
            if (biome < 2)
            {
                trial.BloomIndex = 0; w.Blooms.Add(new Moonbloom(8, 1.6f));
                w.Platforms.Add(new Platform(new Box(12, 2.5f, 5, .4f), Surface.Moonbridge) { LightSource = 0, Enabled = false });
            }
            else w.Add(12, 2.5f, 5, .4f);
            if (biome != 1)
            {
                int perch = w.Platforms.Count;
                w.Add(20, biome == 0 ? 4 : 5, 5, .5f, Surface.Balance, biome == 0 ? 1.3f : 1.8f);
                int bridge = w.Platforms.Count;
                w.Platforms.Add(new Platform(new Box(27, biome == 0 ? 6 : 7, 4, .4f), Surface.Moonbridge) { Enabled = false });
                trial.Balance = new BalancePerch(perch, bridge);
                w.Signs.Add(new Sign(22.5f, biome == 0 ? 4.5f : 5.5f, "SOFT PAWS, STEADY LIGHT", "Hold Q / LT and make small steering corrections inside the ring. The wind ribbon shows the push."));
            }
            if (biome == 0)
            {
                w.Add(33, 3.5f, 3, .5f);
                w.Enemies.Add(new Enemy(EnemyKind.MossHare, 6, 1, 1));
                w.Enemies.Add(new Enemy(EnemyKind.Thornling, 43, 1, 1.5f));
            }
            else if (biome == 1)
            {
                w.Add(18, -2, 1, 7, Surface.Stone); w.Add(23, -2, 1, 10, Surface.Stone);
                w.Add(19, 5.5f, 3, .4f); w.Add(29, 6.5f, 5, .5f);
                trial.Bell = new Moonbell(26, 6.8f);
                w.Enemies.Add(new Enemy(EnemyKind.LanternMoth, 8.5f, 5, 1));
                w.Enemies.Add(new Enemy(EnemyKind.ReedSpitter, 44, 1));
                // The low route offers cover and a roll-through; the upper route avoids the volley.
                w.Add(36, -2, 2, 3); // A standing approach is needed before the low clearance begins.
                w.Add(38, 1.7f, 3.2f, 1.2f, Surface.Stone);
                w.Signs.Add(new Sign(26, 8.5f, "CLAW. RING. RISE.", "Down + claw over the bell gives a fresh pounce and air dash. Use the rebound to cross."));
            }
            else
            {
                trial.Bell = new Moonbell(30, 8.8f); w.Add(32.5f, 9.5f, 3, .5f);
                trial.Gate = new RootGate(w.Platforms.Count);
                w.Add(36, -2, .6f, 13, Surface.RootGate);
                w.Enemies.Add(new Enemy(EnemyKind.Bristleback, 44, 1, 1.5f));
                w.Signs.Add(new Sign(35, 1, "COMMIT TO THE OPENING", "K / RT shatters braided roots. The guardian beyond still has armor: bait the charge or cross above him."));
            }
            w.Signs.Add(new Sign(48, 1, "RESTORE THE WAYSTONE", "Light the room's mechanisms, then press E / Y here. The reward is lasting light bridges back in this region."));
            return w;
        }
    }
}
