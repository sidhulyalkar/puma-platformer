using System;
using System.Collections.Generic;

namespace Wildbound.Core
{
    /// <summary>
    /// Bounded wind ribbon. Additive velocity while the puma overlaps the field.
    /// Same pattern as trial perch wind, but authored into overworld geometry.
    /// </summary>
    public sealed class WindField
    {
        public Box Bounds;
        public V2 Velocity;
        public string Label;

        public WindField(float x, float y, float w, float h, float vx, float vy = 0, string label = "")
        {
            Bounds = new Box(x, y, w, h);
            Velocity = new V2(vx, vy);
            Label = label ?? "";
        }

        public bool Contains(V2 point)
        {
            return point.X >= Bounds.X && point.X <= Bounds.Right
                && point.Y >= Bounds.Y && point.Y <= Bounds.Top;
        }
    }

    public sealed class ScentMark
    {
        public V2 Position;
        public float Life;
        public const float DefaultLife = 4.5f;
        public const float VisibleRange = 8f;

        public ScentMark(V2 position, float life = DefaultLife)
        {
            Position = position;
            Life = life;
        }
    }

    public static class NaturalSystems
    {
        public static V2 SampleWind(WorldDefinition world, V2 point)
        {
            V2 sum = new V2();
            if (world.WindFields == null) return sum;
            foreach (var field in world.WindFields)
            {
                if (field.Contains(point)) sum = sum + field.Velocity;
            }
            return sum;
        }

        public static V2 SampleUpdraft(WorldDefinition world, V2 point)
        {
            V2 sum = new V2();
            if (world.Blooms == null) return sum;
            foreach (var bloom in world.Blooms)
                sum = sum + bloom.SampleUpdraft(point);
            return sum;
        }

        public static void AdvanceScent(WorldDefinition world, float dt)
        {
            if (world.ScentMarks == null) return;
            for (int i = world.ScentMarks.Count - 1; i >= 0; i--)
            {
                world.ScentMarks[i].Life -= dt;
                if (world.ScentMarks[i].Life <= 0) world.ScentMarks.RemoveAt(i);
            }
        }

        public static bool ScentVisible(WorldDefinition world, PumaMotor puma, ScentMark mark)
        {
            return mark.Life > 0
                && puma.Stalking
                && (mark.Position - puma.Position).Length < ScentMark.VisibleRange
                && WorldCollision.ClearLine(world, puma.Bounds.Center, mark.Position + new V2(0, .18f));
        }

        public static void DropHareScent(WorldDefinition world, V2 position)
        {
            if (world.ScentMarks == null) return;
            if (world.ScentMarks.Count >= 16) world.ScentMarks.RemoveAt(0);
            world.ScentMarks.Add(new ScentMark(position));
        }
    }

    public sealed class MemoryVignette
    {
        public string Title;
        public string Body;
        public string Beat;
        public int Biome;
        public float DisplaySeconds;

        public MemoryVignette(string title, string body, string beat, int biome, float displaySeconds = 4.5f)
        {
            Title = title ?? "";
            Body = body ?? "";
            Beat = beat ?? "";
            Biome = biome;
            DisplaySeconds = displaySeconds;
        }
    }
}
