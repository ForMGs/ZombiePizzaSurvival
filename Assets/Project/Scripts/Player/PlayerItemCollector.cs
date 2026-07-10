using UnityEngine;

public class PlayerItemCollector : MonoBehaviour
{
    [Header("Input")]
    [SerializeField] private KeyCode collectKey = KeyCode.F;

    [Header("Collect Range")]
    [SerializeField] private float minCollectRadius = 0.8f;
    [SerializeField] private float maxCollectRadius = 4f;
    [SerializeField] private float radiusGrowSpeed = 3f;
    [SerializeField] private LayerMask itemLayerMask = ~0;

    [Header("Pull")]
    [SerializeField] private float itemPullSpeed = 8f;
    [SerializeField] private float pickupDistance = 0.8f;

    [Header("Range Visual")]
    [SerializeField] private bool showCollectRange = true;
    [SerializeField] private float rangeLineHeight = 0.05f;
    [SerializeField] private int rangeSegments = 72;
    [SerializeField] private float rangeLineWidth = 0.05f;
    [SerializeField] private Color rangeColor = new Color(0.2f, 0.85f, 1f, 0.85f);

    private float currentCollectRadius;
    private LineRenderer rangeRenderer;

    private void Awake()
    {
        currentCollectRadius = minCollectRadius;
        CreateRangeRenderer();
    }

    private void Update()
    {
        if (Input.GetKey(collectKey))
        {
            currentCollectRadius = Mathf.MoveTowards(
                currentCollectRadius,
                maxCollectRadius,
                radiusGrowSpeed * Time.deltaTime
            );

            CollectItemsInRange();
            UpdateRangeRenderer(true);
        }
        else
        {
            currentCollectRadius = minCollectRadius;
            UpdateRangeRenderer(false);
        }
    }

    private void CollectItemsInRange()
    {
        Collider[] hits = Physics.OverlapSphere(
            transform.position,
            currentCollectRadius,
            itemLayerMask,
            QueryTriggerInteraction.Collide
        );

        foreach (Collider hit in hits)
        {
            ItemPickup itemPickup = hit.GetComponentInParent<ItemPickup>();

            if (itemPickup != null)
            {
                itemPickup.CollectTo(transform, itemPullSpeed, pickupDistance);
            }
        }
    }

    private void CreateRangeRenderer()
    {
        GameObject rangeObject = new GameObject("Item Collect Range");
        rangeObject.transform.SetParent(transform);

        rangeRenderer = rangeObject.AddComponent<LineRenderer>();
        rangeRenderer.useWorldSpace = true;
        rangeRenderer.loop = true;
        rangeRenderer.positionCount = rangeSegments;
        rangeRenderer.startWidth = rangeLineWidth;
        rangeRenderer.endWidth = rangeLineWidth;
        rangeRenderer.enabled = false;
        rangeRenderer.material = new Material(Shader.Find("Sprites/Default"));
        rangeRenderer.startColor = rangeColor;
        rangeRenderer.endColor = rangeColor;
    }

    private void UpdateRangeRenderer(bool isVisible)
    {
        if (rangeRenderer == null)
        {
            return;
        }

        if (!showCollectRange)
        {
            rangeRenderer.enabled = false;
            return;
        }

        rangeRenderer.enabled = isVisible;

        if (!isVisible)
            return;

        for (int i = 0; i < rangeSegments; i++)
        {
            float angle = (float)i / rangeSegments * Mathf.PI * 2f;
            Vector3 point = transform.position + new Vector3(
                Mathf.Cos(angle) * currentCollectRadius,
                rangeLineHeight,
                Mathf.Sin(angle) * currentCollectRadius
            );

            rangeRenderer.SetPosition(i, point);
        }
    }
}
