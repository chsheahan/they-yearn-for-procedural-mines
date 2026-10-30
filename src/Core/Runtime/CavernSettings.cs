using System;
using System.Collections.Generic;

namespace ProceduralMines.Core
{
    [Serializable]
    public sealed class CavernSettings
    {
        public int Seed = 1337;
        public float CellSize = 0.8f;
        public float ChamberDistance = 46f;
        public float Depth = 6f;
        public float ChamberRadius = 13f;
        public float ChamberHeight = 15f;
        public float PassageRadius = 3.2f;
        public float PassageHeight = 6f;
        public float Roughness = 0.65f;
        public float PlayerRadius = 0.4f;
        public float PlayerHeight = 1.8f;
        public float Clearance = 0.25f;
        public float MaxSlopeDegrees = 30f;
        public float StepHeight = 0.25f;

        public CavernSettings Copy() => (CavernSettings)MemberwiseClone();

        public IReadOnlyList<string> Validate()
        {
            var errors = new List<string>();
            Range(errors, nameof(CellSize), CellSize, 0.4f, 1.2f);
            Range(errors, nameof(ChamberDistance), ChamberDistance, 36f, 64f);
            Range(errors, nameof(Depth), Depth, 1f, 10f);
            Range(errors, nameof(ChamberRadius), ChamberRadius, 10f, 16f);
            Range(errors, nameof(ChamberHeight), ChamberHeight, 10f, 22f);
            Range(errors, nameof(PassageRadius), PassageRadius, 2.5f, 5f);
            Range(errors, nameof(PassageHeight), PassageHeight, 4f, 8f);
            Range(errors, nameof(Roughness), Roughness, 0f, 1f);
            Range(errors, nameof(PlayerRadius), PlayerRadius, 0.2f, 0.6f);
            Range(errors, nameof(PlayerHeight), PlayerHeight, 1f, 2.4f);
            Range(errors, nameof(Clearance), Clearance, 0.1f, 0.5f);
            Range(errors, nameof(MaxSlopeDegrees), MaxSlopeDegrees, 5f, 40f);
            Range(errors, nameof(StepHeight), StepHeight, 0.05f, 0.4f);
            if (PlayerHeight < PlayerRadius * 2f)
                errors.Add("PlayerHeight must be at least twice PlayerRadius.");
            // Smoothstep's maximum derivative is 1.5; floors are flat at each end.
            float requiredSlope = (float)(Math.Atan(1.5f * Depth / (ChamberDistance - 16f)) * 180 / Math.PI);
            if (requiredSlope + 2f > MaxSlopeDegrees)
                errors.Add($"Depth requires a slope of {requiredSlope:F1} degrees plus a 2-degree meshing margin; increase MaxSlopeDegrees or reduce Depth.");
            if (PassageHeight < PlayerHeight + Clearance + 2f * CellSize)
                errors.Add("PassageHeight must accommodate PlayerHeight, Clearance, and two cells of meshing margin.");
            return errors;
        }

        private static void Range(List<string> errors, string name, float value, float min, float max)
        {
            if (float.IsNaN(value) || float.IsInfinity(value) || value < min || value > max)
                errors.Add($"{name} must be finite and between {min} and {max}.");
        }
    }
}
