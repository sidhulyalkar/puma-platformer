using System;
using System.Collections.Generic;

namespace Wildbound.Core
{
    public enum ClawMove { None, Sweep, Backhand, Crescent, RisingRake, DownRake, PounceRake, DashClaw }

    public struct ClawTiming
    {
        public float Windup, Active, Recovery, Reach;
        public int Damage;
        public float Total { get { return Windup + Active + Recovery; } }
        public ClawTiming(float windup, float active, float recovery, float reach, int damage)
        { Windup = windup; Active = active; Recovery = recovery; Reach = reach; Damage = damage; }
    }

    public sealed class PumaCombat
    {
        public const int MaxHealth = 5;
        public int Health { get; private set; } = MaxHealth;
        public int Instinct { get; private set; }
        public int Hunts { get; private set; }
        public int Defeats { get; private set; }
        public ClawMove Move { get; private set; }
        public float Age { get; private set; }
        public float Invulnerable { get; private set; }
        public int Facing { get; private set; } = 1;
        public int Sequence { get; private set; }
        public bool Empowered { get; private set; }
        public bool Ambush { get; private set; }
        public V2 LastImpact { get; private set; }
        private int combo;
        private float comboWindow, queuedAttack;
        private bool motionApplied, startedGrounded;
        private readonly HashSet<int> hitEnemies = new HashSet<int>();
        private readonly HashSet<int> bodyHits = new HashSet<int>();

        public bool Busy { get { return Move != ClawMove.None; } }
        public ClawTiming Timing { get { return ForMove(Move); } }
        public bool Active { get { return Busy && Age >= Timing.Windup && Age < Timing.Windup + Timing.Active; } }
        public bool Recovering { get { return Busy && Age >= Timing.Windup + Timing.Active; } }
        public float Progress { get { return Busy ? Scalar.Clamp(Age / Timing.Total, 0, 1) : 0; } }
        public string MoveName
        {
            get { return new[] { "READY", "FIRST CLAW", "RETURN CLAW", "CRESCENT FINISH", "RISING RAKE", "FALLING RAKE", "POUNCE RAKE", Empowered ? "MOONFANG RUSH" : "DASH CLAW" }[(int)Move]; }
        }

        public static ClawTiming ForMove(ClawMove move)
        {
            switch (move)
            {
                case ClawMove.Sweep: return new ClawTiming(.07f, .12f, .16f, 1.6f, 1);
                case ClawMove.Backhand: return new ClawTiming(.06f, .13f, .17f, 1.8f, 1);
                case ClawMove.Crescent: return new ClawTiming(.12f, .18f, .23f, 2.1f, 2);
                case ClawMove.RisingRake: return new ClawTiming(.09f, .19f, .22f, 1.4f, 1);
                case ClawMove.DownRake: return new ClawTiming(.09f, .22f, .21f, 1.4f, 1);
                case ClawMove.PounceRake: return new ClawTiming(.025f, .22f, .2f, 1.9f, 2);
                case ClawMove.DashClaw: return new ClawTiming(0, .2f, .22f, 1.55f, 2);
                default: return new ClawTiming(0, 0, 0, 0, 0);
            }
        }

        public void ResetForRespawn()
        {
            Health = MaxHealth; Invulnerable = 1; Instinct = 0;
            Cancel(); bodyHits.Clear();
        }
        public void Heal() { Health = MaxHealth; }
        public void CancelQueue() { queuedAttack = 0; }
        private void Cancel() { Move = ClawMove.None; Age = queuedAttack = comboWindow = 0; combo = 0; hitEnemies.Clear(); }

