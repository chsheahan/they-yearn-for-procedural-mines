# Unity integration

A local Unity package depending on `com.chsheahan.procedural-mines.core`.

- `CavernDemo`: settings, mesh/collider lifecycle, lights, overlay, and route display.
- `CavernExplorer`: capsule movement and mouse capture.
- `Resources/ProceduralMinesStrata.shader`: procedural rock shading for the Built-in Render Pipeline.
- Editor tools: sample navigation, temporary previews, and batch preparation/build.

The example is in `samples/TwoChambers`. For another project, add both packages, enable the legacy Horizontal/Vertical/Mouse X/Mouse Y input axes, and place the demo on an identity-transform GameObject in an empty scene using the Built-in Render Pipeline.

Shaders load from the package's Resources folder, so player builds retain them without special Graphics Settings.
