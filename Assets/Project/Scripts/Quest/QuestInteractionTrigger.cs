using TMPro;
using UnityEngine;

public sealed class QuestInteractionTrigger : MonoBehaviour
{
    [SerializeField] private string interactionId;
    [SerializeField] private KeyCode interactionKey = KeyCode.E;
    [SerializeField] private bool interactOnce = true;

    [Header("Quest")]
    [SerializeField] private bool requireActiveQuest = true;
    [SerializeField] private QuestManager questManager;

    [Header("Prompt")]
    [SerializeField] private string promptMessage = "[E] 휴대전화 확인";
    [SerializeField] private TMP_FontAsset promptFont;
    [SerializeField] private Transform promptAnchor;
    [SerializeField] private Vector3 promptWorldOffset = new(0f, 0.6f, 0f);

    private Transform player;
    private bool hasInteracted;
    private TextMeshPro promptText;
    private Camera mainCamera;

    private void Awake()
    {
        if (questManager == null)
            questManager = FindFirstObjectByType<QuestManager>();

        mainCamera = Camera.main;
        CreatePrompt();
        RefreshPrompt();
    }

    private void Update()
    {
        RefreshPrompt();

        if (!CanInteract() || !Input.GetKeyDown(interactionKey))
            return;

        QuestProgressEvents.ReportInteraction(interactionId);

        if (interactOnce)
            hasInteracted = true;

        RefreshPrompt();
        Debug.Log($"상호작용 완료: {interactionId}", this);
    }

    private void LateUpdate()
    {
        if (promptText == null)
            return;

        Transform anchor = promptAnchor != null ? promptAnchor : transform;
        promptText.transform.position = anchor.position + promptWorldOffset;

        if (mainCamera == null)
            mainCamera = Camera.main;
        if (mainCamera != null)
            promptText.transform.rotation = mainCamera.transform.rotation;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.GetComponentInParent<PlayerController>() == null)
            return;

        player = other.transform.root;
        RefreshPrompt();
    }

    private void OnTriggerExit(Collider other)
    {
        if (player == null || other.transform.root != player)
            return;

        player = null;
        RefreshPrompt();
    }

    private void OnDisable()
    {
        player = null;
        if (promptText != null)
            promptText.gameObject.SetActive(false);
    }

    private void OnDestroy()
    {
        if (promptText != null)
            Destroy(promptText.gameObject);
    }

    private bool CanInteract()
    {
        if (player == null || hasInteracted || GameplayPause.IsPaused)
            return false;

        if (!requireActiveQuest)
            return true;

        return questManager != null &&
               questManager.FindActiveQuest(interactionId) != null;
    }

    private void CreatePrompt()
    {
        GameObject promptObject = new($"{name}_Prompt");
        promptText = promptObject.AddComponent<TextMeshPro>();
        promptText.text = promptMessage;
        if (promptFont != null)
            promptText.font = promptFont;
        promptText.alignment = TextAlignmentOptions.Center;
        promptText.fontSize = 50f;
        promptText.fontStyle = FontStyles.Bold;
        promptText.color = Color.white;
        promptText.enableWordWrapping = false;
        promptText.transform.localScale = Vector3.one * 0.1f;
        promptText.gameObject.SetActive(false);
    }

    private void RefreshPrompt()
    {
        if (promptText != null)
            promptText.gameObject.SetActive(CanInteract());
    }
}
