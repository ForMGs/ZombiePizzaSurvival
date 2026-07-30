using System;

// 게임의 각 시스템이 QuestManager를 직접 참조하지 않고 진행 상황을 전달하는 통로입니다.
public static class QuestProgressEvents
{
    public static event Action<string> ZombieKilled;
    public static event Action<string> UiOpened;
    public static event Action<string> UiClicked;
    public static event Action<string> Interacted;

    public static void ReportZombieKilled(string zombieId) => ZombieKilled?.Invoke(zombieId);
    public static void ReportUiOpened(string uiId) => UiOpened?.Invoke(uiId);
    public static void ReportUiClicked(string uiId) => UiClicked?.Invoke(uiId);
    public static void ReportInteraction(string interactionId) => Interacted?.Invoke(interactionId);
}

