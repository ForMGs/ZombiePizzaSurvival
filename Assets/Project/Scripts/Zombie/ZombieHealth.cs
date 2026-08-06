using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

public class ZombieHealth : MonoBehaviour
{
    [Header("Health")]
    [SerializeField] private int maxHealth = 30;
    [Tooltip("처치 퀘스트에서 이 좀비 종류를 구분하는 ID입니다.")]
    [SerializeField] private string questZombieId = "zombie";

    [Header("Health Bar")]
    [SerializeField] private bool showHealthBar = true;
    [SerializeField] private Vector3 healthBarOffset = new Vector3(0f, 2.4f, 0f);
    [SerializeField] private Vector2 healthBarSize = new Vector2(1.2f, 0.12f);
    [SerializeField] private Color healthBarBackColor = new Color(0.1f, 0.1f, 0.1f, 0.75f);
    [SerializeField] private Color healthBarFillColor = new Color(0.9f, 0.15f, 0.1f, 0.95f);

    [Header("Hit Feedback")]
    [SerializeField] private Color hitFlashColor = new Color(1f, 0.12f, 0.12f, 1f);
    [Min(0f)] [SerializeField] private float hitFlashDuration = 0.08f;
    [SerializeField] private Vector3 damageTextOffset = new Vector3(0f, 2.7f, 0f);
    [SerializeField] private Color damageTextColor = new Color(1f, 0.25f, 0.1f, 1f);
    [Min(0.1f)] [SerializeField] private float damageTextSize = 3f;

    private int currentHealth;
    private Camera mainCamera;
    private Canvas healthBarCanvas;
    private RectTransform healthBarFillRect;
    private Animator animator;
    private bool isDead;
    private static readonly int deadHash = Animator.StringToHash("Dead");
    private static readonly int hitHash = Animator.StringToHash("Hit");
    private Coroutine knockbackRoutine;
    private Coroutine hitFlashRoutine;
    private readonly List<FlashMaterial> flashMaterials = new();

    private sealed class FlashMaterial
    {
        public Renderer Renderer;
        public int MaterialIndex;
        public int ColorProperty;
        public Color OriginalColor;
    }
    
    private ZombieSpawner spawner;

    private void Awake()
    {
        currentHealth = maxHealth;
        mainCamera = Camera.main;
        animator = GetComponentInChildren<Animator>();
        CacheFlashMaterials();
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
        TakeDamage(damage, transform.position + Vector3.up, Vector3.zero, null);
    }

