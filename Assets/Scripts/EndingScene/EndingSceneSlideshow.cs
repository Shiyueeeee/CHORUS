using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EndingSceneSlideshow : MonoBehaviour
{
    [Serializable]
    public class SlideData
    {
        public Sprite image;
        public string title;
        public string subtitle;
    }

    [SerializeField] private Image targetImage;
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private TMP_Text subtitleText;
    [SerializeField] private float secondsPerSlide = 3f;
    [SerializeField] private float transitionFadeDuration = 0.35f;
    [SerializeField] private bool loop = true;
    [SerializeField] private bool useUnscaledTime = true;

    private SlideData[] slides = Array.Empty<SlideData>();
    private int currentIndex = -1;
    private float timer;
    private Coroutine transitionRoutine;

    private void OnEnable()
    {
        timer = Mathf.Max(0.1f, secondsPerSlide);

        if (slides.Length > 0)
            ShowSlide(0);
    }

    private void Update()
    {
        if (transitionRoutine != null)
            return;

        if (slides.Length <= 1)
            return;

        float deltaTime = GetDeltaTime();
        timer -= deltaTime;
        if (timer > 0f)
            return;

        timer = Mathf.Max(0.1f, secondsPerSlide);
        Advance();
    }

    public void Configure(Image imageTarget, TMP_Text titleTarget, TMP_Text subtitleTarget, float slideDuration)
    {
        Configure(imageTarget, titleTarget, subtitleTarget, slideDuration, transitionFadeDuration);
    }

    public void Configure(Image imageTarget, TMP_Text titleTarget, TMP_Text subtitleTarget, float slideDuration, float fadeDuration)
    {
        targetImage = imageTarget;
        titleText = titleTarget;
        subtitleText = subtitleTarget;
        secondsPerSlide = slideDuration;
        transitionFadeDuration = Mathf.Max(0f, fadeDuration);
        timer = Mathf.Max(0.1f, secondsPerSlide);
    }

    public void SetSlides(SlideData[] slideSet)
    {
        slides = slideSet ?? Array.Empty<SlideData>();
        currentIndex = -1;
        timer = Mathf.Max(0.1f, secondsPerSlide);

        if (slides.Length > 0)
            ShowSlide(0);
    }

    private void Advance()
    {
        if (slides.Length == 0)
            return;

        int nextIndex = currentIndex + 1;
        if (nextIndex >= slides.Length)
        {
            if (!loop)
            {
                nextIndex = slides.Length - 1;
                enabled = false;
            }
            else
            {
                nextIndex = 0;
            }
        }

        if (transitionFadeDuration <= 0f || targetImage == null || !targetImage.enabled || targetImage.sprite == null)
        {
            ShowSlide(nextIndex);
            return;
        }

        transitionRoutine = StartCoroutine(TransitionToSlide(nextIndex));
    }

    private void ShowSlide(int index)
    {
        if (index < 0 || index >= slides.Length)
            return;

        currentIndex = index;
        SlideData slide = slides[index];

        if (targetImage != null)
        {
            targetImage.sprite = slide.image;
            targetImage.preserveAspect = true;
            targetImage.enabled = slide.image != null;
        }

        if (titleText != null)
            titleText.text = slide.title ?? string.Empty;

        if (subtitleText != null)
            subtitleText.text = slide.subtitle ?? string.Empty;
    }

    private IEnumerator TransitionToSlide(int index)
    {
        yield return FadeTargetImage(1f, 0f, transitionFadeDuration);
        ShowSlide(index);
        yield return FadeTargetImage(0f, 1f, transitionFadeDuration);
        transitionRoutine = null;
    }

    private IEnumerator FadeTargetImage(float from, float to, float duration)
    {
        if (targetImage == null)
            yield break;

        float elapsed = 0f;
        SetTargetImageAlpha(from);

        if (duration <= 0f)
        {
            SetTargetImageAlpha(to);
            yield break;
        }

        while (elapsed < duration)
        {
            elapsed += GetDeltaTime();
            float t = Mathf.Clamp01(elapsed / duration);
            float alpha = Mathf.Lerp(from, to, Mathf.SmoothStep(0f, 1f, t));
            SetTargetImageAlpha(alpha);
            yield return null;
        }

        SetTargetImageAlpha(to);
    }

    private float GetDeltaTime()
    {
        return useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
    }

    private void SetTargetImageAlpha(float alpha)
    {
        if (targetImage == null)
            return;

        Color color = targetImage.color;
        color.a = alpha;
        targetImage.color = color;
    }
}
