using System;

namespace Wildbound.Core
{
    [Serializable]
    public sealed class MovementTuning
    {
        public float RunSpeed = 8.5f, Acceleration = 62, AirAcceleration = 31, Brake = 78;
        public float JumpSpeed = 14.5f, Gravity = 34, FallMultiplier = 1.45f, MaxFall = 24;
        public float CoyoteSeconds = .11f, BufferSeconds = .13f;
        public float ChargeSeconds = .65f, PounceMinSpeed = 13, PounceMaxSpeed = 24;
        public float WallSlideSpeed = 3, WallKickX = 10.5f, WallKickY = 14, WallLockSeconds = .16f;
        public float DashSpeed = 21, DashSeconds = .18f, DashCooldown = .55f;
        public float RollSpeed = 13, RollSeconds = .34f, RollCooldown = .65f;
    }

    public struct PlayerInput
    {
        public float Move, AimY;
        public bool JumpPressed, JumpHeld, PouncePressed, PounceHeld, PounceReleased, InteractPressed;
        public bool AttackPressed, DashPressed, RollPressed, StalkHeld;
    }

    [Flags]
    public enum GameEvent
    {
        None = 0, Jump = 1, Land = 2, Pounce = 4, WallKick = 8, Spring = 16,
        Collect = 32, Checkpoint = 64, Respawn = 128, Portal = 256, Secret = 512, Stomp = 1024,
        Claw = 2048, DashClaw = 4096, Roll = 8192, Hit = 16384, Hurt = 32768,
        Defeat = 65536, Hunt = 131072, Block = 262144, Bloom = 524288, Ambush = 1048576
    }

    public sealed class PumaMotor
    {
        public const float Width = .9f, Height = 1.05f;
        public V2 Position, Velocity;
        public int Facing = 1, Wall, GroundIndex = -1;
        public bool Grounded, PounceReady = true, Charging;
        public float Charge, PounceTime;
        public float DashTime, DashCooldown, RollTime, RollCooldown;
        public bool AirDashReady = true, LowProfile, Stalking;
        private float coyote, buffer, wallLock;
        public float BodyHeight { get { return LowProfile ? .58f : Height; } }
        public Box Bounds { get { return new Box(Position.X - Width / 2, Position.Y, Width, BodyHeight); } }
        public bool Dodging { get { return RollTime > .09f && RollTime < Tuning.RollSeconds - .04f; } }
        public bool CanDash { get { return DashCooldown <= 0 && RollTime <= 0 && (Grounded || AirDashReady); } }
        public readonly MovementTuning Tuning;
        public PumaMotor(V2 spawn, MovementTuning tuning = null) { Position = spawn; Tuning = tuning ?? new MovementTuning(); }

        public void Reset(V2 spawn)
        {
            Position = spawn; Velocity = new V2(); Grounded = false; GroundIndex = -1; Wall = 0;
            PounceReady = true; Charging = false; Charge = PounceTime = coyote = buffer = wallLock = 0;
            DashTime = DashCooldown = RollTime = RollCooldown = 0;
            AirDashReady = true; LowProfile = Stalking = false;
        }

