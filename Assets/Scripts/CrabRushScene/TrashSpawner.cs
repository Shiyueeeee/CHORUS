using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class TrashVisual
{
    public Sprite sprite;
    public float scale = 1f;
    public Vector3 offset = Vector3.zero;
}

public class TrashSpawner : MonoBehaviour
{
    [Header("Prefab")]
    [SerializeField] private GameObject trashPrefab;

    [Header("Trash Sprites")]
    [SerializeField] private TrashVisual[] trashVisuals;

    [Header("Spawn Timing")]
    [SerializeField] private float spawnInterval = 2f;

    [Header("Spawn Position")]
    [SerializeField] private float spawnX = 15f;

    [Tooltip("Y")]
    [SerializeField] private float[] laneYPositions = new float[4];

    private void Start()
    {
        StartCoroutine(SpawnRoutine());
    }

    private IEnumerator SpawnRoutine()
    {
        while (true)
        {
            SpawnRandomWave();
            yield return new WaitForSeconds(spawnInterval);
        }
    }

    private void SpawnRandomWave()
    {
        if (trashPrefab == null || laneYPositions.Length < 4)
        {
            Debug.LogWarning("TrashSpawner: Prefab or lane positions not set correctly.");
            return;
        }

        // spawn 1-3
        int spawnCount = Random.Range(1, 4);

        // 4 choose 3
        List<int> availableLanes = new List<int> { 0, 1, 2, 3 };

        for (int i = 0; i < spawnCount; i++)
        {
            //pick from rest lane
            int randomIndex = Random.Range(0, availableLanes.Count);
            int laneIndex = availableLanes[randomIndex];

            float y = laneYPositions[laneIndex];
            Vector3 spawnPos = new Vector3(spawnX, y, 0f);

            GameObject trashInstance = Instantiate(trashPrefab, spawnPos, Quaternion.identity);
            ApplyRandomSprite(trashInstance, spawnPos);

            //remove duplicate
            availableLanes.RemoveAt(randomIndex);
        }
    }

    private void ApplyRandomSprite(GameObject trashInstance, Vector3 baseSpawnPos)
    {
        if (trashInstance == null || trashVisuals == null || trashVisuals.Length == 0)
        {
            return;
        }

        SpriteRenderer spriteRenderer = trashInstance.GetComponent<SpriteRenderer>();
        if (spriteRenderer == null)
        {
            return;
        }

        TrashVisual randomVisual = trashVisuals[Random.Range(0, trashVisuals.Length)];
        if (randomVisual == null || randomVisual.sprite == null)
        {
            return;
        }

        spriteRenderer.sprite = randomVisual.sprite;
        trashInstance.transform.localScale = new Vector3(randomVisual.scale, randomVisual.scale, 1f);
        trashInstance.transform.position = baseSpawnPos + randomVisual.offset;
    }
}
