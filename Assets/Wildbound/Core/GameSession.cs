using System;
using System.Collections.Generic;

namespace Wildbound.Core
{
    [Serializable]
    public sealed class JourneySave
    {
        public int Version = 1, Biome, FurthestBiome, Waystones, Discoveries;
        public int[] Collected = new int[3], Checkpoints = { -1, -1, -1 };
        public bool Completed;
        public void Sanitize()
        {
            if (Version != 1) { Biome = FurthestBiome = Waystones = Discoveries = 0; Collected = new int[3]; Checkpoints = new[] { -1, -1, -1 }; Completed = false; }
            Version = 1; Biome = Math.Max(0, Math.Min(2, Biome));
            FurthestBiome = Math.Max(Biome, Math.Max(0, Math.Min(2, FurthestBiome)));
            Waystones = Waystones < 0 ? 0 : Waystones & 7;
            Discoveries = Discoveries < 0 ? 0 : Discoveries & 63;
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
        public PumaCombat Combat { get; private set; } = new PumaCombat();
        public readonly List<Projectile> Projectiles = new List<Projectile>();
        public JourneySave Save { get; private set; }
        public GameEvent Events { get; private set; }
        public float Time { get; private set; }
        public int Deaths { get; private set; }
        public float Recovery { get; private set; }
        public bool Paused;
        public WildPlace LastDiscovery { get; private set; }
        public int DiscoveryCount
        {
            get { int n = 0; for (int i = 0; i < 6; i++) if ((Save.Discoveries & (1 << i)) != 0) n++; return n; }
        }
        private WorldDefinition outsideWorld;
        private V2 outsidePosition;
        private float outsideTime;
        public bool InTrial { get { return World.Trial != null; } }
        public bool WaystoneRestored(int biome) { return biome >= 0 && biome < 3 && (Save.Waystones & (1 << biome)) != 0; }
        public int WaystoneCount { get { return (Save.Waystones & 1) + ((Save.Waystones >> 1) & 1) + ((Save.Waystones >> 2) & 1); } }
        public GameSession(JourneySave save = null)
        {
            Save = save ?? new JourneySave(); Save.Sanitize(); LoadWorld(Save.Biome);
        }
        public void LoadWorld(int biome)
        {
            outsideWorld = null;
            World = WorldDefinition.Create(biome); Save.Biome = biome; Save.FurthestBiome = Math.Max(Save.FurthestBiome, biome); Time = Recovery = 0;
            for (int i = 0; i < World.Pickups.Count; i++) World.Pickups[i].Collected = (Save.Collected[biome] & (1 << i)) != 0;
            Player = new PumaMotor(CheckpointPosition());
            Combat.ResetForRespawn(); Projectiles.Clear();
            RestoreLightBridges();
            foreach (var place in World.Places) if ((Save.Discoveries & place.Mask) != 0) place.OpenPath(World);
            LastDiscovery = null;
        }
        private void RestoreLightBridges()
        {
            if (!WaystoneRestored(Save.Biome) || InTrial) return;
            foreach (var bloom in World.Blooms) bloom.Awakened = true;
            foreach (var platform in World.Platforms) if (platform.Surface == Surface.Moonbridge) platform.Enabled = true;
        }
        public bool TryEnterTrial()
        {
            if (InTrial || Paused || Recovery > 0 || !Player.Grounded || (Player.Position - Moontrial.Entrance).Length > 1.6f) return false;
            outsideWorld = World; outsidePosition = Player.Position; outsideTime = Time;
            World = Moontrial.Create(Save.Biome); Time = Recovery = 0;
            Player.Reset(World.Spawn); Combat.ResetForRespawn(); Projectiles.Clear();
            Events |= GameEvent.TrialTravel; return true;
        }
        public bool LeaveTrial()
        {
            if (!InTrial || outsideWorld == null) return false;
            World = outsideWorld; outsideWorld = null; Time = outsideTime; Recovery = 0;
            Player.Reset(outsidePosition); Combat.ResetForRespawn(); Projectiles.Clear();
            RestoreLightBridges(); Events |= GameEvent.TrialTravel; return true;
        }
        public bool TravelTo(int biome)
        {
            if (biome < 0 || biome > Save.FurthestBiome) return false;
            LoadWorld(biome); return true;
        }
        private V2 CheckpointPosition()
        {
            if (InTrial) return World.Spawn;
            int c = Save.Checkpoints[Save.Biome];
            return c >= 0 && c < World.Checkpoints.Count ? World.Checkpoints[c] : World.Spawn;
        }
        public void Respawn()
        {
            Player.Reset(CheckpointPosition()); Combat.ResetForRespawn(); Projectiles.Clear();
            foreach (var enemy in World.Enemies) enemy.ReturnHome();
            if (InTrial && World.Trial.Balance != null && !World.Trial.Balance.Attuned) World.Trial.Balance.Charge = 0;
            Recovery = .3f; Deaths++; Events |= GameEvent.Respawn;
        }
        public void SetPaused(bool paused) { Paused = paused; Player.CancelInput(); Combat.CancelQueue(); }
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
        public WildPlace NearbyTrail()
        {
            WildPlace nearest = null; float distance = 8;
            foreach (var place in World.Places)
            {
                if (place.Found) continue;
                foreach (var track in place.Tracks)
                {
                    float d = (track - Player.Position).Length;
                    if (d < distance && WildPlace.ScentVisible(World, Player, track)) { nearest = place; distance = d; }
                }
            }
            return nearest;
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
            foreach (var bloom in World.Blooms) bloom.GlowTime = Math.Max(0, bloom.GlowTime - dt);
            if (InTrial) World.Trial.Advance(dt);
            if (Player.LowProfile && Player.RollTime <= 0 && !WorldCollision.OverlapsSolid(World,
                new Box(Player.Position.X - PumaMotor.Width / 2, Player.Position.Y, PumaMotor.Width, PumaMotor.Height)))
                Player.LowProfile = false;
            if (Player.Grounded && Player.GroundIndex >= 0 && Player.GroundIndex < World.Platforms.Count)
                MoveAxis(World.Platforms[Player.GroundIndex].Delta.X, true);
            Events |= Combat.Prepare(ref input, Player, dt);
            Events |= Player.Prepare(input, dt);
            Combat.ApplyMotion(Player); Combat.OnMovement(Events);
            foreach (var enemy in World.Enemies) enemy.Step(World, Player, Projectiles, dt);
            bool wasGrounded = Player.Grounded;
            V2 delta = Player.Velocity * dt;
            if (InTrial) delta.X += World.Trial.WindDrift(Player, Time) * dt;
            // Substeps bound travel below the thinnest authored collider, including fast pounces.
            int steps = Math.Max(1, (int)Math.Ceiling(Math.Max(Math.Abs(delta.X), Math.Abs(delta.Y)) / .12f));
            Player.Grounded = false; Player.GroundIndex = -1;
            for (int i = 0; i < steps; i++)
            {
                float oldFeet = Player.Position.Y;
                MoveAxis(delta.X / steps, true); MoveAxis(delta.Y / steps, false);
                Events |= Combat.ResolveStrike(World, Player);
                Events |= Combat.ResolveBodyHit(World, Player, oldFeet, delta.Y / steps);
                foreach (var hazard in World.Hazards) if (Player.Bounds.Overlaps(hazard)) { Respawn(); return; }
                foreach (var enemy in World.Enemies)
                    if (enemy.ContactDanger && Player.Bounds.Overlaps(enemy.Bounds)) Events |= Combat.TakeDamage(Player, enemy.Position);
                if (Combat.Health <= 0) { Respawn(); return; }
                if ((Events & GameEvent.Hurt) != 0 || (delta.Y < 0 && Player.Velocity.Y > 0)) break;
            }
            ProbeContacts();
            TryMantle();
            if (!wasGrounded && Player.Grounded) Events |= GameEvent.Land;
            if (Player.Grounded && Player.GroundIndex >= 0 && World.Platforms[Player.GroundIndex].Surface == Surface.Spring)
            { Player.Spring(); Events |= GameEvent.Spring; Combat.OnMovement(GameEvent.Spring); }
            if (Player.Position.Y < -8) { Respawn(); return; }
            for (int i = Projectiles.Count - 1; i >= 0; i--)
            {
                var shot = Projectiles[i]; V2 next = shot.Position + shot.Velocity * dt; shot.Life -= dt;
                // Resolve the nearest intersection; a wall behind the player is not a shield.
                float wallFraction = float.PositiveInfinity;
                foreach (var platform in World.Platforms)
                {
                    var b = platform.Bounds; float fraction;
                    if (platform.Enabled && WorldCollision.SegmentHitFraction(shot.Position, next,
                        new Box(b.X - Projectile.Radius, b.Y - Projectile.Radius, b.W + 2 * Projectile.Radius, b.H + 2 * Projectile.Radius), out fraction))
                        wallFraction = Math.Min(wallFraction, fraction);
                }
                Box player = Player.Bounds; float playerFraction;
                bool hit = WorldCollision.SegmentHitFraction(shot.Position, next,
                    new Box(player.X - Projectile.Radius, player.Y - Projectile.Radius, player.W + 2 * Projectile.Radius, player.H + 2 * Projectile.Radius), out playerFraction)
                    && playerFraction < wallFraction;
                if (hit) Events |= Combat.TakeDamage(Player, shot.Position);
                shot.Position = next;
                if (!float.IsPositiveInfinity(wallFraction) || hit || shot.Life <= 0) Projectiles.RemoveAt(i);
            }
            if (Combat.Health <= 0) { Respawn(); return; }
            if (InTrial)
            {
                var trial = World.Trial;
                if (trial.Balance != null) Events |= trial.Balance.Step(World, Player, Combat, Time, dt);
                if (input.InteractPressed)
                {
                    if (Player.Grounded && (Player.Position - Moontrial.Entrance).Length < 1.6f) { LeaveTrial(); return; }
                    if ((Player.Position - trial.Sanctuary).Length < 1.8f)
                    {
                        if (!trial.Ready(World)) Events |= GameEvent.ObjectiveBlocked;
                        else
                        {
                            bool first = !WaystoneRestored(Save.Biome);
                            Save.Waystones |= 1 << Save.Biome;
                            LeaveTrial(); if (first) Events |= GameEvent.Waystone;
                        }
                    }
                }
                return; // Trial progress must never overwrite the outside world's pickup/checkpoint IDs.
            }
            foreach (var place in World.Places)
                if (!place.Found && place.Reached(Player))
                {
                    place.OpenPath(World); Save.Discoveries |= place.Mask;
                    LastDiscovery = place; Events |= GameEvent.Discovery;
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
                { Save.Checkpoints[Save.Biome] = i; Combat.Heal(); Events |= GameEvent.Checkpoint; }
            if (input.InteractPressed && TryEnterTrial()) return;
            if (input.InteractPressed && (Player.Position - World.Exit).Length < 2.2f)
            {
                if (Save.Biome < 2) { LoadWorld(Save.Biome + 1); Events |= GameEvent.Portal; }
                else if (!Save.Completed) { Save.Completed = true; Events |= GameEvent.Portal; }
            }
        }

        /// <summary>
        /// Automatic mantle when the puma rises into a clear ledge lip.
        /// Keeps the main path readable and recovers near-miss vertical approaches (Silksong/Ori-style forgiveness).
        /// </summary>
        private void TryMantle()
        {
            if (Player.Mantling || Player.Grounded || Player.LowProfile || Player.RollTime > 0 || Player.DashTime > 0) return;
            // Prefer rising or near-apex approaches so falling past a ledge does not auto-grab.
            if (Player.Velocity.Y < -2f) return;

            float targetY;
            int index;
            if (!WorldCollision.TryFindLedge(World, Player, out targetY, out index)) return;
            if (!Player.TryStartMantle()) return;

            // Snap horizontally toward the ledge center of mass while the locked rise begins.
            var ledge = World.Platforms[index].Bounds;
            float standX = Player.Facing > 0 ? ledge.X + .4f : ledge.Right - .4f;
            Player.Position = new V2(standX, Math.Min(Player.Position.Y, targetY - .05f));
            Events |= GameEvent.Mantle;
        }

        private void MoveAxis(float amount, bool horizontal)
        {
            if (amount == 0) return;
            if (horizontal) Player.Position.X += amount; else Player.Position.Y += amount;
            foreach (var p in World.Platforms)
            {
                if (!p.Enabled || !Player.Bounds.Overlaps(p.Bounds)) continue;
                if (horizontal)
                {
                    Player.Position.X = amount > 0 ? p.Bounds.X - PumaMotor.Width / 2 : p.Bounds.Right + PumaMotor.Width / 2;
                    Player.Velocity.X = 0;
                }
                else
                {
                    Player.Position.Y = amount > 0 ? p.Bounds.Y - Player.BodyHeight : p.Bounds.Top;
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
                if (!World.Platforms[i].Enabled) continue;
                Box p = World.Platforms[i].Bounds;
                if (Player.Velocity.Y <= 0 && new Box(b.X + .04f, b.Y - .025f, b.W - .08f, .025f).Overlaps(p))
                { Player.Grounded = true; Player.GroundIndex = i; }
                if (new Box(b.Right, b.Y + .08f, .04f, b.H - .16f).Overlaps(p)) Player.Wall = 1;
                else if (new Box(b.X - .04f, b.Y + .08f, .04f, b.H - .16f).Overlaps(p)) Player.Wall = -1;
            }
        }
    }
}
