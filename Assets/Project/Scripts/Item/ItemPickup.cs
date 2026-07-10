using UnityEngine;

public class ItemPickup : MonoBehaviour
{
    [Header("Item Info")]
    [SerializeField] private ItemType itemType;
    [SerializeField] private int amount = 1;

    [Header("Visual")]
    [SerializeField] private float rotateSpeed = 120f;
    [SerializeField] private float bobHeight = 0.12f;
    [SerializeField] private float bobSpeed = 4f;
    [SerializeField] private Color fleshColor = new Color(0.9f, 0.15f, 0.12f, 1f);
    [SerializeField] private Color clothColor = new Color(0.25f, 0.75f, 1f, 1f);
    [SerializeField] private Color toothColor = new Color(1f, 0.92f, 0.55f, 1f);

    public ItemType ItemType => itemType;
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

    public void SetItem(ItemType type, int itemAmount)
    {
        itemType = type;
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

        isPickedUp = true;
        Debug.Log($"Picked up item: {itemType}, Amount: {amount}");

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

        Color itemColor = GetItemColor();
        Material material = itemRenderer.material;

        if (material.HasProperty("_BaseColor"))
        {
            material.SetColor("_BaseColor", itemColor);
        }
        else if (material.HasProperty("_Color"))
        {
            material.SetColor("_Color", itemColor);
        }
    }

    private Color GetItemColor()
    {
        switch (itemType)
        {
            case ItemType.Cloth:
                return clothColor;
            case ItemType.Tooth:
                return toothColor;
            default:
                return fleshColor;
        }
    }
}
