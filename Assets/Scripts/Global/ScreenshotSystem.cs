using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class ScreenshotSystem : MonoBehaviour
{
    public static ScreenshotSystem Instance { get; private set; }
    public static event Action<string> ScreenshotSaved;

    [Header("Capture")]
    [SerializeField] private Key captureKey = Key.F12;
    [SerializeField] private int superSize = 1;

    [Header("Feedback")]
    [SerializeField] private AudioSource feedbackAudioSource;
    [SerializeField] private AudioClip captureSound;
    [SerializeField] [Range(0f, 1f)] private float captureSoundVolume = 1f;
    [SerializeField] private bool copyToClipboardAfterCapture = true;

    [Header("Scene Filter")]
    [SerializeField] private bool exceptInTargetScene = true;
    [SerializeField] private string[] targetScenes = { "RQScene", "TitleScene" };

    private bool isCapturing;

    public static string ScreenshotDirectory
    {
        get
        {
            string path = Path.Combine(Application.persistentDataPath, "Screenshots");
            if (!Directory.Exists(path)) Directory.CreateDirectory(path);
            return path;
        }
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
        EnsureFeedbackAudioSource();
    }

    private void Update()
    {
        if (isCapturing) return;
        if (exceptInTargetScene && targetScenes.Contains(SceneManager.GetActiveScene().name)) return;
        if (GlobalUIRoot.Instance != null && GlobalUIRoot.Instance.IsSettingsOpen) return;

        Keyboard kb = Keyboard.current;
        if (kb == null) return;

        var keyControl = kb[captureKey];
        if (keyControl == null || !keyControl.wasPressedThisFrame) return;

        StartCoroutine(CaptureRoutine());
    }

    public static List<string> GetScreenshotPathsNewestFirst()
    {
        if (!Directory.Exists(ScreenshotDirectory)) return new List<string>();

        return Directory
            .GetFiles(ScreenshotDirectory)
            .Where(IsImageFile)
            .OrderByDescending(File.GetCreationTimeUtc)
            .ToList();
    }

    private static bool IsImageFile(string path)
    {
        string ext = Path.GetExtension(path).ToLowerInvariant();
        return ext == ".png" || ext == ".jpg" || ext == ".jpeg";
    }

    private IEnumerator CaptureRoutine()
    {
        isCapturing = true;
        PlayCaptureFeedback();

        yield return new WaitForEndOfFrame();

        string fileName = $"shot_{DateTime.Now:yyyyMMdd_HHmmss_fff}.png";
        string fullPath = Path.Combine(ScreenshotDirectory, fileName);

        ScreenCapture.CaptureScreenshot(fullPath, superSize);

        float timeout = 3f;
        while (!File.Exists(fullPath) && timeout > 0f)
        {
            timeout -= Time.unscaledDeltaTime;
            yield return null;
        }

        if (File.Exists(fullPath))
        {
            TryCopyScreenshotToClipboard(fullPath);
            ScreenshotSaved?.Invoke(fullPath);
        }

        isCapturing = false;
    }

    private void EnsureFeedbackAudioSource()
    {
        if (feedbackAudioSource != null) return;

        feedbackAudioSource = GetComponent<AudioSource>();
        if (feedbackAudioSource == null)
            feedbackAudioSource = gameObject.AddComponent<AudioSource>();

        feedbackAudioSource.playOnAwake = false;
        feedbackAudioSource.loop = false;
        feedbackAudioSource.spatialBlend = 0f;
        AudioSettingsManager.ApplySfxVolume(feedbackAudioSource);
    }

    private void PlayCaptureFeedback()
    {
        if (captureSound == null) return;

        EnsureFeedbackAudioSource();
        if (feedbackAudioSource == null) return;

        AudioSettingsManager.ApplySfxVolume(feedbackAudioSource);
        feedbackAudioSource.PlayOneShot(captureSound, captureSoundVolume);
    }

    private void TryCopyScreenshotToClipboard(string fullPath)
    {
        if (!copyToClipboardAfterCapture || !ClipboardImageUtility.IsCopySupported) return;

        if (!ClipboardImageUtility.TryCopyImageFileToClipboard(fullPath))
            Debug.LogWarning($"ScreenshotSystem: Failed to copy screenshot to clipboard. Path: {fullPath}");
    }
}
