using System.Collections.Generic;
using UnityEngine;

public class RQResidentAmbassadorInteraction : NpcDialogueInteractionBase
{
    [SerializeField] private RQResidentType residentType;
    [SerializeField] private string speakerName = "Resident";

    [Header("Base Dialogue")]
    [SerializeField] private DialogueLineData[] aboutBaseDialogueLines;

    [Header("Whale Dialogue")]
    [SerializeField] private DialogueLineData[] aboutWhaleDialogueLines;

    [Header("Priority Dialogue")]
    [SerializeField] private DialogueLineData[] requestSeaCucumberEscortDialogueLines;

    [Header("Neighbor Dialogues")]
    [SerializeField] private DialogueLineData[] aboutCrabDialogueLines;
    [SerializeField] private DialogueLineData[] aboutDolphinDialogueLines;
    [SerializeField] private DialogueLineData[] aboutSharkDialogueLines;
    [SerializeField] private DialogueLineData[] aboutOctopusDialogueLines;
    [SerializeField] private DialogueLineData[] aboutSeaCucumberDialogueLines;

    [Header("Symphony Repeat Dialogue")]
    [SerializeField] private DialogueLineData[] symphonyRepeatDialogueLines;
    [SerializeField] private DialogueBlockData[] randomSymphonyRepeatDialogueBlocks;

    [Header("Repeat Dialogue")]
    [SerializeField] private DialogueLineData[] repeatDialogueLines =
    {
        new DialogueLineData { text = "......" }
    };
    [SerializeField] private DialogueBlockData[] randomRepeatDialogueBlocks;

    private ResidentDialogueTopic activeTopic = ResidentDialogueTopic.Repeat;

    protected override string GetSpeakerName()
    {
        return speakerName;
    }

    protected override List<DialogueLineData> GetDialogueLinesForCurrentState()
    {
        if (!IsResidentUnlocked(residentType))
        {
            activeTopic = ResidentDialogueTopic.Repeat;
            return BuildDialogueList(repeatDialogueLines, speakerName);
        }

        if (residentType == RQResidentType.Shark &&
            GameProgress.whalePostOctopusDialogueCompleted &&
            !GameProgress.sharkEscortToSeaCucumberRequested)
        {
            List<DialogueLineData> escortLines = BuildDialogueList(requestSeaCucumberEscortDialogueLines, speakerName);
            if (escortLines != null && escortLines.Count > 0)
            {
                activeTopic = ResidentDialogueTopic.RequestSeaCucumberEscort;
                return escortLines;
            }
        }

        if (!HasSeenTopic(ResidentDialogueTopic.AboutBase))
        {
            List<DialogueLineData> baseLines = BuildDialogueList(aboutBaseDialogueLines, speakerName);
            if (baseLines != null && baseLines.Count > 0)
            {
                activeTopic = ResidentDialogueTopic.AboutBase;
                return baseLines;
            }
        }

        if (!HasSeenTopic(ResidentDialogueTopic.AboutWhale))
        {
            List<DialogueLineData> whaleLines = BuildDialogueList(aboutWhaleDialogueLines, speakerName);
            if (whaleLines != null && whaleLines.Count > 0)
            {
                activeTopic = ResidentDialogueTopic.AboutWhale;
                return whaleLines;
            }
        }

        if (residentType != RQResidentType.Crab && GameProgress.crabCompleted && !HasSeenTopic(ResidentDialogueTopic.AboutCrab))
        {
            List<DialogueLineData> lines = BuildDialogueList(aboutCrabDialogueLines, speakerName);
            if (lines != null && lines.Count > 0)
            {
                activeTopic = ResidentDialogueTopic.AboutCrab;
                return lines;
            }
        }

        if (residentType != RQResidentType.Dolphin && GameProgress.dolphinCompleted && !HasSeenTopic(ResidentDialogueTopic.AboutDolphin))
        {
            List<DialogueLineData> lines = BuildDialogueList(aboutDolphinDialogueLines, speakerName);
            if (lines != null && lines.Count > 0)
            {
                activeTopic = ResidentDialogueTopic.AboutDolphin;
                return lines;
            }
        }

        if (residentType != RQResidentType.Shark && GameProgress.sharkCompleted && !HasSeenTopic(ResidentDialogueTopic.AboutShark))
        {
            List<DialogueLineData> lines = BuildDialogueList(aboutSharkDialogueLines, speakerName);
            if (lines != null && lines.Count > 0)
            {
                activeTopic = ResidentDialogueTopic.AboutShark;
                return lines;
            }
        }

        if (residentType != RQResidentType.Octopus && GameProgress.octopusCompleted && !HasSeenTopic(ResidentDialogueTopic.AboutOctopus))
        {
            List<DialogueLineData> lines = BuildDialogueList(aboutOctopusDialogueLines, speakerName);
            if (lines != null && lines.Count > 0)
            {
                activeTopic = ResidentDialogueTopic.AboutOctopus;
                return lines;
            }
        }

        if (residentType != RQResidentType.SeaCucumber && GameProgress.seaCucumberCompleted && !HasSeenTopic(ResidentDialogueTopic.AboutSeaCucumber))
        {
            List<DialogueLineData> lines = BuildDialogueList(aboutSeaCucumberDialogueLines, speakerName);
            if (lines != null && lines.Count > 0)
            {
                activeTopic = ResidentDialogueTopic.AboutSeaCucumber;
                return lines;
            }
        }

        activeTopic = ResidentDialogueTopic.Repeat;
        if (GameProgress.whalePostSeaCucumberDialogueCompleted && !GameProgress.whaleEnsembleAccepted)
            return BuildRandomDialogueList(randomSymphonyRepeatDialogueBlocks, speakerName) ?? BuildDialogueList(symphonyRepeatDialogueLines, speakerName) ?? BuildRandomDialogueList(randomRepeatDialogueBlocks, speakerName) ?? BuildDialogueList(repeatDialogueLines, speakerName);

        return BuildRandomDialogueList(randomRepeatDialogueBlocks, speakerName) ?? BuildDialogueList(repeatDialogueLines, speakerName);
    }

