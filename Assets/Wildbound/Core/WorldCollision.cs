using System;

namespace Wildbound.Core
{
    public static class WorldCollision
    {
        public static bool OverlapsSolid(WorldDefinition world, Box box)
        {
            foreach (var p in world.Platforms) if (p.Enabled && box.Overlaps(p.Bounds)) return true;
            return false;
        }

        public static bool ClearLine(WorldDefinition world, V2 from, V2 to, int ignorePlatform = -1)
        {
            for (int i = 0; i < world.Platforms.Count; i++)
                if (i != ignorePlatform && world.Platforms[i].Enabled && SegmentHits(from, to, world.Platforms[i].Bounds)) return false;
            return true;
        }

        public static bool SegmentHits(V2 from, V2 to, Box box)
        {
            float fraction;
            return SegmentHitFraction(from, to, box, out fraction);
        }

        public static bool SegmentHitFraction(V2 from, V2 to, Box box, out float fraction)
        {
            float near = 0, far = 1;
            bool hit = Slab(from.X, to.X - from.X, box.X, box.Right, ref near, ref far)
                && Slab(from.Y, to.Y - from.Y, box.Y, box.Top, ref near, ref far);
            fraction = hit ? near : float.PositiveInfinity;
            return hit;
        }

        private static bool Slab(float origin, float delta, float min, float max, ref float near, ref float far)
        {
            if (Math.Abs(delta) < .00001f) return origin >= min && origin <= max;
            float a = (min - origin) / delta, b = (max - origin) / delta;
            near = Math.Max(near, Math.Min(a, b)); far = Math.Min(far, Math.Max(a, b));
            return near <= far;
        }

        public static bool GroundBelow(WorldDefinition world, float x, float feet)
        { return OverlapsSolid(world, new Box(x - .1f, feet - .15f, .2f, .16f)); }

        /// <summary>
        /// Detect a ledge the puma can mantle onto.
        /// A valid ledge is a solid platform top near the upper body with clear standing space above it.
        /// Returns true and the target feet Y if a suitable ledge is found in the facing direction.
        /// </summary>
        public static bool TryFindLedge(WorldDefinition world, PumaMotor puma, out float targetFeetY, out int platformIndex)
        {
            targetFeetY = 0;
            platformIndex = -1;
            if (puma.LowProfile || puma.Mantling || puma.Grounded) return false;

            float reachX = puma.Tuning.MantleReachX;
            float reachY = puma.Tuning.MantleReachY;
            float chestY = puma.Position.Y + puma.BodyHeight * .55f;
            float face = puma.Facing;

            for (int i = 0; i < world.Platforms.Count; i++)
            {
                var p = world.Platforms[i];
                if (!p.Enabled) continue;
                var b = p.Bounds;

                // Lip must be near chest height and in front of the puma.
                float lipY = b.Top;
                if (Math.Abs(lipY - chestY) > reachY) continue;

                bool inFront = face > 0
                    ? (b.X > puma.Position.X - .2f && b.X < puma.Position.X + WidthReach(puma) + reachX)
                    : (b.Right < puma.Position.X + .2f && b.Right > puma.Position.X - WidthReach(puma) - reachX);
                if (!inFront) continue;

                // Standing space on top of the ledge must be clear for full height.
                float standX = face > 0 ? b.X + .35f : b.Right - .35f;
                var standBox = new Box(standX - PumaMotor.Width / 2, lipY, PumaMotor.Width, PumaMotor.Height);
                if (OverlapsSolid(world, standBox)) continue;

                // Must not be already overlapping the platform body.
                if (puma.Bounds.Overlaps(b)) continue;

                targetFeetY = lipY;
                platformIndex = i;
                return true;
            }
            return false;
        }

        private static float WidthReach(PumaMotor puma) { return PumaMotor.Width * .5f + .15f; }

        public static bool MoveEnemy(WorldDefinition world, Enemy e, V2 delta)
        {
            bool blocked = false;
            int steps = Math.Max(1, (int)Math.Ceiling(Math.Max(Math.Abs(delta.X), Math.Abs(delta.Y)) / .12f));
            for (int s = 0; s < steps; s++)
            {
                float dx = delta.X / steps, dy = delta.Y / steps;
                e.Position.X += dx;
                foreach (var p in world.Platforms)
                {
                    if (!p.Enabled || !e.Bounds.Overlaps(p.Bounds)) continue;
                    if (dx != 0) e.Position.X = dx > 0 ? p.Bounds.X - e.Width / 2 : p.Bounds.Right + e.Width / 2;
                    e.Velocity.X = 0; blocked = true;
                }
                e.Position.Y += dy;
                foreach (var p in world.Platforms)
                {
                    if (!p.Enabled || !e.Bounds.Overlaps(p.Bounds)) continue;
                    if (dy != 0) e.Position.Y = dy > 0 ? p.Bounds.Y - e.Height : p.Bounds.Top;
                    e.Velocity.Y = 0; blocked = true;
                }
            }
            e.Grounded = GroundBelow(world, e.Position.X, e.Position.Y);
            return blocked;
        }
    }
}
