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

    [SerializeField] private Color normalHealthColor =
        new Color32(181, 71, 71, 255);

    [SerializeField] private Color dangerHealthColor =
        new Color32(231, 63, 63, 255);

    [Header("Inventory UI")]
    [SerializeField] private TMP_Text fleshText;
    [SerializeField] private TMP_Text clothText;
    [SerializeField] private TMP_Text toothText;

    private void Awake()
    {
        if (healthText != null)
        {
            healthText.textWrappingMode = TextWrappingModes.NoWrap;
            healthText.overflowMode = TextOverflowModes.Overflow;
            healthText.fontSize = 26f;
        }
    }

    private void Update()
    {
        UpdateHealthUI();
        UpdateInventoryUI();
    }

    private void UpdateHealthUI()
    {
        if (playerHealth == null)
            return;

        int currentHealth = playerHealth.CurrentHealth;
        int maxHealth = playerHealth.MaxHealth;

        float healthPercent = maxHealth > 0
            ? Mathf.Clamp01((float)currentHealth / maxHealth)
            : 0f;

        if (healthFill != null)
        {
            healthFill.fillAmount = healthPercent;
            healthFill.color = healthPercent <= 0.25f
                ? dangerHealthColor
                : normalHealthColor;
        }

        if (healthText != null)
        {
            healthText.text =
                $"HP  {currentHealth} / {maxHealth}";
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
