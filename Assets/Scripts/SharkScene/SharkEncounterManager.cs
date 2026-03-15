using UnityEngine;

public class SharkEncounterManager : MonoBehaviour
{
    public static bool StartFromAttackPhaseOnLoad;

    public SharkEncounterPhase CurrentPhase { get; private set; } = SharkEncounterPhase.WaitingForInteraction;

    [Header("Phase Roots")]
    [SerializeField] private GameObject[] waitingPhaseObjects;
    [SerializeField] private GameObject[] attackPhaseObjects;
    [SerializeField] private GameObject[] quizPhaseObjects;
    [SerializeField] private GameObject[] completedPhaseObjects;

    private void Start()
    {
        if (StartFromAttackPhaseOnLoad)
        {
            StartFromAttackPhaseOnLoad = false;
            CurrentPhase = SharkEncounterPhase.AttackChallenge;
        }

        ApplyPhaseState();
    }

    public void BeginAttackPhase()
    {
        if (CurrentPhase != SharkEncounterPhase.WaitingForInteraction)
            return;

        CurrentPhase = SharkEncounterPhase.AttackChallenge;
        ApplyPhaseState();
        Debug.Log("Shark encounter entered Phase 2: Attack Challenge.");
    }

    public void BeginQuizPhase()
    {
        if (CurrentPhase != SharkEncounterPhase.AttackChallenge)
            return;

        CurrentPhase = SharkEncounterPhase.Quiz;
        ApplyPhaseState();
        Debug.Log("Shark encounter entered Phase 3: Quiz.");
    }

    public void CompleteEncounter()
    {
        CurrentPhase = SharkEncounterPhase.Completed;
        ApplyPhaseState();
        Debug.Log("Shark encounter completed.");
    }

    private void ApplyPhaseState()
    {
        SetGroupActive(waitingPhaseObjects, CurrentPhase == SharkEncounterPhase.WaitingForInteraction);
        SetGroupActive(attackPhaseObjects, CurrentPhase == SharkEncounterPhase.AttackChallenge);
        SetGroupActive(quizPhaseObjects, CurrentPhase == SharkEncounterPhase.Quiz);
        SetGroupActive(completedPhaseObjects, CurrentPhase == SharkEncounterPhase.Completed);
    }

    private void SetGroupActive(GameObject[] objects, bool isActive)
    {
        if (objects == null)
            return;

        foreach (GameObject obj in objects)
        {
            if (obj != null)
            {
                obj.SetActive(isActive);
            }
        }
    }
}

public enum SharkEncounterPhase
{
    WaitingForInteraction,
    AttackChallenge,
    Quiz,
    Completed
}
