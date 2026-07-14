using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class QuickSlotUI : MonoBehaviour , IDropHandler
{
    [Header("References")]
    [SerializeField] private Button button;
    [SerializeField] private Image backgroundImage;
    [SerializeField] private Image iconImage;
    [SerializeField] private Image selectedFrame;
    [SerializeField] private TMP_Text keyText;

    [Header("Colors")]
    [SerializeField] private Color occupiedColor =
        new Color32(29, 32, 37, 230);

    [SerializeField] private Color emptyColor =
        new Color32(18, 20, 24, 170);

    [SerializeField] private Color selectedColor =
        new Color32(221, 198, 126, 255);

    [Header("Scale")]
    [SerializeField] private float selectedScale = 1.08f;

    private QuickSlotController controller;
    private int slotIndex;

    private void Awake()
    {
        if (button == null)
            button = GetComponent<Button>();

        if (backgroundImage == null)
            backgroundImage = GetComponent<Image>();

        RectTransform slotRect = (RectTransform)transform;
        slotRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, 64f);
        slotRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, 64f);

        ConfigureStretchImage(iconImage, 7f);
        ConfigureStretchImage(selectedFrame, 0f);

        if (selectedFrame != null)
        {
            selectedFrame.transform.SetAsFirstSibling();
            selectedFrame.raycastTarget = false;
        }

        if (iconImage != null)
            iconImage.raycastTarget = false;

        if (keyText != null)
        {
            keyText.raycastTarget = false;
            RectTransform keyRect = keyText.rectTransform;
            keyRect.anchorMin = new Vector2(0f, 1f);
            keyRect.anchorMax = new Vector2(0f, 1f);
            keyRect.pivot = new Vector2(0f, 1f);
            keyRect.anchoredPosition = new Vector2(5f, -4f);
            keyRect.sizeDelta = new Vector2(20f, 20f);
            keyText.fontSize = 14f;
            keyText.alignment = TextAlignmentOptions.TopLeft;
        }
    }

    private static void ConfigureStretchImage(Image image, float padding)
    {
        if (image == null)
            return;

        RectTransform rect = image.rectTransform;
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.one * padding;
        rect.offsetMax = -Vector2.one * padding;
    }

    public void Initialize(QuickSlotController quickSlotController, int index)
    {
        controller = quickSlotController;
        slotIndex = index;

        if (button == null)
        {
            button = GetComponent<Button>();
        }

        if (button != null)
        {
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(OnClickSlot);
        }

        if (keyText != null)
        {
            keyText.text = (slotIndex + 1).ToString();
        }
    }
    public void Refresh(QuickSlotData slotData, bool isSelected)
    {
        Sprite icon = slotData?.Icon;
        bool hasContent =
            slotData != null &&
            !slotData.IsEmpty;

        if (backgroundImage != null)
        {
            backgroundImage.color =
                hasContent ? occupiedColor : emptyColor;
        }

        if (iconImage != null)
        {
            iconImage.sprite = icon;
            iconImage.enabled = icon != null;
            iconImage.preserveAspect = true;
            iconImage.color = Color.white;
        }

        if (selectedFrame != null)
        {
            selectedFrame.enabled = isSelected;
            Color highlightColor = selectedColor;
            highlightColor.a = 0.35f;
            selectedFrame.color = highlightColor;
        }

        transform.localScale = isSelected
            ? Vector3.one * selectedScale
            : Vector3.one;
    }
    private void OnClickSlot()
    {
        if (controller != null)
        {
            controller.UseSlot(slotIndex);
        }
    }
    public void OnDrop(
        PointerEventData eventData)
    {
        Debug.Log("퀵슬롯 OnDrop 실행");

        if (!DragContext.Current.HasValue)
        {
            Debug.LogWarning("드래그 데이터가 없습니다.");
            return;
        }

        DraggedSlotData dragged =
            DragContext.Current.Value;

        if (dragged.containerType !=
            SlotContainerType.Inventory)
        {
            return;
        }

        controller.AssignFromInventory(
            slotIndex,
            dragged.slotIndex);
    }
}
