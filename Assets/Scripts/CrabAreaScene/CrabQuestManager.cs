using UnityEngine;

public class CrabQuestManager : MonoBehaviour
{
    public static CrabQuestManager Instance { get; private set; }

    [Header("Quest State")]
    [SerializeField] private bool questAccepted = false;
    [SerializeField] private int initialTrashCount = 10;
    [SerializeField] private int remainingTrash = 10;
    [SerializeField] private float trashHintDelaySeconds = 30f;

    [Header("Trash Root")]
    [SerializeField] private GameObject trashGroup;

    private float lastTrashProgressTime;
    private TrashInteraction cachedHintTrash;

    public bool QuestAccepted => questAccepted;
    public int InitialTrashCount => initialTrashCount;
    public int RemainingTrash => remainingTrash;
    public bool AllTrashCleared => questAccepted && remainingTrash <= 0;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void Start()
    {
        lastTrashProgressTime = Time.time;

        if (trashGroup != null)
        {
            trashGroup.SetActive(false);
        }
    }

    public void AcceptQuest()
    {
        if (questAccepted) return;

        questAccepted = true;
        remainingTrash = initialTrashCount;

        if (trashGroup != null)
        {
            trashGroup.SetActive(true);
        }

        lastTrashProgressTime = Time.time;
        cachedHintTrash = null;
    }

    public void ClearOneTrash()
    {
        if (!questAccepted) return;

        remainingTrash--;
        if (remainingTrash < 0)
            remainingTrash = 0;

        lastTrashProgressTime = Time.time;
        cachedHintTrash = null;
    }

    public void CompleteQuest()
    {
        if (!AllTrashCleared) return;

        GameProgress.crabCompleted = true;
    }

    public bool TryGetTrashHintTarget(out Transform target)
    {
        target = null;

        if (!questAccepted || AllTrashCleared)
            return false;

        if (Time.time - lastTrashProgressTime < trashHintDelaySeconds)
            return false;

        if (cachedHintTrash == null)
            cachedHintTrash = ChooseRandomRemainingTrash();

        if (cachedHintTrash == null)
            return false;

        target = cachedHintTrash.transform;
        return true;
    }

    private TrashInteraction ChooseRandomRemainingTrash()
    {
        if (trashGroup == null)
            return null;

        TrashInteraction[] remainingTrashItems = trashGroup.GetComponentsInChildren<TrashInteraction>(true);
        if (remainingTrashItems == null || remainingTrashItems.Length == 0)
            return null;

        int chosenIndex = Random.Range(0, remainingTrashItems.Length);
        return remainingTrashItems[chosenIndex];
    }
}
