using System;
using UnityEngine;

public sealed class QuestManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Inventory playerInventory;

    [Header("Runtime Rewards")]
    [SerializeField] private int coins;
    [SerializeField] private int experience;
    [SerializeField] private int reputation;

    public QuestRuntime ActiveQuest { get; private set; }
    public int Coins => coins;
    public int Experience => experience;
    public int Reputation => reputation;

    public event Action<QuestRuntime> QuestChanged;
    public event Action<int, int, int> RewardsChanged;
    public event Action<string> MessageRaised;

    private void Awake()
    {
        if (playerInventory == null)
            playerInventory = FindFirstObjectByType<Inventory>();
    }

    private void OnEnable()
    {
        if (playerInventory != null)
            playerInventory.Changed += RefreshObjectiveState;
    }

    private void OnDisable()
    {
        if (playerInventory != null)
            playerInventory.Changed -= RefreshObjectiveState;
    }

    private void Update()
    {
        if (ActiveQuest == null || !IsInProgress(ActiveQuest.State))
            return;

        ActiveQuest.RemainingTime = Mathf.Max(0f, ActiveQuest.RemainingTime - Time.deltaTime);

        if (ActiveQuest.RemainingTime <= 0f)
        {
            FailActiveQuest("Order failed: Delivery time expired.");
            return;
        }

        QuestChanged?.Invoke(ActiveQuest);
    }

    public bool AcceptQuest(QuestData questData)
    {
        if (questData == null)
        {
            MessageRaised?.Invoke("Quest data is missing.");
            return false;
        }

        if (ActiveQuest != null && IsInProgress(ActiveQuest.State))
        {
            MessageRaised?.Invoke("Another delivery is already in progress.");
            return false;
        }

        ActiveQuest = new QuestRuntime(questData)
        {
            State = QuestState.Accepted,
            RemainingTime = questData.timeLimitSeconds
        };

        RefreshObjectiveState();
        MessageRaised?.Invoke($"Order accepted: {questData.title}");
        return true;
    }

    public bool TryCompleteDelivery(string destinationId)
    {
        if (ActiveQuest == null || !IsInProgress(ActiveQuest.State))
        {
            MessageRaised?.Invoke("There is no active delivery.");
            return false;
        }

        QuestData data = ActiveQuest.Data;

        if (!string.Equals(data.destinationId, destinationId, StringComparison.Ordinal))
        {
            MessageRaised?.Invoke("This is not the active delivery destination.");
            return false;
        }

        if (playerInventory == null || !playerInventory.HasItem(data.requiredItem, data.requiredAmount))
        {
            MessageRaised?.Invoke("Required pizza is missing.");
            RefreshObjectiveState();
            return false;
        }

        if (!playerInventory.RemoveItem(data.requiredItem, data.requiredAmount))
            return false;

        ActiveQuest.State = QuestState.Completed;
        coins += data.coinReward;
        experience += data.experienceReward;
        reputation += data.reputationReward;

        RewardsChanged?.Invoke(coins, experience, reputation);
        QuestChanged?.Invoke(ActiveQuest);
        MessageRaised?.Invoke($"Order complete: {data.title}");
        return true;
    }

    public void CancelActiveQuest()
    {
        if (ActiveQuest == null || !IsInProgress(ActiveQuest.State))
            return;

        ActiveQuest.State = QuestState.Cancelled;
        QuestChanged?.Invoke(ActiveQuest);
        MessageRaised?.Invoke("Order cancelled.");
    }

    private void RefreshObjectiveState()
    {
        if (ActiveQuest == null || !IsInProgress(ActiveQuest.State))
            return;

        QuestData data = ActiveQuest.Data;
        bool hasRequiredItem = playerInventory != null &&
                               playerInventory.HasItem(data.requiredItem, data.requiredAmount);

        ActiveQuest.State = hasRequiredItem ? QuestState.Delivering : QuestState.Preparing;
        QuestChanged?.Invoke(ActiveQuest);
    }

    private void FailActiveQuest(string message)
    {
        ActiveQuest.State = QuestState.Failed;
        QuestChanged?.Invoke(ActiveQuest);
        MessageRaised?.Invoke(message);
    }

    private static bool IsInProgress(QuestState state)
    {
        return state == QuestState.Accepted ||
               state == QuestState.Preparing ||
               state == QuestState.Delivering;
    }
}
