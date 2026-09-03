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
        // Mantle / ledge grab (v0.5)
        public float MantleSeconds = .22f, MantleSpeed = 14f;
        public float MantleReachX = .55f, MantleReachY = .35f;
        // Soft tail-glide + air-control window (v0.5)
        public float GlideSeconds = .55f, GlideGravityScale = .38f;
        public float AirControlSeconds = .32f, AirControlAccelMult = 2.15f;
        public float FullPounceCharge = .85f;
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
        Defeat = 65536, Hunt = 131072, Block = 262144, Bloom = 524288, Ambush = 1048576,
        Balance = 2097152, Moonbell = 4194304, Breach = 8388608, TrialTravel = 16777216,
        Waystone = 33554432, ObjectiveBlocked = 67108864, Discovery = 134217728,
        Mantle = 268435456, Glide = 536870912
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
        public bool Mantling;
        public float MantleTime;
        public float GlideBudget, AirControlTime;
        public bool Gliding;
        private float coyote, buffer, wallLock;
        public float BodyHeight { get { return LowProfile ? .58f : Height; } }
        public Box Bounds { get { return new Box(Position.X - Width / 2, Position.Y, Width, BodyHeight); } }
        public bool Dodging { get { return RollTime > .09f && RollTime < Tuning.RollSeconds - .04f; } }
        public bool CanDash { get { return DashCooldown <= 0 && RollTime <= 0 && !Mantling && (Grounded || AirDashReady); } }
        public readonly MovementTuning Tuning;
        public PumaMotor(V2 spawn, MovementTuning tuning = null) { Position = spawn; Tuning = tuning ?? new MovementTuning(); }

        public void Reset(V2 spawn)
        {
            Position = spawn; Velocity = new V2(); Grounded = false; GroundIndex = -1; Wall = 0;
            PounceReady = true; Charging = false; Charge = PounceTime = coyote = buffer = wallLock = 0;
            DashTime = DashCooldown = RollTime = RollCooldown = 0;
            AirDashReady = true; LowProfile = Stalking = Mantling = Gliding = false;
            MantleTime = GlideBudget = AirControlTime = 0;
        }

        public GameEvent Prepare(PlayerInput input, float dt)
        {
            GameEvent events = GameEvent.None;
            float move = Scalar.Clamp(input.Move, -1, 1);
            if (Math.Abs(move) > .1f && wallLock <= 0 && RollTime <= 0 && DashTime <= 0 && !Mantling) Facing = move > 0 ? 1 : -1;
            coyote = Grounded ? Tuning.CoyoteSeconds : Math.Max(0, coyote - dt);
            buffer = input.JumpPressed ? Tuning.BufferSeconds : Math.Max(0, buffer - dt);
            wallLock = Math.Max(0, wallLock - dt);
            PounceTime = Math.Max(0, PounceTime - dt);
            DashTime = Math.Max(0, DashTime - dt); DashCooldown = Math.Max(0, DashCooldown - dt);
            RollTime = Math.Max(0, RollTime - dt); RollCooldown = Math.Max(0, RollCooldown - dt);
            AirControlTime = Math.Max(0, AirControlTime - dt);
            if (Grounded)
            {
                PounceReady = true; AirDashReady = true;
                GlideBudget = 0; Gliding = false; AirControlTime = 0;
            }
            Stalking = input.StalkHeld && Grounded && RollTime <= 0 && DashTime <= 0 && !Mantling;

            // Active mantle: lock horizontal, rise onto the ledge, then release into air-control.
            if (Mantling)
            {
                MantleTime = Math.Max(0, MantleTime - dt);
                Velocity.X = 0;
                Velocity.Y = Tuning.MantleSpeed;
                if (MantleTime <= 0)
                {
                    Mantling = false;
                    Velocity = new V2(Facing * 2.5f, 2.5f);
                    Grounded = false; GroundIndex = -1;
                    GrantAirControl();
                }
                return events;
            }

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
                Gliding = false;
                return events;
            }
            if (LowProfile) { input.JumpPressed = input.PouncePressed = input.PounceReleased = false; buffer = 0; }

            if (input.PouncePressed && PounceReady) { Charging = true; Charge = 0; }
            if (Charging && input.PounceHeld) Charge = Math.Min(1, Charge + dt / Tuning.ChargeSeconds);
            if (Charging && input.PounceReleased)
            {
                float speed = Tuning.PounceMinSpeed + (Tuning.PounceMaxSpeed - Tuning.PounceMinSpeed) * Charge;
                float aim = Scalar.Clamp(input.AimY, -.7f, 1);
                float y = aim > .2f ? .72f : aim < -.2f ? -.55f : .33f;
                if (Grounded) y = Math.Max(.33f, y);
                Velocity = new V2(Facing * speed * (float)Math.Sqrt(1 - y * y), speed * y);
                PounceTime = .15f + .12f * Charge;
                // Full coil unlocks a short tail-glide budget in the air.
                if (Charge >= Tuning.FullPounceCharge) GlideBudget = Tuning.GlideSeconds;
                Charging = false; PounceReady = false; Grounded = false; GroundIndex = -1;
                coyote = buffer = 0; Charge = 0;
                GrantAirControl();
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
                    wallLock = Tuning.WallLockSeconds; buffer = 0;
                    GrantAirControl();
                    events |= GameEvent.WallKick;
                }
            }

            // Soft tail-glide: hold jump while budget remains — reduced gravity, not free flight.
            bool wantGlide = !Grounded && input.JumpHeld && GlideBudget > 0 && PounceTime <= 0
                && RollTime <= 0 && DashTime <= 0 && !Mantling;
            if (wantGlide)
            {
                if (!Gliding) events |= GameEvent.Glide;
                Gliding = true;
                GlideBudget = Math.Max(0, GlideBudget - dt);
            }
            else Gliding = false;

            if (PounceTime <= 0)
            {
                if (wallLock <= 0)
                {
                    float target = move * Tuning.RunSpeed * (Charging && Grounded ? .22f : Stalking || LowProfile ? .35f : 1);
                    float accel = Grounded ? (Math.Abs(move) < .1f ? Tuning.Brake : Tuning.Acceleration) : Tuning.AirAcceleration;
                    if (!Grounded && AirControlTime > 0) accel *= Tuning.AirControlAccelMult;
                    if (!Grounded && Math.Abs(Velocity.X) > Tuning.RunSpeed && Math.Sign(Velocity.X) == Math.Sign(move)) accel *= .2f;
                    Velocity.X = Scalar.Move(Velocity.X, target, accel * dt);
                }
                float gravityScale = Gliding ? Tuning.GlideGravityScale : 1f;
                if (!input.JumpHeld && Velocity.Y > 0 && (events & GameEvent.Jump) == 0 && wallLock <= 0 && !Gliding)
                    Velocity.Y -= Tuning.Gravity * 1.25f * gravityScale * dt;
                float fallMult = Velocity.Y < 0 ? Tuning.FallMultiplier : 1;
                if (Gliding) fallMult = 1f; // no extra fall punch while gliding
                Velocity.Y = Math.Max(-Tuning.MaxFall, Velocity.Y - Tuning.Gravity * fallMult * gravityScale * dt);
                if (!Grounded && Wall != 0 && move * Wall > .1f && Velocity.Y < -Tuning.WallSlideSpeed)
                    Velocity.Y = -Tuning.WallSlideSpeed;
            }
            else Velocity.Y -= Tuning.Gravity * .3f * dt;
            return events;
        }

        public void GrantAirControl()
        {
            AirControlTime = Math.Max(AirControlTime, Tuning.AirControlSeconds);
        }

        /// <summary>Grant a short glide budget after a confirmed falling-rake / stomp rebound.</summary>
        public void GrantGlideFromRecovery()
        {
            GlideBudget = Math.Max(GlideBudget, Tuning.GlideSeconds * .7f);
            GrantAirControl();
        }

        public bool TryStartMantle()
        {
            if (Mantling || LowProfile || RollTime > 0 || DashTime > 0 || Grounded) return false;
            Mantling = true;
            MantleTime = Tuning.MantleSeconds;
            Charging = false; Charge = buffer = 0;
            Gliding = false;
            Velocity = new V2(0, Tuning.MantleSpeed);
            return true;
        }

        public void Spring()
        {
            Velocity.Y = 20; Grounded = false; GroundIndex = -1;
            PounceReady = true; PounceTime = 0; coyote = buffer = 0;
            AirDashReady = true; Mantling = false; MantleTime = 0;
            GrantAirControl();
        }

        public void Launch(float horizontal, float vertical)
        {
            Velocity = new V2(horizontal, vertical); Grounded = false; GroundIndex = -1;
            coyote = buffer = 0; wallLock = .14f; Mantling = false; MantleTime = 0;
            GrantAirControl();
            // Upward recovery launches (stomp / falling rake) also top up glide budget.
            if (vertical > 8) GrantGlideFromRecovery();
        }

        public void Interrupt(int away)
        {
            CancelInput(); PounceTime = DashTime = RollTime = 0;
            Mantling = false; MantleTime = 0;
            Gliding = false; GlideBudget = 0;
            Launch(away * 6, 5);
        }

        public void CancelInput() { Charging = false; Charge = buffer = 0; }
    }
}
