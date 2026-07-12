using UnityEngine;
using System.Collections.Generic;

public class PlayerAttack : MonoBehaviour
{
    [Header("Attack")]
    [SerializeField] private int attackDamage = 10;
    [SerializeField] private float attackRange = 1.8f;
    [SerializeField] private float attackRadius = 1.2f;
    [SerializeField , Range(0f, 360f)] private float attackAngle = 90f;
    [SerializeField] private float attackCooldown = 0.5f;
    [SerializeField] private float attackMoveLockDuration = 0.5f;

    [Header("Weapon Movement")]
    [SerializeField] private bool canMoveWhileAttacking = false;
    [Header("Weapon")]
    [SerializeField] private WeaponController weaponController;

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
    private Animator animator;
    private static readonly int AttackHash = Animator.StringToHash("Attack");


    public bool IsMovementLockedByAttack => !canMoveWhileAttacking && Time.time < attackMoveLockEndTime;

    private void Awake()
    {
        CreateAttackRangeRenderer();
        animator = GetComponentInChildren<Animator>();
        if(weaponController == null)
        {
            weaponController = GetComponent<WeaponController>();
        }
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

        if(weaponController == null || weaponController.CurrentWeapon == null)
        {
            Debug.LogWarning("맨손공격!!!");
        }
        animator?.SetTrigger(AttackHash);
        
        WeaponData currentWeapon = weaponController != null ? weaponController.CurrentWeapon : null;
        
        int damage= currentWeapon != null ? currentWeapon.damage : attackDamage;

        lastAttackTime = Time.time;

        attackMoveLockEndTime = Time.time + attackMoveLockDuration;

       
        ShowAttackRange();

        Collider[] hitZombies = Physics.OverlapSphere(
            transform.position,
            attackRange,
            zombieLayer
        );

        HashSet<ZombieHealth> damagedZombies = new HashSet<ZombieHealth>();

        foreach(Collider hitZombie in hitZombies)
        {
            Vector3 directionToZombie = hitZombie.transform.position - transform.position;
            directionToZombie.y = 0f;

            if(directionToZombie.sqrMagnitude <=0.001f)
                continue;
            
            float angleZombie = Vector3.Angle(
                transform.forward,
                directionToZombie
            );

            if (angleZombie > attackAngle * 0.5f)
                continue;
            
            ZombieHealth zombieHealth =
                hitZombie.GetComponentInParent<ZombieHealth>();
            
            if(zombieHealth != null && damagedZombies.Add(zombieHealth))
            {
                zombieHealth.TakeDamage(damage);
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

    private void ShowAttackRange()
    {
        if (!showAttackRange || attackRangeRenderer == null)
            return;
        
        DrawAttackRangeFan();
        attackRangeRenderer.enabled = true;
        hideAttackRangeTime = Time.time + attackRangeVisibleTime;
    }


    private void DrawAttackRangeFan()
    {
        int arcPointCount = Mathf.Max(2, attackRangeSegments);

        // 플레이어 위치 1개 + 부채꼴 곡선 점들 + 다시 플레이어 위치 1개
        attackRangeRenderer.positionCount = arcPointCount + 2;
        attackRangeRenderer.loop = false;

        Vector3 origin =
            transform.position + Vector3.up * attackRangeLineHeight;

        // 부채꼴 시작점은 플레이어 위치
        attackRangeRenderer.SetPosition(0, origin);

        for (int i = 0; i < arcPointCount; i++)
        {
            float t = (float)i / (arcPointCount - 1);

            float angle = Mathf.Lerp(
                -attackAngle * 0.5f,
                attackAngle * 0.5f,
                t
            );

            Vector3 direction =
                Quaternion.Euler(0f, angle, 0f) * transform.forward;

            Vector3 point =
                origin + direction * attackRange;

            attackRangeRenderer.SetPosition(i + 1, point);
        }

        // 마지막 점을 다시 플레이어 위치로 연결
        attackRangeRenderer.SetPosition(
            arcPointCount + 1,
            origin
        );
    }
}
