using System;
using UnityEngine;

public enum QuestConditionType
{
    HasItem,
    ItemAcquired,
    RegionReached,
    QuestCompleted,
    MinimumReputation
}

public enum QuestConditionMode
{
    All,
    Any
}

// QuestData의 Inspector에서 퀘스트 해금 조건 하나를 설정하는 데이터입니다.
[Serializable]
public sealed class QuestCondition
{
    [SerializeField] private QuestConditionType type;

    [Header("아이템 조건")]
    [SerializeField] private ItemData item;
    [Min(1)] [SerializeField] private int requiredAmount = 1;

    [Header("지역 방문 조건")]
    [SerializeField] private string regionId;

    [Header("선행 퀘스트 조건")]
    [SerializeField] private QuestData requiredQuest;

    [Header("평판 조건")]
    [Min(0)] [SerializeField] private int minimumReputation;

    public QuestConditionType Type => type;
    public ItemData Item => item;
    public int RequiredAmount => requiredAmount;
    public string RegionId => regionId;
    public QuestData RequiredQuest => requiredQuest;
    public int MinimumReputation => minimumReputation;
}
