using System;
using UnityEngine;

public class QuickSlotController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private WeaponController weaponController;
    [Header("Quick Slots")]
    [SerializeField] private QuickSlotData[] quickSlots = new QuickSlotData[8];
    [Header("UI")]
    [SerializeField] private QuickSlotUI[] quickSlotUIs;
    [SerializeField] private Inventory inventory;

    private int selectedSlotIndex = -1;
    private int equippedWeaponSlotIndex = -1;
    private void Awake()
    {
        if(weaponController == null)
        {
            weaponController = GetComponent<WeaponController>();
        }
        if(inventory == null)
        {
            inventory = FindFirstObjectByType<Inventory>();
        }
        EnsureUIReferences();
    }

    private void EnsureUIReferences()
    {
        QuickSlotUI[] sceneSlots = FindObjectsByType<QuickSlotUI>(
            FindObjectsInactive.Include,
            FindObjectsSortMode.None);

        Array.Sort(sceneSlots, (left, right) =>
            string.CompareOrdinal(left.gameObject.name, right.gameObject.name));

        if (sceneSlots.Length > 0)
        {
            quickSlotUIs = sceneSlots;
        }
    }

    private void Start()
    {
        InitializeUI();
        RefreshUI();
    }

    private void Update()
    {
        HandleKeyboardInput();
    }
    private void HandleKeyboardInput()
    {
        for(int i =0; i < quickSlots.Length && i < 8; i++)
        {
            KeyCode keyCode = (KeyCode)((int)KeyCode.Alpha1 + i);
            if(Input.GetKeyDown(keyCode))
            {
                UseSlot(i);
            }
        }
    }
    public void UseSlot(int index)
    {
        if (quickSlots == null ||
            index < 0 ||
            index >= quickSlots.Length)
        {
            return;
        }
        QuickSlotData slot = quickSlots[index];

        if(slot == null || slot.IsEmpty)
        {
            Debug.Log("빈 슬롯입니다.");
            return;
        }
        switch (slot.item.category)
        {
            case ItemCategory.Weapon:
                EquipWeaponFromSlot(slot);
                equippedWeaponSlotIndex = index;
                break;
            case ItemCategory.Consumable:
                UseItemFromSlot(slot);
                break;
            default:
                Debug.Log("사용할 수 없는 아이템입니다.");
                break;
        }
        selectedSlotIndex = index;
        RefreshUI();
    }

    public void EquipWeaponFromSlot(QuickSlotData slotData)
    {
        WeaponData weaponData = slotData.item?.weaponData;
        if (weaponData == null)
        {
            Debug.Log("무기 데이터가 없습니다.");
            return;
        }
        if(weaponController == null)
        {
            Debug.Log("WeaponController가 없습니다.");
            return;
        }
        weaponController.EquipWeapon(weaponData);
    }
    private void UseItemFromSlot(QuickSlotData slotData)
    {
        // 나중에 붕대, 회복 아이템 등을 여기서 처리
        Debug.Log($"아이템 사용 예정: {slotData.item.displayName}");
    }

    private void InitializeUI()
    {
        if(quickSlotUIs == null)
            return;
        
        for(int i =0; i < quickSlotUIs.Length; i++)
        {
            if (quickSlotUIs[i] != null)
            {
                quickSlotUIs[i].Initialize(this, i);
            }
        }
    }
    private int FindQuickSlot(ItemData item)
    {
        if (item == null || quickSlots == null)
            return -1;

        for (int i = 0; i < quickSlots.Length; i++)
        {
            QuickSlotData slot = quickSlots[i];

            if (slot != null &&
                !slot.IsEmpty &&
                slot.item == item)
            {
                return i;
            }
        }

        return -1;
    }
    public void AssignFromInventory(
        int quickSlotIndex,
        int inventoryIndex
    )
    {
        if (inventory == null ||
            quickSlots == null ||
            !inventory.IsValidIndex(inventoryIndex) ||
            quickSlotIndex < 0 ||
            quickSlotIndex >= quickSlots.Length)
        {
            return;
        }

        InventorySlotData source = 
            inventory.Slots[inventoryIndex];
        
        if(source.IsEmpty)
            return;

        if(source.item.category != ItemCategory.Weapon &&
            source.item.category != ItemCategory.Consumable)
        {
            Debug.LogWarning(
                $"{source.item.displayName}은 " +
                "퀵슬롯에 등록할 수 없습니다.");

            return;
        }
        int existingSlotIndex =
            FindQuickSlot(source.item);
        
        if (existingSlotIndex >= 0 &&
            existingSlotIndex != quickSlotIndex)
        {
            bool wasEquipped =
                equippedWeaponSlotIndex == existingSlotIndex;

            quickSlots[existingSlotIndex].Clear();

            if (wasEquipped)
            {
                equippedWeaponSlotIndex =
                    quickSlotIndex;

                selectedSlotIndex =
                    quickSlotIndex;
            }
        }
        bool replacingEquippedWeapon =
            equippedWeaponSlotIndex == quickSlotIndex;
        if (inventory == null ||
            !inventory.IsValidIndex(inventoryIndex) ||
            quickSlotIndex < 0 ||
            quickSlotIndex >= quickSlots.Length)
        {
            return;
        }
        if (quickSlots[quickSlotIndex] == null)
            quickSlots[quickSlotIndex] = new QuickSlotData();
        quickSlots[quickSlotIndex].item = source.item;
        quickSlots[quickSlotIndex].amount = source.amount;

        if (replacingEquippedWeapon &&
            source.item.category == ItemCategory.Weapon)
        {
            EquipWeaponFromSlot(
                quickSlots[quickSlotIndex]);

            selectedSlotIndex = quickSlotIndex;
        }

        Debug.Log(
            $"{source.item.displayName}을 " +
            $"퀵슬롯 {quickSlotIndex + 1}에 등록했습니다.");
        RefreshUI();
    }

    private void RefreshUI()
    {
        if(quickSlotUIs == null)
            return;
        for(int i =0; i < quickSlotUIs.Length; i++)
        {
            QuickSlotData slotData = null;

            if( i < quickSlots.Length)
            {
                slotData = quickSlots[i];
            }

            bool isSelected = i == selectedSlotIndex;

            if(quickSlotUIs[i] != null)
            {
                quickSlotUIs[i].Refresh(slotData, isSelected);
            }
        }
    }
}
