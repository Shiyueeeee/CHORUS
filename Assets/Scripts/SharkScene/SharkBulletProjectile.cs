using UnityEngine;

public class SharkBulletProjectile : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 8f;
    [SerializeField] private float lifetime = 6f;
    [SerializeField] private int damage = 10;

    private Vector3 moveDirection = Vector3.left;
    private Quaternion baseRotation = Quaternion.identity;

    private void Awake()
    {
        baseRotation = transform.rotation;
    }

    private void OnEnable()
    {
        Destroy(gameObject, lifetime);
        UpdateRotation();
    }

    private void Update()
    {
        transform.position += moveDirection * moveSpeed * Time.deltaTime;
    }

    public void Initialize(Vector3 direction)
    {
        if (direction.sqrMagnitude > 0.0001f)
        {
            moveDirection = direction.normalized;
            UpdateRotation();
        }
    }

    private void UpdateRotation()
    {
        if (moveDirection.sqrMagnitude <= 0.0001f)
            return;

        float angle = Mathf.Atan2(moveDirection.y, moveDirection.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0f, 0f, angle) * baseRotation;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerHealth playerHealth = other.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(damage);
            }

            Destroy(gameObject);
            return;
        }

        if (!other.isTrigger)
        {
            Destroy(gameObject);
        }
    }
}
