using System.Collections;
using NUnit.Framework;
using ProceduralMines.Core;
using ProceduralMines.Unity;
using UnityEngine;
using UnityEngine.TestTools;

public sealed class CavernTraversalTests
{
    [UnityTest]
    public IEnumerator CapsuleWalksBothDirectionsAcrossThreeSeeds()
    {
        foreach (int seed in new[] { 1337, 42, -17 })
        {
            var root = new GameObject("Traversal test");
            try
            {
                var demo = root.AddComponent<CavernDemo>();
                demo.SpawnPlayer = false;
                demo.ShowInterface = false;
                demo.Settings.Seed = seed;
                Assert.That(demo.Generate(), Is.True, demo.LastError);
                yield return null;
                var player = CavernDemo.CreateWalker(demo.Volume.Settings, root.transform);
                var points = demo.Volume.GetRoute(1);
                for (int direction = 0; direction < 2; direction++)
                {
                    int start = direction == 0 ? 0 : points.Length - 1;
                    player.enabled = false;
                    player.transform.position = CavernDemo.ToUnity(points[start]) + Vector3.up * 0.12f;
                    player.enabled = true;
                    Physics.SyncTransforms();
                    // Fixed synthetic movement steps make the check independent of batch frame rate.
                    for (int step = 0; step < 25; step++) player.Move(Vector3.down * 0.06f);
                    int index = start;
                    int increment = direction == 0 ? 1 : -1;
                    int frames = 0;
                    while (index >= 0 && index < points.Length)
                    {
                        Vector3 target = CavernDemo.ToUnity(points[index]);
                        Vector3 delta = target - player.transform.position;
                        delta.y = 0;
                        if (delta.magnitude < 0.22f) index += increment;
                        else player.Move(Vector3.ClampMagnitude(delta, 0.1f) + Vector3.down * 0.1f);
                        Assert.That(player.transform.position.y, Is.GreaterThan(-demo.Settings.Depth - 1),
                            $"Fell through floor: seed {seed}, direction {direction}.");
                        Assert.That(Mathf.Abs(player.transform.position.y - demo.Volume.FloorHeight(player.transform.position.x)),
                            Is.LessThan(0.6f), $"Lost the floor: seed {seed}.");
                        Assert.That(++frames, Is.LessThan(2000), $"Route blocked: seed {seed}, direction {direction}, waypoint {index}.");
                        if (frames % 100 == 0) yield return null;
                    }
                    Assert.That(Vector3.Distance(player.transform.position, CavernDemo.ToUnity(points[direction == 0 ? points.Length - 1 : 0])),
                        Is.LessThan(0.6f));
                }
                Object.Destroy(player.gameObject);
            }
            finally { Object.Destroy(root); }
            yield return null;
        }
    }

    [UnityTest]
    public IEnumerator RegenerationReplacesCollidersAndProducesFloorWallsAndCeiling()
    {
        var root = new GameObject("Regeneration test");
        try
        {
            var demo = root.AddComponent<CavernDemo>();
            demo.SpawnPlayer = false;
            demo.ShowInterface = false;
            Assert.That(demo.Generate(), Is.True, demo.LastError);
            demo.Settings.Seed = 42;
            Assert.That(demo.Generate(), Is.True, demo.LastError);
            yield return null;
            Assert.That(root.GetComponentsInChildren<MeshCollider>().Length, Is.EqualTo(1));
            Vector3 inside = new Vector3(0, 2, 0);
            foreach (var direction in new[] { Vector3.down, Vector3.up, Vector3.left, Vector3.back })
                Assert.That(Physics.Raycast(inside, direction, 40), Is.True, "Missing collision in " + direction);
            int vertices = demo.Geometry.Vertices.Length;
            demo.Settings.Depth = float.NaN;
            LogAssert.Expect(LogType.Error, new System.Text.RegularExpressions.Regex("Cavern generation failed:"));
            Assert.That(demo.Generate(), Is.False);
            Assert.That(demo.Geometry.Vertices.Length, Is.EqualTo(vertices), "Invalid settings should preserve the active cave.");
        }
        finally { Object.Destroy(root); }
        yield return null;
    }
}
