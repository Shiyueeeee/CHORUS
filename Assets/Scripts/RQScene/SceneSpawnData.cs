using UnityEngine;

public static class SceneSpawnData
{
    public static string returnSpawnPointName;

    public static string checkpointSceneName;
    public static Vector3 checkpointPosition;
    public static int checkpointOrder = -1;

    public static bool HasCheckpointForScene(string sceneName)
    {
        return !string.IsNullOrEmpty(checkpointSceneName)
            && checkpointSceneName == sceneName
            && checkpointOrder >= 0;
    }

    public static void SetCheckpoint(string sceneName, Vector3 position, int order)
    {
        if (string.IsNullOrEmpty(sceneName))
            return;

        if (HasCheckpointForScene(sceneName) && order < checkpointOrder)
            return;

        checkpointSceneName = sceneName;
        checkpointPosition = position;
        checkpointOrder = order;
    }

    public static void ClearCheckpoint(string sceneName = null)
    {
        if (!string.IsNullOrEmpty(sceneName) && checkpointSceneName != sceneName)
            return;

        checkpointSceneName = null;
        checkpointPosition = Vector3.zero;
        checkpointOrder = -1;
    }
}