        public GameEvent Prepare(ref PlayerInput input, PumaMotor player, float dt)
        {
            Invulnerable = Math.Max(0, Invulnerable - dt);
            queuedAttack = Math.Max(0, queuedAttack - dt);
            comboWindow = Math.Max(0, comboWindow - dt);
            if (comboWindow <= 0) combo = 0;
            if (Busy) { Age += dt; if (Age >= Timing.Total) Move = ClawMove.None; }
            if (input.AttackPressed) queuedAttack = .2f;
            if (Busy && !Recovering) input.RollPressed = input.DashPressed = false;
            else if ((input.RollPressed && player.Grounded && player.RollCooldown <= 0)
                || (input.DashPressed && player.CanDash && !player.LowProfile)
                || (Recovering && input.PouncePressed && player.PounceReady && !player.LowProfile)) Cancel();

            if (input.RollPressed || player.RollTime > 0 || player.LowProfile)
            { queuedAttack = 0; input.DashPressed = false; return GameEvent.None; }
            if (input.DashPressed && player.CanDash && !Busy)
            {
                Start(ClawMove.DashClaw, input, player);
                return GameEvent.DashClaw;
            }
            if (queuedAttack > 0 && !Busy && player.DashTime <= 0)
            {
                ClawMove move;
                if (!player.Grounded && input.AimY < -.3f) move = ClawMove.DownRake;
                else if (player.PounceTime > 0) move = ClawMove.PounceRake;
                else if (input.AimY > .3f) move = ClawMove.RisingRake;
                else
                {
                    move = combo == 0 ? ClawMove.Sweep : combo == 1 ? ClawMove.Backhand : ClawMove.Crescent;
                    combo = (combo + 1) % 3;
                }
                Start(move, input, player);
                input.PouncePressed = input.PounceReleased = false;
                return GameEvent.Claw;
            }
            if (Busy)
            {
                input.PouncePressed = input.PounceReleased = false;
                input.DashPressed = false;
            }
            return GameEvent.None;
        }

        private void Start(ClawMove move, PlayerInput input, PumaMotor player)
        {
            Move = move; Age = 0; Sequence++; hitEnemies.Clear(); queuedAttack = 0;
            Facing = Math.Abs(input.Move) > .1f ? (input.Move > 0 ? 1 : -1) : player.Facing;
            startedGrounded = player.Grounded; motionApplied = false;
            Ambush = input.StalkHeld && player.Grounded;
            Empowered = move == ClawMove.DashClaw && Instinct >= 3;
            if (Empowered) Instinct -= 3;
            comboWindow = Timing.Total + .6f; player.CancelInput();
        }

        public void ApplyMotion(PumaMotor player)
        {
            if (!Busy) return;
            player.Facing = Facing;
            if (motionApplied || Age < Timing.Windup) return;
            motionApplied = true;
            if (Move == ClawMove.RisingRake && startedGrounded) player.Launch(player.Velocity.X * .65f, 11);
            if (Move == ClawMove.DownRake) { player.PounceTime = 0; player.Velocity.Y = -18; }
        }

        public Box StrikeBox(PumaMotor player)
        {
            float reach = Timing.Reach + (Empowered ? .45f : 0);
            if (Move == ClawMove.RisingRake) return new Box(player.Position.X - .7f, player.Position.Y + .5f, 1.4f, 2);
            if (Move == ClawMove.DownRake) return new Box(player.Position.X - .8f, player.Position.Y - 1, 1.6f, 1.4f);
            return new Box(player.Position.X + (Facing > 0 ? -.2f : -reach), player.Position.Y - .05f, reach + .2f, 1.65f);
        }

        public void OnMovement(GameEvent events)
        {
            if ((events & (GameEvent.Pounce | GameEvent.Jump | GameEvent.WallKick | GameEvent.Spring | GameEvent.DashClaw)) != 0)
                bodyHits.Clear();
        }

