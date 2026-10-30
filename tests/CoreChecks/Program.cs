using ProceduralMines.Core;
using System.Security.Cryptography;

int checks = 0;
void Check(bool condition, string message)
{
    checks++;
    if (!condition) throw new Exception(message);
}
string Fingerprint(CavernMesh mesh)
{
    using var stream = new MemoryStream();
    using var writer = new BinaryWriter(stream);
    foreach (var v in mesh.Vertices) { writer.Write(v.X); writer.Write(v.Y); writer.Write(v.Z); }
    foreach (var i in mesh.Triangles) writer.Write(i);
    return Convert.ToHexString(SHA256.HashData(stream.ToArray()));
}

try
{
    var invalid = new CavernSettings { CellSize = float.NaN, PlayerHeight = 0.5f, MaxSlopeDegrees = 5 };
    Check(invalid.Validate().Count >= 3, "Invalid settings must report multiple actionable errors.");
    try { _ = new CavernVolume(invalid); throw new Exception("Invalid volume was accepted."); }
    catch (ArgumentException) { checks++; }
    Check(new CavernSettings { Depth = 10, MaxSlopeDegrees = 10 }.Validate().Count > 0,
        "Excessive route slope must be rejected.");
    Check(new CavernSettings { PassageHeight = 4, CellSize = 1.2f }.Validate().Count > 0,
        "Insufficient passage height must be rejected.");

    string? first = null;
    foreach (int seed in new[] { 1337, 42, -17, 0, int.MinValue, int.MaxValue })
    {
        var settings = new CavernSettings { Seed = seed };
        var volume = new CavernVolume(settings);
        settings.Depth = 10;
        Check(volume.Settings.Depth == 6, "The volume must snapshot caller settings.");
        var mesh = SurfaceMesher.Generate(volume);
        Check(mesh.Vertices.Length > 100 && mesh.Triangles.Length % 3 == 0, "Expected a nonempty triangle surface.");
        Check(mesh.Normals.Length == mesh.Vertices.Length, "Every vertex needs a normal.");
        var bounds = new Dictionary<(int, int), int>();
        for (int i = 0; i < mesh.Vertices.Length; i++)
        {
            var v = mesh.Vertices[i];
            Check(float.IsFinite(v.X) && float.IsFinite(v.Y) && float.IsFinite(v.Z), "Vertices must be finite.");
            Check(Math.Abs(mesh.Normals[i].Length - 1) < 0.001f, "Normals must be unit length.");
        }
        for (int i = 0; i < mesh.Triangles.Length; i += 3)
        {
            int a = mesh.Triangles[i], b = mesh.Triangles[i + 1], c = mesh.Triangles[i + 2];
            Check(a >= 0 && b >= 0 && c >= 0 && a < mesh.Vertices.Length && b < mesh.Vertices.Length && c < mesh.Vertices.Length,
                "Triangle indices must be valid.");
            var normal = Vec3.Cross(mesh.Vertices[b] - mesh.Vertices[a], mesh.Vertices[c] - mesh.Vertices[a]);
            Check(normal.Length >= 0.0000000001f, "Degenerate surface triangle.");
            Check(Vec3.Dot(normal, mesh.Normals[a] + mesh.Normals[b] + mesh.Normals[c]) >= -0.00001f,
                "Surface must face cave air.");
            foreach (var edge in new[] { (a, b), (b, c), (c, a) })
            {
                var key = (Math.Min(edge.Item1, edge.Item2), Math.Max(edge.Item1, edge.Item2));
                bounds.TryGetValue(key, out int count);
                bounds[key] = count + 1;
            }
        }
        int open = bounds.Count(pair => pair.Value != 2);
        Check(open == 0, $"Surface must be closed and manifold: seed {seed} has {open} boundary/nonmanifold edges.");

        var route = volume.GetRoute(0.25f);
        foreach (var p in route)
        {
            for (int side = -1; side <= 1; side++)
                for (float h = 0.2f; h <= 2.05f; h += 0.25f)
                    Check(volume.Sample(p + new Vec3(0, h, side * 0.65f)) > 0, "Required route clearance intrudes into solid rock.");
        }
        for (int i = 1; i < route.Length; i++)
        {
            var delta = route[i] - route[i - 1];
            var slope = Math.Atan2(Math.Abs(delta.Y), Math.Sqrt(delta.X * delta.X + delta.Z * delta.Z)) * 180 / Math.PI;
            Check(slope < volume.Settings.MaxSlopeDegrees - 1, "Route slope exceeds its margin.");
        }
        string hash = Fingerprint(mesh);
        if (seed == 1337)
        {
            first = hash;
            Check(hash == Fingerprint(SurfaceMesher.Generate(new CavernVolume(new CavernSettings { Seed = seed }))),
                "Same seed/settings must reproduce mesh vertices and indices.");
        }
        else Check(hash != first, "Different seeds should produce different geometry.");
        Console.WriteLine($"PASS seed {seed}: {mesh.Vertices.Length:N0} vertices, {mesh.Triangles.Length / 3:N0} triangles, {mesh.GenerationMilliseconds:F0} ms; closed surface.");
    }
    Console.WriteLine($"PASS {checks:N0} assertions.");
    return 0;
}
catch (Exception error)
{
    Console.Error.WriteLine($"FAIL after {checks:N0} assertions: {error}");
    return 1;
}
