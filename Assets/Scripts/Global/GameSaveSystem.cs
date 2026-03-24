using UnityEngine;

public static class GameSaveSystem
{
    [System.Serializable]
    private class SaveData
    {
        public string sceneName = "RQScene";
        public string returnSpawnPointName;

        public bool crabCompleted;
        public bool dolphinQuestAccepted;
        public bool dolphinCompleted;
        public bool sharkCompleted;
        public bool octopusQuestAccepted;
        public bool octopusTaskReadyToTurnIn;
        public bool octopusDyingNpcHintDialogueCompleted;
        public bool octopusCompleted;
        public bool sharkEscortToSeaCucumberRequested;
        public bool seaCucumberQuestAccepted;
        public bool seaCucumberTriageCompleted;
        public bool seaCucumberInternalTreatmentCompleted;
        public bool seaCucumberExternalTreatmentCompleted;
        public bool seaCucumberCompleted;

        public bool whaleIntroDialogueCompleted;
        public bool whalePostCrabDialogueCompleted;
        public bool whalePostDolphinDialogueCompleted;
        public bool whalePostSharkDialogueCompleted;
        public bool whalePostOctopusDialogueCompleted;
        public bool whalePostSeaCucumberDialogueCompleted;
        public bool whaleEnsembleAccepted;

        public bool crabAmbassadorIntroDialogueCompleted;
        public bool crabAmbassadorPostDolphinDialogueCompleted;
        public bool crabAmbassadorPostSharkDialogueCompleted;
        public bool crabAmbassadorPostOctopusDialogueCompleted;
        public bool crabAmbassadorPostSeaCucumberDialogueCompleted;

        public int crabResidentAmbassadorDialogueFlags;
        public int dolphinAmbassadorDialogueFlags;
        public int sharkAmbassadorDialogueFlags;
        public int octopusAmbassadorDialogueFlags;
        public int seaCucumberAmbassadorDialogueFlags;

        public bool rqIntroDialogueCompleted;
    }

    private const string SaveKey = "Game.SaveData";

    public static bool HasSave()
    {
        return PlayerPrefs.HasKey(SaveKey);
    }

    public static void ClearSave()
    {
        PlayerPrefs.DeleteKey(SaveKey);
        PlayerPrefs.Save();
    }

    public static void StartNewGame(string startSceneName)
    {
        ClearSave();
        GameProgress.ResetAll();
        ClearRuntimeSceneState();
        SceneTransition.LoadScene(startSceneName);
    }

    public static void SaveCheckpoint(string sceneName, string returnSpawnPointName)
    {
        SaveData data = CaptureCurrentProgress();
        data.sceneName = string.IsNullOrWhiteSpace(sceneName) ? "RQScene" : sceneName;
        data.returnSpawnPointName = returnSpawnPointName;

        PlayerPrefs.SetString(SaveKey, JsonUtility.ToJson(data));
        PlayerPrefs.Save();
    }

    public static bool LoadSaveAndEnterScene()
    {
        if (!HasSave())
            return false;

        string json = PlayerPrefs.GetString(SaveKey, string.Empty);
        if (string.IsNullOrWhiteSpace(json))
            return false;

        SaveData data = JsonUtility.FromJson<SaveData>(json);
        if (data == null)
            return false;

        ApplySaveData(data);
        SceneTransition.LoadScene(string.IsNullOrWhiteSpace(data.sceneName) ? "RQScene" : data.sceneName);
        return true;
    }

    private static SaveData CaptureCurrentProgress()
    {
        return new SaveData
        {
            crabCompleted = GameProgress.crabCompleted,
            dolphinQuestAccepted = GameProgress.dolphinQuestAccepted,
            dolphinCompleted = GameProgress.dolphinCompleted,
            sharkCompleted = GameProgress.sharkCompleted,
            octopusQuestAccepted = GameProgress.octopusQuestAccepted,
            octopusTaskReadyToTurnIn = GameProgress.octopusTaskReadyToTurnIn,
            octopusDyingNpcHintDialogueCompleted = GameProgress.octopusDyingNpcHintDialogueCompleted,
            octopusCompleted = GameProgress.octopusCompleted,
            sharkEscortToSeaCucumberRequested = GameProgress.sharkEscortToSeaCucumberRequested,
            seaCucumberQuestAccepted = GameProgress.seaCucumberQuestAccepted,
            seaCucumberTriageCompleted = GameProgress.seaCucumberTriageCompleted,
            seaCucumberInternalTreatmentCompleted = GameProgress.seaCucumberInternalTreatmentCompleted,
            seaCucumberExternalTreatmentCompleted = GameProgress.seaCucumberExternalTreatmentCompleted,
            seaCucumberCompleted = GameProgress.seaCucumberCompleted,

            whaleIntroDialogueCompleted = GameProgress.whaleIntroDialogueCompleted,
            whalePostCrabDialogueCompleted = GameProgress.whalePostCrabDialogueCompleted,
            whalePostDolphinDialogueCompleted = GameProgress.whalePostDolphinDialogueCompleted,
            whalePostSharkDialogueCompleted = GameProgress.whalePostSharkDialogueCompleted,
            whalePostOctopusDialogueCompleted = GameProgress.whalePostOctopusDialogueCompleted,
            whalePostSeaCucumberDialogueCompleted = GameProgress.whalePostSeaCucumberDialogueCompleted,
            whaleEnsembleAccepted = GameProgress.whaleEnsembleAccepted,

            crabAmbassadorIntroDialogueCompleted = GameProgress.crabAmbassadorIntroDialogueCompleted,
            crabAmbassadorPostDolphinDialogueCompleted = GameProgress.crabAmbassadorPostDolphinDialogueCompleted,
            crabAmbassadorPostSharkDialogueCompleted = GameProgress.crabAmbassadorPostSharkDialogueCompleted,
            crabAmbassadorPostOctopusDialogueCompleted = GameProgress.crabAmbassadorPostOctopusDialogueCompleted,
            crabAmbassadorPostSeaCucumberDialogueCompleted = GameProgress.crabAmbassadorPostSeaCucumberDialogueCompleted,

            crabResidentAmbassadorDialogueFlags = GameProgress.crabResidentAmbassadorDialogueFlags,
            dolphinAmbassadorDialogueFlags = GameProgress.dolphinAmbassadorDialogueFlags,
            sharkAmbassadorDialogueFlags = GameProgress.sharkAmbassadorDialogueFlags,
            octopusAmbassadorDialogueFlags = GameProgress.octopusAmbassadorDialogueFlags,
            seaCucumberAmbassadorDialogueFlags = GameProgress.seaCucumberAmbassadorDialogueFlags,

            rqIntroDialogueCompleted = GameProgress.rqIntroDialogueCompleted
        };
    }

