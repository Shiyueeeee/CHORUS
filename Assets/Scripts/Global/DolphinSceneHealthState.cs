using UnityEngine;
using UnityEngine.SceneManagement;

public static class DolphinSceneHealthState
{
    private static bool initialized;
    private static bool hasStoredPlayerHealth;
    private static int storedPlayerHealth;
    private static int storedPlayerMaxHealth;
    private static bool hasStoredDolphinHealth;
    private static int storedDolphinHealth;
    private static int storedDolphinMaxHealth;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Initialize()
    {
        if (initialized)
            return;

        SceneManager.sceneLoaded += HandleSceneLoaded;
        initialized = true;
    }

    public static bool IsDolphinScene(string sceneName)
    {
        switch (sceneName)
        {
            case "DolphinRushScene":
            case "DolphinAreaScene":
            case "ShipScene":
            case "DolphinFinalRushScene":
            case "DolphinFinalAreaScene":
                return true;
            default:
                return false;
        }
    }

    public static int GetStartingPlayerHealth(string sceneName, int maxHP)
    {
        if (!IsDolphinScene(sceneName))
            return maxHP;

        if (!hasStoredPlayerHealth)
            return maxHP;

        return Mathf.Clamp(storedPlayerHealth, 0, maxHP);
    }

    public static int GetStartingPlayerMaxHealth(string sceneName, int maxHP)
    {
        if (!IsDolphinScene(sceneName))
            return maxHP;

        if (!hasStoredPlayerHealth)
            return maxHP;

        return Mathf.Max(1, storedPlayerMaxHealth);
    }

    public static void StorePlayerHealth(string sceneName, int currentHP, int maxHP)
    {
        if (!IsDolphinScene(sceneName))
            return;

        hasStoredPlayerHealth = true;
        storedPlayerHealth = Mathf.Max(0, currentHP);
        storedPlayerMaxHealth = Mathf.Max(1, maxHP);
    }

    public static int GetStartingDolphinHealth(string sceneName, int maxHP)
    {
        if (!IsDolphinScene(sceneName))
            return maxHP;

        if (!hasStoredDolphinHealth)
            return maxHP;

        return Mathf.Clamp(storedDolphinHealth, 0, maxHP);
    }

    public static int GetStartingDolphinMaxHealth(string sceneName, int maxHP)
    {
        if (!IsDolphinScene(sceneName))
            return maxHP;

        if (!hasStoredDolphinHealth)
            return maxHP;

        return Mathf.Max(1, storedDolphinMaxHealth);
    }

    public static void StoreDolphinHealth(string sceneName, int currentHP, int maxHP)
    {
        if (!IsDolphinScene(sceneName))
            return;

        hasStoredDolphinHealth = true;
        storedDolphinHealth = Mathf.Max(0, currentHP);
        storedDolphinMaxHealth = Mathf.Max(1, maxHP);
    }

    public static void ResetAll()
    {
        hasStoredPlayerHealth = false;
        storedPlayerHealth = 0;
        storedPlayerMaxHealth = 0;
        hasStoredDolphinHealth = false;
        storedDolphinHealth = 0;
        storedDolphinMaxHealth = 0;
    }

    private static void HandleSceneLoaded(Scene scene, LoadSceneMode _)
    {
        if (!IsDolphinScene(scene.name))
            ResetAll();
    }
}
