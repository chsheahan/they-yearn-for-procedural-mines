# Initial architecture

## Boundaries

`src/Core` is a .NET Standard 2.1 library and a local Unity package. Its assembly explicitly has no engine references. `src/Unity` consumes plain core data and owns meshes, collision, exploration, lighting, and editor tools. The sample references both packages by relative path.

Coordinates use metres, Y up, and X along the descent. Positive density means cave air; negative density means solid rock. Normals and triangle winding face cave air.

## Pipeline

1. Validate finite settings, dimensions, slope, and headroom.
2. Snapshot settings and derive seed-dependent wave phases/passage bend using an explicit integer PRNG.
3. Combine two stretched chamber volumes and one arched passage.
4. Clip all empty space to a common floor: level at the ends, smoothly descending between them.
5. Protect a continuous route envelope including capsule dimensions, clearance, and two cells of meshing margin. Organic shape variation cannot fill it.
6. Sample the bounded density grid and extract the zero surface with six consistent tetrahedra per cell.
7. Create the Unity mesh, static non-convex MeshCollider, procedural material, fixed light set, and optional player.

Connectivity is currently exactly two endpoints and one connection. General graphs, branching, and fully organic floors are deferred.

## Meshing

The marching-tetrahedra implementation uses no third-party lookup table. Intersections share vertices by lattice-edge identity. Exact-zero samples share a lattice-vertex key; near-zero values are snapped to avoid floating-point floor slivers. Density gradients orient surfaces inward.

Generation is capped at 2.5 million density samples. Individually valid settings can still exceed the combined budget; generation reports an error before allocating the grid. Unity meshes use 32-bit indices.

## Traversal

A connected volume alone does not establish walkability. The required route uses continuous floors, bounded slope, and a protected clearance envelope. There are no mandatory jumps, climbs, or drops.

Play Mode tests exercise the actual CharacterController in both directions across three seeds. This checks a central route with default settings, not every chamber edge or every possible configuration. Near chamber boundaries, floors may narrow into walls.

## Lifecycle

The saved scene contains a generator. Play creates the environment; Inspector previews are temporary and excluded from scene serialization.

Regeneration builds valid core data before retiring current generated objects. Old geometry is immediately disabled before deferred destruction. Meshes/materials are released on replacement or teardown, and ambient/fog settings are restored.

The demo requires an identity world transform to preserve metre-based capsule/clearance constraints.

## Repeatability and future editing

`CavernVolume.AlgorithmVersion` identifies the algorithm. Same-version/settings/runtime output is tested. Cross-runtime bitwise equality and stable seeds across future versions are not promised.

The volume remains independent of mesh output. There is no writable voxel store, edit persistence, chunk model, partial remeshing, or mining in this milestone.

## Rendering and performance

The sample targets Unity 6000.4.0f1's Built-in Render Pipeline, with a world-space rock shader, ambient light, a fixed light set, an explorer spotlight, and no real-time shadow maps.

The whole bounded cave is loaded. Fog and passage bends provide depth. No streaming, custom occlusion, or frame-rate guarantee is claimed.
