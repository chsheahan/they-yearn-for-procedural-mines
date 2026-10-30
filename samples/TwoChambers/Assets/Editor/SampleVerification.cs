using System.IO;
using ProceduralMines.Unity;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.TestTools.TestRunner.Api;
using UnityEngine;

/// <summary>Sample-only test/capture tools. No dependency is added to either reusable package.</summary>
[InitializeOnLoad]
public static class SampleVerification
{
    private static readonly string Artifacts = Path.GetFullPath(Path.Combine(Application.dataPath, "../../../artifacts"));
    private const string ActiveRun = "ProceduralMines.SampleVerification";
    private static readonly TestRunnerApi Runner;

    static SampleVerification()
    {
        Runner = ScriptableObject.CreateInstance<TestRunnerApi>();
        Runner.hideFlags = HideFlags.HideAndDontSave;
        Runner.RegisterCallbacks(new Results());
        EditorApplication.update += ProcessLocalRequest;
    }

    private static void ProcessLocalRequest()
    {
        if (EditorApplication.isCompiling || EditorApplication.isUpdating || EditorApplication.isPlayingOrWillChangePlaymode
            || SessionState.GetBool(ActiveRun, false)) return;
        string request = Path.Combine(Artifacts, "run-unity-tests.request");
        if (File.Exists(request))
        {
            File.Delete(request);
            RunTests();
            return;
        }
        string capture = Path.Combine(Artifacts, "capture-preview.request");
        if (File.Exists(capture))
        {
            File.Delete(capture);
            CapturePreview();
            return;
        }
        string build = Path.Combine(Artifacts, "build-player.request");
        if (File.Exists(build))
        {
            File.Delete(build);
            EditorApplication.ExecuteMenuItem("Procedural Mines/Build Windows sample");
        }
    }

    [MenuItem("Procedural Mines/Run sample verification")]
    public static void RunTests()
    {
        if (SessionState.GetBool(ActiveRun, false)) return;
        Directory.CreateDirectory(Artifacts);
        SessionState.SetBool(ActiveRun, true);
        Runner.Execute(new ExecutionSettings(new Filter
        {
            testMode = TestMode.PlayMode,
            assemblyNames = new[] { "ProceduralMines.PlayModeTests" }
        }));
    }

    [MenuItem("Procedural Mines/Capture sample preview")]
    public static void CapturePreview()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode) return;
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
        EditorSceneManager.OpenScene("Assets/Scenes/TwoChambers.unity");
        var demo = Object.FindAnyObjectByType<CavernDemo>();
        if (demo == null || !demo.Generate()) return;
        var camera = demo.Player.GetComponentInChildren<Camera>();
        Directory.CreateDirectory(Artifacts);
        Capture(camera, new Vector3(-8, 2.3f, -5), new Vector3(5, 4, 1), Path.Combine(Artifacts, "first-chamber.png"));
        Capture(camera, new Vector3(39, -3.7f, -5), new Vector3(49, 0, 0), Path.Combine(Artifacts, "second-chamber.png"));
        Object.DestroyImmediate(demo.Player.gameObject);
        Debug.Log("Saved cavern previews to " + Artifacts);
    }

    private static void Capture(Camera camera, Vector3 position, Vector3 lookAt, string path)
    {
        camera.transform.position = position;
        camera.transform.LookAt(lookAt);
        var target = new RenderTexture(1440, 900, 24);
        var previous = RenderTexture.active;
        camera.targetTexture = target;
        try
        {
            camera.Render();
            RenderTexture.active = target;
            var texture = new Texture2D(target.width, target.height, TextureFormat.RGB24, false);
            texture.ReadPixels(new Rect(0, 0, target.width, target.height), 0, 0);
            texture.Apply();
            File.WriteAllBytes(path, texture.EncodeToPNG());
            Object.DestroyImmediate(texture);
        }
        finally
        {
            camera.targetTexture = null;
            RenderTexture.active = previous;
            target.Release();
            Object.DestroyImmediate(target);
        }
    }

    private sealed class Results : ICallbacks
    {
        public void RunStarted(ITestAdaptor testsToRun) { }
        public void TestStarted(ITestAdaptor test) { }
        public void TestFinished(ITestResultAdaptor result) { }
        public void RunFinished(ITestResultAdaptor result)
        {
            if (!SessionState.GetBool(ActiveRun, false)) return;
            Directory.CreateDirectory(Artifacts);
            TestRunnerApi.SaveResultToFile(result, Path.Combine(Artifacts, "unity-results.xml"));
            File.WriteAllText(Path.Combine(Artifacts, "unity-summary.txt"),
                $"{result.ResultState}: passed={result.PassCount}, failed={result.FailCount}, skipped={result.SkipCount}; {result.Duration:F2} seconds.");
            SessionState.SetBool(ActiveRun, false);
            Debug.Log("Sample verification: " + result.ResultState);
        }
    }
}
