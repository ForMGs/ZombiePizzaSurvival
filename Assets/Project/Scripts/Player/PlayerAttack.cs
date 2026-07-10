using UnityEngine;

public class PlayerAttack : MonoBehaviour
{
    [Header("Attack")]
    [SerializeField] private int attackDamage = 10;
    [SerializeField] private float attackRange = 1.8f;
    [SerializeField] private float attackRadius = 1.2f;
    [SerializeField] private float attackCooldown = 0.5f;
    [SerializeField] private float attackMoveLockDuration = 0.5f;

    [Header("Weapon Movement")]
    [SerializeField] private bool canMoveWhileAttacking = false;

    [Header("Target")]
    [SerializeField] private LayerMask zombieLayer;

    [Header("Attack Range Visual")]
    [SerializeField] private bool showAttackRange = true;
    [SerializeField] private float attackRangeVisibleTime = 0.15f;
    [SerializeField] private float attackRangeLineHeight = 0.05f;
    [SerializeField] private int attackRangeSegments = 48;
    [SerializeField] private Color attackRangeColor = new Color(1f, 0.2f, 0.1f, 0.85f);

    private float lastAttackTime;
    private float attackMoveLockEndTime;
    private float hideAttackRangeTime;
    private LineRenderer attackRangeRenderer;

    public bool IsMovementLockedByAttack => !canMoveWhileAttacking && Time.time < attackMoveLockEndTime;

    private void Awake()
    {
        CreateAttackRangeRenderer();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space) || Input.GetMouseButtonDown(0))
        {
            Attack();
        }

        // Show the attack range briefly after each attack, then hide it.
        if (attackRangeRenderer != null && attackRangeRenderer.enabled && Time.time >= hideAttackRangeTime)
        {
            attackRangeRenderer.enabled = false;
        }
    }

    private void Attack()
    {
        if (Time.time < lastAttackTime + attackCooldown)
            return;

        lastAttackTime = Time.time;
        attackMoveLockEndTime = Time.time + attackMoveLockDuration;

        // Place the attack hit area in front of the player.
        Vector3 attackCenter = transform.position + transform.forward * attackRange;
        ShowAttackRange(attackCenter);

        Collider[] hitZombies = Physics.OverlapSphere(
            attackCenter,
            attackRadius,
            zombieLayer
        );

        foreach (Collider hitZombie in hitZombies)
        {
            ZombieHealth zombieHealth = hitZombie.GetComponent<ZombieHealth>();

            if (zombieHealth != null)
            {
                zombieHealth.TakeDamage(attackDamage);
            }
        }

        Debug.Log($"Player Attack - Hit Count: {hitZombies.Length}");
    }

    public void SetCanMoveWhileAttacking(bool canMove)
    {
        canMoveWhileAttacking = canMove;
    }

    public void SetWeaponMovementOptions(bool canMoveDuringAttack, float moveLockDuration)
    {
        canMoveWhileAttacking = canMoveDuringAttack;
        attackMoveLockDuration = Mathf.Max(0f, moveLockDuration);
    }

    private void CreateAttackRangeRenderer()
    {
        GameObject rangeObject = new GameObject("Attack Range Visual");
        rangeObject.transform.SetParent(transform);

        attackRangeRenderer = rangeObject.AddComponent<LineRenderer>();
        attackRangeRenderer.useWorldSpace = true;
        attackRangeRenderer.loop = true;
        attackRangeRenderer.positionCount = attackRangeSegments;
        attackRangeRenderer.startWidth = 0.06f;
        attackRangeRenderer.endWidth = 0.06f;
        attackRangeRenderer.enabled = false;

        // Use a runtime material so the range line is visible in Game view.
        attackRangeRenderer.material = new Material(Shader.Find("Sprites/Default"));
        attackRangeRenderer.startColor = attackRangeColor;
        attackRangeRenderer.endColor = attackRangeColor;
    }

    private void ShowAttackRange(Vector3 attackCenter)
    {
        if (!showAttackRange || attackRangeRenderer == null)
            return;

        DrawAttackRangeCircle(attackCenter);
        attackRangeRenderer.enabled = true;
        hideAttackRangeTime = Time.time + attackRangeVisibleTime;
    }

    private void DrawAttackRangeCircle(Vector3 attackCenter)
    {
        // Draw the OverlapSphere hit area as a ground circle for top-down view.
        for (int i = 0; i < attackRangeSegments; i++)
        {
            float angle = (float)i / attackRangeSegments * Mathf.PI * 2f;
            Vector3 point = attackCenter + new Vector3(
                Mathf.Cos(angle) * attackRadius,
                attackRangeLineHeight,
                Mathf.Sin(angle) * attackRadius
            );

            attackRangeRenderer.SetPosition(i, point);
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;

        Vector3 attackCenter = transform.position + transform.forward * attackRange;
        Gizmos.DrawWireSphere(attackCenter, attackRadius);
    }
}
