using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

[RequireComponent(typeof(UIDocument))]
public sealed class QuestUIController : MonoBehaviour
{
    [SerializeField] private QuestManager questManager;
    [SerializeField] private QuestData displayedQuest;

    private VisualElement availableList;
    private VisualElement activeList;
    private VisualElement completedList;
    private VisualElement detailPanel;
    private Button availableSectionButton;
    private Button activeSectionButton;
    private Button completedSectionButton;
    private Button acceptButton;
    private Label titleLabel;
    private Label locationLabel;
    private Label descriptionLabel;
    private Label requirementLabel;
    private Label coinRewardLabel;
    private Label experienceRewardLabel;
    private Label reputationRewardLabel;

    private QuestData selectedQuest;
    private bool availableExpanded = true;
    private bool activeExpanded = true;
    private bool completedExpanded = true;
    private float nextTimerRefresh;

    private void OnEnable()
    {
        if (questManager == null)
            questManager = FindFirstObjectByType<QuestManager>();

        VisualElement root = GetComponent<UIDocument>().rootVisualElement;
        availableList = root.Q<VisualElement>("AvailableQuestList");
        activeList = root.Q<VisualElement>("ActiveQuestList");
        completedList = root.Q<VisualElement>("CompletedQuestList");
        detailPanel = root.Q<VisualElement>("QuestDetailPanel");
        availableSectionButton = root.Q<Button>("AvailableSectionButton");
        activeSectionButton = root.Q<Button>("ActiveSectionButton");
        completedSectionButton = root.Q<Button>("CompletedSectionButton");
        acceptButton = root.Q<Button>("AcceptQuestButton");
        titleLabel = root.Q<Label>("QuestDetailTitle");
        locationLabel = root.Q<Label>("QuestLocation");
        descriptionLabel = root.Q<Label>("QuestDescription");
        requirementLabel = root.Q<Label>("RequiredItemText");
        coinRewardLabel = root.Q<Label>("CoinReward");
        experienceRewardLabel = root.Q<Label>("ExpReward");
        reputationRewardLabel = root.Q<Label>("ReputationReward");

        acceptButton?.RegisterCallback<ClickEvent>(OnAcceptClicked);
        availableSectionButton?.RegisterCallback<ClickEvent>(ToggleAvailable);
        activeSectionButton?.RegisterCallback<ClickEvent>(ToggleActive);
        completedSectionButton?.RegisterCallback<ClickEvent>(ToggleCompleted);

        if (questManager != null)
        {
            // 기존 씬에 연결된 단일 퀘스트를 자동 등록하여 씬 수정을 최소화합니다.
            questManager.RegisterQuest(displayedQuest);
            questManager.QuestsChanged += RefreshAll;
            questManager.QuestChanged += RefreshRuntime;
        }

        RefreshAll();
    }

    private void OnDisable()
    {
        acceptButton?.UnregisterCallback<ClickEvent>(OnAcceptClicked);
        availableSectionButton?.UnregisterCallback<ClickEvent>(ToggleAvailable);
        activeSectionButton?.UnregisterCallback<ClickEvent>(ToggleActive);
        completedSectionButton?.UnregisterCallback<ClickEvent>(ToggleCompleted);

        if (questManager != null)
        {
            questManager.QuestsChanged -= RefreshAll;
            questManager.QuestChanged -= RefreshRuntime;
        }
    }

    private void Update()
    {
        // 활성 퀘스트 타이머는 0.25초 간격으로 갱신하여 UI 재생성 비용을 제한합니다.
        if (questManager == null || Time.unscaledTime < nextTimerRefresh)
            return;

        nextTimerRefresh = Time.unscaledTime + 0.25f;
        if (questManager.ActiveQuests.Count > 0)
        {
            RebuildActiveList(new List<QuestRuntime>(questManager.ActiveQuests));
            UpdateSection(activeSectionButton, activeList, "진행 중인 퀘스트",
                questManager.ActiveQuests.Count, activeExpanded);
        }
    }

    private void RefreshAll()
    {
        if (questManager == null)
            return;

        List<QuestData> available = new(questManager.GetAvailableQuests());
        List<QuestRuntime> active = new(questManager.ActiveQuests);
        List<QuestData> completed = new(questManager.GetCompletedQuests());

        RebuildDataList(availableList, available, "신규");
        RebuildActiveList(active);
        RebuildDataList(completedList, completed, "완료");

        UpdateSection(availableSectionButton, availableList, "수주 가능 퀘스트", available.Count, availableExpanded);
        UpdateSection(activeSectionButton, activeList, "진행 중인 퀘스트", active.Count, activeExpanded);
        UpdateSection(completedSectionButton, completedList, "완료한 퀘스트", completed.Count, completedExpanded);

        // 선택 항목이 사라졌다면 다음으로 볼 수 있는 항목을 자동 선택합니다.
        if (!IsVisibleQuest(selectedQuest))
            selectedQuest = available.Count > 0 ? available[0] : active.Count > 0 ? active[0].Data : completed.Count > 0 ? completed[0] : null;

        RefreshDetail();
    }

