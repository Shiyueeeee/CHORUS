using UnityEngine;

[System.Serializable]
public class DialogueBlockData
{
    [SerializeField] private DialogueLineData[] lines;

    public DialogueLineData[] Lines => lines;
}
