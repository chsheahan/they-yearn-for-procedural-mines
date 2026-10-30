using System;
using System.Collections.Generic;
using ProceduralMines.Core;
using UnityEngine;
using UnityEngine.Rendering;

namespace ProceduralMines.Unity
{
    [DisallowMultipleComponent]
    public sealed class CavernDemo : MonoBehaviour
    {
        public CavernSettings Settings = new CavernSettings();
        public bool SpawnPlayer = true;
        public bool ShowRoute = true;
        public bool ShowInterface = true;
        public CavernVolume Volume { get; private set; }
        public CavernMesh Geometry { get; private set; }
        public CharacterController Player { get; private set; }
        public string LastError { get; private set; }
        public Rect SettingsPanelRect => new Rect(18, 18, 350, Mathf.Max(200, Mathf.Min(460, Screen.height - 100)));

        private GameObject generated;
        private Mesh mesh;
        private readonly List<Material> materials = new List<Material>();
        private string seedText;
        private double sceneMilliseconds;
        private bool hasEnvironment;
        private Color previousAmbient, previousFogColor;
        private AmbientMode previousAmbientMode;
        private bool previousFog;
        private float previousFogDensity;
        private FogMode previousFogMode;
        private GUIStyle titleStyle, bodyStyle;
        private Vector2 panelScroll;

        private void Start()
        {
            seedText = Settings.Seed.ToString();
            if (Geometry == null) Generate();
        }

        public bool Generate()
        {
            try
            {
                if (transform.position != Vector3.zero || transform.rotation != Quaternion.identity || transform.lossyScale != Vector3.one)
                    throw new ArgumentException("The demo generator must have an identity world transform (position/rotation zero, scale one).");
                // Validate/build before replacing the current scene so a settings error preserves it.
                var watch = System.Diagnostics.Stopwatch.StartNew();
                var volume = new CavernVolume(Settings);
                var geometry = SurfaceMesher.Generate(volume);
                ClearGenerated();
                Volume = volume;
                Geometry = geometry;
                generated = new GameObject("Generated cavern");
                generated.transform.SetParent(transform, false);
                mesh = ToUnityMesh(geometry);
                var surface = new GameObject("Cave surface", typeof(MeshFilter), typeof(MeshRenderer), typeof(MeshCollider));
                surface.transform.SetParent(generated.transform, false);
                surface.GetComponent<MeshFilter>().sharedMesh = mesh;
                surface.GetComponent<MeshCollider>().sharedMesh = mesh;
                surface.GetComponent<MeshRenderer>().sharedMaterial = CreateRockMaterial();
                ConfigureEnvironment();
                CreateLights();
                if (SpawnPlayer) CreatePlayer();
                if (!Application.isPlaying)
                {
                    foreach (var child in generated.GetComponentsInChildren<Transform>())
                        child.gameObject.hideFlags = HideFlags.DontSave;
                    mesh.hideFlags = HideFlags.DontSave;
                    foreach (var material in materials) material.hideFlags = HideFlags.DontSave;
                }
                Physics.SyncTransforms();
                watch.Stop();
                sceneMilliseconds = watch.Elapsed.TotalMilliseconds;
                LastError = null;
                seedText = Volume.Settings.Seed.ToString();
                Debug.Log($"Cavern seed {Volume.Settings.Seed}: {geometry.Vertices.Length:N0} vertices, {geometry.Triangles.Length / 3:N0} triangles, " +
                    $"{geometry.SampleCount:N0} density samples; core {geometry.GenerationMilliseconds:F0} ms, scene {sceneMilliseconds:F0} ms.", this);
                return true;
            }
            catch (Exception error)
            {
                LastError = error.Message;
                Debug.LogError("Cavern generation failed: " + error.Message, this);
                return false;
            }
        }

        public static Mesh ToUnityMesh(CavernMesh geometry)
        {
            var result = new Mesh { name = "Procedural cavern", indexFormat = IndexFormat.UInt32 };
            var vertices = new Vector3[geometry.Vertices.Length];
            var normals = new Vector3[vertices.Length];
            for (int i = 0; i < vertices.Length; i++)
            {
                vertices[i] = ToUnity(geometry.Vertices[i]);
                normals[i] = ToUnity(geometry.Normals[i]);
            }
            result.vertices = vertices;
            result.normals = normals;
            result.triangles = geometry.Triangles;
            result.RecalculateBounds();
            return result;
        }

        public static Vector3 ToUnity(Vec3 p) => new Vector3(p.X, p.Y, p.Z);

        public static CharacterController CreateWalker(CavernSettings settings, Transform parent = null)
        {
            var player = new GameObject("Explorer");
            if (parent != null) player.transform.SetParent(parent, false);
            var controller = player.AddComponent<CharacterController>();
            controller.height = settings.PlayerHeight;
            controller.radius = settings.PlayerRadius;
            controller.center = Vector3.up * (settings.PlayerHeight * 0.5f);
            controller.skinWidth = 0.04f;
            controller.stepOffset = settings.StepHeight;
            controller.slopeLimit = settings.MaxSlopeDegrees;
            controller.minMoveDistance = 0;
            return controller;
        }

