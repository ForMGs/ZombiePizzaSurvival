using UnityEngine;

public class ZombieAI : MonoBehaviour
{
    [Header("Target")]
    [SerializeField] private Transform target;

    [Header("Movement")]
    [SerializeField] private float moveSpeed = 3f;
    [SerializeField] private float detectRange = 12f;
    [SerializeField] private float attackRange = 1.5f;

    [Header("Attack")]
    [SerializeField] private int attackDamage = 10;
    [SerializeField] private float attackCooldown = 1.2f;

    [Header("Target Arrow")]
    [SerializeField] private Transform targetArrow;
    [SerializeField] private bool showTargetArrow = true;
    [SerializeField] private float arrowHeight = 2f;
    [SerializeField] private float arrowYawOffset = 270f;

    private Rigidbody rb;
    private float lastAttackTime;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        UpdateTargetArrow(false);
    }

    private void OnDisable()
    {
        UpdateTargetArrow(false);
    }

    private void FixedUpdate()
    {
        if (target == null || !target.gameObject.activeInHierarchy)
        {
            UpdateTargetArrow(false);
            return;
        }

        float distance = Vector3.Distance(transform.position, target.position);
        bool canSeeTarget = distance <= detectRange;

        UpdateTargetArrow(canSeeTarget);

        if (distance <= attackRange)
        {
            Attack();
        }
        else if (canSeeTarget)
        {
            ChaseTarget();
        }
    }

    private void ChaseTarget()
    {
        Vector3 direction = target.position - transform.position;
        direction.y = 0f;
        direction.Normalize();

        Vector3 nextPosition = rb.position + direction * moveSpeed * Time.fixedDeltaTime;
        rb.MovePosition(nextPosition);

        if (direction != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            rb.rotation = Quaternion.Slerp(rb.rotation, targetRotation, 10f * Time.fixedDeltaTime);
        }
    }

    private void Attack()
    {
        if (Time.time < lastAttackTime + attackCooldown)
            return;

        lastAttackTime = Time.time;

        PlayerHealth playerHealth = target.GetComponent<PlayerHealth>();

        if (playerHealth != null)
        {
            playerHealth.TakeDamage(attackDamage);
        }

        Debug.Log("Zombie Attack");
    }

    private void UpdateTargetArrow(bool isVisible)
    {
        if (targetArrow == null)
            return;

        targetArrow.gameObject.SetActive(showTargetArrow && isVisible);

        if (!showTargetArrow || !isVisible || target == null)
            return;

        // Keep the arrow above the zombie while it is tracking the player.
        targetArrow.position = transform.position + Vector3.up * arrowHeight;

        // Rotate the arrow so it points toward the player.
        Vector3 directionToTarget = target.position - transform.position;
        directionToTarget.y = 0f;

        if (directionToTarget == Vector3.zero)
            return;

        targetArrow.rotation = Quaternion.LookRotation(directionToTarget) * Quaternion.Euler(90f, arrowYawOffset, 0f);
    }
}
