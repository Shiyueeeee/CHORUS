using UnityEngine;

[RequireComponent(typeof(UnityEngine.UI.Button))]
public class RQPianoNoteButton : MonoBehaviour
{
    [SerializeField] [Range(0, 35)] private int noteIndex;

    public int NoteIndex => noteIndex;
}
