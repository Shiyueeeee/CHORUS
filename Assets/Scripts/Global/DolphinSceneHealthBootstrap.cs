using UnityEngine;
using UnityEngine.SceneManagement;

public static class DolphinSceneHealthBootstrap
{
    private static bool initialized;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Initialize()
    {
        if (initialized)
            return;

        SceneManager.sceneLoaded += HandleSceneLoaded;
        initialized = true;
    }

    private static void HandleSceneLoaded(Scene scene, LoadSceneMode _)
    {
        if (!DolphinSceneHealthState.IsDolphinScene(scene.name))
            return;

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null || player.GetComponent<PlayerHealth>() != null)
            return;

        player.AddComponent<PlayerHealth>();
    }
}