        public void ResetPlayer()
        {
            if (Player == null || Volume == null) return;
            Player.enabled = false;
            Player.transform.position = ToUnity(Volume.RoutePoint(0)) + Vector3.up * 0.12f;
            var direction = ToUnity(Volume.RoutePoint(3) - Volume.RoutePoint(0));
            direction.y = 0;
            Player.transform.rotation = Quaternion.LookRotation(direction);
            Player.enabled = true;
            Player.GetComponent<CavernExplorer>()?.ResetLook();
            Physics.SyncTransforms();
        }

        private void CreatePlayer()
        {
            Player = CreateWalker(Volume.Settings, generated.transform);
            var cameraObject = new GameObject("Explorer camera", typeof(Camera), typeof(AudioListener));
            cameraObject.tag = "MainCamera";
            cameraObject.transform.SetParent(Player.transform, false);
            cameraObject.transform.localPosition = Vector3.up * (Volume.Settings.PlayerHeight - 0.15f);
            var camera = cameraObject.GetComponent<Camera>();
            camera.nearClipPlane = 0.05f;
            camera.farClipPlane = 140f;
            camera.fieldOfView = 76f;
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0.012f, 0.02f, 0.026f);
            var lamp = cameraObject.AddComponent<Light>();
            lamp.type = LightType.Spot;
            lamp.range = 38f;
            lamp.spotAngle = 88f;
            lamp.intensity = 3.5f;
            lamp.color = new Color(1f, 0.88f, 0.68f);
            lamp.shadows = LightShadows.None;
            var explorer = Player.gameObject.AddComponent<CavernExplorer>();
            explorer.View = cameraObject.transform;
            explorer.Owner = this;
            ResetPlayer();
        }

        private Material CreateRockMaterial()
        {
            var shader = Resources.Load<Shader>("ProceduralMinesStrata");
            if (shader == null) throw new InvalidOperationException("The ProceduralMines/Strata shader is missing.");
            var material = new Material(shader) { name = "Cavern strata" };
            materials.Add(material);
            return material;
        }

        private void CreateLights()
        {
            // A small fixed light budget; no per-triangle objects or real-time shadow maps.
            AddLight("Amber chamber", new Vector3(-2, 6, 0), new Color(1f, 0.62f, 0.26f), 2.3f, 32);
            AddLight("Blue chamber", new Vector3(Settings.ChamberDistance + 2, -Settings.Depth + 7, -1),
                new Color(0.26f, 0.68f, 1f), 3.2f, 38);
            for (int i = 1; i <= 5; i++)
            {
                var point = ToUnity(Volume.RoutePoint(Settings.ChamberDistance * i / 6f));
                AddLight("Passage beacon " + i, point + new Vector3(0, 2.5f, 0),
                    Color.Lerp(new Color(1f, 0.64f, 0.28f), new Color(0.3f, 0.75f, 1f), i / 6f), 1.4f, 13);
            }
            if (ShowRoute)
            {
                var routeObject = new GameObject("Route guide", typeof(LineRenderer));
                routeObject.transform.SetParent(generated.transform, false);
                var line = routeObject.GetComponent<LineRenderer>();
                var route = Volume.GetRoute();
                line.positionCount = route.Length;
                for (int i = 0; i < route.Length; i++) line.SetPosition(i, ToUnity(route[i]) + Vector3.up * 0.08f);
                line.startWidth = line.endWidth = 0.045f;
                var material = new Material(Resources.Load<Shader>("ProceduralMinesGuide")) { name = "Route guide" };
                material.color = new Color(0.35f, 0.95f, 0.82f, 0.7f);
                materials.Add(material);
                line.sharedMaterial = material;
                line.shadowCastingMode = ShadowCastingMode.Off;
                line.receiveShadows = false;
            }
        }

        private void AddLight(string label, Vector3 position, Color color, float intensity, float range)
        {
            var marker = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            marker.name = label;
            marker.transform.SetParent(generated.transform, false);
            marker.transform.position = position;
            marker.transform.localScale = Vector3.one * 0.22f;
            var markerCollider = marker.GetComponent<Collider>();
            markerCollider.enabled = false;
            Release(markerCollider);
            var material = new Material(Resources.Load<Shader>("ProceduralMinesGuide")) { color = color };
            materials.Add(material);
            marker.GetComponent<Renderer>().sharedMaterial = material;
            var light = marker.AddComponent<Light>();
            light.type = LightType.Point;
            light.color = color;
            light.intensity = intensity;
            light.range = range;
            light.shadows = LightShadows.None;
            light.renderMode = LightRenderMode.ForcePixel;
        }

