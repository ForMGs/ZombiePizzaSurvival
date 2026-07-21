using System;
using System.Collections.Generic;
using UnityEngine;

// 아이템을 현재 가지고 있는지와 별개로, 한 번이라도 획득한 누적 기록을 저장합니다.
public sealed class PlayerProgressTracker : MonoBehaviour
{
    private const string SaveKey = "ZombiePizzaSurvival.ItemProgress.v1";

    [SerializeField] private Inventory inventory;
    private readonly Dictionary<string, int> acquiredAmounts = new();

    public event Action ProgressChanged;

    private void Awake()
    {
        if (inventory == null)
            inventory = FindFirstObjectByType<Inventory>();

        Load();
    }

    private void OnEnable()
    {
        if (inventory != null)
            inventory.ItemAdded += RecordItem;
    }

    private void OnDisable()
    {
        if (inventory != null)
            inventory.ItemAdded -= RecordItem;
    }

    public bool HasAcquired(ItemData item, int amount)
    {
        if (item == null || amount <= 0 || string.IsNullOrWhiteSpace(item.itemId))
            return false;

        return acquiredAmounts.TryGetValue(item.itemId, out int current) && current >= amount;
    }

    private void RecordItem(ItemData item, int amount)
    {
        if (item == null || amount <= 0 || string.IsNullOrWhiteSpace(item.itemId))
            return;

        acquiredAmounts.TryGetValue(item.itemId, out int current);
        acquiredAmounts[item.itemId] = current + amount;
        Save();
        ProgressChanged?.Invoke();
    }

    private void Save()
    {
        ItemProgressSaveData data = new();
        foreach (KeyValuePair<string, int> pair in acquiredAmounts)
            data.items.Add(new ItemProgressEntry { itemId = pair.Key, amount = pair.Value });

        PlayerPrefs.SetString(SaveKey, JsonUtility.ToJson(data));
        PlayerPrefs.Save();
    }

    private void Load()
    {
        acquiredAmounts.Clear();
        if (!PlayerPrefs.HasKey(SaveKey))
            return;

        ItemProgressSaveData data = JsonUtility.FromJson<ItemProgressSaveData>(PlayerPrefs.GetString(SaveKey));
        if (data?.items == null)
            return;

        foreach (ItemProgressEntry entry in data.items)
        {
            if (!string.IsNullOrWhiteSpace(entry.itemId) && entry.amount > 0)
                acquiredAmounts[entry.itemId] = entry.amount;
        }
    }

    [Serializable]
    private sealed class ItemProgressSaveData
    {
        public List<ItemProgressEntry> items = new();
    }

    [Serializable]
    private sealed class ItemProgressEntry
    {
        public string itemId;
        public int amount;
    }
}