    public void TakeDamage(
        int damage,
        Vector3 hitPoint,
        Vector3 hitDirection,
        WeaponData weapon)
    {
        if (isDead)
            return;
        ZombieAI zombieAI = GetComponent<ZombieAI>();
    
        zombieAI?.BeginHit();
        animator?.SetTrigger(hitHash);
        currentHealth -= damage;
        currentHealth = Mathf.Max(currentHealth, 0);

        UpdateHealthBar();
        ShowDamageFeedback(damage);

        if (weapon != null)
        {
            CombatHitFeedback.Play(hitPoint, hitDirection, weapon);

            if (knockbackRoutine != null)
                StopCoroutine(knockbackRoutine);
            if (weapon.knockbackDistance > 0f)
                knockbackRoutine = StartCoroutine(ApplyKnockback(
                    hitDirection,
                    weapon.knockbackDistance,
                    weapon.knockbackDuration));
        }


        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private void CacheFlashMaterials()
    {
        foreach (Renderer targetRenderer in GetComponentsInChildren<Renderer>(true))
        {
            Material[] materials = targetRenderer.sharedMaterials;
            for (int i = 0; i < materials.Length; i++)
            {
                Material material = materials[i];
                if (material == null)
                    continue;

                int colorProperty = material.HasProperty("_BaseColor")
                    ? Shader.PropertyToID("_BaseColor")
                    : material.HasProperty("_Color")
                        ? Shader.PropertyToID("_Color")
                        : -1;

                if (colorProperty < 0)
                    continue;

                flashMaterials.Add(new FlashMaterial
                {
                    Renderer = targetRenderer,
                    MaterialIndex = i,
                    ColorProperty = colorProperty,
                    OriginalColor = material.GetColor(colorProperty)
                });
            }
        }
    }

    private void ShowDamageFeedback(int damage)
    {
        if (hitFlashRoutine != null)
            StopCoroutine(hitFlashRoutine);
        RestoreFlashColor();
        hitFlashRoutine = StartCoroutine(FlashRoutine());

        DamagePopup.Create(
            transform.position + damageTextOffset,
            damage,
            damageTextColor,
            damageTextSize);
    }

    private IEnumerator FlashRoutine()
    {
        MaterialPropertyBlock propertyBlock = new();
        foreach (FlashMaterial flashMaterial in flashMaterials)
        {
            if (flashMaterial.Renderer == null)
                continue;

            flashMaterial.Renderer.GetPropertyBlock(
                propertyBlock,
                flashMaterial.MaterialIndex);
            propertyBlock.SetColor(flashMaterial.ColorProperty, hitFlashColor);
            flashMaterial.Renderer.SetPropertyBlock(
                propertyBlock,
                flashMaterial.MaterialIndex);
            propertyBlock.Clear();
        }

        if (hitFlashDuration > 0f)
            yield return new WaitForSeconds(hitFlashDuration);

        RestoreFlashColor();
        hitFlashRoutine = null;
    }

    private void RestoreFlashColor()
    {
        MaterialPropertyBlock propertyBlock = new();
        foreach (FlashMaterial flashMaterial in flashMaterials)
        {
            if (flashMaterial.Renderer == null)
                continue;

            flashMaterial.Renderer.GetPropertyBlock(
                propertyBlock,
                flashMaterial.MaterialIndex);
            propertyBlock.SetColor(
                flashMaterial.ColorProperty,
                flashMaterial.OriginalColor);
            flashMaterial.Renderer.SetPropertyBlock(
                propertyBlock,
                flashMaterial.MaterialIndex);
            propertyBlock.Clear();
        }
    }

    private void OnDestroy()
    {
        RestoreFlashColor();
    }

    private IEnumerator ApplyKnockback(
        Vector3 direction,
        float distance,
        float duration)
    {
        direction.y = 0f;
        if (direction.sqrMagnitude <= 0.001f)
            yield break;

        direction.Normalize();
        NavMeshAgent agent = GetComponent<NavMeshAgent>();
        float elapsed = 0f;
        float movedDistance = 0f;

        while (elapsed < duration && !isDead)
        {
            float deltaTime = Time.deltaTime;
            elapsed += deltaTime;
            float targetDistance = distance * Mathf.Clamp01(elapsed / duration);
            float stepDistance = targetDistance - movedDistance;

            if (agent != null && agent.enabled && agent.isOnNavMesh)
                agent.Move(direction * stepDistance);
            else
                transform.position += direction * stepDistance;

            movedDistance = targetDistance;
            yield return null;
        }

        knockbackRoutine = null;
    }

    private void Die()
    {
        if (isDead)
            return;
        isDead = true;
        QuestProgressEvents.ReportZombieKilled(questZombieId);
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

public sealed class DamagePopup : MonoBehaviour
{
    private const float Lifetime = 0.65f;
    private const float RiseSpeed = 1.2f;

    private TextMeshPro textMesh;
    private Camera targetCamera;
    private float elapsed;
    private Color startColor;

    public static void Create(
        Vector3 position,
        int damage,
        Color color,
        float fontSize)
    {
        GameObject popupObject = new("Damage Popup");
        popupObject.transform.position = position;

        TextMeshPro popupText = popupObject.AddComponent<TextMeshPro>();
        popupText.text = damage.ToString();
        popupText.fontSize = fontSize;
        popupText.fontStyle = FontStyles.Bold;
        popupText.alignment = TextAlignmentOptions.Center;
        popupText.color = color;
        popupText.textWrappingMode = TextWrappingModes.NoWrap;
        popupText.sortingOrder = 100;

        DamagePopup popup = popupObject.AddComponent<DamagePopup>();
        popup.textMesh = popupText;
        popup.startColor = color;
        popup.targetCamera = Camera.main;
    }

    private void LateUpdate()
    {
        float deltaTime = Time.deltaTime;
        elapsed += deltaTime;
        transform.position += Vector3.up * (RiseSpeed * deltaTime);

        if (targetCamera == null)
            targetCamera = Camera.main;
        if (targetCamera != null)
            transform.rotation = targetCamera.transform.rotation;

        float progress = Mathf.Clamp01(elapsed / Lifetime);
        Color color = startColor;
        color.a = 1f - progress;
        textMesh.color = color;
        transform.localScale = Vector3.one * Mathf.Lerp(0.7f, 1f, progress);

        if (elapsed >= Lifetime)
            Destroy(gameObject);
    }
}
