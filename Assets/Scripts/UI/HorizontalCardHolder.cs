using NUnit.Framework;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System.Collections;
using DG.Tweening;
using System;
using TMPro;
using GFD.Quest;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class HorizontalCardHolder : MonoBehaviour
{
    private Script_QuestManager questManager;
    private Script_TimeManager timeManager;
    private Script_QueueManager queueManager;

    [Header("Selection")]
    [SerializeField]
    private CardUI selectedCard;
    [SerializeField]
    private CardUI hoveredCard;
    [SerializeField] 
    private GameObject selectedSlot;

    [Header("Spawn Settings")]
    [SerializeField]
    private GameObject cardPrefab; // Prefab for the card UI

    [Header("Quest Card Settings")]
    [SerializeField]
    private Sprite Elimination;
    [SerializeField]
    private Sprite Exploration;
    [SerializeField]
    private Sprite Escort;
    [SerializeField]
    private Sprite Retrieval;
    [SerializeField]
    private Sprite Assassination;

    private List<CardUI> cards;
    private RectTransform rect;
    private bool isCrossing = false;
    private int cardCount = 0;
    private int selectedCardIndex = 0;
    private void Start()
    {
        questManager = Script_QuestManager.Instance;
        timeManager = Script_TimeManager.Instance;
        queueManager = Script_QueueManager.Instance;

        questManager.OnDailyQuestUpdated += SpawnCards;
    }

    private void SpawnCards()
    {
        if (selectedCard)
        {
            selectedCard.transform.parent.transform.SetParent(gameObject.transform);
            selectedCard.Deselect();
            selectedCard = null;
        }

        var quests = questManager.GetQuestsOfTheDay();

        StartCoroutine(DestroyChildrenSequentially(0.1f));
        StartCoroutine(SpawnCardsSequentially(quests, 0.1f));

    }

    private IEnumerator SpawnCardsSequentially(IReadOnlyList<Quest> quests, float v)
    {
        
        foreach (Quest quest in quests)
        {
            var card = Instantiate(cardPrefab, transform);
            InitCard(card.GetComponentInChildren<CardUI>(), quest);
            yield return new WaitForSeconds(v); // Wait before spawning the next card
        }

        cards = GetComponentsInChildren<CardUI>().ToList();

        foreach (CardUI card in cards)
        {
            card.OnCardPointerEnter += CardPointerEnter;
            card.OnCardPointerExit += CardPointerExit;
            card.OnCardSelected += CardSelected;
            card.name = cardCount.ToString();
            cardCount++;
        }

        StartCoroutine(Frame());

        IEnumerator Frame()
        {
            yield return new WaitForSecondsRealtime(.1f);
            for (int i = 0; i < cards.Count; i++)
            {
                if (cards[i].cardVisual != null)
                    cards[i].cardVisual.UpdateIndex(transform.childCount);
            }
        }
    }

    private void InitCard(CardUI cardUI, Quest quest)
    {
        cardUI.Initialize();

        CardVisual cardVisual = cardUI.cardVisual;
        Transform cardTransform = cardVisual.transform;

        var logo = quest.data.questType switch
        {
            QuestType.Elimination => Elimination,
            QuestType.Exploration => Exploration,
            QuestType.Escort => Escort,
            QuestType.Retrieval => Retrieval,
            QuestType.Assassination => Assassination,
            _ => Elimination // Default case
        };

        var color = quest.data.difficulty switch
        {
            QuestDifficulty.Easy => new Color(0.3f, 0.8f, 0.3f, 1f), // Green
            QuestDifficulty.Normal => new Color(0.8f, 0.8f, 0.2f, 1f), // Yellow
            QuestDifficulty.Hard => new Color(0.9f, 0.6f, 0.2f, 1f), // Orange
            QuestDifficulty.Deadly => new Color(0.8f, 0.2f, 0.2f, 1f), // Red
            _ => throw new NotImplementedException(),
        };

        cardVisual.SetupButtons(quest, OnAssignClick, OnLeftClick, OnRightClick, logo, color);
        cardVisual.SetupButtonEventTriggers();
    }

    private void OnAssignClick(CardVisual cardVisual, Quest quest)
    {
        transform.GetComponentInParent<Image>().enabled = false;
        StartCoroutine(cardVisual.Assign());
        cards.Remove(cardVisual.parentCard);
        timeManager.SpendTime();
        queueManager.AssignQuestToCurrentAdventurer(quest);
        queueManager.ResumeQueue();
        cardCount--;
    }

    private void OnLeftClick(CardVisual cardVisual)
    {
        int prevIndex = (selectedCardIndex - 1 + cardCount) % cardCount;
        CardUI prevCard = cards[prevIndex];
        selectedCard.Deselect();
        selectedCard = prevCard;
        selectedCard.Select(true);
    }

    private void OnRightClick(CardVisual cardVisual)
    {
        int nextIndex = (selectedCardIndex + 1) % cardCount;
        CardUI nextCard = cards[nextIndex];
        selectedCard.Deselect();
        selectedCard = nextCard;
        selectedCard.Select(true);
    }

    private IEnumerator DestroyChildrenSequentially(float delay = 0.1f)
    {
        // Make a copy of the children to avoid modifying the collection while iterating
        List<Transform> children = new List<Transform>();
        foreach (Transform child in transform)
            children.Add(child);

        foreach (Transform child in children)
        {
            Destroy(child.gameObject);
            yield return new WaitForSeconds(delay); // Wait before destroying the next child
        }
    }

    private void CardSelected(CardUI card, bool selected)
    {
        if (selected)
        {
            
            if (selectedCard != null && selectedCard != card)
            {
                selectedCard.transform.parent.transform.SetParent(gameObject.transform);
                selectedCard.Deselect();
            }
            transform.GetComponentInParent<Image>().enabled = true;
            selectedCard = card;
            selectedCard.transform.parent.transform.SetParent(selectedSlot.transform);
            selectedCard.transform.parent.transform.localPosition = Vector3.zero;
        }
        else
        {
            if (selectedCard == card)
            {
                if (selectedCard.cardVisual.isFlipped)
                    StartCoroutine(selectedCard.cardVisual.Flip());
                transform.GetComponentInParent<Image>().enabled = false;
                selectedCard.transform.parent.transform.SetParent(gameObject.transform);
                selectedCard = null;
            }
        }
    }

    private void CardPointerEnter(CardUI card)
    {
        hoveredCard = card;
    }

    private void CardPointerExit(CardUI card)
    {
        hoveredCard = null;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Delete))
        {
            if (hoveredCard != null)
            {
                Destroy(hoveredCard.transform.parent.gameObject);
                cards.Remove(hoveredCard);

            }
        }

        if (Input.GetMouseButtonDown(1))
        {
            foreach (CardUI card in cards)
            {
                card.Deselect();
            }
        }

        if (selectedCard == null)
            return;

        if (isCrossing)
            return;

        selectedCardIndex = selectedCard == null ? -1 : cards.IndexOf(selectedCard);
        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            int nextIndex = (selectedCardIndex + 1) % cardCount;
            Debug.Log("Next Index: " + nextIndex);
            CardUI nextCard = cards[nextIndex];
            selectedCard.Deselect();
            selectedCard = nextCard;
            selectedCard.Select(true);
        }
        else if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            int prevIndex = (selectedCardIndex - 1 + cardCount) % cardCount;
            Debug.Log("Previous Index: " + prevIndex);
            CardUI prevCard = cards[prevIndex];
            selectedCard.Deselect();
            selectedCard = prevCard;
            selectedCard.Select(true);
        }

        if (Input.GetKeyDown(KeyCode.Backspace))
        {
            StartCoroutine(selectedCard.cardVisual.Flip());
        }
    }
}
