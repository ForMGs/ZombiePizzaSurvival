using UnityEngine;

public class ItemPickup : MonoBehaviour
{
    [Header("Item Info")]
    [SerializeField] private ItemData itemData;
    [SerializeField] private int amount = 1;

    [Header("Visual")]
    [SerializeField] private float rotateSpeed = 120f;
    [SerializeField] private float bobHeight = 0.12f;
    [SerializeField] private float bobSpeed = 4f;

    public ItemData ItemData => itemData;
    public int Amount => amount;

    private Renderer itemRenderer;
    private Vector3 basePosition;
    private Transform collectTarget;
    private float collectSpeed;
    private float collectDistance;
    private bool isBeingCollected;
    private bool isPickedUp;

    private void Awake()
    {
        itemRenderer = GetComponentInChildren<Renderer>();
        basePosition = transform.position;
        ApplyVisuals();
    }

    private void Update()
    {
        transform.Rotate(Vector3.up, rotateSpeed * Time.deltaTime, Space.World);

        if (isBeingCollected)
        {
            MoveToCollectTarget();
            return;
        }

        Vector3 bobPosition = basePosition;
        bobPosition.y += Mathf.Sin(Time.time * bobSpeed) * bobHeight;
        transform.position = bobPosition;
    }

    public void SetItem(ItemData data, int itemAmount)
    {
        itemData = data;
        amount = itemAmount;
        basePosition = transform.position;
        ApplyVisuals();
    }

    public void CollectTo(Transform target, float speed, float pickupDistance)
    {
        if (target == null || isPickedUp)
            return;

        collectTarget = target;
        collectSpeed = speed;
        collectDistance = pickupDistance;
        isBeingCollected = true;
    }

    public void Pickup()
    {
        if (isPickedUp)
            return;
        Inventory inventory = null;

        if(collectTarget != null)
        {
            inventory = collectTarget.GetComponentInParent<Inventory>();
        }

        if(inventory == null)
        {
            Debug.LogWarning($"Inventory not found item pickup : {itemData.displayName}");
            isBeingCollected = false;
            basePosition = transform.position;
            return;
        }

        
        bool added = inventory.AddItem(itemData, amount);
        if (!added)
        {
            Debug.Log("인벤토리 공간이 부족합니다.");
            isPickedUp = false;
            isBeingCollected = false;
            basePosition = transform.position;
            return;
        }
        isPickedUp = true;
        Debug.Log($"Picked up item: {itemData.displayName}, Amount: {amount}");
        // Later, connect this to the inventory system.
        Destroy(gameObject);

    }

    private void MoveToCollectTarget()
    {
        if (collectTarget == null)
        {
            isBeingCollected = false;
            basePosition = transform.position;
            return;
        }

        Vector3 targetPosition = collectTarget.position + Vector3.up * 0.5f;
        transform.position = Vector3.MoveTowards(transform.position, targetPosition, collectSpeed * Time.deltaTime);
        basePosition = transform.position;

        if (Vector3.Distance(transform.position, targetPosition) <= collectDistance)
        {
            Pickup();
        }
    }

    private void ApplyVisuals()
    {
        if (itemRenderer == null)
            return;

        Sprite itemIcon = GetItemSprite();
        Material material = itemRenderer.material;

        if (itemIcon != null)
        {
            material.mainTexture = itemIcon.texture;
        }
    }

    private Sprite GetItemSprite()
    {
        return itemData != null ? itemData.icon : null;
    }
}
