// 조건 판단을 한 곳에 모아 QuestManager와 UI가 같은 기준을 사용하게 합니다.
public sealed class QuestConditionEvaluator
{
    private readonly Inventory inventory;
    private readonly PlayerProgressTracker progressTracker;
    private readonly RegionDiscoveryManager regionManager;
    private readonly QuestManager questManager;

    public QuestConditionEvaluator(Inventory inventory, PlayerProgressTracker progressTracker,
        RegionDiscoveryManager regionManager, QuestManager questManager)
    {
        this.inventory = inventory;
        this.progressTracker = progressTracker;
        this.regionManager = regionManager;
        this.questManager = questManager;
    }

    public bool AreConditionsMet(QuestData quest)
    {
        if (quest == null)
            return false;
        if (quest.conditions == null || quest.conditions.Count == 0)
            return true;

        if (quest.conditionMode == QuestConditionMode.All)
        {
            foreach (QuestCondition condition in quest.conditions)
                if (!IsConditionMet(condition)) return false;
            return true;
        }

        foreach (QuestCondition condition in quest.conditions)
            if (IsConditionMet(condition)) return true;
        return false;
    }

    private bool IsConditionMet(QuestCondition condition)
    {
        if (condition == null)
            return false;

        return condition.Type switch
        {
            QuestConditionType.HasItem => inventory != null &&
                inventory.HasItem(condition.Item, condition.RequiredAmount),
            QuestConditionType.ItemAcquired => progressTracker != null &&
                progressTracker.HasAcquired(condition.Item, condition.RequiredAmount),
            QuestConditionType.RegionReached => regionManager != null &&
                regionManager.HasVisited(condition.RegionId),
            QuestConditionType.QuestCompleted => condition.RequiredQuest != null &&
                questManager.IsCompleted(condition.RequiredQuest.questId),
            QuestConditionType.MinimumReputation => questManager.Reputation >= condition.MinimumReputation,
            _ => false
        };
    }
}
