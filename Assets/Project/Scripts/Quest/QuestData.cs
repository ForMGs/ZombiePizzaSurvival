using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Quest", menuName = "Game/Quest Data")]
public sealed class QuestData : ScriptableObject
{
    [Header("Identity")]
    public string questId;
    public string title;
    public string customerName;
    [TextArea(3, 6)] public string description;
    public QuestCategory category = QuestCategory.Request;

    [Header("등장 조건")]
    [Tooltip("조건이 없으면 게임 시작부터 수주할 수 있습니다.")]
    public QuestConditionMode conditionMode = QuestConditionMode.All;
    public List<QuestCondition> conditions = new();

    [Header("Objectives")]
    [Tooltip("퀘스트 완료에 필요한 목표입니다. 모든 목표를 달성하면 퀘스트가 완료됩니다.")]
    public List<QuestObjective> objectives = new();

    [Header("Time Limit")]
    [Tooltip("0이면 제한 시간이 없습니다.")]
    [Min(0f)] public float timeLimitSeconds;

    [Header("Legacy Delivery (기존 데이터 호환용)")]
    [Tooltip("Objectives가 비어 있는 기존 배달 퀘스트에만 사용됩니다.")]
    public string destinationId;
    public string destinationName;
    public ItemData requiredItem;
    [Min(1)] public int requiredAmount = 1;

    [Header("Rewards")]
    [Min(0)] public int coinReward = 250;
    [Min(0)] public int experienceReward = 150;
    [Min(0)] public int reputationReward = 2;
}
