using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(AudioSource))]
public class GlobalSceneMusicManager : MonoBehaviour
{
    private enum MusicCue
    {
        None,
        RQBase,
        RQCrab,
        RQDolphin,
        RQShark,
        RQOctopus,
        RQSeaCucumber,
        Crab,
        Dolphin,
        Shark,
        Octopus,
        SeaCucumber,
        Title
    }

    public static GlobalSceneMusicManager Instance { get; private set; }

    [Header("Playback")]
    [SerializeField] [Range(0f, 1f)] private float baseVolume = 1f;
    [SerializeField] private bool loop = true;
    [SerializeField] private bool playOnStart = true;

    [Header("Title")]
    [SerializeField] private AudioClip titleBgm;

    [Header("RQ Scene")]
    [SerializeField] private AudioClip rqBaseBgm;
    [SerializeField] private AudioClip rqCrabBgm;
    [SerializeField] private AudioClip rqDolphinBgm;
    [SerializeField] private AudioClip rqSharkBgm;
    [SerializeField] private AudioClip rqOctopusBgm;
    [SerializeField] private AudioClip rqSeaCucumberBgm;

    [Header("Area Themes")]
    [SerializeField] private AudioClip crabBgm;
    [SerializeField] private AudioClip dolphinBgm;
    [SerializeField] private AudioClip sharkBgm;
    [SerializeField] private AudioClip octopusBgm;
    [SerializeField] private AudioClip seaCucumberBgm;

    private AudioSource audioSource;
    private MusicCue currentCue = MusicCue.None;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        audioSource = GetComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.loop = loop;
        audioSource.spatialBlend = 0f;

        RefreshVolume();
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += HandleSceneLoaded;
        AudioSettingsManager.MusicVolumeChanged += HandleMusicVolumeChanged;
    }

    private void Start()
    {
        RefreshForScene(SceneManager.GetActiveScene(), playOnStart);
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= HandleSceneLoaded;
        AudioSettingsManager.MusicVolumeChanged -= HandleMusicVolumeChanged;
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    private void HandleSceneLoaded(Scene scene, LoadSceneMode _)
    {
        RefreshForScene(scene, playIfChanged: true);
    }

    private void HandleMusicVolumeChanged(float _)
    {
        RefreshVolume();
    }

    [ContextMenu("Refresh Current Scene Music")]
    public void RefreshCurrentSceneMusic()
    {
        RefreshForScene(SceneManager.GetActiveScene(), playIfChanged: true);
    }

    public void RefreshCurrentSceneMusic(bool forceRestart)
    {
        RefreshForScene(SceneManager.GetActiveScene(), playIfChanged: true, forceRestart: forceRestart);
    }

    private void RefreshForScene(Scene scene, bool playIfChanged, bool forceRestart = false)
    {
        RefreshVolume();

        MusicCue nextCue = ResolveCue(scene.name);
        AudioClip nextClip = ResolveClip(nextCue);

        bool cueChanged = nextCue != currentCue || audioSource.clip != nextClip;
        currentCue = nextCue;

        if (nextClip == null)
        {
            audioSource.Stop();
            audioSource.clip = null;
            return;
        }

        audioSource.loop = loop;

        if (!cueChanged && !forceRestart)
            return;

        audioSource.clip = nextClip;

        if (playIfChanged || forceRestart)
        {
            audioSource.Stop();
            audioSource.Play();
        }
    }

    private MusicCue ResolveCue(string sceneName)
    {
        switch (sceneName)
        {
            case "TitleScene":
                return MusicCue.Title;

            case "RQScene":
                return ResolveRqCueFromProgress();

            case "CrabAreaScene":
            case "CrabRushScene":
                return MusicCue.Crab;

            case "DolphinAreaScene":
            case "DolphinRushScene":
            case "DolphinFinalAreaScene":
            case "DolphinFinalRushScene":
                return MusicCue.Dolphin;

            case "SharkScene":
            case "ShipScene":
                return MusicCue.Shark;

            case "OctopusAreaScene":
            case "OctopusPathScene":
            case "OctopusQuestAreaScene":
                return MusicCue.Octopus;

            case "SeaCucumberAreaScene":
                return MusicCue.SeaCucumber;

            default:
                return MusicCue.None;
        }
    }

    private MusicCue ResolveRqCueFromProgress()
    {
        if (GameProgress.seaCucumberCompleted) return MusicCue.RQSeaCucumber;
        if (GameProgress.octopusCompleted) return MusicCue.RQOctopus;
        if (GameProgress.sharkCompleted) return MusicCue.RQShark;
        if (GameProgress.dolphinCompleted) return MusicCue.RQDolphin;
        if (GameProgress.crabCompleted) return MusicCue.RQCrab;
        return MusicCue.RQBase;
    }

    private AudioClip ResolveClip(MusicCue cue)
    {
        switch (cue)
        {
            case MusicCue.Title:
                return titleBgm;
            case MusicCue.RQBase:
                return rqBaseBgm;
            case MusicCue.RQCrab:
                return rqCrabBgm;
            case MusicCue.RQDolphin:
                return rqDolphinBgm;
            case MusicCue.RQShark:
                return rqSharkBgm;
            case MusicCue.RQOctopus:
                return rqOctopusBgm;
            case MusicCue.RQSeaCucumber:
                return rqSeaCucumberBgm;
            case MusicCue.Crab:
                return crabBgm;
            case MusicCue.Dolphin:
                return dolphinBgm;
            case MusicCue.Shark:
                return sharkBgm;
            case MusicCue.Octopus:
                return octopusBgm;
            case MusicCue.SeaCucumber:
                return seaCucumberBgm;
            default:
                return null;
        }
    }

    private void RefreshVolume()
    {
        AudioSettingsManager.ApplyMusicVolume(audioSource, baseVolume);
    }
}
