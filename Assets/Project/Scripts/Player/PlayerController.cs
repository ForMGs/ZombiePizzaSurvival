using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 6f;

    [Header("Aim")]
    [SerializeField] private Camera aimCamera;
    [SerializeField] private float rotationSpeed = 12f;

    private Rigidbody rb;
    private Animator animator;
    private static readonly int SpeedHash = Animator.StringToHash("Speed");
    private PlayerAttack playerAttack;
    private Vector3 moveDirection;
    private Vector3 aimDirection;

    private static readonly int MoveXHash = Animator.StringToHash("MoveX");
    private static readonly int MoveYHash = Animator.StringToHash("MoveY");
    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        animator = GetComponentInChildren<Animator>();
        playerAttack = GetComponent<PlayerAttack>();

        if (aimCamera == null)
        {
            aimCamera = Camera.main;
        }
    }

    private void Update()
    {
        float horizontal = 0f;
        float vertical = 0f;

        if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow))
            horizontal = -1f;

        if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow))
            horizontal = 1f;

        if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow))
            vertical = 1f;

        if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow))
            vertical = -1f;

        moveDirection = new Vector3(horizontal, 0f, vertical).normalized;
        Vector3 localMoveDirection = transform.InverseTransformDirection(moveDirection);
        animator?.SetFloat(MoveXHash, localMoveDirection.x, 0.1f, Time.deltaTime);
        animator?.SetFloat(MoveYHash, localMoveDirection.z, 0.1f, Time.deltaTime);
        animator?.SetFloat(SpeedHash, moveDirection.magnitude, 0.1f, Time.deltaTime);
        
        UpdateAimDirection();
    }

    private void FixedUpdate()
    {
        Vector3 currentMoveDirection = CanMove() ? moveDirection : Vector3.zero;
        Vector3 nextPosition = rb.position + currentMoveDirection * moveSpeed * Time.fixedDeltaTime;
        rb.MovePosition(nextPosition);

        if (aimDirection != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(aimDirection);
            rb.rotation = Quaternion.Slerp(rb.rotation, targetRotation, rotationSpeed * Time.fixedDeltaTime);
        }
    }

    private void UpdateAimDirection()
    {
        if (aimCamera == null)
            return;

        // Project the mouse position onto the player's ground height.
        Ray mouseRay = aimCamera.ScreenPointToRay(Input.mousePosition);
        Plane groundPlane = new Plane(Vector3.up, transform.position);

        if (!groundPlane.Raycast(mouseRay, out float hitDistance))
            return;

        Vector3 mouseWorldPosition = mouseRay.GetPoint(hitDistance);
        Vector3 direction = mouseWorldPosition - transform.position;
        direction.y = 0f;

        if (direction.sqrMagnitude <= 0.001f)
            return;

        aimDirection = direction.normalized;
    }

    private bool CanMove()
    {
        return playerAttack == null || !playerAttack.IsMovementLockedByAttack;
    }
}
