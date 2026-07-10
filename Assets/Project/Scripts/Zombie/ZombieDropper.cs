using UnityEngine;

public class ZombieDropper : MonoBehaviour
{
    [Header("Drop Settings")]
    [SerializeField] private ItemPickup itemPrefab;
    [SerializeField] private float dropChance = 0.8f;
    [SerializeField] private float minDropRadius = 0.4f;
    [SerializeField] private float maxDropRadius = 0.8f;
    [SerializeField] private float dropHeight = 0.45f;

    [Header("Drop Amount")]
    [SerializeField] private int minAmount = 1;
    [SerializeField] private int maxAmount = 2;

    public void DropItem()
    {
        Debug.Log("드랍아이템함수 실행");
        if (itemPrefab == null)
        {
            Debug.Log("널?");
            return;
        }
            

        float randomValue = Random.value;

        if (randomValue > dropChance)
            return;

        ItemType droppedItem = GetRandomItemType();
        int amount = Random.Range(minAmount, maxAmount + 1);

        Vector2 randomCircle = Random.insideUnitCircle.normalized;

        if (randomCircle == Vector2.zero)
        {
            randomCircle = Vector2.right;
        }

        float dropRadius = Random.Range(minDropRadius, maxDropRadius);
        Vector3 dropOffset = new Vector3(randomCircle.x, 0f, randomCircle.y) * dropRadius;
        Vector3 dropPosition = transform.position + dropOffset;
        dropPosition.y = dropHeight;

        ItemPickup item = Instantiate(itemPrefab, dropPosition, Quaternion.identity);
        item.SetItem(droppedItem, amount);

        Debug.Log($"좀비 드랍: {droppedItem}, 개수: {amount}");
    }

    private ItemType GetRandomItemType()
    {
        int randomValue = Random.Range(1, 101);

        if(randomValue <= 50)
        {
            return ItemType.Flesh;
        }
        else if(randomValue <= 85)
        {
            return ItemType.Cloth;
        }
        else
        {
            return ItemType.Tooth;
        }
    }
}