        public GameEvent ResolveStrike(WorldDefinition world, PumaMotor player)
        {
            if (!Active) return GameEvent.None;
            GameEvent events = GameEvent.None;
            Box strike = StrikeBox(player);
            for (int i = 0; i < world.Enemies.Count; i++)
            {
                var enemy = world.Enemies[i];
                if (!enemy.Alive || hitEnemies.Contains(i) || !strike.Overlaps(enemy.Bounds)
                    || !WorldCollision.ClearLine(world, player.Bounds.Center, enemy.Bounds.Center)) continue;
                hitEnemies.Add(i);
                bool ambush = Ambush && enemy.Phase == EnemyPhase.Idle;
                events |= Strike(enemy, player, Timing.Damage + (Empowered ? 1 : 0) + (ambush ? 1 : 0), Move == ClawMove.DownRake, ambush);
            }
            for (int i = 0; i < world.Blooms.Count; i++)
            {
                var bloom = world.Blooms[i];
                if (bloom.LastAttack == Sequence || bloom.GlowTime > 5 || !strike.Overlaps(bloom.Bounds)
                    || !WorldCollision.ClearLine(world, player.Bounds.Center, bloom.Position)) continue;
                bloom.LastAttack = Sequence; bloom.Awakened = true; bloom.GlowTime = 6;
                LastImpact = bloom.Position; events |= GameEvent.Bloom;
                foreach (var platform in world.Platforms) if (platform.LightSource == i) platform.Enabled = true;
                foreach (var enemy in world.Enemies)
                    if (enemy.Kind == EnemyKind.LanternMoth && (enemy.Position - bloom.Position).Length < 5
                        && WorldCollision.ClearLine(world, bloom.Position, enemy.Bounds.Center)) enemy.Stun(1.2f);
            }
            if (world.Trial != null)
            {
                var mechanism = world.Trial.ResolveStrike(world, player, this);
                events |= mechanism;
                if (mechanism != GameEvent.None) LastImpact = world.Trial.LastImpact;
            }
            return events;
        }

        public GameEvent ResolveBodyHit(WorldDefinition world, PumaMotor player, float oldFeet, float dy)
        {
            GameEvent events = GameEvent.None;
            for (int i = 0; i < world.Enemies.Count; i++)
            {
                var enemy = world.Enemies[i];
                if (!enemy.Alive || enemy.Kind == EnemyKind.ClawPost || !player.Bounds.Overlaps(enemy.Bounds)) continue;
                // A connected rake already owns this impact; do not stack a body hit on it.
                if (Active && hitEnemies.Contains(i)) continue;
                bool descending = dy < 0 && oldFeet >= enemy.Bounds.Top - .13f;
                if ((player.PounceTime > 0 || descending) && !bodyHits.Contains(i))
                {
                    bodyHits.Add(i);
                    events |= Strike(enemy, player, 1, descending, false);
                    if (descending && (events & GameEvent.Hit) != 0) events |= GameEvent.Stomp;
                }
            }
            return events;
        }

        private GameEvent Strike(Enemy enemy, PumaMotor player, int damage, bool downward, bool ambush)
        {
            LastImpact = enemy.Bounds.Center;
            bool front = (player.Position.X - enemy.Position.X) * enemy.Facing >= 0;
            if (enemy.Armored && front && !downward && player.Position.Y < enemy.Bounds.Top)
            { enemy.HitFlash = .1f; return GameEvent.Block; }
            enemy.ReceiveHit(damage, Facing);
            GameEvent events = GameEvent.Hit | (ambush ? GameEvent.Ambush : GameEvent.None);
            if (downward && !player.Grounded)
            {
                player.Launch(player.Velocity.X, 12);
                player.PounceReady = player.AirDashReady = true; player.PounceTime = 0;
            }
            if (!enemy.Alive && enemy.Kind != EnemyKind.ClawPost)
            {
                Defeats++; Instinct = Math.Min(3, Instinct + 1); events |= GameEvent.Defeat;
                player.PounceReady = player.AirDashReady = true;
                if (enemy.Kind == EnemyKind.MossHare)
                { Hunts++; Health = Math.Min(MaxHealth, Health + 1); events |= GameEvent.Hunt; }
            }
            return events;
        }

        public GameEvent TakeDamage(PumaMotor player, V2 source)
        {
            if (Invulnerable > 0 || player.Dodging) return GameEvent.None;
            Health = Math.Max(0, Health - 1); Invulnerable = .85f; Instinct = 0; Cancel();
            player.Interrupt(player.Position.X >= source.X ? 1 : -1);
            LastImpact = player.Bounds.Center;
            return GameEvent.Hurt;
        }
    }
}
