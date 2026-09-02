using System;
using System.Collections.Generic;

namespace Wildbound.Core
{
    public enum EnemyKind { ClawPost, MossHare, Thornling, Bristleback, ReedSpitter, LanternMoth }
    public enum EnemyPhase { Idle, Tell, Active, Recover, Stunned, Defeated }

    public sealed class Projectile
    {
        public V2 Position, Velocity;
        public float Life = 3;
        public const float Radius = .12f;
        public Projectile(V2 position, V2 velocity) { Position = position; Velocity = velocity; }
    }

    public sealed class Moonbloom
    {
        public readonly V2 Position;
        public bool Awakened;
        public float GlowTime;
        public int LastAttack = -1;
        public Moonbloom(float x, float y) { Position = new V2(x, y); }
        public Box Bounds { get { return new Box(Position.X - .4f, Position.Y - .4f, .8f, .8f); } }
    }

    public sealed class Enemy
    {
        public readonly EnemyKind Kind;
        public readonly V2 Home;
        public readonly float Range;
        public V2 Position, Velocity, LockedTarget;
        public int Facing = -1, Health;
        public EnemyPhase Phase = EnemyPhase.Idle;
        public float PhaseTime, Clock, Cooldown, HitFlash;
        public bool Grounded;
        private int shots;
        private float nextShot;
        public int MaxHealth { get { return Kind == EnemyKind.ClawPost ? 6 : Kind == EnemyKind.MossHare ? 1 : Kind == EnemyKind.Bristleback ? 4 : Kind == EnemyKind.ReedSpitter ? 3 : 2; } }
        public float Width { get { return Kind == EnemyKind.Bristleback ? 1.2f : Kind == EnemyKind.MossHare ? .65f : .85f; } }
        public float Height { get { return Kind == EnemyKind.ClawPost ? 1.6f : Kind == EnemyKind.ReedSpitter ? 1.2f : Kind == EnemyKind.Bristleback ? .9f : .7f; } }
        public Box Bounds { get { return new Box(Position.X - Width / 2, Position.Y, Width, Height); } }
        public bool Alive { get { return Health > 0; } }
        public bool Harmless { get { return Kind == EnemyKind.MossHare || Kind == EnemyKind.ClawPost; } }
        public bool ContactDanger { get { return Alive && !Harmless && Phase != EnemyPhase.Stunned; } }
        public bool Armored { get { return Kind == EnemyKind.Bristleback && Phase != EnemyPhase.Recover && Phase != EnemyPhase.Stunned; } }
        public string Name { get { return new[] { "Scratch post", "Moss hare", "Thornling", "Bristleback", "Reed spitter", "Lantern moth" }[(int)Kind]; } }

        public Enemy(EnemyKind kind, float x, float y, float range = 1)
        { Kind = kind; Home = Position = new V2(x, y); Range = range; Health = MaxHealth; }

        public void Stun(float seconds)
        { if (Alive) { ChangePhase(EnemyPhase.Stunned); PhaseTime = -seconds; Velocity = new V2(); } }

        public void ReturnHome()
        {
            if (!Alive) return;
            Position = Home; Velocity = new V2(); Cooldown = 1;
            ChangePhase(EnemyPhase.Idle);
        }

        public void ReceiveHit(int damage, int direction)
        {
            Health = Math.Max(0, Health - damage); HitFlash = .16f;
            if (!Alive) { ChangePhase(EnemyPhase.Defeated); Velocity = new V2(); }
            else { Stun(.2f); Velocity.X = direction * 3; }
        }