    private void RebuildDataList(VisualElement container, List<QuestData> quests, string status)
    {
        if (container == null)
            return;

        container.Clear();
        if (quests.Count == 0)
        {
            AddEmptyLabel(container, status == "완료" ? "완료한 퀘스트가 없습니다" : "수주 가능한 퀘스트가 없습니다");
            return;
        }

        foreach (QuestData quest in quests)
            container.Add(CreateQuestItem(quest, status, quest.timeLimitSeconds));
    }

    private void RebuildActiveList(List<QuestRuntime> runtimes)
    {
        if (activeList == null)
            return;

        activeList.Clear();
        if (runtimes.Count == 0)
        {
            AddEmptyLabel(activeList, "진행 중인 퀘스트가 없습니다");
            return;
        }

        foreach (QuestRuntime runtime in runtimes)
        {
            string status = runtime.State == QuestState.Delivering ? "배달 중" : "준비 중";
            activeList.Add(CreateQuestItem(runtime.Data, status, runtime.RemainingTime));
        }
    }

    private VisualElement CreateQuestItem(QuestData quest, string status, float time)
    {
        VisualElement item = new();
        item.AddToClassList("quest-item");
        if (quest == selectedQuest)
            item.AddToClassList("quest-item--selected");

        Label portrait = new(quest.customerName);
        portrait.AddToClassList("quest-item-portrait");

        VisualElement copy = new();
        copy.AddToClassList("quest-item-text");
        Label title = new(quest.title);
        title.AddToClassList("quest-item-title");
        Label location = new($"목적지  {quest.destinationName}");
        location.AddToClassList("quest-item-subtext");
        Label timer = new($"제한 시간  {FormatTime(time)}");
        timer.AddToClassList("quest-item-time");
        copy.Add(title);
        copy.Add(location);
        copy.Add(timer);

        Label badge = new(status);
        badge.AddToClassList("quest-item-status");
        if (status != "신규") badge.AddToClassList("progress-status");

        item.Add(portrait);
        item.Add(copy);
        item.Add(badge);
        item.RegisterCallback<ClickEvent>(_ => SelectQuest(quest));
        return item;
    }

    private void SelectQuest(QuestData quest)
    {
        selectedQuest = quest;
        RefreshAll();
    }

    private void RefreshDetail()
    {
        if (detailPanel == null)
            return;

        detailPanel.style.display = selectedQuest == null ? DisplayStyle.None : DisplayStyle.Flex;
        if (selectedQuest == null)
            return;

        SetText(titleLabel, selectedQuest.title);
        SetText(locationLabel, $"목적지  {selectedQuest.destinationName}");
        SetText(descriptionLabel, selectedQuest.description);
        string itemName = selectedQuest.requiredItem != null ? selectedQuest.requiredItem.displayName : "필요 아이템";
        SetText(requirementLabel, $"{itemName} {selectedQuest.requiredAmount}개");
        SetText(coinRewardLabel, $"코인 {selectedQuest.coinReward}");
        SetText(experienceRewardLabel, $"경험치 {selectedQuest.experienceReward}");
        SetText(reputationRewardLabel, $"평판 {selectedQuest.reputationReward}");

        if (acceptButton == null)
            return;

        if (questManager.IsCompleted(selectedQuest.questId))
        {
            acceptButton.text = "완료한 퀘스트";
            acceptButton.SetEnabled(false);
        }
        else if (questManager.FindActiveQuest(selectedQuest.questId) != null)
        {
            acceptButton.text = "진행 중";
            acceptButton.SetEnabled(false);
        }
        else
        {
            acceptButton.text = "퀘스트 수락";
            acceptButton.SetEnabled(questManager.CanAcceptQuest(selectedQuest));
        }
    }

    private void OnAcceptClicked(ClickEvent _)
    {
        if (selectedQuest != null && questManager != null)
            questManager.AcceptQuest(selectedQuest);
    }

    private void RefreshRuntime(QuestRuntime runtime)
    {
        // 매 프레임 발생하는 타이머 이벤트에서는 상세 버튼만 갱신하고 목록은 필요할 때 재생성합니다.
        if (runtime != null && runtime.Data == selectedQuest)
            RefreshDetail();
    }

    private bool IsVisibleQuest(QuestData quest)
    {
        return quest != null && (questManager.CanAcceptQuest(quest) ||
            questManager.FindActiveQuest(quest.questId) != null || questManager.IsCompleted(quest.questId));
    }

    private void ToggleAvailable(ClickEvent _) { availableExpanded = !availableExpanded; RefreshAll(); }
    private void ToggleActive(ClickEvent _) { activeExpanded = !activeExpanded; RefreshAll(); }
    private void ToggleCompleted(ClickEvent _) { completedExpanded = !completedExpanded; RefreshAll(); }

    private static void UpdateSection(Button button, VisualElement content, string title, int count, bool expanded)
    {
        if (button != null) button.text = $"{(expanded ? "▼" : "▶")} {title} ({count})";
        if (content != null) content.style.display = expanded ? DisplayStyle.Flex : DisplayStyle.None;
    }

    private static void AddEmptyLabel(VisualElement container, string text)
    {
        Label label = new(text);
        label.AddToClassList("quest-empty-label");
        container.Add(label);
    }

    private static string FormatTime(float seconds)
    {
        int total = Mathf.Max(0, Mathf.FloorToInt(seconds));
        return $"{total / 60:00}:{total % 60:00}";
    }

    private static void SetText(Label label, string value)
    {
        if (label != null) label.text = value;
    }
}
