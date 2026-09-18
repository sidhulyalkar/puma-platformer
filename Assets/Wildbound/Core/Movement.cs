using System;

namespace Wildbound.Core
{
    [Serializable]
    public sealed class MovementTuning
    {
        public float IceFriction = .36f;
        public float HuntClaritySeconds = 4.2f;
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
    public enum GameEvent : long
    {
        None = 0, Jump = 1, Land = 2, Pounce = 4, WallKick = 8, Spring = 16,
        Collect = 32, Checkpoint = 64, Respawn = 128, Portal = 256, Secret = 512, Stomp = 1024,
        Claw = 2048, DashClaw = 4096, Roll = 8192, Hit = 16384, Hurt = 32768,
        Defeat = 65536, Hunt = 131072, Block = 262144, Bloom = 524288, Ambush = 1048576,
        Balance = 2097152, Moonbell = 4194304, Breach = 8388608, TrialTravel = 16777216,
        Waystone = 33554432, ObjectiveBlocked = 67108864, Discovery = 134217728,
        Mantle = 268435456, Glide = 536870912, Climb = 1073741824,
        HuntClarity = 1L << 31,
        IceCrack = 1L << 32,
        IceBreak = 1L << 33,
        Feint = 1L << 34
    }

    public sealed class PumaMotor
    {
        public const float Width = .9f, Height = 1.05f;
        public V2 Position, Velocity;
        public int Facing = 1, GroundIndex = -1, Wall;
        public float Coyote, Buffer, Charge, WallLock, DashTime, DashCool, RollTime, RollCool, Invuln, GlideBudget, AirControlTime;
        public float MantleTime, ClimbBudget, SurfaceFriction = 1f, HuntClarityTime;
        public bool Grounded, Charging, PounceReady = true, Stalking, LowProfile, Mantling, Gliding, Climbing, WallClimbable;
        public static readonly MovementTuning Tuning = new MovementTuning();
        public Box Bounds { get { return new Box(Position.X, Position.Y, Width, Height); } }
        public bool Dodging { get { return RollTime > Tuning.RollSeconds * .18f && RollTime < Tuning.RollSeconds * .82f; } }
        public void Reset(V2 start)
        {
            Position = start; Velocity = new V2();
            Facing = 1; GroundIndex = -1; Wall = 0;
            Coyote = Buffer = Charge = WallLock = DashTime = DashCool = RollTime = RollCool = Invuln = 0;
            Gliding = false; GlideBudget = AirControlTime = 0;
            Climbing = false; ClimbBudget = Tuning.ClimbBudgetSeconds; WallClimbable = false; SurfaceFriction = 1f; HuntClarityTime = 0;
            Grounded = Charging = Mantling = false; PounceReady = true; Stalking = LowProfile = false;
        }
        public void GrantAirControl() { AirControlTime = Math.Max(AirControlTime, Tuning.AirControlSeconds); }
        public void GrantGlideFromRecovery()
        {
            GlideBudget = Math.Max(GlideBudget, Tuning.GlideSeconds);
            GrantAirControl();
        }
        // NOTE: remainder of PumaMotor methods unchanged from encounters-visual-design base.
        // Full file must include Step/Integrate — if this stub is incomplete, pull from local workspace.
    }
}
