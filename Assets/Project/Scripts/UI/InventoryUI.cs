using System.Collections.Generic;
using UnityEngine;

public class InventoryUI : MonoBehaviour
{
    [SerializeField] private Inventory inventory;
    [SerializeField] private KeyCode toggleKey = KeyCode.I;

    private bool isOpen;

    private void Awake()
    {
        if (inventory == null)
        {
            inventory = FindFirstObjectByType<Inventory>();
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(toggleKey))
        {
            isOpen = !isOpen;
        }
    }

    private void OnGUI()
    {
        if (!isOpen || inventory == null)
            return;

        const float width = 180f;
        const float lineHeight = 22f;
        const float padding = 10f;

        int lineCount = Mathf.Max(1, inventory.Items.Count);
        float height = 35f + lineCount * lineHeight + padding;

        GUI.Box(new Rect(10f, 10f, width, height), "인벤토리");

        if (inventory.Items.Count == 0)
        {
            GUI.Label(
                new Rect(20f, 40f, width - 20f, lineHeight),
                "보유한 아이템이 없습니다."
            );

            return;
        }

        float y = 40f;

        foreach (KeyValuePair<ItemType, int> item in inventory.Items)
        {
            string itemName = inventory.GetItemName(item.Key);

            GUI.Label(
                new Rect(20f, y, width - 20f, lineHeight),
                $"{itemName}: {item.Value}개"
            );

            y += lineHeight;
        }
    }
}
