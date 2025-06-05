using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using System.Collections;
using UnityEngine.UI;
using System;
using GFD.Utilities;

public class CardUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerUpHandler, IPointerDownHandler
{
    [Header("Drag Settings")]
    [SerializeField] private bool instantiateVisual = true;

    private VisualCardsHandler visualHandler;
    private Vector2 offset;
    private Canvas canvas;// The canvas this card is part of, used for coordinate conversion
    private Image imageComponent;

    [Header("Selection")]
    [SerializeField] internal bool selected;
    [SerializeField] internal float selectionOffset = 50.0f;
    private float pointerDownTime;
    private float pointerUpTime;

    [Header("Visual")]
    [SerializeField] private GameObject cardVisualPrefab;
    [HideInInspector] public CardVisual cardVisual;

    [HideInInspector] public bool isHovering;
    [HideInInspector] public bool isMoving;

    public event Action<CardUI> OnCardPointerEnter;
    public event Action<CardUI> OnCardPointerExit;

    public event Action<CardUI, bool> OnCardPointerUp;
    public event Action<CardUI> OnCardPointerDown;

    public event Action<CardUI, bool> OnCardSelected;

    public void Initialize()
    {
        canvas = GetComponentInParent<Canvas>();
        imageComponent = GetComponent<Image>();

        if (!instantiateVisual) return;

        cardVisual = Instantiate(cardVisualPrefab, gameObject.transform).GetComponent<CardVisual>();
        cardVisual.Initialize(this);
    }

    private void OnEnable()
    {
        if (canvas != null)
            canvas.GetComponent<GraphicRaycaster>().enabled = true;

        if (imageComponent != null)
            imageComponent.raycastTarget = true;
    }
    void Update()
    {

    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        OnCardPointerEnter?.Invoke(this);
        isHovering = true;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        OnCardPointerExit?.Invoke(this);
        isHovering = false;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Left || selected) 
            return;

        OnCardPointerDown?.Invoke(this);
        pointerDownTime = Time.time;
    }


    public void OnPointerUp(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Left || selected) 
            return;

        pointerUpTime = Time.time;
        bool isSelected = pointerUpTime - pointerDownTime <= .2f;
        OnCardPointerUp?.Invoke(this, isSelected);

        if (isSelected)
            return;

        Select(!selected);
    }

    public void Select(bool select)
    {
        selected = select;
        if (select)
        {
            // Block all interactions with the card

        }
        OnCardSelected?.Invoke(this, selected);
    }

    public void Deselect()
    {
        if (selected)
        {
            selected = false;
            transform.localPosition = Vector3.zero;
            cardVisual.SetScalable(!selected);
            OnCardSelected?.Invoke(this, selected);
        }
    }

    public int SiblingAmount()
    {
        return transform.parent.CompareTag("Slot") ? transform.parent.parent.childCount - 1 : 0;
    }

    public int ParentIndex()
    {
        return transform.parent.CompareTag("Slot") ? transform.parent.GetSiblingIndex() : 0;
    }

    public float NormalizedPosition()
    {
        return transform.parent.CompareTag("Slot") ? ExtensionMethods.Remap((float)ParentIndex(), 0, (float)(transform.parent.parent.childCount - 1), 0, 1) : 0;
    }

    private void OnDestroy()
    {
        if (cardVisual != null)
            Destroy(cardVisual.gameObject);
    }
}