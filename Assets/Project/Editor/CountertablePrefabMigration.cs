using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class CountertablePrefabMigration
{
    private const string ModelPath = "Assets/Project/Art/Props/SM_Countertable_01.fbx";
    private const string PrefabPath = "Assets/Project/Art/Props/SM_Countertable_01.prefab";

    private static readonly string[] ScenePaths =
    {
        "Assets/Project/Scenes/TutorialScene.unity",
        "Assets/Project/Scenes/PizzStore.unity"
    };

    [MenuItem("Tools/Zombie Pizza/Migrate Countertables To Prefab")]
    public static void Run()
    {
        GameObject replacementPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath);
        if (replacementPrefab == null)
            throw new System.InvalidOperationException($"Prefab not found: {PrefabPath}");

        int replacementCount = 0;

        foreach (string scenePath in ScenePaths)
        {
            Scene scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
            var instanceRoots = new List<GameObject>();

            foreach (GameObject root in scene.GetRootGameObjects())
            {
                foreach (Transform transform in root.GetComponentsInChildren<Transform>(true))
                {
                    GameObject candidate = transform.gameObject;
                    if (!PrefabUtility.IsAnyPrefabInstanceRoot(candidate))
                        continue;

                    if (PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(candidate) == ModelPath)
                        instanceRoots.Add(candidate);
                }
            }

            foreach (GameObject instanceRoot in instanceRoots)
            {
                PrefabUtility.ReplacePrefabAssetOfPrefabInstance(
                    instanceRoot,
                    replacementPrefab,
                    InteractionMode.AutomatedAction);
                replacementCount++;
            }

            if (instanceRoots.Count > 0)
            {
                EditorSceneManager.MarkSceneDirty(scene);
                EditorSceneManager.SaveScene(scene);
            }
        }

        AssetDatabase.SaveAssets();
        Debug.Log($"Migrated {replacementCount} SM_Countertable_01 instances to {PrefabPath}");
    }
}
