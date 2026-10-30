using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace ProceduralMines.Core
{
    public sealed class CavernMesh
    {
        public Vec3[] Vertices { get; }
        public Vec3[] Normals { get; }
        public int[] Triangles { get; }
        public int SampleCount { get; }
        public double GenerationMilliseconds { get; }
        internal CavernMesh(Vec3[] vertices, Vec3[] normals, int[] triangles, int samples, double milliseconds)
        {
            Vertices = vertices; Normals = normals; Triangles = triangles;
            SampleCount = samples; GenerationMilliseconds = milliseconds;
        }
    }

    /// <summary>Marching tetrahedra with shared edge vertices and inward-facing surfaces.</summary>
    public static class SurfaceMesher
    {
        private static readonly int[,] Offsets = {
            {0,0,0}, {1,0,0}, {1,1,0}, {0,1,0}, {0,0,1}, {1,0,1}, {1,1,1}, {0,1,1}
        };
        private static readonly int[,] Tetrahedra = {
            {0,5,1,6}, {0,1,2,6}, {0,2,3,6}, {0,3,7,6}, {0,7,4,6}, {0,4,5,6}
        };

        public static CavernMesh Generate(CavernVolume volume)
        {
            if (volume == null) throw new ArgumentNullException(nameof(volume));
            return new Builder(volume).Build();
        }

        private sealed class Builder
        {
            private readonly CavernVolume volume;
            private readonly float cell;
            private readonly int nx, ny, nz;
            private readonly Vec3 origin;
            private readonly float[] samples;
            private readonly List<Vec3> vertices = new List<Vec3>();
            private readonly List<Vec3> normals = new List<Vec3>();
            private readonly List<int> triangles = new List<int>();
            private readonly Dictionary<ulong, int> edges = new Dictionary<ulong, int>();
            private readonly int[] corners = new int[8], inside = new int[4], outside = new int[4];

            internal Builder(CavernVolume volume)
            {
                this.volume = volume;
                cell = volume.Settings.CellSize;
                origin = volume.Minimum;
                var size = volume.Maximum - origin;
                nx = (int)Math.Ceiling(size.X / cell) + 1;
                ny = (int)Math.Ceiling(size.Y / cell) + 1;
                nz = (int)Math.Ceiling(size.Z / cell) + 1;
                long count = (long)nx * ny * nz;
                if (count > 2500000) throw new ArgumentException("Generation exceeds the 2.5 million density-sample budget. Increase CellSize or reduce chamber dimensions.");
                samples = new float[count];
            }

            internal CavernMesh Build()
            {
                var watch = Stopwatch.StartNew();
                for (int z = 0; z < nz; z++)
                    for (int y = 0; y < ny; y++)
                        for (int x = 0; x < nx; x++)
                        {
                            int id = Id(x, y, z);
                            samples[id] = volume.Sample(Position(id));
                            if (Math.Abs(samples[id]) < 0.00001f) samples[id] = 0;
                            if ((x == 0 || y == 0 || z == 0 || x == nx - 1 || y == ny - 1 || z == nz - 1) && samples[id] >= 0)
                                throw new InvalidOperationException("Cave reaches the sampling boundary; generation bounds must enclose solid rock.");
                        }
                for (int z = 0; z < nz - 1; z++)
                    for (int y = 0; y < ny - 1; y++)
                        for (int x = 0; x < nx - 1; x++)
                        {
                            int positives = 0;
                            for (int i = 0; i < 8; i++)
                            {
                                corners[i] = Id(x + Offsets[i, 0], y + Offsets[i, 1], z + Offsets[i, 2]);
                                if (samples[corners[i]] > 0) positives++;
                            }
                            if (positives == 0 || positives == 8) continue;
                            for (int t = 0; t < 6; t++) Polygonize(t);
                        }
                watch.Stop();
                return new CavernMesh(vertices.ToArray(), normals.ToArray(), triangles.ToArray(), samples.Length, watch.Elapsed.TotalMilliseconds);
            }

            private void Polygonize(int tetrahedron)
            {
                int n = 0, m = 0;
                for (int j = 0; j < 4; j++)
                {
                    int id = corners[Tetrahedra[tetrahedron, j]];
                    if (samples[id] > 0) inside[n++] = id; else outside[m++] = id;
                }
                if (n == 0 || n == 4) return;
                if (n == 1)
                    Triangle(Edge(inside[0], outside[0]), Edge(inside[0], outside[1]), Edge(inside[0], outside[2]));
                else if (n == 3)
                    Triangle(Edge(outside[0], inside[0]), Edge(outside[0], inside[1]), Edge(outside[0], inside[2]));
                else
                {
                    int a = Edge(inside[0], outside[0]), b = Edge(inside[0], outside[1]);
                    int c = Edge(inside[1], outside[0]), d = Edge(inside[1], outside[1]);
                    Triangle(a, b, c);
                    Triangle(b, d, c);
                }
            }

            private int Edge(int a, int b)
            {
                // Exact-zero lattice vertices must share one index across all incident edges.
                if (samples[a] == 0) b = a;
                else if (samples[b] == 0) a = b;
                uint low = (uint)Math.Min(a, b), high = (uint)Math.Max(a, b);
                ulong key = ((ulong)low << 32) | high;
                if (edges.TryGetValue(key, out int index)) return index;
                float t = a == b ? 0f : samples[a] / (samples[a] - samples[b]);
                var point = Vec3.Lerp(Position(a), Position(b), t);
                index = vertices.Count;
                vertices.Add(point);
                normals.Add(volume.InwardNormal(point));
                edges.Add(key, index);
                return index;
            }

            private void Triangle(int a, int b, int c)
            {
                var normal = Vec3.Cross(vertices[b] - vertices[a], vertices[c] - vertices[a]);
                if (normal.Length < 0.0000000001f) return;
                if (Vec3.Dot(normal, normals[a] + normals[b] + normals[c]) < 0)
                {
                    int swap = b; b = c; c = swap;
                }
                triangles.Add(a); triangles.Add(b); triangles.Add(c);
            }

            private int Id(int x, int y, int z) => x + nx * (y + ny * z);
            private Vec3 Position(int id)
            {
                int x = id % nx, y = (id / nx) % ny, z = id / (nx * ny);
                return origin + new Vec3(x * cell, y * cell, z * cell);
            }
        }
    }
}
