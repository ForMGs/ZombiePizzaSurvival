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

    private Rigidbody rb;
    private float lastAttackTime;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    private void FixedUpdate()
    {
        if (target == null)
            return;

        float distance = Vector3.Distance(transform.position, target.position);

        if (distance <= attackRange)
        {
            Attack();
        }
        else if (distance <= detectRange)
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
}