using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.UIElements;

[RequireComponent(typeof(UIDocument))]
public sealed class QuestUIActionReporter : MonoBehaviour
{
    [System.Serializable]
    private sealed class TrackedButton
    {
        public string elementName = string.Empty;
        public string actionId = string.Empty;
    }

    [SerializeField] private string openedUiId;
    [SerializeField] private List<TrackedButton> trackedButtons = new();

    private readonly List<ButtonRegistration> registrations = new();

    private sealed class ButtonRegistration
    {
        public Button Button;
        public Action Callback;
    }

    private void OnEnable()
    {
        if (!string.IsNullOrWhiteSpace(openedUiId))
            QuestProgressEvents.ReportUiOpened(openedUiId);

        VisualElement root = GetComponent<UIDocument>().rootVisualElement;
        foreach (TrackedButton tracked in trackedButtons)
        {
            if (tracked == null || string.IsNullOrWhiteSpace(tracked.elementName))
                continue;

            Button button = root.Q<Button>(tracked.elementName);
            if (button == null)
                continue;

            string actionId = tracked.actionId;
            Action callback = () => ReportButton(actionId);
            button.clicked += callback;
            registrations.Add(new ButtonRegistration { Button = button, Callback = callback });
        }
    }

    private void OnDisable()
    {
        foreach (ButtonRegistration registration in registrations)
            registration.Button.clicked -= registration.Callback;
        registrations.Clear();
    }

    private static void ReportButton(string actionId)
    {
        if (!string.IsNullOrWhiteSpace(actionId))
            QuestProgressEvents.ReportUiClicked(actionId);
    }
}
