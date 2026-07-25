using System;
using System.Collections.Generic;
using UnityEngine;

public sealed class QuestManager : MonoBehaviour
{
    private const string CompletedSaveKey = "ZombiePizzaSurvival.CompletedQuests.v1";

    [Header("퀘스트 목록")]
    [Tooltip("게임에서 사용할 모든 QuestData를 등록합니다.")]
    [SerializeField] private List<QuestData> quests = new();

    [Header("참조")]
    [SerializeField] private Inventory playerInventory;
    [SerializeField] private PlayerProgressTracker progressTracker;
    [SerializeField] private RegionDiscoveryManager regionManager;

    [Header("런타임 보상")]
    [SerializeField] private int coins;
    [SerializeField] private int experience;
    [SerializeField] private int reputation;

    // 여러 퀘스트가 각자의 상태와 제한 시간을 갖고 동시에 진행됩니다.
    private readonly List<QuestRuntime> activeQuests = new();
    private readonly HashSet<string> completedQuestIds = new();
    private QuestConditionEvaluator conditionEvaluator;

    public IReadOnlyList<QuestData> Quests => quests;
    public IReadOnlyList<QuestRuntime> ActiveQuests => activeQuests;
    public int Coins => coins;
    public int Experience => experience;
    public int Reputation => reputation;

    public event Action<QuestRuntime> QuestChanged;
    public event Action QuestsChanged;
    public event Action<int, int, int> RewardsChanged;
    public event Action<string> MessageRaised;

    private void Awake()
    {
        if (playerInventory == null)
            playerInventory = FindFirstObjectByType<Inventory>();
        if (progressTracker == null)
            progressTracker = FindFirstObjectByType<PlayerProgressTracker>();
        if (regionManager == null)
            regionManager = FindFirstObjectByType<RegionDiscoveryManager>();

        // 기존 씬을 열어 별도 오브젝트를 배치하지 않아도 기록 시스템이 동작하게 보완합니다.
        if (progressTracker == null)
            progressTracker = gameObject.AddComponent<PlayerProgressTracker>();
        if (regionManager == null)
            regionManager = gameObject.AddComponent<RegionDiscoveryManager>();

        LoadCompletedQuests();
        conditionEvaluator = new QuestConditionEvaluator(
            playerInventory, progressTracker, regionManager, this);
        ValidateQuestData();
    }

    private void OnEnable()
    {
        if (playerInventory != null)
            playerInventory.Changed += RefreshAllObjectiveStates;
        if (progressTracker != null)
            progressTracker.ProgressChanged += NotifyQuestListChanged;
        if (regionManager != null)
            regionManager.RegionDiscovered += OnRegionDiscovered;
    }

    private void OnDisable()
    {
        if (playerInventory != null)
            playerInventory.Changed -= RefreshAllObjectiveStates;
        if (progressTracker != null)
            progressTracker.ProgressChanged -= NotifyQuestListChanged;
        if (regionManager != null)
            regionManager.RegionDiscovered -= OnRegionDiscovered;
    }

    private void Update()
    {
        // 역순 순회하면 시간 만료로 항목을 제거해도 다음 항목을 안전하게 검사할 수 있습니다.
        for (int i = activeQuests.Count - 1; i >= 0; i--)
        {
            QuestRuntime runtime = activeQuests[i];
            if (!IsInProgress(runtime.State))
                continue;

            runtime.RemainingTime = Mathf.Max(0f, runtime.RemainingTime - Time.deltaTime);
            QuestChanged?.Invoke(runtime);

            if (runtime.RemainingTime <= 0f)
                FailQuest(runtime, $"퀘스트에 실패했습니다: {runtime.Data.title}");
        }
    }

    public IEnumerable<QuestData> GetAvailableQuests()
    {
        foreach (QuestData quest in quests)
            if (CanAcceptQuest(quest)) yield return quest;
    }

    public void RegisterQuest(QuestData quest)
    {
        // 기존 씬의 displayedQuest 연결도 새 목록 구조에서 그대로 사용할 수 있게 합니다.
        if (quest != null && !quests.Contains(quest))
        {
            quests.Add(quest);
            QuestsChanged?.Invoke();
        }
    }

    public IEnumerable<QuestData> GetCompletedQuests()
    {
        foreach (QuestData quest in quests)
            if (quest != null && IsCompleted(quest.questId)) yield return quest;
    }

    public bool CanAcceptQuest(QuestData questData)
    {
        if (questData == null || string.IsNullOrWhiteSpace(questData.questId))
            return false;
        if (IsCompleted(questData.questId) || FindActiveQuest(questData.questId) != null)
            return false;
        return conditionEvaluator != null && conditionEvaluator.AreConditionsMet(questData);
    }

    public bool AcceptQuest(QuestData questData)
    {
        // UI를 우회해 호출해도 잠긴 퀘스트와 완료한 퀘스트는 수락되지 않습니다.
        if (!CanAcceptQuest(questData))
        {
            MessageRaised?.Invoke("현재 수락할 수 없는 퀘스트입니다.");
            return false;
        }

        QuestRuntime runtime = new(questData)
        {
            State = QuestState.Accepted,
            RemainingTime = questData.timeLimitSeconds
        };

        activeQuests.Add(runtime);
        RefreshObjectiveState(runtime);
        QuestChanged?.Invoke(runtime);
        QuestsChanged?.Invoke();
        MessageRaised?.Invoke($"퀘스트를 수락했습니다: {questData.title}");
        return true;
    }

    public QuestRuntime FindActiveQuest(string questId)
    {
        if (string.IsNullOrWhiteSpace(questId))
            return null;

        return activeQuests.Find(runtime => runtime.Data != null && runtime.Data.questId == questId);
    }

