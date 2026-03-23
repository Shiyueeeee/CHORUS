using UnityEngine;

[CreateAssetMenu(fileName = "SceneTransitionSettings", menuName = "CHORUS/Scene Transition Settings")]
public class SceneTransitionSettings : ScriptableObject
{
    [Header("Fade In")]
    [Min(0f)]
    [SerializeField] private float defaultFadeDuration = 1.35f;
    [SerializeField] private Color fadeColor = Color.black;
    [SerializeField] private int sortingOrder = short.MaxValue;

    [Header("Skip Auto Fade")]
    [SerializeField] private string[] skipFadeInScenes = { "EndingScene" };

    public float DefaultFadeDuration => defaultFadeDuration;
    public Color FadeColor => fadeColor;
    public int SortingOrder => sortingOrder;

    public bool ShouldSkipFadeIn(string sceneName)
    {
        if (string.IsNullOrWhiteSpace(sceneName) || skipFadeInScenes == null)
            return false;

        for (int i = 0; i < skipFadeInScenes.Length; i++)
        {
            if (sceneName == skipFadeInScenes[i])
                return true;
        }

        return false;
    }
}
