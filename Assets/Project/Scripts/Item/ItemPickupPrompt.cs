using TMPro;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public sealed class ItemPickupPrompt : MonoBehaviour
{
    [SerializeField] private string message = "[F] 습득";
    [SerializeField] private TMP_FontAsset font;
    [SerializeField] private float fontSize = 30f;
    [SerializeField] private Vector3 worldOffset = new(0f, 0.8f, 0f);

    private Transform player;
    private TextMeshPro promptText;
    private Camera mainCamera;

    private void Awake()
    {
        mainCamera = Camera.main;
        CreatePrompt();
        SetPromptVisible(false);
    }

    private void LateUpdate()
    {
        if (promptText == null)
            return;

        promptText.transform.position = transform.position + worldOffset;

        if (mainCamera == null)
            mainCamera = Camera.main;
        if (mainCamera != null)
            promptText.transform.rotation = mainCamera.transform.rotation;

        SetPromptVisible(player != null && !GameplayPause.IsPaused);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.GetComponentInParent<PlayerController>() != null)
            player = other.transform.root;
    }

    private void OnTriggerExit(Collider other)
    {
        if (player != null && other.transform.root == player)
            player = null;
    }

    private void OnDisable()
    {
        player = null;
        SetPromptVisible(false);
    }

    private void OnDestroy()
    {
        if (promptText != null)
            Destroy(promptText.gameObject);
    }

    private void CreatePrompt()
    {
        GameObject promptObject = new($"{name}_PickupPrompt");
        promptText = promptObject.AddComponent<TextMeshPro>();
        promptText.text = message;
        if (font != null)
            promptText.font = font;
        promptText.fontSize = fontSize;
        promptText.fontStyle = FontStyles.Bold;
        promptText.alignment = TextAlignmentOptions.Center;
        promptText.color = Color.white;
        promptText.enableWordWrapping = false;
        promptText.transform.localScale = Vector3.one * 0.1f;
    }

    private void SetPromptVisible(bool visible)
    {
        if (promptText != null && promptText.gameObject.activeSelf != visible)
            promptText.gameObject.SetActive(visible);
    }
}
