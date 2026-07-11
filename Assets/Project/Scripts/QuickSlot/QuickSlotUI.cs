using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class QuickSlotUI : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private Button button;
    [SerializeField] private Image iconImage;
    [SerializeField] private Image selectedFrame;
    [SerializeField] private TMP_Text keyText;

    private QuickSlotController controller;
    private int slotIndex;

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
        if (iconImage != null)
        {
            if (slotData != null && slotData.Icon != null)
            {
                iconImage.enabled = true;
                iconImage.sprite = slotData.Icon;
            }
            else
            {
                iconImage.enabled = false;
                iconImage.sprite = null;
            }
        }

        if (selectedFrame != null)
        {
            selectedFrame.enabled = isSelected;
        }
    }

    private void OnClickSlot()
    {
        if (controller != null)
        {
            controller.UseSlot(slotIndex);
        }
    }
}