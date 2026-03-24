using UnityEngine;

public class TrashMover : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 6f;
    [SerializeField] private float destroyX = -30f;
    [SerializeField] private int damage = 25;

    private bool hasAppliedDamage;
    private Collider2D trashCollider;

    private void Awake()
    {
        trashCollider = GetComponent<Collider2D>();
    }

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
        if (hasAppliedDamage || !other.CompareTag("Player"))
            return;

        DolphinHealth dolphinHealth = other.GetComponentInParent<DolphinHealth>();
        if (dolphinHealth != null)
        {
            ApplyHit();
            dolphinHealth.TakeDamage(damage);
            return;
        }

        PlayerHealth playerHealth = other.GetComponentInParent<PlayerHealth>();
        if (playerHealth == null)
            return;

        ApplyHit();
        playerHealth.TakeDamage(damage);
    }

    private void ApplyHit()
    {
        hasAppliedDamage = true;

        if (trashCollider != null)
            trashCollider.enabled = false;

        Destroy(gameObject);
    }
}
