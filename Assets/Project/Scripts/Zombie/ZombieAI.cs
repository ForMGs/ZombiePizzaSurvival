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
    [SerializeField] private float arrowForwardDistance = 1.2f;
    [SerializeField] private float arrowGroundHeight = 0.15f;
    [SerializeField] private float arrowYawOffset = 270f;

    private Rigidbody rb;
    private float lastAttackTime;

    private Animator animator;
    private static readonly int SpeedHash = Animator.StringToHash("Speed");
    private static readonly int AttackHash = Animator.StringToHash("Attack");
    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        animator = GetComponentInChildren<Animator>();
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
            animator?.SetFloat(SpeedHash, 0f);
            UpdateTargetArrow(false);
            return;
        }

        float distance = Vector3.Distance(transform.position, target.position);
        bool canSeeTarget = distance <= detectRange;

        UpdateTargetArrow(canSeeTarget);

        if (distance <= attackRange)
        {
            animator?.SetFloat(SpeedHash,0f);
            Attack();
        }
        else if (canSeeTarget)
        {
            animator?.SetFloat(SpeedHash, moveSpeed);
            ChaseTarget();
        }
        else
        {
            animator?.SetFloat(SpeedHash,0f);
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
        animator?.SetFloat(SpeedHash, 0f);
        animator?.SetTrigger(AttackHash);

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

        Vector3 directionToTarget = target.position - transform.position;
        directionToTarget.y = 0f;

        float targetDistance = directionToTarget.magnitude;

        if (targetDistance <= 0.001f)
            return;

        directionToTarget /= targetDistance;
        float arrowDistance = Mathf.Min(arrowForwardDistance, targetDistance * 0.5f);

        // Place the arrow in front of the zombie so it points toward the target.
        targetArrow.position = transform.position + directionToTarget * arrowDistance + Vector3.up * arrowGroundHeight;
        targetArrow.rotation = Quaternion.LookRotation(directionToTarget) * Quaternion.Euler(90f, arrowYawOffset, 0f);
    }
}
