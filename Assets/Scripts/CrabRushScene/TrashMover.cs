using UnityEngine;

public class TrashMover : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 6f;
    [SerializeField] private float destroyX = -30f;
    [SerializeField] private int damage = 25;

    private void Update()
    {
        transform.Translate(Vector3.left * moveSpeed * Time.deltaTime);

        if (transform.position.x <= destroyX)
        {
            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerHealth playerHealth = other.GetComponentInParent<PlayerHealth>();
            DolphinHealth dolphinHealth = other.GetComponentInParent<DolphinHealth>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(damage);
            }

            if (dolphinHealth != null)
            {
                dolphinHealth.TakeDamage(damage);
            }

            Destroy(gameObject);
        }
    }
}
