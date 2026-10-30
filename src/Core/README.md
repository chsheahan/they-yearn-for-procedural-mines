# Generation core

A Unity-free C# library targeting .NET Standard 2.1 and a local Unity package.

```csharp
var volume = new CavernVolume(new CavernSettings { Seed = 1337 });
var mesh = SurfaceMesher.Generate(volume);
// Plain Vec3[] vertices/normals and int[] triangle indices.
```

`CavernVolume` snapshots settings. `Sample` is positive inside air and negative in rock; `GetRoute` exposes the required walking route.

Build with `dotnet build src/Core/ProceduralMines.Core.csproj`, or import this folder as a Unity package. No external .NET dependencies are required. The root `Directory.Build.props` keeps .NET build artifacts outside package folders.
