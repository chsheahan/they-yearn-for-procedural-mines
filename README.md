# They Yearn for Procedural Mines

A procedural cavern-generation toolkit in C#, with a Unity integration layer.

The goal is to create underground environments that feel vast, with connected chambers, vertical variation, and routes a player can actually walk.

**Status:** planning and repository scaffolding. There is no runnable generator or Unity project yet.

## First milestone: two chambers, one passage

Generate two chambers at different elevations, connect them with a walkable passage, and explore the result in Unity.

- Repeatable geometry from a seed and generation settings.
- Usable chamber floors and a continuous connecting route.
- Clearance and slope constraints based on a defined player capsule.
- Cave meshes, collision, basic lighting, and a first-person exploration sample.
- Debug views of chamber locations and the intended route.

See [the milestone plan](docs/first-milestone.md) for acceptance criteria and [the architecture](docs/architecture.md) for the proposed pipeline.

## Repository layout

| Path | Purpose |
| --- | --- |
| `src/Core/` | Engine-independent C# generation logic and data |
| `src/Unity/` | Unity adapters, scene construction, and editor integration |
| `samples/TwoChambers/` | First playable demonstration |
| `docs/` | Design decisions and milestone scope |

These directories currently contain planning notes. Unity Editor version, .NET target, package layout, and build instructions will be pinned when implementation begins.

## Design direction

Plan chamber and passage connectivity first, construct a three-dimensional cave volume, then extract its surface into mesh data. Keep volume data separate from rendered geometry so future editing can rebuild affected surfaces.

Protect walking routes while adding organic variation, and validate the resulting geometry rather than assuming a connected graph guarantees traversal.

Start with a bounded environment and plain materials. Demonstrate scale through chamber proportions, passage bends, elevation, and lighting.

## Later possibilities

Larger cavern networks, flora placement, resource distribution, persistent terrain edits, mining, and chunk loading are possible future milestones. They are outside the first demo.

No third-party generation library is included yet. Any future dependency must have a compatible license and retain required notices.

## License

[MIT](LICENSE).
