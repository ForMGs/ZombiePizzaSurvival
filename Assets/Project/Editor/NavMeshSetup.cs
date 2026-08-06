using Unity.AI.Navigation;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;

public static class NavMeshSetup
{
    private static readonly string[] GameplayScenes =
    {
        "Assets/Project/Scenes/TutorialScene.unity",
        "Assets/Project/Scenes/MainScene.unity",
        "Assets/Project/Scenes/MainScene_test.unity"
    };

    [MenuItem("Tools/Zombie Pizza/Setup and Bake NavMesh")]
    public static void SetupAndBake()
    {
        ConfigureZombiePrefab();

        foreach (string scenePath in GameplayScenes)
        {
            Scene scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
            NavMeshSurface surface = Object.FindFirstObjectByType<NavMeshSurface>();

            if (surface == null)
            {
                GameObject navMeshRoot = new GameObject("NavMesh Surface");
                surface = navMeshRoot.AddComponent<NavMeshSurface>();
            }

            surface.collectObjects = CollectObjects.All;
            surface.useGeometry = NavMeshCollectGeometry.PhysicsColliders;
            surface.layerMask = ~0;
            surface.BuildNavMesh();

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
        }

        AssetDatabase.SaveAssets();
        Debug.Log("NavMesh setup and bake completed for all zombie gameplay scenes.");
    }

    private static void ConfigureZombiePrefab()
    {
        const string prefabPath = "Assets/Project/Prefabs/Zombies/Zombie.prefab";
        GameObject root = PrefabUtility.LoadPrefabContents(prefabPath);

        NavMeshAgent agent = root.GetComponent<NavMeshAgent>();
        if (agent == null)
            agent = root.AddComponent<NavMeshAgent>();

        agent.radius = 0.5f;
        agent.height = 2f;
        agent.speed = 3f;
        agent.acceleration = 12f;
        agent.angularSpeed = 360f;
        agent.stoppingDistance = 1.5f;
        agent.autoBraking = true;

        Rigidbody body = root.GetComponent<Rigidbody>();
        if (body != null)
        {
            body.isKinematic = true;
            body.useGravity = false;
        }

        PrefabUtility.SaveAsPrefabAsset(root, prefabPath);
        PrefabUtility.UnloadPrefabContents(root);
    }
}
