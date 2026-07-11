using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerHUD : MonoBehaviour
{
    [Header("Player")]
    [SerializeField] private PlayerHealth playerHealth;
    [SerializeField] private Inventory inventory;
    
    [Header("Helath UI")]
    [SerializeField] private Image healthFill;
    [SerializeField] private TMP_Text healthText;

    [Header("Inventory UI")]
    [SerializeField] private TMP_Text fleshText;
    [SerializeField] private TMP_Text clothText;
    [SerializeField] private TMP_Text toothText;

    private void Update()
    {
        UpdateHealthUI();
        UpdateInventoryUI();
    }

    private void UpdateHealthUI()
    {
        if (playerHealth == null)
        return;

        float healthPercent = (float)playerHealth.CurrentHealth / playerHealth.MaxHealth;

        if(healthFill != null)
        {
            healthFill.fillAmount = healthPercent;
        }

        if(healthText != null)
        {
            healthText.text = $"{playerHealth.CurrentHealth} / {playerHealth.MaxHealth}";
        }
    }

    private void UpdateInventoryUI()
    {
         if (inventory == null)
            return;

        if (fleshText != null)
            fleshText.text = $"살점 {inventory.GetAmount(ItemType.Flesh)}";

        if (clothText != null)
            clothText.text = $"천 {inventory.GetAmount(ItemType.Cloth)}";

        if (toothText != null)
            toothText.text = $"이빨 {inventory.GetAmount(ItemType.Tooth)}";
    }

}