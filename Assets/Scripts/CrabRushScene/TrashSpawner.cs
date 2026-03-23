using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrashSpawner : MonoBehaviour
{
    [Header("Prefab")]
    [SerializeField] private GameObject trashPrefab;

    [Header("Trash Sprites")]
    [SerializeField] private Sprite[] trashSprites;
    [SerializeField] private float targetHeight = 1f;

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
            ApplyRandomSprite(trashInstance);

            //remove duplicate
            availableLanes.RemoveAt(randomIndex);
        }
    }

    private void ApplyRandomSprite(GameObject trashInstance)
    {
        if (trashInstance == null || trashSprites == null || trashSprites.Length == 0)
        {
            return;
        }

        SpriteRenderer spriteRenderer = trashInstance.GetComponent<SpriteRenderer>();
        if (spriteRenderer == null)
        {
            return;
        }

        Sprite randomSprite = trashSprites[Random.Range(0, trashSprites.Length)];
        if (randomSprite == null)
        {
            return;
        }

        spriteRenderer.sprite = randomSprite;

        float spriteHeight = randomSprite.bounds.size.y;
        if (spriteHeight > 0f)
        {
            float uniformScale = targetHeight / spriteHeight;
            trashInstance.transform.localScale = new Vector3(uniformScale, uniformScale, 1f);
        }
    }
}
