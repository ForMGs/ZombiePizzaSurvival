using UnityEngine;

[RequireComponent(typeof(Collider))]
public sealed class DeliveryPoint : MonoBehaviour
{
    [SerializeField] private string destinationId = "sunset_street_12";
    [SerializeField] private KeyCode deliverKey = KeyCode.E;
    [SerializeField] private QuestManager questManager;

    private bool playerInRange;

    private void Awake()
    {
        Collider trigger = GetComponent<Collider>();
        trigger.isTrigger = true;

        if (questManager == null)
            questManager = FindFirstObjectByType<QuestManager>();
    }

    private void Update()
    {
        if (playerInRange && Input.GetKeyDown(deliverKey) && questManager != null)
            questManager.TryCompleteDelivery(destinationId);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.GetComponentInParent<Inventory>() != null)
            playerInRange = true;
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.GetComponentInParent<Inventory>() != null)
            playerInRange = false;
    }
}