    private static void ApplySaveData(SaveData data)
    {
        GameProgress.ResetAll();

        GameProgress.crabCompleted = data.crabCompleted;
        GameProgress.dolphinQuestAccepted = data.dolphinQuestAccepted;
        GameProgress.dolphinCompleted = data.dolphinCompleted;
        GameProgress.sharkCompleted = data.sharkCompleted;
        GameProgress.octopusQuestAccepted = data.octopusQuestAccepted;
        GameProgress.octopusTaskReadyToTurnIn = data.octopusTaskReadyToTurnIn;
        GameProgress.octopusDyingNpcHintDialogueCompleted = data.octopusDyingNpcHintDialogueCompleted;
        GameProgress.octopusCompleted = data.octopusCompleted;
        GameProgress.sharkEscortToSeaCucumberRequested = data.sharkEscortToSeaCucumberRequested;
        GameProgress.seaCucumberQuestAccepted = data.seaCucumberQuestAccepted;
        GameProgress.seaCucumberTriageCompleted = data.seaCucumberTriageCompleted;
        GameProgress.seaCucumberInternalTreatmentCompleted = data.seaCucumberInternalTreatmentCompleted;
        GameProgress.seaCucumberExternalTreatmentCompleted = data.seaCucumberExternalTreatmentCompleted;
        GameProgress.seaCucumberCompleted = data.seaCucumberCompleted;

        GameProgress.whaleIntroDialogueCompleted = data.whaleIntroDialogueCompleted;
        GameProgress.whalePostCrabDialogueCompleted = data.whalePostCrabDialogueCompleted;
        GameProgress.whalePostDolphinDialogueCompleted = data.whalePostDolphinDialogueCompleted;
        GameProgress.whalePostSharkDialogueCompleted = data.whalePostSharkDialogueCompleted;
        GameProgress.whalePostOctopusDialogueCompleted = data.whalePostOctopusDialogueCompleted;
        GameProgress.whalePostSeaCucumberDialogueCompleted = data.whalePostSeaCucumberDialogueCompleted;
        GameProgress.whaleEnsembleAccepted = data.whaleEnsembleAccepted;

        GameProgress.crabAmbassadorIntroDialogueCompleted = data.crabAmbassadorIntroDialogueCompleted;
        GameProgress.crabAmbassadorPostDolphinDialogueCompleted = data.crabAmbassadorPostDolphinDialogueCompleted;
        GameProgress.crabAmbassadorPostSharkDialogueCompleted = data.crabAmbassadorPostSharkDialogueCompleted;
        GameProgress.crabAmbassadorPostOctopusDialogueCompleted = data.crabAmbassadorPostOctopusDialogueCompleted;
        GameProgress.crabAmbassadorPostSeaCucumberDialogueCompleted = data.crabAmbassadorPostSeaCucumberDialogueCompleted;

        GameProgress.crabResidentAmbassadorDialogueFlags = data.crabResidentAmbassadorDialogueFlags;
        GameProgress.dolphinAmbassadorDialogueFlags = data.dolphinAmbassadorDialogueFlags;
        GameProgress.sharkAmbassadorDialogueFlags = data.sharkAmbassadorDialogueFlags;
        GameProgress.octopusAmbassadorDialogueFlags = data.octopusAmbassadorDialogueFlags;
        GameProgress.seaCucumberAmbassadorDialogueFlags = data.seaCucumberAmbassadorDialogueFlags;

        GameProgress.rqIntroDialogueCompleted = data.rqIntroDialogueCompleted;

        ClearRuntimeSceneState();
        SceneSpawnData.returnSpawnPointName = data.returnSpawnPointName;
    }

    private static void ClearRuntimeSceneState()
    {
        SceneSpawnData.returnSpawnPointName = null;
        SceneSpawnData.hasPendingReturnFacing = false;
        SceneSpawnData.returnFacingRight = true;
        SceneSpawnData.ClearCheckpoint();
    }
}
