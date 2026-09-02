using System;

namespace Wildbound.Core
{
    [Serializable]
    public struct V2
    {
        public float X, Y;
        public V2(float x, float y) { X = x; Y = y; }
        public static V2 operator +(V2 a, V2 b) { return new V2(a.X + b.X, a.Y + b.Y); }
        public static V2 operator -(V2 a, V2 b) { return new V2(a.X - b.X, a.Y - b.Y); }
        public static V2 operator *(V2 a, float n) { return new V2(a.X * n, a.Y * n); }
        public float Length { get { return (float)Math.Sqrt(X * X + Y * Y); } }
    }

    [Serializable]
    public struct Box
    {
        public float X, Y, W, H;
        public Box(float x, float y, float w, float h) { X = x; Y = y; W = w; H = h; }
        public float Right { get { return X + W; } }
        public float Top { get { return Y + H; } }
        public V2 Center { get { return new V2(X + W / 2, Y + H / 2); } }
        public bool Overlaps(Box b) { return X < b.Right && Right > b.X && Y < b.Top && Top > b.Y; }
        public Box Offset(V2 d) { return new Box(X + d.X, Y + d.Y, W, H); }
    }

    public static class Scalar
    {
        public static float Clamp(float v, float lo, float hi) { return Math.Max(lo, Math.Min(hi, v)); }
        public static float Move(float current, float target, float step)
        { return current < target ? Math.Min(current + step, target) : Math.Max(current - step, target); }
        public static bool Finite(float value) { return !float.IsNaN(value) && !float.IsInfinity(value); }
    }
}