    protected override void OnDialogueFinished()
    {
        if (activeTopic == ResidentDialogueTopic.Repeat)
            return;

        if (activeTopic == ResidentDialogueTopic.RequestSeaCucumberEscort)
        {
            GameProgress.sharkEscortToSeaCucumberRequested = true;
            return;
        }

        SetSeenTopic(activeTopic);
    }

    private bool IsResidentUnlocked(RQResidentType type)
    {
        switch (type)
        {
            case RQResidentType.Crab:
                return GameProgress.crabCompleted;
            case RQResidentType.Dolphin:
                return GameProgress.dolphinCompleted;
            case RQResidentType.Shark:
                return GameProgress.sharkCompleted;
            case RQResidentType.Octopus:
                return GameProgress.octopusCompleted;
            case RQResidentType.SeaCucumber:
                return GameProgress.seaCucumberCompleted;
            default:
                return false;
        }
    }

    private bool HasSeenTopic(ResidentDialogueTopic topic)
    {
        int mask = (int)topic;
        return (GetResidentDialogueFlags() & mask) != 0;
    }

    private void SetSeenTopic(ResidentDialogueTopic topic)
    {
        int flags = GetResidentDialogueFlags();
        flags |= (int)topic;
        SetResidentDialogueFlags(flags);
    }

    private int GetResidentDialogueFlags()
    {
        switch (residentType)
        {
            case RQResidentType.Crab:
                return GameProgress.crabResidentAmbassadorDialogueFlags;
            case RQResidentType.Dolphin:
                return GameProgress.dolphinAmbassadorDialogueFlags;
            case RQResidentType.Shark:
                return GameProgress.sharkAmbassadorDialogueFlags;
            case RQResidentType.Octopus:
                return GameProgress.octopusAmbassadorDialogueFlags;
            case RQResidentType.SeaCucumber:
                return GameProgress.seaCucumberAmbassadorDialogueFlags;
            default:
                return 0;
        }
    }

    private void SetResidentDialogueFlags(int flags)
    {
        switch (residentType)
        {
            case RQResidentType.Crab:
                GameProgress.crabResidentAmbassadorDialogueFlags = flags;
                break;
            case RQResidentType.Dolphin:
                GameProgress.dolphinAmbassadorDialogueFlags = flags;
                break;
            case RQResidentType.Shark:
                GameProgress.sharkAmbassadorDialogueFlags = flags;
                break;
            case RQResidentType.Octopus:
                GameProgress.octopusAmbassadorDialogueFlags = flags;
                break;
            case RQResidentType.SeaCucumber:
                GameProgress.seaCucumberAmbassadorDialogueFlags = flags;
                break;
        }
    }
}

[System.Flags]
public enum ResidentDialogueTopic
{
    Repeat = 0,
    AboutBase = 1 << 0,
    AboutWhale = 1 << 1,
    AboutCrab = 1 << 2,
    AboutDolphin = 1 << 3,
    AboutShark = 1 << 4,
    AboutOctopus = 1 << 5,
    AboutSeaCucumber = 1 << 6,
    RequestSeaCucumberEscort = 1 << 7
}

public enum RQResidentType
{
    Crab,
    Dolphin,
    Shark,
    Octopus,
    SeaCucumber
}
