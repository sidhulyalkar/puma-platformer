using System;

namespace Wildbound.Core
{
    [Serializable]
    public sealed class JourneySave
    {
        public int Version = 1, Biome, FurthestBiome;
        public int[] Collected = new int[3], Checkpoints = { -1, -1, -1 };
        public bool Completed;
        public void Sanitize()
        {
            if (Version != 1) { Biome = FurthestBiome = 0; Collected = new int[3]; Checkpoints = new[] { -1, -1, -1 }; Completed = false; }
            Version = 1; Biome = Math.Max(0, Math.Min(2, Biome));
            FurthestBiome = Math.Max(Biome, Math.Max(0, Math.Min(2, FurthestBiome)));
            if (Collected == null || Collected.Length != 3) Collected = new int[3];
            if (Checkpoints == null || Checkpoints.Length != 3) Checkpoints = new[] { -1, -1, -1 };
            for (int i = 0; i < 3; i++) { Collected[i] &= (1 << 13) - 1; Checkpoints[i] = Math.Max(-1, Math.Min(1, Checkpoints[i])); }
        }
    }

    /// <summary>The same simulation is used by Unity and the CPU regression runner.</summary>
    public sealed class GameSession
    {
        public const float StepSeconds = 1f / 120;
        public WorldDefinition World { get; private set; }
        public PumaMotor Player { get; private set; }
        public JourneySave Save { get; private set; }
        public GameEvent Events { get; private set; }
        public float Time { get; private set; }
        public int Deaths { get; private set; }
        public float Recovery { get; private set; }
        public bool Paused;
        public GameSession(JourneySave save = null)
        {
            Save = save ?? new JourneySave(); Save.Sanitize(); LoadWorld(Save.Biome);
        }
        public void LoadWorld(int biome)
        {
            World = WorldDefinition.Create(biome); Save.Biome = biome; Save.FurthestBiome = Math.Max(Save.FurthestBiome, biome); Time = Recovery = 0;
            for (int i = 0; i < World.Pickups.Count; i++) World.Pickups[i].Collected = (Save.Collected[biome] & (1 << i)) != 0;
            Player = new PumaMotor(CheckpointPosition());
        }
        public bool TravelTo(int biome)
        {
            if (biome < 0 || biome > Save.FurthestBiome) return false;
            LoadWorld(biome); return true;
        }
        private V2 CheckpointPosition()
        {
            int c = Save.Checkpoints[Save.Biome];
            return c >= 0 && c < World.Checkpoints.Count ? World.Checkpoints[c] : World.Spawn;
        }
        public void Respawn()
        {
            Player.Reset(CheckpointPosition()); Recovery = .3f; Deaths++; Events |= GameEvent.Respawn;
        }
        public void SetPaused(bool paused) { Paused = paused; Player.CancelInput(); }
        public int Motes
        {
            get { int n = 0; for (int i = 0; i < World.Pickups.Count; i++) if (World.Pickups[i].Collected && World.Pickups[i].Kind == PickupKind.Mote) n++; return n; }
        }
        public int Memories
        {
            get { int n = 0; for (int i = 0; i < 3; i++) if ((Save.Collected[i] & 1) != 0) n++; return n; }
        }
        public Sign NearbySign()
        {
            Sign result = null; float best = 3.4f;
            foreach (var sign in World.Signs)
            {
                float d = (Player.Position - sign.Position).Length;
                if (d < best) { best = d; result = sign; }
            }
            return result;
        }
        public void Step(PlayerInput input, float dt = StepSeconds)
        {
            Events = GameEvent.None;
            if (!Scalar.Finite(dt) || dt <= 0 || dt > .05f) throw new ArgumentOutOfRangeException("dt");
            if (!Scalar.Finite(input.Move) || !Scalar.Finite(input.AimY)) throw new ArgumentException("Input must be finite.");
            if (Paused) return;
            if (Recovery > 0) { Recovery = Math.Max(0, Recovery - dt); return; }
            Time += dt;
            foreach (var platform in World.Platforms) platform.Update(Time);
            foreach (var critter in World.Critters) critter.Update(Time);
            if (Player.Grounded && Player.GroundIndex >= 0 && Player.GroundIndex < World.Platforms.Count)
                MoveAxis(World.Platforms[Player.GroundIndex].Delta.X, true);
            Events |= Player.Prepare(input, dt);
            bool wasGrounded = Player.Grounded;
            float oldFeet = Player.Position.Y;
            V2 delta = Player.Velocity * dt;
            // Substeps bound travel below the thinnest authored collider, including fast pounces.
            int steps = Math.Max(1, (int)Math.Ceiling(Math.Max(Math.Abs(delta.X), Math.Abs(delta.Y)) / .12f));
            Player.Grounded = false; Player.GroundIndex = -1;
            for (int i = 0; i < steps; i++) { MoveAxis(delta.X / steps, true); MoveAxis(delta.Y / steps, false); }
            ProbeContacts();
            if (!wasGrounded && Player.Grounded) Events |= GameEvent.Land;
            if (Player.Grounded && Player.GroundIndex >= 0 && World.Platforms[Player.GroundIndex].Surface == Surface.Spring)
            { Player.Spring(); Events |= GameEvent.Spring; }
            if (Player.Position.Y < -8) { Respawn(); return; }
            foreach (var hazard in World.Hazards) if (Player.Bounds.Overlaps(hazard)) { Respawn(); return; }
            foreach (var critter in World.Critters)
            {
                if (critter.Asleep || !Player.Bounds.Overlaps(critter.Bounds)) continue;
                if (Player.PounceTime > 0 || (delta.Y < 0 && oldFeet >= critter.Bounds.Top - .12f))
                { critter.Asleep = true; Player.Spring(); Player.Velocity.Y = 11; Events |= GameEvent.Stomp; }
                else { Respawn(); return; }
            }
            for (int i = 0; i < World.Pickups.Count; i++)
            {
                var p = World.Pickups[i];
                if (p.Collected || (p.Position - Player.Bounds.Center).Length > .95f) continue;
                p.Collected = true; Save.Collected[Save.Biome] |= 1 << i;
                Events |= p.Kind == PickupKind.Memory ? GameEvent.Secret : GameEvent.Collect;
            }
            for (int i = 0; i < World.Checkpoints.Count; i++)
                if (Save.Checkpoints[Save.Biome] != i && (Player.Position - World.Checkpoints[i]).Length < 1)
                { Save.Checkpoints[Save.Biome] = i; Events |= GameEvent.Checkpoint; }
            if (input.InteractPressed && (Player.Position - World.Exit).Length < 2.2f)
            {
                if (Save.Biome < 2) { LoadWorld(Save.Biome + 1); Events |= GameEvent.Portal; }
                else if (!Save.Completed) { Save.Completed = true; Events |= GameEvent.Portal; }
            }
        }

