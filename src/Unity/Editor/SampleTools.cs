using System;
using System.IO;
using ProceduralMines.Unity;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

namespace ProceduralMines.Editor
{
    public static class SampleTools
    {
        // Called once during repository setup; also usable to recreate the minimal sample scene.
        [MenuItem("Procedural Mines/Prepare sample project")]
        public static void PrepareSample()
        {
            if (!Directory.Exists("Assets/Scenes")) Directory.CreateDirectory("Assets/Scenes");
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            new GameObject("Cavern generator").AddComponent<CavernDemo>();
            RenderSettings.ambientMode = AmbientMode.Flat;
            RenderSettings.ambientLight = new Color(0.12f, 0.14f, 0.17f);
            RenderSettings.fog = false;
            EditorSceneManager.SaveScene(scene, "Assets/Scenes/TwoChambers.unity");
            EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene("Assets/Scenes/TwoChambers.unity", true) };
            PlayerSettings.companyName = "Procedural Mines";
            PlayerSettings.productName = "They Yearn for Procedural Mines";
            PlayerSettings.defaultScreenWidth = 1440;
            PlayerSettings.defaultScreenHeight = 900;
            PlayerSettings.fullScreenMode = FullScreenMode.Windowed;
            PlayerSettings.runInBackground = true;
            QualitySettings.pixelLightCount = 8;
            QualitySettings.shadows = ShadowQuality.Disable;
            ConfigureInput();
            IncludeShader();
            AssetDatabase.SaveAssets();
            Debug.Log("Prepared TwoChambers scene and project settings.");
        }

        private static void IncludeShader()
        {
            var graphics = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/GraphicsSettings.asset")[0]);
            var shaders = graphics.FindProperty("m_AlwaysIncludedShaders");
            var shader = Shader.Find("ProceduralMines/Strata");
            if (shader == null) throw new InvalidOperationException("Strata shader was not imported.");
            for (int i = 0; i < shaders.arraySize; i++)
                if (shaders.GetArrayElementAtIndex(i).objectReferenceValue == shader) return;
            int index = shaders.arraySize++;
            shaders.GetArrayElementAtIndex(index).objectReferenceValue = shader;
            graphics.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void ConfigureInput()
        {
            var input = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/InputManager.asset")[0]);
            var axes = input.FindProperty("m_Axes");
            axes.arraySize = 4;
            Axis(axes.GetArrayElementAtIndex(0), "Horizontal", "a", "d", "left", "right", 0, 0);
            Axis(axes.GetArrayElementAtIndex(1), "Vertical", "s", "w", "down", "up", 0, 1);
            Axis(axes.GetArrayElementAtIndex(2), "Mouse X", "", "", "", "", 1, 0);
            Axis(axes.GetArrayElementAtIndex(3), "Mouse Y", "", "", "", "", 1, 1);
            input.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void Axis(SerializedProperty axis, string name, string negative, string positive, string altNegative, string altPositive, int type, int dimension)
        {
            axis.FindPropertyRelative("m_Name").stringValue = name;
            axis.FindPropertyRelative("descriptiveName").stringValue = "";
            axis.FindPropertyRelative("descriptiveNegativeName").stringValue = "";
            axis.FindPropertyRelative("negativeButton").stringValue = negative;
            axis.FindPropertyRelative("positiveButton").stringValue = positive;
            axis.FindPropertyRelative("altNegativeButton").stringValue = altNegative;
            axis.FindPropertyRelative("altPositiveButton").stringValue = altPositive;
            axis.FindPropertyRelative("gravity").floatValue = type == 0 ? 3 : 0;
            axis.FindPropertyRelative("dead").floatValue = 0.001f;
            axis.FindPropertyRelative("sensitivity").floatValue = type == 0 ? 3 : 0.1f;
            axis.FindPropertyRelative("snap").boolValue = type == 0;
            axis.FindPropertyRelative("invert").boolValue = false;
            axis.FindPropertyRelative("type").intValue = type;
            axis.FindPropertyRelative("axis").intValue = dimension;
            axis.FindPropertyRelative("joyNum").intValue = 0;
        }

        [MenuItem("Procedural Mines/Build Windows sample")]
        public static void BuildWindows()
        {
            string output = Environment.GetEnvironmentVariable("PROCEDURAL_MINES_BUILD");
            if (string.IsNullOrEmpty(output)) output = Path.Combine(Application.dataPath, "../../../artifacts/Windows/ProceduralMines.exe");
            var report = BuildPipeline.BuildPlayer(new BuildPlayerOptions
            {
                scenes = new[] { "Assets/Scenes/TwoChambers.unity" },
                locationPathName = Path.GetFullPath(output),
                target = BuildTarget.StandaloneWindows64,
                options = BuildOptions.Development
            });
            string summaryPath = Path.Combine(Application.dataPath, "../../../artifacts/windows-build-summary.txt");
            Directory.CreateDirectory(Path.GetDirectoryName(summaryPath));
            File.WriteAllText(summaryPath, $"{report.summary.result}; errors={report.summary.totalErrors}; " +
                $"warnings={report.summary.totalWarnings}; size={report.summary.totalSize}; duration={report.summary.totalTime}");
            if (report.summary.result != UnityEditor.Build.Reporting.BuildResult.Succeeded)
                throw new InvalidOperationException("Windows build failed: " + report.summary.result);
        }
    }
}
