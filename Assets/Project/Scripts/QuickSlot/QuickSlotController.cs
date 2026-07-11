using UnityEngine;

public class QuickSlotController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private WeaponController weaponController;
    [Header("Quick Slots")]
    [SerializeField] private QuickSlotData[] quickSlots = new QuickSlotData[8];
    [Header("UI")]
    [SerializeField] private QuickSlotUI[] quickSlotUIs;

    private int selectedSlotIndex = -1;

    private void Awake()
    {
        if(weaponController == null)
        {
            weaponController = GetComponent<WeaponController>();
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
        if(index < 0 || index >= quickSlots.Length)
            return;
        QuickSlotData slotData = quickSlots[index];
        if(slotData == null)
        {
            return;
        }
        selectedSlotIndex = index;

        switch(slotData.contentType)
        {
            case QuickSlotContentType.Weapon:
                EquipWeaponFromSlot(slotData);
                break;
            case QuickSlotContentType.Item:
                UseItemFromSlot(slotData);
                break;
            case QuickSlotContentType.Empty:
                Debug.Log("빈 슬롯입니다.");
                break;
        }
        RefreshUI();
    }

    public void EquipWeaponFromSlot(QuickSlotData slotData)
    {
        if(slotData.weaponData == null)
        {
            Debug.LogWarning("슬롯에 무기 데이터가 없습니다.");
            return;
        }
        if(weaponController == null)
        {
            Debug.LogWarning("WeaponController가 없습니다.");
            return;
        }
        weaponController.EquipWeapon(slotData.weaponData);
    }
    private void UseItemFromSlot(QuickSlotData slotData)
    {
        // 나중에 붕대, 회복 아이템 등을 여기서 처리
        Debug.Log($"아이템 사용 예정: {slotData.itemType}");
    }

    private void InitializeUI()
    {
        if(quickSlots == null)
            return;
        
        for(int i =0; i < quickSlotUIs.Length; i++)
        {
            if (quickSlotUIs[i] != null)
            {
                quickSlotUIs[i].Initialize(this, i);
            }
        }
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