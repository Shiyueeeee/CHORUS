using System;
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
    [SerializeField] private bool loop = true;
    [SerializeField] private bool useUnscaledTime = true;

    private SlideData[] slides = Array.Empty<SlideData>();
    private int currentIndex = -1;
    private float timer;

    private void OnEnable()
    {
        timer = Mathf.Max(0.1f, secondsPerSlide);

        if (slides.Length > 0)
            ShowSlide(0);
    }

    private void Update()
    {
        if (slides.Length <= 1)
            return;

        float deltaTime = useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
        timer -= deltaTime;
        if (timer > 0f)
            return;

        timer = Mathf.Max(0.1f, secondsPerSlide);
        Advance();
    }

    public void Configure(Image imageTarget, TMP_Text titleTarget, TMP_Text subtitleTarget, float slideDuration)
    {
        targetImage = imageTarget;
        titleText = titleTarget;
        subtitleText = subtitleTarget;
        secondsPerSlide = slideDuration;
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

        ShowSlide(nextIndex);
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
}
