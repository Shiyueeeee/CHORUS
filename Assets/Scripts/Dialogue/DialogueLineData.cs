using UnityEngine;

[System.Serializable]
public class DialogueLineData
{
    public string speaker;

    [TextArea(2, 5)]
    public string text;
}