        public void Step(WorldDefinition world, PumaMotor puma, List<Projectile> projectiles, float dt)
        {
            Clock += dt; PhaseTime += dt;
            HitFlash = Math.Max(0, HitFlash - dt); Cooldown = Math.Max(0, Cooldown - dt);
            if (!Alive)
            {
                if (Kind == EnemyKind.ClawPost && PhaseTime > 1.8f) { Health = MaxHealth; ChangePhase(EnemyPhase.Idle); }
                return;
            }
            if (Kind == EnemyKind.ClawPost) return;
            Grounded = WorldCollision.GroundBelow(world, Position.X, Position.Y);
            if (Phase == EnemyPhase.Stunned)
            {
                if (Kind != EnemyKind.LanternMoth) GroundMotion(world, dt);
                if (PhaseTime >= 0) { ChangePhase(EnemyPhase.Recover); Velocity = new V2(); }
                return;
            }
            float distance = (puma.Bounds.Center - Bounds.Center).Length;
            bool visible = distance < (Kind == EnemyKind.MossHare && puma.Stalking ? 1.65f : 9)
                && WorldCollision.ClearLine(world, Bounds.Center, puma.Bounds.Center);
            switch (Kind)
            {
                case EnemyKind.MossHare: Hare(world, puma, visible, distance, dt); break;
                case EnemyKind.Thornling: Thornling(world, puma, visible, distance, dt); break;
                case EnemyKind.Bristleback: Bristleback(world, puma, visible, distance, dt); break;
                case EnemyKind.ReedSpitter: Spitter(puma, projectiles, visible, dt); break;
                case EnemyKind.LanternMoth: Moth(world, puma, visible, dt); break;
            }
            if (Position.Y < -7 || (Position - Home).Length > 24) ReturnHome();
        }

        private void Hare(WorldDefinition world, PumaMotor puma, bool visible, float distance, float dt)
        {
            if (Phase == EnemyPhase.Idle)
            {
                Velocity.X = 0;
                if (visible && distance < (puma.Stalking ? 1.65f : 5.5f) && Cooldown <= 0)
                { Facing = Position.X >= puma.Position.X ? 1 : -1; ChangePhase(EnemyPhase.Tell); }
            }
            else if (Phase == EnemyPhase.Tell && PhaseTime >= .18f)
            { ChangePhase(EnemyPhase.Active); Velocity = new V2(Facing * 6, 6); }
            else if (Phase == EnemyPhase.Active && Grounded && PhaseTime > .15f)
            { ChangePhase(EnemyPhase.Recover); Velocity.X = 0; }
            else if (Phase == EnemyPhase.Recover && PhaseTime > .4f)
            { ChangePhase(EnemyPhase.Idle); Cooldown = .35f; }
            GroundMotion(world, dt);
        }

        private void Thornling(WorldDefinition world, PumaMotor puma, bool visible, float distance, float dt)
        {
            if (Phase == EnemyPhase.Idle)
            {
                Patrol(world, 1.1f);
                if (visible && distance < 4.5f && Cooldown <= 0)
                { LockTarget(puma); Velocity.X = 0; ChangePhase(EnemyPhase.Tell); }
            }
            else if (Phase == EnemyPhase.Tell && PhaseTime > .48f)
            { ChangePhase(EnemyPhase.Active); Velocity = new V2(Facing * 5.5f, 7); }
            else if (Phase == EnemyPhase.Active && Grounded && PhaseTime > .2f)
            { ChangePhase(EnemyPhase.Recover); Velocity.X = 0; }
            else if (Phase == EnemyPhase.Recover && PhaseTime > .75f)
            { ChangePhase(EnemyPhase.Idle); Cooldown = .5f; }
            GroundMotion(world, dt);
        }

        private void Bristleback(WorldDefinition world, PumaMotor puma, bool visible, float distance, float dt)
        {
            if (Phase == EnemyPhase.Idle)
            {
                Patrol(world, .8f);
                if (visible && distance < 7 && Math.Abs(puma.Position.Y - Position.Y) < 1.7f && Cooldown <= 0)
                { LockTarget(puma); Velocity.X = 0; ChangePhase(EnemyPhase.Tell); }
            }
            else if (Phase == EnemyPhase.Tell && PhaseTime > .75f)
            { ChangePhase(EnemyPhase.Active); Velocity.X = Facing * 11; }
            else if (Phase == EnemyPhase.Active)
            {
                if (PhaseTime >= .58f || !WorldCollision.GroundBelow(world, Position.X + Facing * .8f, Position.Y))
                { ChangePhase(EnemyPhase.Recover); Velocity.X = 0; }
            }
            else if (Phase == EnemyPhase.Recover && PhaseTime >= 1.2f)
            { ChangePhase(EnemyPhase.Idle); Cooldown = .5f; }
            bool blocked = GroundMotion(world, dt);
            if (blocked && Phase == EnemyPhase.Active && Math.Abs(Velocity.X) < .01f)
                ChangePhase(EnemyPhase.Recover);
        }

