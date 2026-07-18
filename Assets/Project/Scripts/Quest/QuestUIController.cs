using UnityEngine;
using UnityEngine.UIElements;

[RequireComponent(typeof(UIDocument))]
public sealed class QuestUIController : MonoBehaviour
{
    [SerializeField] private QuestManager questManager;
    [SerializeField] private QuestData displayedQuest;

    private Button acceptButton;
    private Label titleLabel;
    private Label locationLabel;
    private Label descriptionLabel;
    private Label requirementLabel;
    private Label coinRewardLabel;
    private Label experienceRewardLabel;
    private Label reputationRewardLabel;

    private void OnEnable()
    {
        if (questManager == null)
            questManager = FindFirstObjectByType<QuestManager>();

        VisualElement root = GetComponent<UIDocument>().rootVisualElement;
        acceptButton = root.Q<Button>("AcceptQuestButton");
        titleLabel = root.Q<Label>("QuestDetailTitle");
        locationLabel = root.Q<Label>("QuestLocation");
        descriptionLabel = root.Q<Label>("QuestDescription");
        requirementLabel = root.Q<Label>("RequiredItemText");
        coinRewardLabel = root.Q<Label>("CoinReward");
        experienceRewardLabel = root.Q<Label>("ExpReward");
        reputationRewardLabel = root.Q<Label>("ReputationReward");

        if (acceptButton != null)
            acceptButton.clicked += AcceptDisplayedQuest;

        if (questManager != null)
            questManager.QuestChanged += RefreshRuntime;

        RefreshStaticData();
    }

    private void OnDisable()
    {
        if (acceptButton != null)
            acceptButton.clicked -= AcceptDisplayedQuest;

        if (questManager != null)
            questManager.QuestChanged -= RefreshRuntime;
    }

    private void AcceptDisplayedQuest()
    {
        if (questManager != null && questManager.AcceptQuest(displayedQuest))
            RefreshRuntime(questManager.ActiveQuest);
    }

    private void RefreshStaticData()
    {
        if (displayedQuest == null)
            return;

        SetText(titleLabel, displayedQuest.title);
        SetText(locationLabel, $"PIN  {displayedQuest.destinationName}");
        SetText(descriptionLabel, displayedQuest.description);

        string itemName = displayedQuest.requiredItem != null
            ? displayedQuest.requiredItem.displayName
            : "Required item";

        SetText(requirementLabel, $"{displayedQuest.requiredAmount} x {itemName}");
        SetText(coinRewardLabel, $"$ {displayedQuest.coinReward}  COINS");
        SetText(experienceRewardLabel, $"* {displayedQuest.experienceReward}  EXP");
        SetText(reputationRewardLabel, $"P {displayedQuest.reputationReward}  REPUTATION");
    }

    private void RefreshRuntime(QuestRuntime runtime)
    {
        if (runtime == null || runtime.Data != displayedQuest || acceptButton == null)
            return;

        int minutes = Mathf.FloorToInt(runtime.RemainingTime / 60f);
        int seconds = Mathf.FloorToInt(runtime.RemainingTime % 60f);

        switch (runtime.State)
        {
            case QuestState.Preparing:
                acceptButton.text = $"PREPARE PIZZA  {minutes:00}:{seconds:00}";
                acceptButton.SetEnabled(false);
                break;

            case QuestState.Delivering:
                acceptButton.text = $"DELIVERING  {minutes:00}:{seconds:00}";
                acceptButton.SetEnabled(false);
                break;

            case QuestState.Completed:
                acceptButton.text = "ORDER COMPLETE";
                acceptButton.SetEnabled(false);
                break;

            case QuestState.Failed:
                acceptButton.text = "ORDER FAILED";
                acceptButton.SetEnabled(false);
                break;

            default:
                acceptButton.text = "ACCEPT ORDER  >";
                acceptButton.SetEnabled(true);
                break;
        }
    }

    private static void SetText(Label label, string value)
    {
        if (label != null)
            label.text = value;
    }
}
