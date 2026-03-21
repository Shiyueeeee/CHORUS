using UnityEngine;

public class ScenePlayerSpawnSetter : MonoBehaviour
{
    [SerializeField] private Transform playerTransform;

    private void Start()
    {
        if (playerTransform == null)
            return;

        bool hasFacing = false;
        bool facingRight = true;

        if (!string.IsNullOrEmpty(SceneSpawnData.returnSpawnPointName))
        {
            GameObject spawnPoint = GameObject.Find(SceneSpawnData.returnSpawnPointName);
            if (spawnPoint != null)
                playerTransform.position = spawnPoint.transform.position;

            if (SceneSpawnData.TryConsumeReturnFacing(out facingRight))
                hasFacing = true;
            else if (PlayerFacingUtility.TryGetFacingFromSpawnPoint(spawnPoint, out facingRight))
                hasFacing = true;

            SceneSpawnData.returnSpawnPointName = null;
        }

        if (!hasFacing && PlayerFacingUtility.TryGetSceneDefaultFacing(out facingRight))
            hasFacing = true;

        if (hasFacing)
            PlayerFacingUtility.TryApplyFacing(playerTransform, facingRight);
    }
}
