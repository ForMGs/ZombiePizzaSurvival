using UnityEngine;

public class ZombieDropper : MonoBehaviour
{
    [Header("Drop Settings")]
    [SerializeField] private ItemPickup itemPrefab;
    [SerializeField] private float dropChance = 0.8f;
    [SerializeField] private float minDropRadius = 0.4f;
    [SerializeField] private float maxDropRadius = 0.8f;
    [SerializeField] private float dropHeight = 0.45f;
    [SerializeField] private DropEntry[] dropTable;

    public void DropItem()
    {
        if (itemPrefab == null)
            return;

        if (Random.value > dropChance)
            return;

        DropEntry selectedDrop = GetRandomDrop();

        if (selectedDrop == null)
        {
            Debug.LogWarning(
                "사용 가능한 드롭 데이터가 없습니다.");

            return;
        }

        int minAmount =
            Mathf.Max(1, selectedDrop.minAmount);

        int maxAmount =
            Mathf.Max(minAmount, selectedDrop.maxAmount);

        int amount =
            Random.Range(minAmount, maxAmount + 1);

        Vector2 direction =
            Random.insideUnitCircle.normalized;

        if (direction == Vector2.zero)
            direction = Vector2.right;

        float radius =
            Random.Range(minDropRadius, maxDropRadius);

        Vector3 offset =
            new Vector3(direction.x, 0f, direction.y)
            * radius;

        Vector3 dropPosition =
            transform.position + offset;

        dropPosition.y = dropHeight;

        ItemPickup pickup = Instantiate(
            itemPrefab,
            dropPosition,
            Quaternion.identity);

        pickup.SetItem(
            selectedDrop.item,
            amount);
    }
    private DropEntry GetRandomDrop()
    {
        if (dropTable == null || dropTable.Length == 0)
            return null;

        float totalWeight = 0f;

        foreach (DropEntry entry in dropTable)
        {
            if (entry != null &&
                entry.item != null &&
                entry.weight > 0f)
            {
                totalWeight += entry.weight;
            }
        }

        if (totalWeight <= 0f)
            return null;

        float randomValue =
            Random.Range(0f, totalWeight);

        foreach (DropEntry entry in dropTable)
        {
            if (entry == null ||
                entry.item == null ||
                entry.weight <= 0f)
            {
                continue;
            }

            randomValue -= entry.weight;

            if (randomValue <= 0f)
                return entry;
        }

        return null;
    }

    
}
