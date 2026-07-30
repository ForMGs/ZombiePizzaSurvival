using System.Linq;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class TutorialSceneSetup
{
    private const float InteriorWidth = 30f;
    private const float InteriorDepth = 20f;
    private const float InteriorHeight = 3f;
    private const float WallThickness = 0.2f;
    private const float FloorThickness = 0.2f;
    private const float DoorWidth = 4f;
    private static readonly Vector3 TopViewCameraPosition = new(0f, 18f, -12f);
    private static readonly Vector3 TopViewCameraEulerAngles = new(55f, 0f, 0f);

    private const string IntroScenePath = "Assets/Project/Scenes/IntroScene.unity";
    private const string TutorialScenePath = "Assets/Project/Scenes/TutorialScene.unity";
    private const string MainScenePath = "Assets/Project/Scenes/MainScene.unity";
    private const string MaterialFolderPath = "Assets/Project/Materials/Tutorial";

    // Unity 배치 모드와 메뉴에서 같은 초기 설정을 실행할 수 있게 공개 진입점을 둡니다.
    [MenuItem("Tools/Zombie Pizza/Setup Tutorial Scene")]
    public static void Setup()
    {
        CreateTutorialSceneIfMissing();
        ConfigureTutorialInterior();
        ImportGameplayObjectsFromMainScene();
        ConnectIntroToTutorial();
        RegisterIntroAndTutorialScenes();
        AssetDatabase.SaveAssets();
        Debug.Log("TutorialScene 생성 및 IntroScene 연결이 완료되었습니다.");
    }

    private static void CreateTutorialSceneIfMissing()
    {
        if (AssetDatabase.LoadAssetAtPath<SceneAsset>(TutorialScenePath) != null)
            return;

        Scene tutorialScene = EditorSceneManager.NewScene(
            NewSceneSetup.EmptyScene,
            NewSceneMode.Single);

        CreateCamera();
        CreateDirectionalLight();
        EditorSceneManager.SaveScene(tutorialScene, TutorialScenePath);
    }

    // 별도의 건물 프리팹 없이도 피자가게 실내처럼 느껴지도록
    // X축 30m x Z축 20m 크기의 바닥과 벽, 따뜻한 조명을 구성합니다.
    // 천장은 만들지 않아 3D 탑뷰 카메라에서 내부 전체가 보이게 합니다.
    private static void ConfigureTutorialInterior()
    {
        Scene tutorialScene = EditorSceneManager.OpenScene(
            TutorialScenePath,
            OpenSceneMode.Single);

        // 이전 설정에서 배치한 피자가게 외부 프리팹 인스턴스만 씬에서 제거합니다.
        // 프로젝트에 있는 프리팹 원본 에셋은 삭제하지 않습니다.
        GameObject oldPizzaStore = tutorialScene.GetRootGameObjects()
            .FirstOrDefault(root => root.name == "PizzaStore");
        if (oldPizzaStore != null)
            Object.DestroyImmediate(oldPizzaStore);

        GameObject interiorRoot = FindOrCreateRoot("TutorialInterior", tutorialScene);
        ClearGeneratedRoomParts(interiorRoot.transform);

        Material floorMaterial = GetOrCreateMaterial(
            "TutorialFloor",
            new Color(0.32f, 0.12f, 0.08f));
        Material wallMaterial = GetOrCreateMaterial(
            "TutorialWall",
            new Color(0.82f, 0.63f, 0.42f));
        CreateRoomPart(
            interiorRoot.transform,
            "Floor",
            new Vector3(0f, -FloorThickness * 0.5f, 0f),
            new Vector3(InteriorWidth, FloorThickness, InteriorDepth),
            floorMaterial);
        CreateRoomPart(
            interiorRoot.transform,
            "BackWall",
            new Vector3(0f, InteriorHeight * 0.5f, InteriorDepth * 0.5f),
            new Vector3(InteriorWidth, InteriorHeight, WallThickness),
            wallMaterial);
        CreateRoomPart(
            interiorRoot.transform,
            "LeftWall",
            new Vector3(-InteriorWidth * 0.5f, InteriorHeight * 0.5f, 0f),
            new Vector3(WallThickness, InteriorHeight, InteriorDepth),
            wallMaterial);
        CreateRoomPart(
            interiorRoot.transform,
            "RightWall",
            new Vector3(InteriorWidth * 0.5f, InteriorHeight * 0.5f, 0f),
            new Vector3(WallThickness, InteriorHeight, InteriorDepth),
            wallMaterial);

        // 앞 벽 중앙에는 이후 출구로 사용할 4m 폭의 빈 공간을 남깁니다.
        float frontSegmentWidth = (InteriorWidth - DoorWidth) * 0.5f;
        float frontSegmentOffset = DoorWidth * 0.5f + frontSegmentWidth * 0.5f;
        CreateRoomPart(
            interiorRoot.transform,
            "FrontWallLeft",
            new Vector3(-frontSegmentOffset, InteriorHeight * 0.5f, -InteriorDepth * 0.5f),
            new Vector3(frontSegmentWidth, InteriorHeight, WallThickness),
            wallMaterial);
        CreateRoomPart(
            interiorRoot.transform,
            "FrontWallRight",
            new Vector3(frontSegmentOffset, InteriorHeight * 0.5f, -InteriorDepth * 0.5f),
            new Vector3(frontSegmentWidth, InteriorHeight, WallThickness),
            wallMaterial);

        GameObject tutorialStart = FindOrCreateChild(interiorRoot.transform, "TutorialStart");
        tutorialStart.transform.SetLocalPositionAndRotation(
            new Vector3(0f, 0f, 4f),
            Quaternion.Euler(0f, 180f, 0f));
        tutorialStart.transform.localScale = Vector3.one;

        Camera camera = Object.FindFirstObjectByType<Camera>(FindObjectsInactive.Include);
        if (camera != null)
        {
            camera.transform.SetPositionAndRotation(
                TopViewCameraPosition,
                Quaternion.Euler(TopViewCameraEulerAngles));
            camera.fieldOfView = 50f;
        }

        ConfigureInteriorLights(interiorRoot.transform);
        EditorSceneManager.MarkSceneDirty(tutorialScene);
        EditorSceneManager.SaveScene(tutorialScene);
    }

    // 메인 씬에서 서로 참조하는 플레이어와 UI 루트들을 한 번에 옮깁니다.
    // MainScene은 저장하지 않고 닫으므로 원본 씬 구성에는 영향을 주지 않습니다.
    private static void ImportGameplayObjectsFromMainScene()
    {
        Scene tutorialScene = EditorSceneManager.OpenScene(
            TutorialScenePath,
            OpenSceneMode.Single);
        Transform tutorialStart = tutorialScene.GetRootGameObjects()
            .FirstOrDefault(root => root.name == "TutorialInterior")
            ?.transform.Find("TutorialStart");
        if (tutorialStart == null)
            throw new MissingReferenceException("TutorialStart를 찾을 수 없습니다.");

        string[] gameplayRootNames =
        {
            "Player",
            "GameHUDCanvas",
            "UIManager",
            "EventSystem"
        };

        // 설정을 다시 실행해도 같은 오브젝트가 중복 생성되지 않게 기존 복사본을 제거합니다.
        foreach (string rootName in gameplayRootNames)
        {
            GameObject existing = tutorialScene.GetRootGameObjects()
                .FirstOrDefault(root => root.name == rootName);
            if (existing != null)
                Object.DestroyImmediate(existing);
        }

        Scene mainScene = EditorSceneManager.OpenScene(
            MainScenePath,
            OpenSceneMode.Additive);
        GameObject[] mainRoots = mainScene.GetRootGameObjects();

        foreach (string rootName in gameplayRootNames)
        {
            GameObject source = mainRoots.FirstOrDefault(root => root.name == rootName);
            if (source == null)
                throw new MissingReferenceException(
                    $"MainScene에서 {rootName} 오브젝트를 찾을 수 없습니다.");

            // 네 루트를 모두 같은 씬으로 이동하므로 Player와 HUD 사이의 직렬화 참조가 유지됩니다.
            SceneManager.MoveGameObjectToScene(source, tutorialScene);
        }

        GameObject player = tutorialScene.GetRootGameObjects()
            .First(root => root.name == "Player");
        player.transform.SetPositionAndRotation(
            tutorialStart.position + Vector3.up * 1.137f,
            tutorialStart.rotation);
        ConfigureCameraFollow(tutorialScene, player.transform);

        EditorSceneManager.MarkSceneDirty(tutorialScene);
        EditorSceneManager.SaveScene(tutorialScene);

        // 이동으로 MainScene이 dirty 상태가 되지만 저장하지 않고 닫아 원본을 보존합니다.
        EditorSceneManager.CloseScene(mainScene, true);
    }

    // 메인 씬과 같은 카메라 추적 컴포넌트를 사용하되,
    // 넓어진 30m x 20m 튜토리얼 공간에 맞춰 더 높은 오프셋을 적용합니다.
    private static void ConfigureCameraFollow(Scene tutorialScene, Transform player)
    {
        Camera camera = tutorialScene.GetRootGameObjects()
            .FirstOrDefault(root => root.name == "Main Camera")
            ?.GetComponent<Camera>();
        if (camera == null)
            throw new MissingReferenceException("TutorialScene에서 Main Camera를 찾을 수 없습니다.");

        TopDownCameraFollow cameraFollow =
            camera.GetComponent<TopDownCameraFollow>();
        if (cameraFollow == null)
            cameraFollow = camera.gameObject.AddComponent<TopDownCameraFollow>();

        SerializedObject serializedFollow = new(cameraFollow);
        serializedFollow.FindProperty("target").objectReferenceValue = player;
        serializedFollow.FindProperty("offset").vector3Value = TopViewCameraPosition;
        serializedFollow.FindProperty("followSpeed").floatValue = 8f;
        serializedFollow.ApplyModifiedPropertiesWithoutUndo();

        // 첫 프레임에 멀리서 보간되어 들어오지 않도록 시작 위치도 즉시 맞춥니다.
        camera.transform.position = player.position + TopViewCameraPosition;
        camera.transform.LookAt(player.position);
    }

    private static void ClearGeneratedRoomParts(Transform interiorRoot)
    {
        string[] generatedNames =
        {
            "Floor", "Ceiling", "BackWall", "LeftWall", "RightWall",
            "FrontWallLeft", "FrontWallRight", "InteriorLightLeft", "InteriorLightRight"
        };

        foreach (string objectName in generatedNames)
        {
            Transform existing = interiorRoot.Find(objectName);
            if (existing != null)
                Object.DestroyImmediate(existing.gameObject);
        }
    }

    private static void CreateRoomPart(
        Transform parent,
        string objectName,
        Vector3 localPosition,
        Vector3 localScale,
        Material material)
    {
        GameObject part = GameObject.CreatePrimitive(PrimitiveType.Cube);
        part.name = objectName;
        part.transform.SetParent(parent, false);
        part.transform.localPosition = localPosition;
        part.transform.localScale = localScale;
        part.GetComponent<MeshRenderer>().sharedMaterial = material;
    }

    private static Material GetOrCreateMaterial(string materialName, Color color)
    {
        if (!AssetDatabase.IsValidFolder(MaterialFolderPath))
        {
            Directory.CreateDirectory(MaterialFolderPath);
            AssetDatabase.Refresh();
        }

        string materialPath = $"{MaterialFolderPath}/{materialName}.mat";
        Material material = AssetDatabase.LoadAssetAtPath<Material>(materialPath);
        if (material == null)
        {
            Shader shader = Shader.Find("Universal Render Pipeline/Lit")
                ?? Shader.Find("Standard");
            material = new Material(shader) { name = materialName };
            AssetDatabase.CreateAsset(material, materialPath);
        }

        material.color = color;
        EditorUtility.SetDirty(material);
        return material;
    }

    private static void ConfigureInteriorLights(Transform interiorRoot)
    {
        CreateInteriorLight(interiorRoot, "InteriorLightLeft", new Vector3(-5f, 3.2f, 0f));
        CreateInteriorLight(interiorRoot, "InteriorLightRight", new Vector3(5f, 3.2f, 0f));
    }

    private static void CreateInteriorLight(Transform parent, string objectName, Vector3 position)
    {
        GameObject lightObject = new(objectName, typeof(Light));
        lightObject.transform.SetParent(parent, false);
        lightObject.transform.localPosition = position;

        Light light = lightObject.GetComponent<Light>();
        light.type = LightType.Point;
        light.color = new Color(1f, 0.72f, 0.45f);
        light.intensity = 7f;
        light.range = 14f;
        light.shadows = LightShadows.Soft;
    }

    private static GameObject FindOrCreateRoot(string objectName, Scene scene)
    {
        GameObject existing = scene.GetRootGameObjects()
            .FirstOrDefault(root => root.name == objectName);
        if (existing != null)
            return existing;

        GameObject created = new(objectName);
        SceneManager.MoveGameObjectToScene(created, scene);
        return created;
    }

    private static GameObject FindOrCreateChild(Transform parent, string objectName)
    {
        Transform existing = parent.Find(objectName);
        if (existing != null)
            return existing.gameObject;

        GameObject created = new(objectName);
        created.transform.SetParent(parent, false);
        return created;
    }

    private static void CreateCamera()
    {
        GameObject cameraObject = new("Main Camera", typeof(Camera), typeof(AudioListener));
        cameraObject.tag = "MainCamera";
        cameraObject.transform.SetPositionAndRotation(
            TopViewCameraPosition,
            Quaternion.Euler(TopViewCameraEulerAngles));
        cameraObject.GetComponent<Camera>().fieldOfView = 50f;
    }

    private static void CreateDirectionalLight()
    {
        GameObject lightObject = new("Directional Light", typeof(Light));
        Light light = lightObject.GetComponent<Light>();
        light.type = LightType.Directional;
        light.intensity = 1f;
        lightObject.transform.rotation = Quaternion.Euler(50f, -30f, 0f);
    }

    private static void ConnectIntroToTutorial()
    {
        Scene introScene = EditorSceneManager.OpenScene(IntroScenePath, OpenSceneMode.Single);
        IntroController controller =
            Object.FindFirstObjectByType<IntroController>(FindObjectsInactive.Include);
        if (controller == null)
            throw new MissingReferenceException("IntroScene에서 IntroController를 찾을 수 없습니다.");

        SerializedObject serializedController = new(controller);
        SerializedProperty nextSceneName =
            serializedController.FindProperty("nextSceneName");
        nextSceneName.stringValue = "TutorialScene";
        serializedController.ApplyModifiedPropertiesWithoutUndo();

        EditorSceneManager.MarkSceneDirty(introScene);
        EditorSceneManager.SaveScene(introScene);
    }

    private static void RegisterIntroAndTutorialScenes()
    {
        string[] requiredScenePaths = { IntroScenePath, TutorialScenePath };
        EditorBuildSettingsScene[] otherScenes = EditorBuildSettings.scenes
            .Where(scene => !requiredScenePaths.Contains(scene.path))
            .ToArray();

        EditorBuildSettings.scenes = requiredScenePaths
            .Select(path => new EditorBuildSettingsScene(path, true))
            .Concat(otherScenes)
            .ToArray();
    }
}
