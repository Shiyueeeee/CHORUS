using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class SeaCucumberPatientDropZone : MonoBehaviour, IDropHandler
{
    [SerializeField] private SeaCucumberPatientType acceptedType;
    [SerializeField] private RectTransform cardsRoot;
    [SerializeField] private Vector2 topLeftPadding = new Vector2(24f, 24f);
    [SerializeField] private Vector2 cellSpacing = new Vector2(120f, 120f);
    [SerializeField] private int columns = 2;

    private readonly List<SeaCucumberPatientCard> cards = new List<SeaCucumberPatientCard>();
    private SeaCucumberTriageManager triageManager;

    public void Initialize(SeaCucumberTriageManager manager)
    {
        triageManager = manager;
        cards.Clear();
    }

    public void OnDrop(PointerEventData eventData)
    {
        if (triageManager == null || !triageManager.IsPuzzleOpen)
            return;

        SeaCucumberPatientCard card = eventData.pointerDrag != null
            ? eventData.pointerDrag.GetComponent<SeaCucumberPatientCard>() ?? eventData.pointerDrag.GetComponentInParent<SeaCucumberPatientCard>()
            : null;

        if (card == null || card.IsAssigned || !card.CanBeDragged)
            return;

        if (card.PatientType != acceptedType)
        {
            triageManager.ReturnCardToLeftZone(card);
            return;
        }

        triageManager.AssignCardToDropZone(card, this);
    }

    public void AddCard(SeaCucumberPatientCard card)
    {
        if (card == null || cards.Contains(card))
            return;

        cards.Add(card);
        LayoutCards();
    }

    public void RemoveCard(SeaCucumberPatientCard card)
    {
        if (card == null)
            return;

        if (cards.Remove(card))
            LayoutCards();
    }

    public void LayoutCards()
    {
        RectTransform root = cardsRoot != null ? cardsRoot : transform as RectTransform;
        if (root == null)
            return;

        int safeColumns = Mathf.Max(1, columns);

        for (int i = 0; i < cards.Count; i++)
        {
            SeaCucumberPatientCard card = cards[i];
            if (card == null)
                continue;

            int column = i % safeColumns;
            int row = i / safeColumns;
            Vector2 anchoredPosition = new Vector2(
                topLeftPadding.x + cellSpacing.x * column,
                -(topLeftPadding.y + cellSpacing.y * row)
            );
            card.PlaceIntoContainer(root, anchoredPosition, i, true);
        }
    }
}
