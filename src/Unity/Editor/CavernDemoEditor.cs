using ProceduralMines.Unity;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace ProceduralMines.Editor
{
    [CustomEditor(typeof(CavernDemo))]
    public sealed class CavernDemoEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            EditorGUILayout.HelpBox("Press Play to explore. Settings apply when you regenerate. Preview geometry is temporary and is not saved into the scene.", MessageType.Info);
            if (GUILayout.Button("Generate preview / regenerate"))
            {
                var demo = (CavernDemo)target;
                demo.Generate();
                SceneView.RepaintAll();
            }
        }

        [MenuItem("Procedural Mines/Open Two Chambers")]
        public static void OpenSample()
        {
            const string path = "Assets/Scenes/TwoChambers.unity";
            if (AssetDatabase.LoadAssetAtPath<SceneAsset>(path) == null)
            {
                Debug.LogError("Open the repository's samples/TwoChambers Unity project to use this menu.");
                return;
            }
            if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                EditorSceneManager.OpenScene(path);
        }
    }
}
