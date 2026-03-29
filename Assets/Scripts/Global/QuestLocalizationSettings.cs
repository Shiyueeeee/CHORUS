using UnityEngine;

[CreateAssetMenu(fileName = "QuestLocalizationSettings", menuName = "CHORUS/Quest Localization Settings")]
public class QuestLocalizationSettings : ScriptableObject
{
    [System.Serializable]
    public class LocalizedQuestText
    {
        public string english;
        public string chinese;

        public string GetText()
        {
            return LanguageSettingsManager.GetLocalizedText(english, chinese);
        }

        public string GetFormattedText(params object[] args)
        {
            string format = GetText();
            return args == null || args.Length == 0 ? format : string.Format(format, args);
        }
    }

    [Header("RQ / Whale")]
    public LocalizedQuestText talkToWhale = new LocalizedQuestText { english = "Talk to Whale", chinese = "和鲸鱼对话" };
    public LocalizedQuestText playTheSymphony = new LocalizedQuestText { english = "Play the Symphony", chinese = "演奏交响乐" };

    [Header("Crab")]
    public LocalizedQuestText talkToCrab = new LocalizedQuestText { english = "Talk to Crab", chinese = "和螃蟹对话" };
    public LocalizedQuestText collectTrash = new LocalizedQuestText { english = "Collect Trash ({0}/{1})", chinese = "收集垃圾 ({0}/{1})" };

    [Header("Dolphin")]
    public LocalizedQuestText findTheirMom = new LocalizedQuestText { english = "Find Their Mom", chinese = "找到它们的妈妈" };
    public LocalizedQuestText talkToDolphin = new LocalizedQuestText { english = "Talk to Dolphin", chinese = "和海豚对话" };

    [Header("Shark")]
    public LocalizedQuestText talkToShark = new LocalizedQuestText { english = "Talk to Shark", chinese = "和鲨鱼对话" };
    public LocalizedQuestText fightTheShark = new LocalizedQuestText { english = "Fight the Shark", chinese = "和鲨鱼战斗" };
    public LocalizedQuestText answerToTheShark = new LocalizedQuestText { english = "Answer to the Shark", chinese = "回答鲨鱼的问题" };

    [Header("Octopus")]
    public LocalizedQuestText talkToOctopus = new LocalizedQuestText { english = "Talk to Octopus", chinese = "和章鱼对话" };
    public LocalizedQuestText closeTheOilRigs = new LocalizedQuestText { english = "Close the Oil Rigs", chinese = "关闭采油井" };

    [Header("Sea Cucumber")]
    public LocalizedQuestText talkToSeaCucumber = new LocalizedQuestText { english = "Talk to Sea Cucumber", chinese = "和海参对话" };
    public LocalizedQuestText sortThePatients = new LocalizedQuestText { english = "Sort the Patients", chinese = "给病人分诊" };
    public LocalizedQuestText treatSickAndHurt = new LocalizedQuestText { english = "Treat\n-Sick pile\n-Hurt pile", chinese = "治疗\n-病患堆\n-伤患堆" };
    public LocalizedQuestText treatSick = new LocalizedQuestText { english = "Treat\n-Sick pile", chinese = "治疗\n-病患堆" };
    public LocalizedQuestText treatHurt = new LocalizedQuestText { english = "Treat\n-Hurt pile", chinese = "治疗\n-伤患堆" };
}
