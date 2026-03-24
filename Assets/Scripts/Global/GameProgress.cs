public static class GameProgress
{
    public static bool crabCompleted = false;
    public static bool dolphinQuestAccepted = false;
    public static bool dolphinCompleted = false;
    public static bool sharkCompleted = false;
    public static bool octopusQuestAccepted = false;
    public static bool octopusTaskReadyToTurnIn = false;
    public static bool octopusDyingNpcHintDialogueCompleted = false;
    public static bool octopusCompleted = false;
    public static bool sharkEscortToSeaCucumberRequested = false;
    public static bool seaCucumberQuestAccepted = false;
    public static bool seaCucumberTriageCompleted = false;
    public static bool seaCucumberInternalTreatmentCompleted = false;
    public static bool seaCucumberExternalTreatmentCompleted = false;
    public static bool seaCucumberCompleted = false;

    public static bool whaleIntroDialogueCompleted = false;
    public static bool whalePostCrabDialogueCompleted = false;
    public static bool whalePostDolphinDialogueCompleted = false;
    public static bool whalePostSharkDialogueCompleted = false;
    public static bool whalePostOctopusDialogueCompleted = false;
    public static bool whalePostSeaCucumberDialogueCompleted = false;
    public static bool whaleEnsembleAccepted = false;

    public static bool crabAmbassadorIntroDialogueCompleted = false;
    public static bool crabAmbassadorPostWhaleDialogueCompleted = false;
    public static bool crabAmbassadorPostDolphinDialogueCompleted = false;
    public static bool crabAmbassadorPostSharkDialogueCompleted = false;
    public static bool crabAmbassadorPostOctopusDialogueCompleted = false;
    public static bool crabAmbassadorPostSeaCucumberDialogueCompleted = false;

    public static int crabResidentAmbassadorDialogueFlags = 0;
    public static int dolphinAmbassadorDialogueFlags = 0;
    public static int sharkAmbassadorDialogueFlags = 0;
    public static int octopusAmbassadorDialogueFlags = 0;
    public static int seaCucumberAmbassadorDialogueFlags = 0;

    public static bool rqIntroDialogueCompleted = false;

    public static void ResetAll()
    {
        crabCompleted = false;
        dolphinQuestAccepted = false;
        dolphinCompleted = false;
        sharkCompleted = false;
        octopusQuestAccepted = false;
        octopusTaskReadyToTurnIn = false;
        octopusDyingNpcHintDialogueCompleted = false;
        octopusCompleted = false;
        sharkEscortToSeaCucumberRequested = false;
        seaCucumberQuestAccepted = false;
        seaCucumberTriageCompleted = false;
        seaCucumberInternalTreatmentCompleted = false;
        seaCucumberExternalTreatmentCompleted = false;
        seaCucumberCompleted = false;

        whaleIntroDialogueCompleted = false;
        whalePostCrabDialogueCompleted = false;
        whalePostDolphinDialogueCompleted = false;
        whalePostSharkDialogueCompleted = false;
        whalePostOctopusDialogueCompleted = false;
        whalePostSeaCucumberDialogueCompleted = false;
        whaleEnsembleAccepted = false;

        crabAmbassadorIntroDialogueCompleted = false;
        crabAmbassadorPostWhaleDialogueCompleted = false;
        crabAmbassadorPostDolphinDialogueCompleted = false;
        crabAmbassadorPostSharkDialogueCompleted = false;
        crabAmbassadorPostOctopusDialogueCompleted = false;
        crabAmbassadorPostSeaCucumberDialogueCompleted = false;

        crabResidentAmbassadorDialogueFlags = 0;
        dolphinAmbassadorDialogueFlags = 0;
        sharkAmbassadorDialogueFlags = 0;
        octopusAmbassadorDialogueFlags = 0;
        seaCucumberAmbassadorDialogueFlags = 0;

        rqIntroDialogueCompleted = false;
    }
}
