using UnityEngine;
using UnityEngine.UI;

public class LanguageToggleButton : MonoBehaviour
{
    [SerializeField] private Button targetButton;
    [SerializeField] private Image targetImage;
    [SerializeField] private Sprite englishSprite;
    [SerializeField] private Sprite chineseSprite;

    private void Awake()
    {
        if (targetButton == null)
            targetButton = GetComponent<Button>();

        if (targetImage == null)
            targetImage = GetComponent<Image>();
    }

    private void OnEnable()
    {
        if (targetButton != null)
            targetButton.onClick.AddListener(HandleClick);

        LanguageSettingsManager.LanguageChanged += HandleLanguageChanged;
        RefreshVisual(LanguageSettingsManager.CurrentLanguage);
    }

    private void OnDisable()
    {
        if (targetButton != null)
            targetButton.onClick.RemoveListener(HandleClick);

        LanguageSettingsManager.LanguageChanged -= HandleLanguageChanged;
    }

    private void HandleClick()
    {
        LanguageSettingsManager.ToggleLanguage();
    }

    private void HandleLanguageChanged(SupportedLanguage language)
    {
        RefreshVisual(language);
    }

    private void RefreshVisual(SupportedLanguage language)
    {
        if (targetImage == null)
            return;

        Sprite nextSprite = language == SupportedLanguage.Chinese ? chineseSprite : englishSprite;
        if (nextSprite != null)
            targetImage.sprite = nextSprite;
    }
}
