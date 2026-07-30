using UnityEngine;

public sealed class QuestStartSpawner : MonoBehaviour
{
    [Header("Quest")]
    [SerializeField] private QuestManager questManager;
    [SerializeField] private QuestData triggerQuest;

    [Header("Spawn")]
    [SerializeField] private GameObject prefab;
    [SerializeField] private Transform spawnPoint;

    private GameObject spawnedObject;
    private bool hasSpawned;

    private void Awake()
    {
        if (questManager == null)
            questManager = FindFirstObjectByType<QuestManager>();
    }

    private void OnEnable()
    {
        if (questManager != null)
            questManager.QuestChanged += OnQuestChanged;
    }

    private void Start()
    {
        // 스크립트보다 퀘스트가 먼저 수주된 경우에도 대응
        if (triggerQuest != null &&
            questManager?.FindActiveQuest(triggerQuest.questId) != null)
        {
            Spawn();
        }
    }

    private void OnDisable()
    {
        if (questManager != null)
            questManager.QuestChanged -= OnQuestChanged;
    }

    private void OnQuestChanged(QuestRuntime runtime)
    {
        if (runtime == null || runtime.Data != triggerQuest)
            return;

        if (runtime.State == QuestState.Preparing ||
            runtime.State == QuestState.Accepted)
        {
            Spawn();
        }
    }

    private void Spawn()
    {
        if (hasSpawned || prefab == null || spawnPoint == null)
            return;

        spawnedObject = Instantiate(
            prefab,
            spawnPoint.position,
            spawnPoint.rotation);

        hasSpawned = true;
    }
}