# Milestone 1: two chambers and a passage

## Outcome

A developer can open a documented Unity project, generate two cavern chambers at different elevations from a seed, and walk from one to the other.

Current status: not implemented.

## Acceptance criteria

- The repository documents the exact Unity Editor version and setup steps.
- The sample generates two distinct chambers and a connecting passage.
- Both chambers provide usable floor space.
- The required route supports the documented player capsule, clearance, maximum slope, and step allowance.
- Traversal requires no jumping, climbing, or falling.
- Floors, walls, and ceilings have collision without blocking the intended route.
- The same seed, settings, and generator version reproduce the geometry in the supported environment.
- A debug view shows chamber locations and the route.
- Invalid settings produce actionable validation errors.
- The sample reports generation duration and mesh vertex/triangle counts.

## Implementation order

1. Pin the Unity Editor version and compatible C#/.NET targets.
2. Establish the core data model and Unity adapter boundary.
3. Generate simple chamber volumes and a conservative sloped passage.
4. Extract and display their surface with collision.
5. Add an exploration controller and verify the route in Play Mode.
6. Add seeded variation while preserving traversal constraints.
7. Document reproduction steps and capture the working demo.

## Verification plan

Add focused checks for repeatability, invalid settings, valid mesh indices, and route constraints. Exercise more than one seed. Use Unity Play Mode to check actual controller traversal and collision; core-level connectivity checks alone are insufficient.

Choose numeric dimensions, seed coverage, and performance budgets during the first implementation pass and record them with the results.

## Deferred

Flora and fauna, resources, mining, destructible terrain, persistent edits, infinite worlds, chunk streaming, multiplayer, constructed settlements, and a node editor.
