# Milestone 1: two chambers and a passage

Tracking: [issue #1](https://github.com/chsheahan/they-yearn-for-procedural-mines/issues/1).

## Outcome

Generate two chambers at different elevations from a seed and walk between them in Unity 6000.4.0f1.

## Acceptance criteria

- Document the exact Editor version and setup.
- Provide usable floor space in both chambers.
- Support the configured capsule, clearance, slope limit, and step allowance along the required route.
- Require no jumping, climbing, or falling.
- Provide floor/wall/ceiling collision without blocking the route.
- Reproduce geometry with the same seed/settings/algorithm/runtime.
- Display the route, chamber debug information, geometry counts, and generation duration.
- Report invalid settings with actionable errors.

## Implemented scope

Two chamber volumes, a curved passage, smooth descent, seeded surface variation, an engine-independent mesher, Unity rendering/collision, first-person movement, settings controls, route debug display, reset, diagnostics, core checks, and Play Mode tests.

See [verification](verification.md) for completed runs and test limits.

## Default traversal contract

| Setting | Value |
| --- | --- |
| Distance / descent | 46 m / 6 m |
| Capsule radius / height | 0.4 m / 1.8 m |
| Clearance | 0.25 m plus two cells of meshing margin |
| Slope / step allowance | 30 degrees / 0.25 m |
| Cell size | 0.8 m |

## Deferred

General cave graphs, fully organic floors, flora/fauna, resources, mining, destruction, persistent edits, infinite worlds, streaming, multiplayer, settlements, and a node editor.
