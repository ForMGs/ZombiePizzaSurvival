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

    [Header("등장 조건")]
    [Tooltip("조건이 없으면 게임 시작부터 수주할 수 있습니다.")]
    public QuestConditionMode conditionMode = QuestConditionMode.All;
    public List<QuestCondition> conditions = new();

    [Header("Delivery")]
    public string destinationId;
    public string destinationName;
    [Min(1f)] public float timeLimitSeconds = 900f;
    public ItemData requiredItem;
    [Min(1)] public int requiredAmount = 1;

    [Header("Rewards")]
    [Min(0)] public int coinReward = 250;
    [Min(0)] public int experienceReward = 150;
    [Min(0)] public int reputationReward = 2;
}
