using UnityEngine;

public class InventoryUI : MonoBehaviour
{
    [SerializeField] private GameObject inventoryOverlay;
    [SerializeField] private KeyCode toggleKey = KeyCode.I;

     public bool IsOpen { get; private set; }

    private void Awake()
    {
        if (inventoryOverlay == null)
        {
            Debug.LogError("Inventory Overlay가 연결되지 않았습니다.");
        }
    }
    private void Start()
    {
        SetOpen(false);
    }
    private void Update()
    {
        if (Input.GetKeyDown(toggleKey))
            SetOpen(!IsOpen);
    }

    private void SetOpen(bool open)
    {
        if (inventoryOverlay == null)
            return;
        IsOpen = open;
        inventoryOverlay.SetActive(open);

        Time.timeScale = open ? 0f : 1f;
    }

    private void OnDisable()
    {
        Time.timeScale = 1f;
    }

    
}
