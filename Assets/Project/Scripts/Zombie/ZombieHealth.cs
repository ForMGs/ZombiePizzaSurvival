using UnityEngine;
using UnityEngine.UI;

public class ZombieHealth : MonoBehaviour
{
    [Header("Health")]
    [SerializeField] private int maxHealth = 30;

    [Header("Health Bar")]
    [SerializeField] private bool showHealthBar = true;
    [SerializeField] private Vector3 healthBarOffset = new Vector3(0f, 2.4f, 0f);
    [SerializeField] private Vector2 healthBarSize = new Vector2(1.2f, 0.12f);
    [SerializeField] private Color healthBarBackColor = new Color(0.1f, 0.1f, 0.1f, 0.75f);
    [SerializeField] private Color healthBarFillColor = new Color(0.9f, 0.15f, 0.1f, 0.95f);

    private int currentHealth;
    private Camera mainCamera;
    private Canvas healthBarCanvas;
    private RectTransform healthBarFillRect;
    private Animator animator;
    private bool isDead;
    private static readonly int deadHash = Animator.StringToHash("Dead");
    private static readonly int hitHash = Animator.StringToHash("Hit");
    
    private ZombieSpawner spawner;

    private void Awake()
    {
        currentHealth = maxHealth;
        mainCamera = Camera.main;
        animator = GetComponentInChildren<Animator>();
        CreateHealthBar();
        UpdateHealthBar();
    }

    public void SetSpawner(ZombieSpawner zombieSpawner)
    {
        spawner = zombieSpawner;
    }
    private void LateUpdate()
    {
        UpdateHealthBarRotation();
    }

    public void TakeDamage(int damage)
    {
        if (isDead)
            return;
        ZombieAI zombieAI = GetComponent<ZombieAI>();
    
        zombieAI?.BeginHit();
        animator?.SetTrigger(hitHash);
        currentHealth -= damage;
        currentHealth = Mathf.Max(currentHealth, 0);

        UpdateHealthBar();


        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        if (isDead)
            return;
        isDead = true;
        animator?.SetBool(deadHash, true);

        ZombieAI zombieAI = GetComponent<ZombieAI>();
        if (zombieAI != null)
            zombieAI.enabled = false;

        Collider zombieCollider = GetComponent<Collider>();
        if (zombieCollider != null)
            zombieCollider.enabled = false;
         Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = true;
            rb.useGravity = false;
        }
        ZombieDropper dropper = GetComponent<ZombieDropper>();

        if (dropper != null)
        {
            dropper.DropItem();
        }

        spawner?.RequestRespawn();

        Destroy(gameObject, 2f);
    }

    private void CreateHealthBar()
    {
        if (!showHealthBar)
            return;

        GameObject canvasObject = new GameObject("Health Bar");
        canvasObject.transform.SetParent(transform);
        canvasObject.transform.localPosition = healthBarOffset;
        canvasObject.transform.localScale = Vector3.one;

        healthBarCanvas = canvasObject.AddComponent<Canvas>();
        healthBarCanvas.renderMode = RenderMode.WorldSpace;

        RectTransform canvasRect = healthBarCanvas.GetComponent<RectTransform>();
        canvasRect.sizeDelta = healthBarSize;

        GameObject backgroundObject = new GameObject("Background");
        backgroundObject.transform.SetParent(canvasObject.transform, false);

        RectTransform backgroundRect = backgroundObject.AddComponent<RectTransform>();
        backgroundRect.anchorMin = Vector2.zero;
        backgroundRect.anchorMax = Vector2.one;
        backgroundRect.offsetMin = Vector2.zero;
        backgroundRect.offsetMax = Vector2.zero;

        Image backgroundImage = backgroundObject.AddComponent<Image>();
        backgroundImage.color = healthBarBackColor;

        GameObject fillObject = new GameObject("Fill");
        fillObject.transform.SetParent(backgroundObject.transform, false);

        healthBarFillRect = fillObject.AddComponent<RectTransform>();
        healthBarFillRect.anchorMin = Vector2.zero;
        healthBarFillRect.anchorMax = Vector2.one;
        healthBarFillRect.offsetMin = Vector2.zero;
        healthBarFillRect.offsetMax = Vector2.zero;

        Image fillImage = fillObject.AddComponent<Image>();
        fillImage.color = healthBarFillColor;
    }

    private void UpdateHealthBar()
    {
        if (healthBarCanvas == null || healthBarFillRect == null)
            return;

        float healthPercent = maxHealth > 0 ? (float)currentHealth / maxHealth : 0f;
        healthBarFillRect.anchorMax = new Vector2(Mathf.Clamp01(healthPercent), 1f);
        healthBarCanvas.gameObject.SetActive(showHealthBar && currentHealth > 0);
    }

    private void UpdateHealthBarRotation()
    {
        if (healthBarCanvas == null)
            return;

        if (mainCamera == null)
        {
            mainCamera = Camera.main;
        }

        if (mainCamera == null)
            return;

        healthBarCanvas.transform.rotation = mainCamera.transform.rotation;
    }
}