    public bool IsCompleted(string questId)
    {
        return !string.IsNullOrWhiteSpace(questId) && completedQuestIds.Contains(questId);
    }

    public int TryCompleteDeliveries(string destinationId)
    {
        int completedCount = 0;
        List<QuestRuntime> candidates = activeQuests.FindAll(runtime =>
            IsInProgress(runtime.State) &&
            string.Equals(runtime.Data.destinationId, destinationId, StringComparison.Ordinal));

        // 같은 목적지의 퀘스트가 여러 개면 인벤토리 수량이 허용하는 만큼 차례대로 완료합니다.
        foreach (QuestRuntime runtime in candidates)
        {
            QuestData data = runtime.Data;
            if (playerInventory == null || !playerInventory.HasItem(data.requiredItem, data.requiredAmount))
                continue;
            if (!playerInventory.RemoveItem(data.requiredItem, data.requiredAmount))
                continue;

            CompleteQuest(runtime);
            completedCount++;
        }

        if (completedCount == 0)
            MessageRaised?.Invoke("이곳에서 완료할 수 있는 퀘스트가 없습니다.");

        return completedCount;
    }

    // 기존 씬이나 다른 스크립트가 단일 완료 메서드를 호출해도 동작하도록 호환 메서드를 유지합니다.
    public bool TryCompleteDelivery(string destinationId)
    {
        return TryCompleteDeliveries(destinationId) > 0;
    }

    public bool CancelQuest(string questId)
    {
        QuestRuntime runtime = FindActiveQuest(questId);
        if (runtime == null)
            return false;

        runtime.State = QuestState.Cancelled;
        activeQuests.Remove(runtime);
        QuestChanged?.Invoke(runtime);
        QuestsChanged?.Invoke();
        MessageRaised?.Invoke($"퀘스트를 취소했습니다: {runtime.Data.title}");
        return true;
    }

    // 과거 코드와의 호환을 위해 첫 번째 활성 퀘스트 취소 기능을 남깁니다.
    public void CancelActiveQuest()
    {
        if (activeQuests.Count > 0)
            CancelQuest(activeQuests[0].Data.questId);
    }

    private void CompleteQuest(QuestRuntime runtime)
    {
        QuestData data = runtime.Data;
        runtime.State = QuestState.Completed;
        activeQuests.Remove(runtime);
        completedQuestIds.Add(data.questId);

        coins += data.coinReward;
        experience += data.experienceReward;
        reputation += data.reputationReward;

        SaveCompletedQuests();
        RewardsChanged?.Invoke(coins, experience, reputation);
        QuestChanged?.Invoke(runtime);
        QuestsChanged?.Invoke();
        MessageRaised?.Invoke($"퀘스트를 완료했습니다: {data.title}");
    }

    private void RefreshAllObjectiveStates()
    {
        foreach (QuestRuntime runtime in activeQuests)
            RefreshObjectiveState(runtime);

        // HasItem 조건도 인벤토리 변화에 따라 바로 목록에 반영됩니다.
        QuestsChanged?.Invoke();
    }

    private void RefreshObjectiveState(QuestRuntime runtime)
    {
        if (runtime == null || !IsInProgress(runtime.State))
            return;

        QuestData data = runtime.Data;
        bool hasRequiredItem = playerInventory != null &&
                               playerInventory.HasItem(data.requiredItem, data.requiredAmount);
        runtime.State = hasRequiredItem ? QuestState.Delivering : QuestState.Preparing;
        QuestChanged?.Invoke(runtime);
    }

    private void FailQuest(QuestRuntime runtime, string message)
    {
        runtime.State = QuestState.Failed;
        activeQuests.Remove(runtime);
        QuestChanged?.Invoke(runtime);
        QuestsChanged?.Invoke();
        MessageRaised?.Invoke(message);
    }

    private void NotifyQuestListChanged() => QuestsChanged?.Invoke();
    private void OnRegionDiscovered(string _) => QuestsChanged?.Invoke();

    private static bool IsInProgress(QuestState state)
    {
        return state == QuestState.Accepted ||
               state == QuestState.Preparing ||
               state == QuestState.Delivering;
    }

    private void SaveCompletedQuests()
    {
        CompletedQuestSaveData data = new() { questIds = new List<string>(completedQuestIds) };
        PlayerPrefs.SetString(CompletedSaveKey, JsonUtility.ToJson(data));
        PlayerPrefs.Save();
    }

    private void LoadCompletedQuests()
    {
        completedQuestIds.Clear();
        if (!PlayerPrefs.HasKey(CompletedSaveKey))
            return;

        CompletedQuestSaveData data = JsonUtility.FromJson<CompletedQuestSaveData>(
            PlayerPrefs.GetString(CompletedSaveKey));
        if (data?.questIds == null)
            return;

        foreach (string questId in data.questIds)
            if (!string.IsNullOrWhiteSpace(questId)) completedQuestIds.Add(questId);
    }

    private void ValidateQuestData()
    {
        HashSet<string> ids = new();
        foreach (QuestData quest in quests)
        {
            if (quest == null)
                continue;
            if (string.IsNullOrWhiteSpace(quest.questId))
                Debug.LogWarning($"퀘스트 ID가 비어 있습니다: {quest.name}", quest);
            else if (!ids.Add(quest.questId))
                Debug.LogWarning($"중복된 퀘스트 ID입니다: {quest.questId}", quest);
        }
    }

    [Serializable]
    private sealed class CompletedQuestSaveData
    {
        public List<string> questIds = new();
    }
}