        private void MoveAxis(float amount, bool horizontal)
        {
            if (amount == 0) return;
            if (horizontal) Player.Position.X += amount; else Player.Position.Y += amount;
            foreach (var p in World.Platforms)
            {
                if (!Player.Bounds.Overlaps(p.Bounds)) continue;
                if (horizontal)
                {
                    Player.Position.X = amount > 0 ? p.Bounds.X - PumaMotor.Width / 2 : p.Bounds.Right + PumaMotor.Width / 2;
                    Player.Velocity.X = 0;
                }
                else
                {
                    Player.Position.Y = amount > 0 ? p.Bounds.Y - PumaMotor.Height : p.Bounds.Top;
                    Player.Velocity.Y = 0;
                }
            }
        }
        private void ProbeContacts()
        {
            Box b = Player.Bounds;
            Player.Wall = 0;
            for (int i = 0; i < World.Platforms.Count; i++)
            {
                Box p = World.Platforms[i].Bounds;
                if (Player.Velocity.Y <= 0 && new Box(b.X + .04f, b.Y - .025f, b.W - .08f, .025f).Overlaps(p))
                { Player.Grounded = true; Player.GroundIndex = i; }
                if (new Box(b.Right, b.Y + .08f, .04f, b.H - .16f).Overlaps(p)) Player.Wall = 1;
                else if (new Box(b.X - .04f, b.Y + .08f, .04f, b.H - .16f).Overlaps(p)) Player.Wall = -1;
            }
        }
    }
}
