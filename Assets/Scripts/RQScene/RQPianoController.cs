using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

[RequireComponent(typeof(BoxCollider2D))]
[RequireComponent(typeof(AudioSource))]
[RequireComponent(typeof(SpriteRenderer))]
public class RQPianoController : MonoBehaviour
{
    private const int NoteCount = 36;

    [Header("World")]
    [SerializeField] private SpriteRenderer worldRenderer;
    [SerializeField] private AudioSource noteAudioSource;
    [SerializeField] private GameObject promptObject;
    [SerializeField] private Sprite pianoWorldSprite;
    [SerializeField] private Color placeholderColor = new Color(0.18f, 0.18f, 0.18f, 1f);

    [Header("UI")]
    [SerializeField] private GameObject panelRoot;
    [SerializeField] private Button closeButton;
    [SerializeField] private RQPianoNoteButton[] noteButtons;

    [Header("Audio")]
    [SerializeField] private AudioClip[] noteClips = new AudioClip[NoteCount];
    [SerializeField] [Range(0f, 1f)] private float noteVolume = 1f;

    private bool playerInRange;
    private bool isPanelOpen;
    private readonly List<Button> registeredButtons = new List<Button>();

    private void Reset()
    {
        worldRenderer = GetComponent<SpriteRenderer>();
        noteAudioSource = GetComponent<AudioSource>();

        BoxCollider2D trigger = GetComponent<BoxCollider2D>();
        trigger.isTrigger = true;
        trigger.size = new Vector2(4f, 2.4f);
        trigger.offset = new Vector2(0f, 0.6f);

        if (worldRenderer != null)
        {
            worldRenderer.color = placeholderColor;
            worldRenderer.sortingOrder = 1;
        }

        if (noteClips == null || noteClips.Length != NoteCount)
            noteClips = new AudioClip[NoteCount];
    }

    private void Awake()
    {
        EnsureReferences();
        ApplyWorldVisual();
        BindUi();
        SetPromptVisible(false);

        if (panelRoot != null)
            panelRoot.SetActive(false);
    }

    private void OnDestroy()
    {
        UnbindUi();

        if (isPanelOpen && GlobalUIRoot.Instance != null)
            GlobalUIRoot.Instance.SetExternalModalInputLock(false);
    }

    private void Update()
    {
        if (isPanelOpen)
        {
            if (Keyboard.current != null && (Keyboard.current.escapeKey.wasPressedThisFrame || Keyboard.current.eKey.wasPressedThisFrame))
                ClosePanel();

            return;
        }

        if (!playerInRange)
            return;

        if (DialogueManager.IsDialogueActive || GlobalUIRoot.IsModalInputLocked)
            return;

        if (Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame)
            OpenPanel();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player"))
            return;

        playerInRange = true;
        SetPromptVisible(true);
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player"))
            return;

        playerInRange = false;
        SetPromptVisible(false);
    }

    private void EnsureReferences()
    {
        if (worldRenderer == null)
            worldRenderer = GetComponent<SpriteRenderer>();

        if (noteAudioSource == null)
            noteAudioSource = GetComponent<AudioSource>();

        if (noteAudioSource != null)
        {
            noteAudioSource.playOnAwake = false;
            noteAudioSource.loop = false;
            noteAudioSource.spatialBlend = 0f;
        }

        if (noteClips == null || noteClips.Length != NoteCount)
            noteClips = new AudioClip[NoteCount];

        if (panelRoot == null)
        {
            Transform panelTransform = transform.root.Find("Canvas/PianoPanel");
            if (panelTransform != null)
                panelRoot = panelTransform.gameObject;
        }

        if (closeButton == null && panelRoot != null)
            closeButton = panelRoot.GetComponentInChildren<Button>(true);

        if ((noteButtons == null || noteButtons.Length == 0) && panelRoot != null)
            noteButtons = panelRoot.GetComponentsInChildren<RQPianoNoteButton>(true);
    }

    private void ApplyWorldVisual()
    {
        if (worldRenderer == null)
            return;

        if (pianoWorldSprite != null)
        {
            worldRenderer.sprite = pianoWorldSprite;
            worldRenderer.color = Color.white;
            return;
        }

        if (worldRenderer.sprite == null)
            Debug.LogWarning("RQPianoController is using a placeholder square. Assign a pianoWorldSprite when ready.", this);

        worldRenderer.color = placeholderColor;
    }

    private void BindUi()
    {
        UnbindUi();

        if (closeButton != null)
        {
            closeButton.onClick.AddListener(ClosePanel);
            registeredButtons.Add(closeButton);
        }

        if (noteButtons == null)
            return;

        foreach (RQPianoNoteButton noteButton in noteButtons)
        {
            if (noteButton == null)
                continue;

            Button button = noteButton.GetComponent<Button>();
            if (button == null)
                continue;

            int capturedNoteIndex = noteButton.NoteIndex;
            RQPianoNoteButton capturedNoteButton = noteButton;
            button.onClick.AddListener(() => HandleNoteButtonPressed(capturedNoteButton, capturedNoteIndex));
            registeredButtons.Add(button);
        }
    }

    private void UnbindUi()
    {
        foreach (Button button in registeredButtons)
        {
            if (button == null)
                continue;

            button.onClick.RemoveAllListeners();
        }

        registeredButtons.Clear();
    }

    private void OpenPanel()
    {
        if (panelRoot == null)
        {
            Debug.LogWarning("RQPianoController is missing panelRoot. Create a PianoPanel in the Canvas hierarchy and assign it in the inspector.", this);
            return;
        }

        if (isPanelOpen)
            return;

        isPanelOpen = true;
        panelRoot.SetActive(true);
        SetPromptVisible(false);

        if (GlobalUIRoot.Instance != null)
            GlobalUIRoot.Instance.SetExternalModalInputLock(true);

        EventSystem eventSystem = FindFirstObjectByType<EventSystem>();
        if (eventSystem != null)
            eventSystem.SetSelectedGameObject(null);
    }

    public void ClosePanel()
    {
        if (panelRoot == null || !isPanelOpen)
            return;

        isPanelOpen = false;
        panelRoot.SetActive(false);

        if (GlobalUIRoot.Instance != null)
            GlobalUIRoot.Instance.SetExternalModalInputLock(false);

        if (playerInRange)
            SetPromptVisible(true);
    }

    private void PlayNote(int noteIndex)
    {
        if (noteIndex < 0 || noteIndex >= NoteCount)
            return;

        AudioClip clip = noteClips[noteIndex];
        if (clip == null || noteAudioSource == null)
            return;

        AudioSettingsManager.ApplySfxVolume(noteAudioSource, noteVolume);
        noteAudioSource.PlayOneShot(clip, 1f);
    }

    private void HandleNoteButtonPressed(RQPianoNoteButton noteButton, int noteIndex)
    {
        if (noteButton != null)
            noteButton.PlayPressEffect();

        PlayNote(noteIndex);
    }

    private void SetPromptVisible(bool visible)
    {
        if (promptObject != null)
            promptObject.SetActive(visible);
    }
}
