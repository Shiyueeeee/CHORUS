using System.Collections;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class RQPianoNoteButton : MonoBehaviour
{
    [Header("Note")]
    [SerializeField] [Range(0, 35)] private int noteIndex;

    [Header("Glow Trail")]
    [SerializeField] private RectTransform effectSpawnRoot;
    [SerializeField] private RectTransform effectContainer;
    [SerializeField] private Sprite effectSprite;
    [SerializeField] private Color effectColor = new Color(0.55f, 0.95f, 1f, 0.95f);
    [SerializeField] [Min(1)] private int streakCount = 3;
    [SerializeField] private Vector2 streakWidthRange = new Vector2(18f, 28f);
    [SerializeField] private Vector2 streakHeightRange = new Vector2(90f, 180f);
    [SerializeField] private Vector2 horizontalJitter = new Vector2(-10f, 10f);
    [SerializeField] private Vector2 verticalTravelRange = new Vector2(90f, 160f);
    [SerializeField] private Vector2 lifetimeRange = new Vector2(0.35f, 0.65f);
    [SerializeField] private Vector2 spawnDelayRange = new Vector2(0f, 0.06f);
    [SerializeField] private AnimationCurve alphaCurve = AnimationCurve.EaseInOut(0f, 1f, 1f, 0f);
    [SerializeField] private AnimationCurve widthCurve = AnimationCurve.EaseInOut(0f, 0.8f, 1f, 0.25f);

    public int NoteIndex => noteIndex;

    public void PlayPressEffect()
    {
        RectTransform spawnRoot = effectSpawnRoot != null ? effectSpawnRoot : transform as RectTransform;
        RectTransform container = effectContainer != null ? effectContainer : spawnRoot;

        if (spawnRoot == null || container == null)
            return;

        for (int i = 0; i < Mathf.Max(1, streakCount); i++)
            StartCoroutine(SpawnTrailRoutine(spawnRoot, container));
    }

    private IEnumerator SpawnTrailRoutine(RectTransform spawnRoot, RectTransform container)
    {
        float delay = Random.Range(
            Mathf.Min(spawnDelayRange.x, spawnDelayRange.y),
            Mathf.Max(spawnDelayRange.x, spawnDelayRange.y));

        if (delay > 0f)
            yield return new WaitForSecondsRealtime(delay);

        GameObject trailObject = new GameObject("PianoGlowTrail", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        RectTransform trailRect = trailObject.GetComponent<RectTransform>();
        trailRect.SetParent(container, false);
        trailRect.anchorMin = new Vector2(0.5f, 0f);
        trailRect.anchorMax = new Vector2(0.5f, 0f);
        trailRect.pivot = new Vector2(0.5f, 0f);

        Vector3 worldSpawnPosition = spawnRoot.TransformPoint(new Vector3(Random.Range(horizontalJitter.x, horizontalJitter.y), 0f, 0f));
        trailRect.position = worldSpawnPosition;
        trailRect.SetAsLastSibling();

        Image image = trailObject.GetComponent<Image>();
        image.sprite = effectSprite;
        image.color = effectColor;
        image.raycastTarget = false;
        image.type = Image.Type.Sliced;
        image.maskable = false;

        float startWidth = Random.Range(Mathf.Min(streakWidthRange.x, streakWidthRange.y), Mathf.Max(streakWidthRange.x, streakWidthRange.y));
        float startHeight = Random.Range(Mathf.Min(streakHeightRange.x, streakHeightRange.y), Mathf.Max(streakHeightRange.x, streakHeightRange.y));
        float travel = Random.Range(Mathf.Min(verticalTravelRange.x, verticalTravelRange.y), Mathf.Max(verticalTravelRange.x, verticalTravelRange.y));
        float lifetime = Random.Range(Mathf.Min(lifetimeRange.x, lifetimeRange.y), Mathf.Max(lifetimeRange.x, lifetimeRange.y));

        Color baseColor = effectColor;
        Vector2 startSize = new Vector2(startWidth, startHeight);
        Vector2 endSize = new Vector2(startWidth * Mathf.Max(0.1f, widthCurve.Evaluate(1f)), startHeight * 1.1f);
        Vector3 startPosition = trailRect.position;
        Vector3 endPosition = startPosition + new Vector3(0f, travel, 0f);

        float elapsed = 0f;
        while (elapsed < lifetime)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / lifetime);

            trailRect.position = Vector3.LerpUnclamped(startPosition, endPosition, t);
            trailRect.sizeDelta = Vector2.LerpUnclamped(startSize, endSize, t);

            Color color = baseColor;
            color.a *= Mathf.Clamp01(alphaCurve.Evaluate(t));
            image.color = color;

            yield return null;
        }

        Destroy(trailObject);
    }
}
