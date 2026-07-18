using System;
using System.Collections.Generic;
using UnityEngine;

public class Inventory : MonoBehaviour
{
    [Header("Inventory")]
    [SerializeField] private int capacity = 10;
    [SerializeField] private InventorySlotData[] slots;

    [Header("Legacy Item Mapping")]
    [SerializeField] private ItemData fleshItemData;
    [SerializeField] private ItemData clothItemData;
    [SerializeField] private ItemData toothItemData;

    // 기존 PlayerHUD와 제작 시스템 호환용
    private readonly Dictionary<ItemType, int> items = new();

    public InventorySlotData[] Slots => slots;
    public IReadOnlyDictionary<ItemType, int> Items => items;

    public event Action Changed;

    private void Awake()
    {
        InitializeSlots();
    }

    private void InitializeSlots()
    {
        if (capacity <= 0)
            capacity = 10;

        if (slots == null)
        {
            slots = new InventorySlotData[capacity];
        }
        else if (slots.Length != capacity)
        {
            Array.Resize(ref slots, capacity);
        }

        for (int i = 0; i < slots.Length; i++)
        {
            if (slots[i] == null)
                slots[i] = new InventorySlotData();
        }
    }

    // 새로운 ItemData 기반 획득
    public bool AddItem(ItemData item, int amount)
    {
        if (item == null || amount <= 0)
            return false;

        int remainingAmount = amount;

        // 기존 스택에 먼저 합치기
        if (item.stackable)
        {
            for (int i = 0; i < slots.Length; i++)
            {
                InventorySlotData slot = slots[i];

                if (slot.IsEmpty ||
                    slot.item != item ||
                    slot.amount >= item.maxStack)
                {
                    continue;
                }

                int availableSpace =
                    item.maxStack - slot.amount;

                int addedAmount =
                    Mathf.Min(availableSpace, remainingAmount);

                slot.amount += addedAmount;
                remainingAmount -= addedAmount;

                if (remainingAmount <= 0)
                {
                    SyncLegacyAmount(item, amount);
                    Changed?.Invoke();
                    return true;
                }
            }
        }

        // 남은 수량을 빈 슬롯에 배치
        for (int i = 0; i < slots.Length; i++)
        {
            InventorySlotData slot = slots[i];

            if (!slot.IsEmpty)
                continue;

            int addedAmount = item.stackable
                ? Mathf.Min(item.maxStack, remainingAmount)
                : 1;

            slot.item = item;
            slot.amount = addedAmount;

            remainingAmount -= addedAmount;

            if (remainingAmount <= 0)
            {
                SyncLegacyAmount(item, amount);
                Changed?.Invoke();
                return true;
            }
        }

        // 일부만 들어간 경우 실제 추가된 수량만 동기화
        int actuallyAdded = amount - remainingAmount;

        if (actuallyAdded > 0)
        {
            SyncLegacyAmount(item, actuallyAdded);
            Changed?.Invoke();
        }

        return remainingAmount <= 0;
    }

    // 기존 ItemPickup 호환용
    public bool AddItem(ItemType itemType, int amount)
    {
        if (amount <= 0)
            return false;

        ItemData itemData = GetItemData(itemType);

        if (itemData == null)
        {
            Debug.LogWarning(
                $"{itemType}에 해당하는 ItemData가 연결되지 않았습니다.");

            return false;
        }

        // AddItem(ItemData)가 Dictionary까지 동기화함
        return AddItem(itemData, amount);
    }

    public bool HasItem(ItemType itemType, int amount)
    {
        if (amount <= 0)
            return false;

        return items.TryGetValue(
            itemType,
            out int currentAmount)
            && currentAmount >= amount;
    }

    public bool UseItem(ItemType itemType, int amount)
    {
        if (amount <= 0 ||
            !HasItem(itemType, amount))
        {
            Debug.Log(
                $"{GetItemName(itemType)}이 부족합니다.");

            return false;
        }

        ItemData itemData = GetItemData(itemType);

        if (itemData == null)
            return false;

        if (!RemoveFromSlots(itemData, amount))
            return false;

        items[itemType] -= amount;

        if (items[itemType] <= 0)
            items.Remove(itemType);

        Changed?.Invoke();
        return true;
    }

    public int GetAmount(ItemType itemType)
    {
        return items.TryGetValue(
            itemType,
            out int amount)
            ? amount
            : 0;
    }

    public int GetAmount(ItemData item)
    {
        if (item == null || slots == null)
            return 0;

        int total = 0;

        for (int i = 0; i < slots.Length; i++)
        {
            InventorySlotData slot = slots[i];

            if (!slot.IsEmpty && slot.item == item)
                total += slot.amount;
        }

        return total;
    }

    public bool HasItem(ItemData item, int amount)
    {
        return item != null && amount > 0 && GetAmount(item) >= amount;
    }

    public bool RemoveItem(ItemData item, int amount)
    {
        if (!HasItem(item, amount) || !RemoveFromSlots(item, amount))
            return false;

        if (TryGetItemType(item, out ItemType itemType) && items.ContainsKey(itemType))
        {
            items[itemType] -= amount;

            if (items[itemType] <= 0)
                items.Remove(itemType);
        }

        Changed?.Invoke();
        return true;
    }

    public bool IsValidIndex(int index)
    {
        return slots != null &&
               index >= 0 &&
               index < slots.Length;
    }

    public void Swap(int firstIndex, int secondIndex)
    {
        if (!IsValidIndex(firstIndex) ||
            !IsValidIndex(secondIndex) ||
            firstIndex == secondIndex)
        {
            return;
        }

        (slots[firstIndex], slots[secondIndex]) =
            (slots[secondIndex], slots[firstIndex]);

        Changed?.Invoke();
    }

    public string GetItemName(ItemType itemType)
    {
        ItemData itemData = GetItemData(itemType);

        if (itemData != null &&
            !string.IsNullOrWhiteSpace(itemData.displayName))
        {
            return itemData.displayName;
        }

        return itemType.ToString();
    }

    private bool RemoveFromSlots(
        ItemData item,
        int amount)
    {
        int remainingAmount = amount;

        for (int i = slots.Length - 1; i >= 0; i--)
        {
            InventorySlotData slot = slots[i];

            if (slot.IsEmpty || slot.item != item)
                continue;

            int removedAmount =
                Mathf.Min(slot.amount, remainingAmount);

            slot.amount -= removedAmount;
            remainingAmount -= removedAmount;

            if (slot.amount <= 0)
                slot.Clear();

            if (remainingAmount <= 0)
                return true;
        }

        return false;
    }

    private void SyncLegacyAmount(
        ItemData item,
        int amount)
    {
        if (!TryGetItemType(item, out ItemType itemType))
            return;

        if (!items.ContainsKey(itemType))
            items[itemType] = 0;

        items[itemType] += amount;
    }

    private ItemData GetItemData(ItemType itemType)
    {
        switch (itemType)
        {
            case ItemType.Flesh:
                return fleshItemData;

            case ItemType.Cloth:
                return clothItemData;

            case ItemType.Tooth:
                return toothItemData;

            default:
                return null;
        }
    }

    private bool TryGetItemType(
        ItemData item,
        out ItemType itemType)
    {
        if (item == fleshItemData)
        {
            itemType = ItemType.Flesh;
            return true;
        }

        if (item == clothItemData)
        {
            itemType = ItemType.Cloth;
            return true;
        }

        if (item == toothItemData)
        {
            itemType = ItemType.Tooth;
            return true;
        }

        itemType = default;
        return false;
    }
}
