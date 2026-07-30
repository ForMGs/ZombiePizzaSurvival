using System.Collections.Generic;
using UnityEngine;

// Target Quest가 수락되는 순간 연결된 ZombieSpawner들을 한 번씩 실행합니다.
public sealed class QuestZombieSpawnOnAccept : MonoBehaviour
{
    [SerializeField] private QuestManager questManager;
    [SerializeField] private QuestData targetQuest;
    [SerializeField] private List<ZombieSpawner> spawners = new();

    private void Awake()
    {
        if (questManager == null)
            questManager = FindFirstObjectByType<QuestManager>();
    }

    private void OnEnable()
    {
        if (questManager != null)
            questManager.QuestAccepted += OnQuestAccepted;
    }

    private void OnDisable()
    {
        if (questManager != null)
            questManager.QuestAccepted -= OnQuestAccepted;
    }

    private void OnQuestAccepted(QuestRuntime runtime)
    {
        if (runtime == null || runtime.Data != targetQuest)
            return;

        int spawnedCount = 0;
        foreach (ZombieSpawner spawner in spawners)
        {
            if (spawner != null && spawner.SpawnZombie())
                spawnedCount++;
        }

        Debug.Log(
            $"퀘스트 수락 스폰: {targetQuest.questId}, 생성={spawnedCount}/{spawners.Count}",
            this);
    }
}

