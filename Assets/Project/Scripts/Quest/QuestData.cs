using UnityEngine;

[CreateAssetMenu(fileName = "New Quest", menuName = "Game/Quest Data")]
public sealed class QuestData : ScriptableObject
{
    [Header("Identity")]
    public string questId;
    public string title;
    public string customerName;
    [TextArea(3, 6)] public string description;

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
