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
        public float MantleSeconds = .22f, MantleSpeed = 14f;
        public float MantleReachX = .55f, MantleReachY = .35f;
        public float GlideSeconds = .55f, GlideGravityScale = .38f;
        public float AirControlSeconds = .32f, AirControlAccelMult = 2.15f;
        public float FullPounceCharge = .85f;
        public float ClimbSpeed = 4.2f;
        public float ClimbBudgetSeconds = 1.35f;
        public float ClimbRegenSeconds = 0.55f;
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
        Mantle = 268435456, Glide = 536870912, Climb = 1073741824
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
        public bool WallClimbable;
        public bool Climbing;
        public float ClimbBudget;
        private float coyote, buffer, wallLock;
        public static readonly MovementTuning Tuning = new MovementTuning();
        public float BodyHeight { get { return LowProfile || RollTime > 0 ? Height * .55f : Height; } }
        public Box Bounds { get { return new Box(Position.X - Width / 2, Position.Y, Width, BodyHeight); } }
        public bool Dodging { get { return RollTime > Tuning.RollSeconds * .18f && RollTime < Tuning.RollSeconds * .82f; } }
        public bool CanDash { get { return DashCooldown <= 0 && RollTime <= 0 && !Mantling && !Climbing && (Grounded || AirDashReady); } }

        public PumaMotor(V2 spawn = default(V2)) { Reset(spawn); }

        public void Reset(V2 spawn)
        {
            Position = spawn; Velocity = new V2(); Grounded = false; GroundIndex = -1; Wall = 0;
            PounceReady = true; Charging = false; Charge = PounceTime = 0;
            DashTime = DashCooldown = RollTime = RollCooldown = 0;
            AirDashReady = true; LowProfile = Stalking = false;
            Mantling = false; MantleTime = 0;
            Gliding = false; GlideBudget = AirControlTime = 0;
            Climbing = false; ClimbBudget = Tuning.ClimbBudgetSeconds; WallClimbable = false;
            coyote = buffer = wallLock = 0; Facing = 1;
        }

        public GameEvent Prepare(PlayerInput input, float dt)
        {
            GameEvent events = GameEvent.None;
            float move = Scalar.Clamp(input.Move, -1, 1);
            if (Math.Abs(move) > .1f && wallLock <= 0 && !Mantling && !Climbing) Facing = move > 0 ? 1 : -1;
            coyote = Grounded ? Tuning.CoyoteSeconds : Math.Max(0, coyote - dt);
            buffer = input.JumpPressed ? Tuning.BufferSeconds : Math.Max(0, buffer - dt);
            wallLock = Math.Max(0, wallLock - dt);
            DashCooldown = Math.Max(0, DashCooldown - dt);
            RollCooldown = Math.Max(0, RollCooldown - dt);
            AirControlTime = Math.Max(0, AirControlTime - dt);
            if (Grounded)
            {
                PounceReady = true; AirDashReady = true; PounceTime = 0;
                GlideBudget = 0; Gliding = false;
                ClimbBudget = Math.Min(Tuning.ClimbBudgetSeconds, ClimbBudget + Tuning.ClimbRegenSeconds * dt);
                Climbing = false;
            }
            Stalking = input.StalkHeld && Grounded && RollTime <= 0 && DashTime <= 0 && !Mantling && !Climbing;

            if (Mantling)
            {
                MantleTime -= dt;
                Velocity = new V2(0, Tuning.MantleSpeed);
                if (MantleTime <= 0) { Mantling = false; GrantAirControl(); }
                return events;
            }

            if (Climbing)
            {
                if (Wall == 0 || !WallClimbable || ClimbBudget <= 0 || Grounded
                    || RollTime > 0 || DashTime > 0 || input.RollPressed)
                {
                    Climbing = false;
                }
                else if (input.JumpPressed)
                {
                    Climbing = false;
                    Facing = -Wall;
                    Velocity = new V2(Facing * Tuning.WallKickX, Tuning.WallKickY);
                    wallLock = Tuning.WallLockSeconds;
                    buffer = 0;
                    ClimbBudget = Math.Max(0, ClimbBudget - .25f);
                    GrantAirControl();
                    events |= GameEvent.WallKick;
                }
                else
                {
                    float climbDir = 0;
                    if (input.JumpHeld || input.AimY > .2f) climbDir = 1;
                    else if (input.AimY < -.2f) climbDir = -.65f;
                    Velocity.X = Wall * 0.15f;
                    Velocity.Y = climbDir * Tuning.ClimbSpeed;
                    if (climbDir != 0)
                    {
                        ClimbBudget = Math.Max(0, ClimbBudget - dt);
                        events |= GameEvent.Climb;
                    }
                    else
                        ClimbBudget = Math.Max(0, ClimbBudget - dt * .35f);
                    if (ClimbBudget <= 0) Climbing = false;
                    return events;
                }
            }

            if (!Climbing && !Grounded && Wall != 0 && WallClimbable && ClimbBudget > .08f
                && move * Wall > .25f && (input.JumpHeld || input.AimY > .15f)
                && RollTime <= 0 && DashTime <= 0 && PounceTime <= 0)
            {
                Climbing = true;
                Gliding = false;
                Charging = false; Charge = 0;
                Velocity = new V2(Wall * 0.15f, 0);
                events |= GameEvent.Climb;
            }

            if (input.RollPressed && Grounded && RollCooldown <= 0 && DashTime <= 0)
            {
                RollTime = Tuning.RollSeconds; RollCooldown = Tuning.RollCooldown;
                LowProfile = true; Charging = false; Charge = 0;
                Velocity.X = Facing * Tuning.RollSpeed; Velocity.Y = 0;
                events |= GameEvent.Roll;
            }
            if (RollTime > 0)
            {
                RollTime = Math.Max(0, RollTime - dt);
                Velocity.X = Facing * Tuning.RollSpeed;
                Velocity.Y = Math.Min(Velocity.Y, 0);
                if (RollTime <= 0) LowProfile = true;
                return events;
            }
            if (DashTime > 0)
            {
                DashTime = Math.Max(0, DashTime - dt);
                Velocity.X = Facing * Tuning.DashSpeed; Velocity.Y = 0;
                if (DashTime <= 0) DashCooldown = Tuning.DashCooldown;
                return events;
            }
            if (input.DashPressed && CanDash)
            {
                DashTime = Tuning.DashSeconds; AirDashReady = false; Stalking = false;
                Velocity.Y = 0; events |= GameEvent.DashClaw;
            }

            if (input.PouncePressed && PounceReady && !LowProfile)
            { Charging = true; Charge = 0; }
            if (Charging)
            {
                if (input.PounceHeld) Charge = Math.Min(1, Charge + dt / Tuning.ChargeSeconds);
                if (input.PounceReleased || (!input.PounceHeld && Charge > 0))
                {
                    float t = Math.Max(.15f, Charge);
                    float speed = Scalar.Lerp(Tuning.PounceMinSpeed, Tuning.PounceMaxSpeed, t);
                    float aim = Scalar.Clamp(input.AimY, -1, 1);
                    float x = Facing * speed * (1f - Math.Abs(aim) * .28f);
                    float y = speed * (.42f + aim * .38f);
                    if (Grounded) y = Math.Max(.33f, y);
                    Velocity = new V2(x, y);
                    if (t >= Tuning.FullPounceCharge)
                        GlideBudget = Math.Max(GlideBudget, Tuning.GlideSeconds);
                    Charging = false; PounceReady = false; Grounded = false; GroundIndex = -1;
                    PounceTime = .35f; coyote = buffer = 0;
                    GrantAirControl();
                    events |= GameEvent.Pounce;
                }
            }
            else if (input.PounceReleased) Charge = 0;

            PounceTime = Math.Max(0, PounceTime - dt);

            if (buffer > 0 && !Charging)
            {
                if (Grounded || coyote > 0)
                {
                    Velocity.Y = Tuning.JumpSpeed; Grounded = false; GroundIndex = -1;
                    coyote = buffer = 0; events |= GameEvent.Jump;
                }
                else if (Wall != 0 && !Climbing)
                {
                    Facing = -Wall; Velocity = new V2(Facing * Tuning.WallKickX, Tuning.WallKickY);
                    wallLock = Tuning.WallLockSeconds; buffer = 0;
                    GrantAirControl();
                    events |= GameEvent.WallKick;
                }
            }
            if (!input.JumpHeld && Velocity.Y > 0 && PounceTime <= 0 && !Climbing)
                Velocity.Y *= .55f;

            if (wallLock <= 0)
            {
                float target = move * Tuning.RunSpeed * (Charging && Grounded ? .22f : Stalking || LowProfile ? .35f : 1);
                float accel = Grounded ? (Math.Abs(move) < .1f ? Tuning.Brake : Tuning.Acceleration) : Tuning.AirAcceleration;
                if (!Grounded && AirControlTime > 0) accel *= Tuning.AirControlAccelMult;
                if (!Grounded && Math.Abs(Velocity.X) > Tuning.RunSpeed && Math.Sign(Velocity.X) == Math.Sign(move)) accel *= .2f;
                Velocity.X = Scalar.MoveToward(Velocity.X, target, accel * dt);
            }

            bool wantGlide = !Grounded && input.JumpHeld && GlideBudget > 0 && PounceTime <= 0
                && !Mantling && !Climbing && RollTime <= 0 && DashTime <= 0;
            if (wantGlide)
            {
                if (!Gliding) { Gliding = true; events |= GameEvent.Glide; }
                GlideBudget = Math.Max(0, GlideBudget - dt);
            }
            else Gliding = false;

            float gravityScale = Gliding ? Tuning.GlideGravityScale : 1f;
            if (PounceTime <= 0 && !Gliding)
                Velocity.Y -= Tuning.Gravity * 1.25f * gravityScale * dt;
            float fallMult = Velocity.Y < 0 ? Tuning.FallMultiplier : 1;
            if (Gliding) fallMult = 1f;
            Velocity.Y = Math.Max(-Tuning.MaxFall, Velocity.Y - Tuning.Gravity * fallMult * gravityScale * dt);
            if (!Grounded && !Climbing && Wall != 0 && move * Wall > .1f && Velocity.Y < -Tuning.WallSlideSpeed)
                Velocity.Y = -Tuning.WallSlideSpeed;

            return events;
        }

        public void GrantAirControl()
        {
            AirControlTime = Math.Max(AirControlTime, Tuning.AirControlSeconds);
        }

        public void GrantGlideFromRecovery()
        {
            GlideBudget = Math.Max(GlideBudget, Tuning.GlideSeconds * .7f);
            GrantAirControl();
        }

        public bool TryStartMantle()
        {
            if (Mantling || Climbing || LowProfile || RollTime > 0 || DashTime > 0 || Grounded) return false;
            Mantling = true;
            MantleTime = Tuning.MantleSeconds;
            Charging = false; Charge = buffer = 0;
            Gliding = false; Climbing = false;
            Velocity = new V2(0, Tuning.MantleSpeed);
            return true;
        }

        public void Spring()
        {
            Velocity.Y = 20; Grounded = false; GroundIndex = -1;
            PounceReady = true; PounceTime = 0; coyote = buffer = 0;
            AirDashReady = true; Mantling = false; MantleTime = 0;
            Climbing = false;
            GrantAirControl();
        }

        public void Launch(float horizontal, float vertical)
        {
            Velocity = new V2(horizontal, vertical); Grounded = false; GroundIndex = -1;
            coyote = buffer = 0; wallLock = .14f; Mantling = false; MantleTime = 0;
            Climbing = false;
            GrantAirControl();
            if (vertical > 8) GrantGlideFromRecovery();
        }

        public void Interrupt(int away)
        {
            CancelInput(); PounceTime = DashTime = RollTime = 0;
            Mantling = false; MantleTime = 0;
            Gliding = false; GlideBudget = 0;
            Climbing = false;
            Launch(away * 6, 5);
        }

        public void CancelInput() { Charging = false; Charge = buffer = 0; }
    }
}
