using UnityEngine;

[System.Serializable]
public class SharkQuizQuestion
{
    [TextArea(2, 4)]
    public string question;

    public string[] answers = new string[3];

    [Min(0)]
    public int correctAnswerIndex;
}
