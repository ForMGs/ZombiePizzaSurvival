//인벤토리 슬롯 드래그
using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;
using UnityEngine.UI;

public class InventorySlotUI:
    MonoBehaviour, IBeginDragHandler,IDragHandler, IEndDragHandler
{   
    [Header("Drag")]
    [SerializeField] private DragIconUI dragIconUI;
    [Header("UI")]
    [SerializeField] private Image backgroundImage;
    [SerializeField] private Image iconImage;
    [SerializeField] private TMP_Text amountText;
    [SerializeField] private int slotIndex;
    [SerializeField] private Inventory inventory;

    private void Awake()
    {
        if (inventory == null)
            inventory = FindFirstObjectByType<Inventory>();

        if (backgroundImage == null)
            backgroundImage = GetComponent<Image>();

        if (iconImage != null)
        {
            iconImage.raycastTarget = false;
            iconImage.preserveAspect = true;
        }

        if (amountText != null)
            amountText.raycastTarget = false;
        if (dragIconUI == null)
        {
            dragIconUI = FindFirstObjectByType<DragIconUI>();
        }
    }

    private void OnEnable()
    {
        if (inventory != null)
            inventory.Changed += Refresh;

        Refresh();
    }

    private void OnDisable()
    {
        if (inventory != null)
            inventory.Changed -= Refresh;

        if (dragIconUI != null)
            dragIconUI.Hide();

        DragContext.End();
    }

    private void Refresh()
    {
        if (inventory == null ||
            !inventory.IsValidIndex(slotIndex))
        {
            ShowEmpty();
            return;
        }

        InventorySlotData slot =
            inventory.Slots[slotIndex];

        if (slot == null || slot.IsEmpty)
        {
            ShowEmpty();
            return;
        }

        if (iconImage != null)
        {
            iconImage.sprite = slot.item.icon;
            iconImage.enabled = slot.item.icon != null;
        }

        if (amountText != null)
        {
            amountText.text =
                slot.item.stackable && slot.amount > 1
                    ? slot.amount.ToString()
                    : string.Empty;
        }
    }

    private void ShowEmpty()
    {
        if (iconImage != null)
        {
            iconImage.sprite = null;
            iconImage.enabled = false;
        }

        if (amountText != null)
            amountText.text = string.Empty;
    }
    public void OnBeginDrag(
        PointerEventData eventData)
    {
        if (inventory == null ||
            !inventory.IsValidIndex(slotIndex))
        {
            return;
        }

        InventorySlotData slot =
            inventory.Slots[slotIndex];

        if (slot == null || slot.IsEmpty)
            return;

        DragContext.Begin(new DraggedSlotData
        {
            containerType =
                SlotContainerType.Inventory,

            slotIndex = slotIndex,
            itemData = slot.item,
            amount = slot.amount
        });
        if (dragIconUI != null)
        {
            dragIconUI.Show(
                slot.item.icon,
                eventData.position);
        }

        Debug.Log(
            $"드래그 시작: {slot.item.displayName}");
    }

    public void OnDrag(
        PointerEventData eventData)
    {
        if (!DragContext.Current.HasValue)
            return;

        if (dragIconUI != null)
            dragIconUI.SetPosition(eventData.position);
    }

    public void OnEndDrag(
        PointerEventData eventData)
    {
       
        if (dragIconUI != null)
            dragIconUI.Hide();
        Debug.Log("드래그 종료");
        DragContext.End();
    }

}