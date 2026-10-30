using System;

namespace ProceduralMines.Core
{
    /// <summary>Core coordinates use metres, Y up, and X along the required route.</summary>
    public readonly struct Vec3
    {
        public readonly float X, Y, Z;
        public Vec3(float x, float y, float z) { X = x; Y = y; Z = z; }
        public float Length => (float)Math.Sqrt(Dot(this, this));
        public Vec3 Normalized => Length > 0.000001f ? this / Length : new Vec3(0, 1, 0);
        public static Vec3 operator +(Vec3 a, Vec3 b) => new Vec3(a.X + b.X, a.Y + b.Y, a.Z + b.Z);
        public static Vec3 operator -(Vec3 a, Vec3 b) => new Vec3(a.X - b.X, a.Y - b.Y, a.Z - b.Z);
        public static Vec3 operator *(Vec3 a, float b) => new Vec3(a.X * b, a.Y * b, a.Z * b);
        public static Vec3 operator /(Vec3 a, float b) => a * (1f / b);
        public static float Dot(Vec3 a, Vec3 b) => a.X * b.X + a.Y * b.Y + a.Z * b.Z;
        public static Vec3 Cross(Vec3 a, Vec3 b) => new Vec3(
            a.Y * b.Z - a.Z * b.Y, a.Z * b.X - a.X * b.Z, a.X * b.Y - a.Y * b.X);
        public static Vec3 Lerp(Vec3 a, Vec3 b, float t) => a + (b - a) * t;
        public override string ToString() => $"({X:F2}, {Y:F2}, {Z:F2})";
    }
}