        private void ConfigureEnvironment()
        {
            if (!hasEnvironment)
            {
                previousAmbient = RenderSettings.ambientLight;
                previousAmbientMode = RenderSettings.ambientMode;
                previousFog = RenderSettings.fog;
                previousFogColor = RenderSettings.fogColor;
                previousFogDensity = RenderSettings.fogDensity;
                previousFogMode = RenderSettings.fogMode;
                hasEnvironment = true;
            }
            RenderSettings.ambientMode = AmbientMode.Flat;
            RenderSettings.ambientLight = new Color(0.12f, 0.14f, 0.17f);
            RenderSettings.fog = true;
            RenderSettings.fogMode = FogMode.ExponentialSquared;
            RenderSettings.fogColor = new Color(0.015f, 0.025f, 0.035f);
            RenderSettings.fogDensity = 0.012f;
        }

        private void OnGUI()
        {
            if (!ShowInterface) return;
            if (titleStyle == null)
            {
                titleStyle = new GUIStyle(GUI.skin.label) { fontSize = 20, fontStyle = FontStyle.Bold };
                bodyStyle = new GUIStyle(GUI.skin.label) { wordWrap = true, fontSize = 13 };
            }
            GUILayout.BeginArea(SettingsPanelRect, GUI.skin.box);
            panelScroll = GUILayout.BeginScrollView(panelScroll);
            GUILayout.Label("THEY YEARN FOR\nPROCEDURAL MINES", titleStyle);
            GUILayout.Label("Two chambers / one descent / a very early mine", bodyStyle);
            GUILayout.Space(8);
            GUILayout.Label("Seed");
            seedText = GUILayout.TextField(seedText ?? Settings.Seed.ToString());
            GUILayout.Label($"Depth: {Settings.Depth:F1} m");
            Settings.Depth = GUILayout.HorizontalSlider(Settings.Depth, 1, 10);
            GUILayout.Label($"Rock variation: {Settings.Roughness:F2}");
            Settings.Roughness = GUILayout.HorizontalSlider(Settings.Roughness, 0, 1);
            bool unlocked = Cursor.lockState != CursorLockMode.Locked;
            GUI.enabled = unlocked;
            if (GUILayout.Button("Generate"))
            {
                if (int.TryParse(seedText, out int seed)) { Settings.Seed = seed; Generate(); }
                else LastError = "Seed must be a whole number between -2147483648 and 2147483647.";
            }
            if (GUILayout.Button("Return to entrance")) ResetPlayer();
            GUI.enabled = true;
            if (Geometry != null)
                GUILayout.Label($"Active seed {Volume.Settings.Seed} | {Geometry.Vertices.Length:N0} vertices\n" +
                    $"{Geometry.Triangles.Length / 3:N0} triangles | core {Geometry.GenerationMilliseconds:F0} ms\nScene + collision: {sceneMilliseconds:F0} ms", bodyStyle);
            if (!string.IsNullOrEmpty(LastError)) GUILayout.Label(LastError, bodyStyle);
            GUILayout.EndScrollView();
            GUILayout.EndArea();
            GUI.Label(new Rect(18, Screen.height - 66, 650, 55),
                "WASD move  /  Shift walk faster  /  Mouse look\nClick outside the panel to explore  /  Esc releases the mouse  /  R returns to entrance", bodyStyle);
        }

        private void OnDrawGizmos()
        {
            if (!ShowRoute || Volume == null) return;
            Gizmos.color = Color.cyan;
            var route = Volume.GetRoute(1);
            for (int i = 1; i < route.Length; i++)
                Gizmos.DrawLine(ToUnity(route[i - 1]) + Vector3.up * 0.2f, ToUnity(route[i]) + Vector3.up * 0.2f);
            foreach (var center in Volume.ChamberCenters) Gizmos.DrawWireSphere(ToUnity(center) + Vector3.up * 2, 1);
        }

        private void ClearGenerated()
        {
            if (generated != null)
            {
                // Disable immediately: Destroy is deferred until end of frame.
                generated.SetActive(false);
                Release(generated);
            }
            if (mesh != null) Release(mesh);
            foreach (var material in materials) if (material != null) Release(material);
            materials.Clear();
            Player = null;
        }

        private static void Release(UnityEngine.Object value)
        {
            if (Application.isPlaying) Destroy(value); else DestroyImmediate(value);
        }

        private void OnDestroy()
        {
            ClearGenerated();
            if (!hasEnvironment) return;
            RenderSettings.ambientLight = previousAmbient;
            RenderSettings.ambientMode = previousAmbientMode;
            RenderSettings.fog = previousFog;
            RenderSettings.fogColor = previousFogColor;
            RenderSettings.fogDensity = previousFogDensity;
            RenderSettings.fogMode = previousFogMode;
        }
    }
}
