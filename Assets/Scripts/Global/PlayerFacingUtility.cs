using Seahorse;
using UnityEngine;

public static class PlayerFacingUtility
{
    public static bool TryApplyFacing(Transform playerTransform, bool facingRight)
    {
        if (playerTransform == null)
            return false;

        SeahorseVisual visual = playerTransform.GetComponentInChildren<SeahorseVisual>();
        if (visual == null)
            return false;

        visual.SetFacingImmediate(facingRight);
        return true;
    }

    public static bool TryGetCurrentFacing(Transform playerTransform, out bool facingRight)
    {
        facingRight = true;
        if (playerTransform == null)
            return false;

        SeahorseVisual visual = playerTransform.GetComponentInChildren<SeahorseVisual>();
        if (visual == null)
            return false;

        facingRight = visual.IsFacingRight;
        return true;
    }

    public static bool TryGetFacingFromSpawnPoint(GameObject spawnPoint, out bool facingRight)
    {
        facingRight = true;
        if (spawnPoint == null)
            return false;

        SpawnPointFacing facing = spawnPoint.GetComponent<SpawnPointFacing>();
        return facing != null && facing.TryGetFacing(out facingRight);
    }

    public static bool TryGetSceneDefaultFacing(out bool facingRight)
    {
        facingRight = true;
        SceneDefaultFacing sceneDefaultFacing = Object.FindFirstObjectByType<SceneDefaultFacing>();
        if (sceneDefaultFacing == null)
            return false;

        facingRight = sceneDefaultFacing.FacingRight;
        return true;
    }

    public static bool TryResolveRespawnFacing(string sceneName, Transform fallbackPoint, out bool facingRight)
    {
        if (SceneSpawnData.TryGetCheckpointFacing(sceneName, out facingRight))
            return true;

        if (fallbackPoint != null && TryGetFacingFromSpawnPoint(fallbackPoint.gameObject, out facingRight))
            return true;

        return TryGetSceneDefaultFacing(out facingRight);
    }
}
