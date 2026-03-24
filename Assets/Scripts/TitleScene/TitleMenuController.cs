using UnityEngine;
using UnityEngine.SceneManagement;

public class TitleMenuController : MonoBehaviour
{
    [Header("Scene")]
    [SerializeField] private string gameSceneName = "RQScene"; 

    public void OnClickStartGame()
    {
        GameSaveSystem.StartNewGame(gameSceneName);
    }

    public void OnClickStartFromSave()
    {
        if (!GameSaveSystem.LoadSaveAndEnterScene())
            GameSaveSystem.StartNewGame(gameSceneName);
    }

}
