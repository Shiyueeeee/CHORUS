using UnityEngine;

public static class SceneSpawnData
{
    public static string returnSpawnPointName;
    public static bool hasPendingReturnFacing;
    public static bool returnFacingRight = true;

    public static string checkpointSceneName;
    public static Vector3 checkpointPosition;
    public static int checkpointOrder = -1;
    public static bool hasCheckpointFacing;
    public static bool checkpointFacingRight = true;

    public static void SetReturnSpawn(string spawnPointName, bool facingRight, bool overrideFacing = false)
    {
        returnSpawnPointName = spawnPointName;
        hasPendingReturnFacing = overrideFacing;
        returnFacingRight = facingRight;
    }

    public static bool TryConsumeReturnFacing(out bool facingRight)
    {
        if (!hasPendingReturnFacing)
        {
            facingRight = true;
            return false;
        }

        facingRight = returnFacingRight;
        hasPendingReturnFacing = false;
        return true;
    }

    public static bool HasCheckpointForScene(string sceneName)
    {
        return !string.IsNullOrEmpty(checkpointSceneName)
            && checkpointSceneName == sceneName
            && checkpointOrder >= 0;
    }

    public static void SetCheckpoint(string sceneName, Vector3 position, int order, bool facingRight)
    {
        if (string.IsNullOrEmpty(sceneName))
            return;

        if (HasCheckpointForScene(sceneName) && order < checkpointOrder)
            return;

        checkpointSceneName = sceneName;
        checkpointPosition = position;
        checkpointOrder = order;
        checkpointFacingRight = facingRight;
        hasCheckpointFacing = true;
    }

    public static bool TryGetCheckpointFacing(string sceneName, out bool facingRight)
    {
        if (!HasCheckpointForScene(sceneName) || !hasCheckpointFacing)
        {
            facingRight = true;
            return false;
        }

        facingRight = checkpointFacingRight;
        return true;
    }

    public static void ClearCheckpoint(string sceneName = null)
    {
        if (!string.IsNullOrEmpty(sceneName) && checkpointSceneName != sceneName)
            return;

        checkpointSceneName = null;
        checkpointPosition = Vector3.zero;
        checkpointOrder = -1;
        checkpointFacingRight = true;
        hasCheckpointFacing = false;
    }
}
