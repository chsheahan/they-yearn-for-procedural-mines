# Proposed architecture

This document records the initial direction; it does not describe an implemented system.

## Core

The core must not depend on UnityEngine. It owns generation settings, seeded randomness, chamber and route descriptions, density sampling, mesh data, and validation results.

Use a bounded three-dimensional density field to represent solid rock and empty cave space. A surface extraction algorithm will convert that field to vertices and triangles. The specific meshing algorithm is an implementation decision to validate with the first prototype.

Keep generation settings and algorithm version alongside the seed. Repeatability is initially scoped to the same generator version and supported runtime; cross-platform bit-for-bit equivalence is not promised.

## Unity integration

Unity adapters translate core mesh data into Unity meshes and colliders, expose settings, draw debug information, and construct the sample scene. Unity-specific materials, lighting, input, and player movement belong here or in the sample.

## Pipeline

1. Validate settings and generation bounds.
2. Place two chambers and plan a route between their usable floor areas.
3. Build chamber and tunnel volumes with protected floor and clearance envelopes.
4. Add bounded shape variation outside the protected route.
5. Sample the density field and extract surface geometry.
6. Validate mesh and route constraints; report failures rather than silently producing an inaccessible result.
7. Build the Unity scene and verify traversal using the sample player controller.

Coordinate conventions and measurement units must be specified before implementing the adapters.

## Walkability

Graph connectivity is a planning property. Walkability also depends on floor continuity, slope, clearance, collision geometry, and controller behavior.

Define a player capsule radius and height, a clearance margin, maximum slope, and step allowance. Protect the full swept player volume along required routes. Check the meshed and collidable result because sampling can introduce obstacles or gaps.

Start with a conservative ramp between elevations. Jumps, ladders, climbing, and mandatory drops are outside the first milestone.

## Performance and future editing

Use a small configurable generation volume and explicit resolution limits. Record generation duration and mesh size in the prototype before setting performance budgets.

Keep cave volume data separate from mesh output. This enables later experiments with local edits, but the first milestone does not implement destruction, streaming, save formats, or partial rebuilds.

## Dependencies

Start with Unity's built-in rendering and physics capabilities and our own generation code. A small noise dependency can be evaluated when surface variation is needed. Do not adopt a full terrain framework before the basic cavern pipeline is demonstrated.
