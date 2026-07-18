using UnityEngine;

public class Bullet : MonoBehaviour
{
    [SerializeField] private float speed = 35f;
    [SerializeField] private float lifeTime = 0.5f;

    private Vector3 direction;

    public void Initialize(Vector3 fireDirection, float bulletSpeed)
    {
        direction = fireDirection.normalized;
        speed = bulletSpeed;
        Destroy(gameObject, lifeTime);
    }

    private void Update()
    {
        transform.position += direction * speed * Time.deltaTime;
    }
}