        private void Spitter(PumaMotor puma, List<Projectile> projectiles, bool visible, float dt)
        {
            if (Phase == EnemyPhase.Idle && visible && Cooldown <= 0)
            { LockTarget(puma); ChangePhase(EnemyPhase.Tell); }
            else if (Phase == EnemyPhase.Tell && PhaseTime >= .8f)
            { ChangePhase(EnemyPhase.Active); shots = 0; nextShot = 0; }
            else if (Phase == EnemyPhase.Active)
            {
                nextShot -= dt;
                if (nextShot <= 0 && shots < 3)
                {
                    V2 muzzle = Bounds.Center + new V2(Facing * .55f, .1f);
                    V2 aim = LockedTarget + new V2(0, (shots - 1) * .35f) - muzzle;
                    if (projectiles.Count < 24) projectiles.Add(new Projectile(muzzle, aim * (8 / Math.Max(.01f, aim.Length))));
                    shots++; nextShot = .22f;
                }
                if (shots == 3 && nextShot <= 0) ChangePhase(EnemyPhase.Recover);
            }
            else if (Phase == EnemyPhase.Recover && PhaseTime > 1.35f)
            { ChangePhase(EnemyPhase.Idle); Cooldown = .3f; }
        }

        private void Moth(WorldDefinition world, PumaMotor puma, bool visible, float dt)
        {
            if (Phase == EnemyPhase.Idle)
            {
                Position = Home + new V2((float)Math.Sin(Clock) * .45f, (float)Math.Sin(Clock * 2) * .2f);
                if (visible && Cooldown <= 0) { LockTarget(puma); ChangePhase(EnemyPhase.Tell); }
            }
            else if (Phase == EnemyPhase.Tell && PhaseTime >= .8f)
            {
                V2 direction = LockedTarget - Bounds.Center;
                Velocity = direction * (11 / Math.Max(.01f, direction.Length));
                ChangePhase(EnemyPhase.Active);
            }
            else if (Phase == EnemyPhase.Active)
            {
                bool blocked = WorldCollision.MoveEnemy(world, this, Velocity * dt);
                if (blocked || PhaseTime > .8f || (Bounds.Center - LockedTarget).Length < .4f)
                { ChangePhase(EnemyPhase.Recover); Velocity = new V2(); }
            }
            else if (Phase == EnemyPhase.Recover)
            {
                V2 toHome = Home - Position;
                // Retreat still collides with terrain. After a blocked retreat, wait before resetting.
                WorldCollision.MoveEnemy(world, this, toHome * Math.Min(1, dt * 3));
                if (toHome.Length < .1f || PhaseTime > 2.5f)
                { Position = Home; ChangePhase(EnemyPhase.Idle); Cooldown = 1; }
            }
        }

        private void LockTarget(PumaMotor puma)
        { LockedTarget = puma.Bounds.Center; Facing = puma.Position.X >= Position.X ? 1 : -1; }
        private void ChangePhase(EnemyPhase phase) { Phase = phase; PhaseTime = 0; }

        private void Patrol(WorldDefinition world, float speed)
        {
            if (Math.Abs(Position.X - Home.X) >= Range || !WorldCollision.GroundBelow(world, Position.X + Facing * .6f, Position.Y))
                Facing = Position.X >= Home.X ? -1 : 1;
            Velocity.X = Facing * speed;
        }

        private bool GroundMotion(WorldDefinition world, float dt)
        {
            if (Kind == EnemyKind.MossHare && Math.Abs(Position.X - Home.X) >= Range && Velocity.X * (Position.X - Home.X) > 0)
                Velocity.X = 0;
            Velocity.Y = Math.Max(-18, Velocity.Y - 22 * dt);
            return WorldCollision.MoveEnemy(world, this, Velocity * dt);
        }
    }
}
