using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    [Header("Health")]
    [SerializeField] private int maxHealth = 100;

    private int currentHealth;

    private void Awake()
    {
        currentHealth = maxHealth;
    }

    public void TakeDamage(int damage)
    {
        currentHealth -= damage;
        currentHealth = Mathf.Max(currentHealth, 0);

        Debug.Log($"Player HP: {currentHealth}");

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        Debug.Log("Player Dead");

        // 나중에 Game Over UI 연결
        gameObject.SetActive(false);
    }
}