using UnityEngine;

public class SharkBarrageController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform playerTransform;
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private BoxCollider2D arenaBoundsCollider;

    [Header("Arena")]
    [SerializeField] private float spawnPadding = 1f;

    [Header("Pattern")]
    [SerializeField] private float spawnInterval = 0.35f;
    [SerializeField] private int bulletsPerVolley = 3;
    [SerializeField] private bool aimAtPlayer = true;

    private float spawnTimer;
    private bool barrageActive;

    private void Awake()
    {
        AutoAssignReferences();
    }

    private void OnEnable()
    {
        spawnTimer = 0f;
    }

    private void Update()
    {
        if (!barrageActive || bulletPrefab == null)
            return;

        spawnTimer -= Time.deltaTime;
        if (spawnTimer <= 0f)
        {
            SpawnVolley();
            spawnTimer = spawnInterval;
        }
    }

    public void BeginBarrage()
    {
        barrageActive = true;
        spawnTimer = 0f;
    }

    public void EndBarrage()
    {
        barrageActive = false;
    }

    private void SpawnVolley()
    {
        for (int i = 0; i < bulletsPerVolley; i++)
        {
            SpawnSingleBullet();
        }
    }

    private void SpawnSingleBullet()
    {
        Vector3 spawnPosition = GetRandomEdgeSpawnPosition();
        GameObject bullet = Instantiate(bulletPrefab, spawnPosition, Quaternion.identity);

        SharkBulletProjectile projectile = bullet.GetComponent<SharkBulletProjectile>();
        if (projectile != null)
        {
            projectile.Initialize(GetBulletDirection(spawnPosition));
        }
    }

    private Vector3 GetRandomEdgeSpawnPosition()
    {
        Bounds bounds = GetArenaBounds();
        int edge = Random.Range(0, 4);
        switch (edge)
        {
            case 0:
                return new Vector3(bounds.min.x - spawnPadding, Random.Range(bounds.min.y, bounds.max.y), 0f);
            case 1:
                return new Vector3(bounds.max.x + spawnPadding, Random.Range(bounds.min.y, bounds.max.y), 0f);
            case 2:
                return new Vector3(Random.Range(bounds.min.x, bounds.max.x), bounds.max.y + spawnPadding, 0f);
            default:
                return new Vector3(Random.Range(bounds.min.x, bounds.max.x), bounds.min.y - spawnPadding, 0f);
        }
    }

    private Vector3 GetBulletDirection(Vector3 spawnPosition)
    {
        if (aimAtPlayer && playerTransform != null)
        {
            return playerTransform.position - spawnPosition;
        }

        Bounds bounds = GetArenaBounds();
        Vector3 arenaCenter = bounds.center;
        arenaCenter.z = 0f;
        return arenaCenter - spawnPosition;
    }

    private Bounds GetArenaBounds()
    {
        if (arenaBoundsCollider != null)
        {
            return arenaBoundsCollider.bounds;
        }

        return new Bounds(Vector3.zero, new Vector3(20f, 10f, 0f));
    }

    private void AutoAssignReferences()
    {
        if (playerTransform == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                playerTransform = player.transform;
            }
        }
    }
}
