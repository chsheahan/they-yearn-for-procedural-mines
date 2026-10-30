# Verification

## Core

Run `dotnet run --project tests/CoreChecks -c Release` with .NET 10 SDK. This dependency-free executable suite exits nonzero on failure. GitHub Actions runs it on pushes and pull requests.

Coverage: invalid settings, immutable generation snapshots, same-input fingerprints, different-seed variation, finite geometry, unit normals, valid indices, nondegenerate triangles, inward winding, closed manifold topology, and route clearance/slope. Seeds: 1337, 42, -17, 0, int.MinValue, int.MaxValue.

## Unity

Open the sample with an activated Unity 6000.4.0f1 Editor. Test Framework restores on first import. Use **Window → General → Test Runner → PlayMode → Run All**.

For batch tests, close any Editor using the project and run Unity with these arguments, replacing paths with absolute paths:

```text
-batchmode -nographics -projectPath <repo>/samples/TwoChambers -runTests -testPlatform PlayMode -testResults <results.xml> -logFile <tests.log>
```

Do not add `-quit`; the test runner manages completion. Wait for the process and inspect XML/log output rather than treating launch as a pass.

Play Mode tests walk a CharacterController both directions across three seeds, detect stalls/falls, raycast floor/ceiling/walls, and check collider replacement and invalid-setting preservation.

## Recorded results

- Core: passed across six seeds, 1,212,202 assertions.
- Default seed: 24,813 vertices and 49,622 triangles.
- Unity-facing C#: compiled against installed 6000.4.0f1 reference assemblies with zero warnings/errors.
- Unity Editor import: completed in 6000.4.0f1 using the normal licensed Hub session.
- Unity Play Mode: both tests passed (no skipped tests), including capsule traversal in both directions across three seeds and collider/regeneration checks. Latest run: approximately 1.2 seconds.
- Rendering: both chamber previews captured and inspected; the sample was also opened in Play Mode with its settings panel and exploration camera.
- Windows development player: successfully built. Build output is generated locally under artifacts/Windows and is excluded from Git.

The first sandboxed batch launches could not connect to Unity licensing. Verification succeeded through the normal Editor launched from Hub; this was an environment limitation rather than a reported test pass.

## Limits

Core sampling is not proof of collision traversal. Play Mode checks the required central route with default traversal settings, not every chamber edge or every settings combination. Manual exploration is needed to assess the feel of the space.
