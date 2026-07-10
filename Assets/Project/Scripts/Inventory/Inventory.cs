using System.Collections.Generic;
using UnityEngine;

public class Inventory : MonoBehaviour
{
    private readonly Dictionary<ItemType, int> items =
        new Dictionary<ItemType, int>();

    public IReadOnlyDictionary<ItemType, int> Items => items;

    public void AddItem(ItemType itemType, int amount)
    {
        if (amount <= 0)
            return;

        if (!items.ContainsKey(itemType))
        {
            items[itemType] = 0;
        }

        items[itemType] += amount;

        Debug.Log(
            $"{GetItemName(itemType)} {amount}개 획득 / " +
            $"현재 보유: {items[itemType]}개"
        );
    }

    public bool HasItem(ItemType itemType, int amount)
    {
        return items.TryGetValue(itemType, out int currentAmount)
            && currentAmount >= amount;
    }

    public bool UseItem(ItemType itemType, int amount)
    {
        if (amount <= 0 || !HasItem(itemType, amount))
        {
            Debug.Log($"{GetItemName(itemType)} 부족");
            return false;
        }

        items[itemType] -= amount;

        Debug.Log(
            $"{GetItemName(itemType)} {amount}개 사용 / " +
            $"남은 개수: {items[itemType]}개"
        );

        return true;
    }

    public int GetAmount(ItemType itemType)
    {
        return items.TryGetValue(itemType, out int amount) ? amount : 0;
    }

    public string GetItemName(ItemType itemType)
    {
        switch (itemType)
        {
            case ItemType.Flesh:
                return "살점";

            case ItemType.Cloth:
                return "천";

            case ItemType.Tooth:
                return "이빨";

            default:
                return itemType.ToString();
        }
    }
}
