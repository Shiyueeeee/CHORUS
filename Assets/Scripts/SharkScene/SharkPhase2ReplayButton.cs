using UnityEngine;
using UnityEngine.SceneManagement;

public class SharkPhase2ReplayButton : MonoBehaviour
{
    public void ReplayPhase2()
    {
        Time.timeScale = 1f;
        SharkEncounterManager.StartFromAttackPhaseOnLoad = true;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
