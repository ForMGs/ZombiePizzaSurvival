using UnityEngine;

public class ZombieHealth : MonoBehaviour
{
    [Header("Health")]
    [SerializeField] private int maxHealth = 30;

    private int currentHealth;

    private void Awake()
    {
        currentHealth = maxHealth;
    }

    public void TakeDamage(int damage)
    {
        currentHealth -= damage;
        currentHealth = Mathf.Max(currentHealth, 0);

        Debug.Log($"Zombie HP: {currentHealth}");

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        Debug.Log("Zombie Dead");

        // 나중에 사망 이펙트, 아이템 드랍 연결 예정
        Destroy(gameObject);
    }
}