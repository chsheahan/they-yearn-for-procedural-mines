using System;

namespace ProceduralMines.Core
{
    /// <summary>Positive density is empty cave; negative density is solid rock.</summary>
    public sealed class CavernVolume
    {
        public const string AlgorithmVersion = "two-chambers-v1";
        private readonly CavernSettings settings;
        private readonly float phase, bend;
        public CavernSettings Settings => settings.Copy();
        public Vec3 Minimum { get; }
        public Vec3 Maximum { get; }
        public Vec3[] ChamberCenters => new[] { RoutePoint(0), RoutePoint(settings.ChamberDistance) };
        public float ProtectedHalfWidth => settings.PlayerRadius + settings.Clearance + 2f * settings.CellSize;
        public float ProtectedHeight => settings.PlayerHeight + settings.Clearance + 2f * settings.CellSize;

        public CavernVolume(CavernSettings settings)
        {
            if (settings == null) throw new ArgumentNullException(nameof(settings));
            var errors = settings.Validate();
            if (errors.Count > 0) throw new ArgumentException(string.Join("\n", errors), nameof(settings));
            this.settings = settings.Copy();
            uint state = unchecked((uint)settings.Seed) ^ 0x9e3779b9u;
            phase = Next(ref state) * 6.2831853f;
            bend = 1.5f + Next(ref state) * 2.5f;
            float margin = settings.ChamberRadius * 1.2f + settings.Roughness + 3f * settings.CellSize;
            Minimum = new Vec3(-margin, -settings.Depth - 3f * settings.CellSize, -margin);
            Maximum = new Vec3(settings.ChamberDistance + margin, settings.ChamberHeight * 1.2f + 4f, margin + bend);
        }

        private static float Next(ref uint state)
        {
            state ^= state << 13; state ^= state >> 17; state ^= state << 5;
            return (state & 0x00ffffff) / 16777216f;
        }

        public float FloorHeight(float x)
        {
            float t = Clamp01((x - 8f) / (settings.ChamberDistance - 16f));
            return -settings.Depth * t * t * (3f - 2f * t);
        }

        public Vec3 RoutePoint(float x)
        {
            float t = Clamp01(x / settings.ChamberDistance);
            return new Vec3(x, FloorHeight(x), bend * (float)Math.Sin(Math.PI * t));
        }

        public Vec3[] GetRoute(float spacing = 0.5f)
        {
            if (float.IsNaN(spacing) || float.IsInfinity(spacing) || spacing < 0.05f)
                throw new ArgumentOutOfRangeException(nameof(spacing), "Route spacing must be finite and at least 0.05 metres.");
            int count = Math.Max(2, (int)Math.Ceiling(settings.ChamberDistance / spacing) + 1);
            var points = new Vec3[count];
            for (int i = 0; i < count; i++) points[i] = RoutePoint(settings.ChamberDistance * i / (count - 1));
            return points;
        }

        public float Sample(Vec3 p)
        {
            float floor = FloorHeight(p.X);
            float dy = p.Y - floor;
            float z = p.Z - RoutePoint(p.X).Z;
            float roomA = Room(p, new Vec3(0, 0, 0), 1f);
            float roomB = Room(p, new Vec3(settings.ChamberDistance, -settings.Depth, 0), 1.12f);
            float halfHeight = settings.PassageHeight * 0.72f;
            float tunnel = (1f - (float)Math.Sqrt(z * z / (settings.PassageRadius * settings.PassageRadius)
                + Square((dy - settings.PassageHeight * 0.28f) / halfHeight))) * settings.PassageRadius;
            float extent = Math.Min(p.X + 2f, settings.ChamberDistance + 2f - p.X);
            tunnel = Math.Min(tunnel, extent);
            float cavity = Math.Max(Math.Max(roomA, roomB), tunnel) + Noise(p) * settings.Roughness;
            // Protect a continuous swept route, including the discretization margin.
            float safe = Math.Min(ProtectedHalfWidth - Math.Abs(z), ProtectedHeight - dy);
            safe = Math.Min(safe, extent);
            return Math.Min(dy, Math.Max(cavity, safe));
        }

        public Vec3 InwardNormal(Vec3 p)
        {
            const float e = 0.03f;
            return new Vec3(
                Sample(p + new Vec3(e, 0, 0)) - Sample(p - new Vec3(e, 0, 0)),
                Sample(p + new Vec3(0, e, 0)) - Sample(p - new Vec3(0, e, 0)),
                Sample(p + new Vec3(0, 0, e)) - Sample(p - new Vec3(0, 0, e))).Normalized;
        }

        private float Room(Vec3 p, Vec3 center, float scale)
        {
            float rx = settings.ChamberRadius * scale;
            float rz = rx * 0.85f;
            float ry = settings.ChamberHeight * scale * 0.72f;
            float yCenter = center.Y + settings.ChamberHeight * scale * 0.28f;
            return (1f - (float)Math.Sqrt(Square((p.X - center.X) / rx)
                + Square((p.Y - yCenter) / ry) + Square((p.Z - center.Z) / rz))) * Math.Min(rx, ry);
        }

        private float Noise(Vec3 p)
        {
            // Analytic, versioned waves: no external noise dependency or runtime-specific Random.
            return (float)(0.6 * Math.Sin(p.X * 0.43 + phase) * Math.Sin(p.Y * 0.51 + p.Z * 0.35)
                + 0.3 * Math.Sin(p.X * 0.91 - p.Z * 0.74 + phase) * Math.Cos(p.Y * 0.83)
                + 0.1 * Math.Sin(p.Z * 1.8 + p.Y * 1.1 + phase));
        }

        private static float Square(float x) => x * x;
        private static float Clamp01(float t) => Math.Max(0f, Math.Min(1f, t));
    }
}
