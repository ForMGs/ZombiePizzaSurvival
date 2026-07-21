using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public sealed class DeliveryPoint : MonoBehaviour
{
    [SerializeField] private string destinationId = "sunset_street_12";
    [SerializeField] private KeyCode deliverKey = KeyCode.E;
    [SerializeField] private QuestManager questManager;
    [SerializeField] private GameObject deliveryPrompt;

    private bool playerInRange;

    private void Awake()
    {
        BoxCollider trigger = GetComponent<BoxCollider>();
        trigger.isTrigger = true;
        Debug.Log("trigger :   " + trigger);
        if (questManager == null)
            questManager = FindFirstObjectByType<QuestManager>();
        Debug.Log("questManager :   " + questManager);
        if (deliveryPrompt != null)
            deliveryPrompt.SetActive(false);
        Debug.Log("deliveryPrompt :   " + deliveryPrompt);
    }

    private void Update()
    {
        if (!playerInRange || !Input.GetKeyDown(deliverKey) || questManager == null)
            return;

        bool completed = questManager.TryCompleteDelivery(destinationId);

        if (completed && deliveryPrompt != null)
            deliveryPrompt.SetActive(false);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.GetComponentInParent<Inventory>() == null)
            return;

        playerInRange = true;

        if (deliveryPrompt != null)
            deliveryPrompt.SetActive(true);

        Debug.Log($"Player entered delivery point: {destinationId}");
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.GetComponentInParent<Inventory>() == null)
            return;

        playerInRange = false;

        if (deliveryPrompt != null)
            deliveryPrompt.SetActive(false);

        Debug.Log($"Player exited delivery point: {destinationId}");
    }
}
