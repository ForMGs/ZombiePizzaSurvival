using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 6f;

    [Header("Aim")]
    [SerializeField] private Camera aimCamera;
    [SerializeField] private float rotationSpeed = 12f;

    private Rigidbody rb;
    private Vector3 moveDirection;
    private Vector3 aimDirection;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();

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
        UpdateAimDirection();
    }

    private void FixedUpdate()
    {
        Vector3 nextPosition = rb.position + moveDirection * moveSpeed * Time.fixedDeltaTime;
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
}
