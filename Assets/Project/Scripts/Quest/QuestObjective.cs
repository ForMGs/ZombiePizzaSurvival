using System;
using UnityEngine;

public enum QuestCategory
{
    Main,
    Side,
    Request,
    Tutorial
}

public enum QuestObjectiveType
{
    ReachRegion,
    KillZombie,
    AcquireItem,
    DeliverItem,
    OpenUI,
    ClickUI,
    Interact
}

[Serializable]
public sealed class QuestObjective
{
    [SerializeField] private QuestObjectiveType type;
    [Tooltip("지역 ID, 좀비 ID, UI ID, 상호작용 ID처럼 목표 대상을 구분하는 값입니다.")]
    [SerializeField] private string targetId;
    [SerializeField] private ItemData item;
    [Min(1)] [SerializeField] private int requiredAmount = 1;
    [Tooltip("비워 두면 목표 종류에 맞는 문구가 자동으로 표시됩니다.")]
    [SerializeField] private string description;

    public QuestObjectiveType Type => type;
    public string TargetId => targetId;
    public ItemData Item => item;
    public int RequiredAmount => Mathf.Max(1, requiredAmount);
    public string Description => description;

    public bool MatchesTarget(string value)
    {
        return string.IsNullOrWhiteSpace(targetId) ||
               string.Equals(targetId, value, StringComparison.Ordinal);
    }
}

