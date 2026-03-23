using UnityEngine;

[System.Serializable]
public class DialogueLineData
{
    public string speaker;

    [TextArea(2, 5)]
    public string text;

    public AudioClip sfxClip;

    [Range(0f, 1f)]
    public float sfxVolumeScale = 1f;
}