        public GameEvent Prepare(PlayerInput input, float dt)
        {
            GameEvent events = GameEvent.None;
            float move = Scalar.Clamp(input.Move, -1, 1);
            if (Math.Abs(move) > .1f && wallLock <= 0 && RollTime <= 0 && DashTime <= 0) Facing = move > 0 ? 1 : -1;
            coyote = Grounded ? Tuning.CoyoteSeconds : Math.Max(0, coyote - dt);
            buffer = input.JumpPressed ? Tuning.BufferSeconds : Math.Max(0, buffer - dt);
            wallLock = Math.Max(0, wallLock - dt);
            PounceTime = Math.Max(0, PounceTime - dt);
            DashTime = Math.Max(0, DashTime - dt); DashCooldown = Math.Max(0, DashCooldown - dt);
            RollTime = Math.Max(0, RollTime - dt); RollCooldown = Math.Max(0, RollCooldown - dt);
            if (Grounded) { PounceReady = true; AirDashReady = true; }
            Stalking = input.StalkHeld && Grounded && RollTime <= 0 && DashTime <= 0;

            if (input.RollPressed && Grounded && RollCooldown <= 0 && DashTime <= 0)
            {
                CancelInput(); PounceTime = 0; RollTime = Tuning.RollSeconds; RollCooldown = Tuning.RollCooldown;
                LowProfile = true; Stalking = false; events |= GameEvent.Roll;
            }
            if (input.DashPressed && CanDash && !LowProfile)
            {
                CancelInput(); PounceTime = 0; DashTime = Tuning.DashSeconds; DashCooldown = Tuning.DashCooldown;
                AirDashReady = false; Stalking = false; Velocity.Y = 0; events |= GameEvent.DashClaw;
            }
            if (RollTime > 0 || DashTime > 0)
            {
                Velocity.X = Facing * (RollTime > 0 ? Tuning.RollSpeed : Tuning.DashSpeed);
                Velocity.Y = DashTime > 0 ? 0 : Math.Max(-Tuning.MaxFall, Velocity.Y - Tuning.Gravity * dt);
                return events;
            }
            // A roll stays low under a ceiling until there is enough room to stand.
            if (LowProfile) { input.JumpPressed = input.PouncePressed = input.PounceReleased = false; buffer = 0; }

            if (input.PouncePressed && PounceReady) { Charging = true; Charge = 0; }
            if (Charging && input.PounceHeld) Charge = Math.Min(1, Charge + dt / Tuning.ChargeSeconds);
            if (Charging && input.PounceReleased)
            {
                float speed = Tuning.PounceMinSpeed + (Tuning.PounceMaxSpeed - Tuning.PounceMinSpeed) * Charge;
                float aim = Scalar.Clamp(input.AimY, -.7f, 1);
                float y = aim > .2f ? .72f : aim < -.2f ? -.55f : .33f;
                // Ground pounces always clear the surface, even when aiming down.
                if (Grounded) y = Math.Max(.33f, y);
                Velocity = new V2(Facing * speed * (float)Math.Sqrt(1 - y * y), speed * y);
                PounceTime = .15f + .12f * Charge;
                Charging = false; PounceReady = false; Grounded = false; GroundIndex = -1;
                coyote = buffer = 0; Charge = 0;
                events |= GameEvent.Pounce;
            }
            if (buffer > 0 && PounceTime <= 0)
            {
                if (Grounded || coyote > 0)
                {
                    Velocity.Y = Tuning.JumpSpeed; Grounded = false; GroundIndex = -1;
                    buffer = coyote = 0; events |= GameEvent.Jump;
                }
                else if (Wall != 0)
                {
                    Facing = -Wall; Velocity = new V2(Facing * Tuning.WallKickX, Tuning.WallKickY);
                    wallLock = Tuning.WallLockSeconds; buffer = 0; events |= GameEvent.WallKick;
                }
            }
            if (PounceTime <= 0)
            {
                if (wallLock <= 0)
                {
                    float target = move * Tuning.RunSpeed * (Charging && Grounded ? .22f : Stalking || LowProfile ? .35f : 1);
                    // Retain launch momentum in the air; steering still bends the arc.
                    float accel = Grounded ? (Math.Abs(move) < .1f ? Tuning.Brake : Tuning.Acceleration) : Tuning.AirAcceleration;
                    if (!Grounded && Math.Abs(Velocity.X) > Tuning.RunSpeed && Math.Sign(Velocity.X) == Math.Sign(move)) accel *= .2f;
                    Velocity.X = Scalar.Move(Velocity.X, target, accel * dt);
                }
                if (!input.JumpHeld && Velocity.Y > 0 && (events & GameEvent.Jump) == 0 && wallLock <= 0)
                    Velocity.Y -= Tuning.Gravity * 1.25f * dt;
                Velocity.Y = Math.Max(-Tuning.MaxFall, Velocity.Y - Tuning.Gravity * (Velocity.Y < 0 ? Tuning.FallMultiplier : 1) * dt);
                if (!Grounded && Wall != 0 && move * Wall > .1f && Velocity.Y < -Tuning.WallSlideSpeed)
                    Velocity.Y = -Tuning.WallSlideSpeed;
            }
            else Velocity.Y -= Tuning.Gravity * .3f * dt;
            return events;
        }

        public void Spring()
        {
            Velocity.Y = 20; Grounded = false; GroundIndex = -1;
            PounceReady = true; PounceTime = 0; coyote = buffer = 0;
            AirDashReady = true;
        }

        public void Launch(float horizontal, float vertical)
        {
            Velocity = new V2(horizontal, vertical); Grounded = false; GroundIndex = -1;
            coyote = buffer = 0; wallLock = .14f;
        }

        public void Interrupt(int away)
        {
            CancelInput(); PounceTime = DashTime = RollTime = 0;
            Launch(away * 6, 5);
        }

        public void CancelInput() { Charging = false; Charge = buffer = 0; }
    }
}
