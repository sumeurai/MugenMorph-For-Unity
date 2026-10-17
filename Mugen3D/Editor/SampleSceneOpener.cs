using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Mugen3D.Editor
{
    [InitializeOnLoad]
    static class SampleSceneOpener
    {
        const string DestFolder = "Assets/Mugen3D Samples";

        static SampleSceneOpener()
        {
            EditorSceneManager.sceneOpened += (_, __) => EditorApplication.delayCall += EnsureHeadInOpenSample;
        }

        [MenuItem("Tools/Mugen3D/Samples/Open HDRP Scene (MugenMorph)", false, 10)]
        static void OpenHdrp()
        {
            Open("MugenMorph.unity");
        }

        [MenuItem("Tools/Mugen3D/Samples/Open URP Scene (MugenMorph_URP)", false, 11)]
        static void OpenUrp()
        {
            Open("MugenMorph_URP.unity");
        }

        [MenuItem("Tools/Mugen3D/Samples/Open Built-in Scene (MugenMorph_BuiltIn)", false, 12)]
        static void OpenBuiltIn()
        {
            Open("MugenMorph_BuiltIn.unity");
        }

        static void Open(string sceneFile)
        {
            string srcAsset = Mugen3DEditorPaths.RootAssetPath.TrimEnd('/') + "/Samples/Scenes/" + sceneFile;
            string srcDisk = ToDiskPath(srcAsset);
            if (string.IsNullOrEmpty(srcDisk) || !File.Exists(srcDisk))
            {
                EditorUtility.DisplayDialog(
                    "Mugen3D Sample",
                    "Could not find " + srcAsset + ".\nReinstall the package or copy Mugen3D into Assets.",
                    "OK");
                return;
            }

            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            {
                return;
            }

            if (!Mugen3DEditorPaths.IsPackagePath(srcAsset))
            {
                EditorSceneManager.OpenScene(srcAsset);
                return;
            }

            if (!AssetDatabase.IsValidFolder(DestFolder))
            {
                AssetDatabase.CreateFolder("Assets", "Mugen3D Samples");
            }

            string destAsset = DestFolder + "/" + sceneFile;
            string destDisk = ToDiskPath(destAsset);
            if (!File.Exists(destDisk))
            {
                File.Copy(srcDisk, destDisk, false);
                AssetDatabase.Refresh();
                EditorUtility.DisplayDialog(
                    "Mugen3D Sample",
                    "Git packages are read-only, so the scene was copied to:\n" + destAsset + "\n\nOpen that copy. Do not double-click the scene inside Packages.",
                    "OK");
            }

            EditorSceneManager.OpenScene(destAsset);
            EnsureHeadInOpenSample();
        }

        static void EnsureHeadInOpenSample()
        {
            if (Application.isPlaying)
            {
                return;
            }

            var scene = EditorSceneManager.GetActiveScene();
            if (!scene.IsValid() || string.IsNullOrEmpty(scene.path))
            {
                return;
            }

            string sceneName = Path.GetFileNameWithoutExtension(scene.path);
            if (sceneName != "MugenMorph" && sceneName != "MugenMorph_URP" && sceneName != "MugenMorph_BuiltIn")
            {
                return;
            }

            if (Object.FindObjectsOfType<SkinnedMeshRenderer>().Length > 0)
            {
                return;
            }

            string assetPath = Mugen3DEditorPaths.RootAssetPath.TrimEnd('/') + "/Samples/Models/ARKit_head.fbx";
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
            if (prefab == null)
            {
                return;
            }

            GameObject instance = PrefabUtility.InstantiatePrefab(prefab, scene) as GameObject;
            if (instance == null)
            {
                return;
            }

            instance.name = "ARKit_head";
            instance.transform.SetPositionAndRotation(Vector3.zero, Quaternion.Euler(0f, 180f, 0f));
            EditorSceneManager.MarkSceneDirty(scene);
        }

        static string ToDiskPath(string assetPath)
        {
            if (string.IsNullOrEmpty(assetPath))
            {
                return null;
            }

            string projectRoot = Path.GetDirectoryName(Application.dataPath);
            return Path.GetFullPath(Path.Combine(projectRoot, assetPath.Replace('/', Path.DirectorySeparatorChar)));
        }
    }
}
