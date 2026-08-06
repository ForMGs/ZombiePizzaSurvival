using UnityEngine;
using UnityEngine.AI;

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
    [Tooltip("공격 타격 프레임에서 데미지가 적용되는 최대 거리입니다.")]
    [SerializeField] private float attackHitRange = 1.6f;
    [Tooltip("좀비 정면을 기준으로 데미지가 적용되는 각도입니다.")]
    [SerializeField, Range(0f, 360f)] private float attackHitAngle = 110f;

    [Header("Target Arrow")]
    [SerializeField] private Transform targetArrow;
    [SerializeField] private bool showTargetArrow = true;
    [SerializeField] private float arrowForwardDistance = 1.2f;
    [SerializeField] private float arrowGroundHeight = 0.15f;
    [SerializeField] private float arrowYawOffset = 270f;

    private NavMeshAgent agent;
    private float lastAttackTime;
    private bool isHit;
    private bool isAttacking;
    private bool attackHitPending;
    private Animator animator;
    private static readonly int SpeedHash = Animator.StringToHash("Speed");
    private static readonly int AttackHash = Animator.StringToHash("Attack");

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        if (agent != null)
        {
            agent.speed = moveSpeed;
            agent.stoppingDistance = attackRange;
        }
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
            StopMoving();
            UpdateTargetArrow(false);
            return;
        }
        if(isHit || isAttacking)
        {
            StopMoving();
            return;
        }

        float distance = Vector3.Distance(transform.position, target.position);
        bool canSeeTarget = distance <= detectRange;

        UpdateTargetArrow(canSeeTarget);

        if (distance <= attackRange)
        {
            StopMoving();
            Attack();
        }
        else if (canSeeTarget)
        {
            animator?.SetFloat(SpeedHash, moveSpeed);
            ChaseTarget();
        }
        else
        {
            StopMoving();
        }
    }

    private void ChaseTarget()
    {
        if (agent == null || !agent.isOnNavMesh)
            return;

        agent.isStopped = false;
        agent.SetDestination(target.position);
    }
    private void StopMoving()
    {
        if(agent != null && agent.isOnNavMesh)
        {
            agent.isStopped = true;
            agent.ResetPath();
        }
        animator?.SetFloat(SpeedHash, 0f);
    }
    private void Attack()
    {
        if (Time.time < lastAttackTime + attackCooldown)
            return;
        
        isAttacking = true;
        attackHitPending = true;
        lastAttackTime = Time.time;
        animator?.SetFloat(SpeedHash, 0f);
        animator?.SetTrigger(AttackHash);

    }

    public void OnAttackHit()
    {
        if (!isAttacking || !attackHitPending || target == null ||
            !target.gameObject.activeInHierarchy)
            return;

        attackHitPending = false;

        Vector3 directionToTarget = target.position - transform.position;
        directionToTarget.y = 0f;
        float distance = directionToTarget.magnitude;
        if (distance > attackHitRange || distance <= 0.001f)
            return;

        float angle = Vector3.Angle(transform.forward, directionToTarget);
        if (angle > attackHitAngle * 0.5f)
            return;

        PlayerHealth playerHealth = target.GetComponent<PlayerHealth>();
        playerHealth?.TakeDamage(attackDamage);
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
    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
    }
    public void BeginHit()
    {
        isHit = true;
        isAttacking = false;
        attackHitPending = false;
        animator?.SetFloat(SpeedHash, 0f);
    }
    public void EndHit()
    {
        isHit = false;
    }
    public void EndAttack()
    {
        isAttacking = false;
        attackHitPending = false;
    }
}
