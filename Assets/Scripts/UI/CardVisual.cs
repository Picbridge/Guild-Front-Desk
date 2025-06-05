using System;
using UnityEngine;
using DG.Tweening;
using System.Collections;
using UnityEngine.EventSystems;
using Unity.Collections;
using UnityEngine.UI;
using Unity.VisualScripting;
using GFD.Quest;
using TMPro;

public class CardVisual : MonoBehaviour
{
    private bool initalize = false;

    [Header("Card")]
    public CardUI parentCard;
    private Transform cardTransform;
    private Vector3 rotationDelta;
    private int savedIndex;
    Vector3 movementDelta;
    private Canvas canvas;

    [Header("References")]
    public Transform visualShadow;
    private float shadowOffset = 20;
    private Vector2 shadowDistance;
    private Canvas shadowCanvas;
    [SerializeField] private Transform shakeParent;
    [SerializeField] private Transform tiltParent;
    [SerializeField] private Image cardImage;

    [Header("Follow Parameters")]
    [SerializeField] private float followSpeed = 30;

    [Header("Rotation Parameters")]
    [SerializeField] private float rotationAmount = 20;
    [SerializeField] private float rotationSpeed = 20;
    [SerializeField] private float autoTiltAmount = 30;
    [SerializeField] private float autoTiltAmountOnSelect = 5;
    [SerializeField] private float manualTiltAmount = 20;
    [SerializeField] private float tiltSpeed = 20;
    [SerializeField] private float screenTiltFactor = 0.01f;
    [SerializeField] private float flipTransition = 0.5f;
    [SerializeField] private AnimationCurve flipEasingCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    [Header("Flip Button Animation Parameters")]
    [SerializeField] private Sprite[] flipAnim;
    [SerializeField] private float flipButtonTransition = 0.3f;
    [SerializeField] private AnimationCurve flipButtonEasingCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    [Header("Scale Parameters")]
    [SerializeField] private bool scaleAnimations = true;
    [SerializeField] private float scaleOnHover = 1.15f;
    [SerializeField] private float scaleOnPressed = 1.25f;
    [SerializeField] private float scaleOnSelect = 4f;
    [SerializeField] private float scaleTransition = .15f;
    [SerializeField] private Ease scaleEase = Ease.OutBack;

    [Header("Assign Parameters")]
    [SerializeField] private float assignTransition = .1f;
    [SerializeField] private float assignMoveAmount = 200f;
    [SerializeField] private float assignScaleTransition = 1f;

    [Header("Select Parameters")]
    [SerializeField] private float selectPunchAmount = 20;
    [SerializeField] private Transform rightButtonImage;
    [SerializeField] private Transform rightShadow;
    [SerializeField] private Transform leftButtonImage;
    [SerializeField] private Transform leftShadow;
    [SerializeField] float moveDistance = 20f;
    [SerializeField] private float shadowFollowFactor = 1.1f;
    [SerializeField] private float moveTransition = 0.2f;

    [Header("Quest Type Sprites")]
    [SerializeField] private Sprite[] questTypeSprites;

    [Header("Hover Parameters")]
    [SerializeField] private float hoverPunchAngle = 5;
    [SerializeField] private float hoverTransition = .15f;


    [HideInInspector] public bool isFlipped = false;
    private bool modifiable = true;
    private GraphicRaycaster raycaster;

    private Transform front;
    private Transform back;
    private Transform leftButton;
    private Transform rightButton;
    private Transform flipButton;

    private Coroutine flipCoroutine;
    private void Start()
    {
        shadowDistance = visualShadow.localPosition;
    }

    public void Initialize(CardUI target, int index = 0)
    {
        //Declarations
        parentCard = target;
        cardTransform = target.transform;
        canvas = GetComponent<Canvas>();
        shadowCanvas = visualShadow.GetComponent<Canvas>();
        raycaster = canvas.GetComponent<GraphicRaycaster>();
        //Event Listening
        parentCard.OnCardPointerEnter += PointerEnter;
        parentCard.OnCardPointerExit += PointerExit;
        parentCard.OnCardPointerDown += PointerDown;
        parentCard.OnCardPointerUp += PointerUp;
        parentCard.OnCardSelected += Select;

        front = tiltParent.Find("Front");
        back = tiltParent.Find("Back");
        flipButton = tiltParent.Find("Flip");

        leftButton = leftButtonImage.parent;
        rightButton = rightButtonImage.parent;
        //Initialization
        initalize = true;

        if (scaleAnimations)
            transform.DOScale(1f, scaleTransition).SetEase(scaleEase);
    }

    public void UpdateIndex(int length)
    {
        transform.SetSiblingIndex(parentCard.transform.parent.GetSiblingIndex());
    }

    void Update()
    {
        if (!initalize || parentCard == null) return;
        if (back)
        SmoothFollow();
        FollowRotation();
        CardTilt();

    }

    public void SetupButtons(
    Quest quest,
    Action<CardVisual, Quest> onAssign,
    Action<CardVisual> onLeft,
    Action<CardVisual> onRight,
    Sprite logo,
    Color color)
    {
        front.Find("Name").GetComponent<TMP_Text>().text = quest.data.questName;
        front.Find("DescriptionBG/Viewport/Content").GetComponent<TMP_Text>().text = quest.data.description;
        var difficultyImage = front.Find("Difficulty").GetComponent<Image>();
        difficultyImage.sprite = logo;
        difficultyImage.color = color;

        // Assign button
        var assignBtn = front.Find("Assign").GetComponent<Button>();
        assignBtn.onClick.RemoveAllListeners();
        assignBtn.onClick.AddListener(() => onAssign(this, quest));

        // Left button
        var leftBtn = leftButton.GetComponent<Button>();
        leftBtn.onClick.RemoveAllListeners();
        leftBtn.onClick.AddListener(() => onLeft(this));

        // Right button
        var rightBtn = rightButton.GetComponent<Button>();
        rightBtn.onClick.RemoveAllListeners();
        rightBtn.onClick.AddListener(() => onRight(this));

        var flipBtn = flipButton.GetComponent<Button>();
        flipBtn.onClick.RemoveAllListeners();
        flipBtn.onClick.AddListener(() =>
        {
            StartCoroutine(Flip());
        });
    }

    public void SetupButtonEventTriggers()
    {
        // Right Button
        var rightBtnGO = rightButton.gameObject;
        var rightTrigger = rightBtnGO.GetComponent<EventTrigger>() ?? rightBtnGO.AddComponent<EventTrigger>();

        // PointerEnter
        var entryEnter = new EventTrigger.Entry { eventID = EventTriggerType.PointerEnter };
        entryEnter.callback.AddListener((eventData) => {
            MoveLRButtonTween(false, false); // Move right button out
        });
        rightTrigger.triggers.Add(entryEnter);

        // PointerExit
        var entryExit = new EventTrigger.Entry { eventID = EventTriggerType.PointerExit };
        entryExit.callback.AddListener((eventData) => {
            MoveLRButtonTween(false, true); // Move right button back
        });
        rightTrigger.triggers.Add(entryExit);

        // Left Button
        var leftBtnGO = leftButton.gameObject;
        var leftTrigger = leftBtnGO.GetComponent<EventTrigger>() ?? leftBtnGO.AddComponent<EventTrigger>();

        var leftEntryEnter = new EventTrigger.Entry { eventID = EventTriggerType.PointerEnter };
        leftEntryEnter.callback.AddListener((eventData) => {
            MoveLRButtonTween(true, false); // Move left button out
        });
        leftTrigger.triggers.Add(leftEntryEnter);

        var leftEntryExit = new EventTrigger.Entry { eventID = EventTriggerType.PointerExit };
        leftEntryExit.callback.AddListener((eventData) => {
            MoveLRButtonTween(true, true); // Move left button back
        });
        leftTrigger.triggers.Add(leftEntryExit);

        // Flip Button
        var flipBtnGO = flipButton.gameObject;
        var flipTrigger = flipBtnGO.GetComponent<EventTrigger>() ?? flipBtnGO.AddComponent<EventTrigger>();
        var flipEntryEnter = new EventTrigger.Entry { eventID = EventTriggerType.PointerEnter };
        flipEntryEnter.callback.AddListener((eventData) => {
            flipCoroutine = StartCoroutine(PlayFlipAnimation(true));
        });
        flipTrigger.triggers.Add(flipEntryEnter);

        var flipEntryExit = new EventTrigger.Entry { eventID = EventTriggerType.PointerExit };
        flipEntryExit.callback.AddListener((eventData) =>
        {
            flipCoroutine = StartCoroutine(PlayFlipAnimation(false));
        });
        flipTrigger.triggers.Add(flipEntryExit);
    }
    
    private IEnumerator PlayFlipAnimation(bool isEnter)
    {
        if (flipCoroutine != null)
        {
            // Stop the previous coroutine if it's running
            StopCoroutine(flipCoroutine);
        }

        int totalFrames = flipAnim.Length;
        int half = totalFrames / 2;
        float transition = flipButtonTransition;

        float elapsed = 0f;
        int start, end, finalFrame;

        if (isEnter)
        {
            start = 0;
            end = half - 1;
            finalFrame = half;
        }
        else
        {
            start = half;
            end = totalFrames - 1;
            finalFrame = 0;
        }

        while (elapsed < transition)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / transition);
            float curveT = flipButtonEasingCurve.Evaluate(t);

            int frameCount = end - start + 1;
            int frameIndex = start + Mathf.FloorToInt(curveT * frameCount);

            // Clamp to valid range
            frameIndex = Mathf.Clamp(frameIndex, start, end);

            flipButton.GetComponent<Image>().sprite = flipAnim[frameIndex];
            yield return null;
        }

        // Set to the final frame
        flipButton.GetComponent<Image>().sprite = flipAnim[finalFrame];
    }

    private void MoveLRButtonTween(bool isLeft, bool moveLeft)
    {
        float targetButtonX, targetShadowX;
        if (isLeft)
        {
            targetButtonX = leftButtonImage.localPosition.x + (moveLeft ? -moveDistance : moveDistance);
            targetShadowX = leftShadow.localPosition.x + (moveLeft ? -moveDistance * shadowFollowFactor : moveDistance * shadowFollowFactor);

            leftButtonImage.DOLocalMoveX(targetButtonX, moveTransition).SetId(leftButtonImage);
            leftShadow.DOLocalMoveX(targetShadowX, moveTransition).SetId(leftShadow);
        }
        else
        {
            targetButtonX = rightButtonImage.localPosition.x + (moveLeft ? -moveDistance : moveDistance);
            targetShadowX = rightShadow.localPosition.x + (moveLeft ? -moveDistance * shadowFollowFactor : moveDistance * shadowFollowFactor);

            rightButtonImage.DOLocalMoveX(targetButtonX, moveTransition).SetId(rightButtonImage);
            rightShadow.DOLocalMoveX(targetShadowX, moveTransition).SetId(rightShadow);
        }
    }

    public IEnumerator Flip()
    {
        float transition = flipTransition * 0.5f;
        float elapsed = 0f;
        
        var targetRotation = 90f;
        var targetScale = Vector3.one * 1.3f;

        var originalRotation = tiltParent.eulerAngles;
        var originalScale = tiltParent.localScale;  
        // First half of the flip
        while (elapsed < transition)
        {
            elapsed += Time.deltaTime;
            float t = flipEasingCurve.Evaluate(elapsed / transition);

            // Rotate the card
            float lerpY = Mathf.LerpAngle(originalRotation.y, targetRotation, t);
            tiltParent.eulerAngles = new Vector3(tiltParent.eulerAngles.x, lerpY, tiltParent.eulerAngles.z);
            tiltParent.localScale = Vector3.Lerp(originalScale, targetScale, t);

            yield return null;
        }

        isFlipped = !isFlipped;
        tiltParent.Find("Front").gameObject.SetActive(!isFlipped);
        tiltParent.Find("Back").gameObject.SetActive(isFlipped);

        elapsed = 0f;

        // Second half of the flip
        while (elapsed < transition)
        {
            elapsed += Time.deltaTime;
            float t = flipEasingCurve.Evaluate(elapsed / transition);

            // Rotate the card back to original position
            float lerpY = Mathf.LerpAngle(targetRotation, 0, t);
            tiltParent.eulerAngles = new Vector3(tiltParent.eulerAngles.x, lerpY, tiltParent.eulerAngles.z);
            tiltParent.localScale = Vector3.Lerp(targetScale, originalScale, t);

            yield return null;
        }
    }

    private void SmoothFollow()
    {
        transform.position = Vector3.Lerp(transform.position, cardTransform.position, followSpeed * Time.deltaTime);
    }

    private void FollowRotation()
    {
        Vector3 movement = (transform.position - cardTransform.position);
        movementDelta = Vector3.Lerp(movementDelta, movement, 25 * Time.deltaTime);
        Vector3 movementRotation = (parentCard.isMoving ? movementDelta : movement) * rotationAmount;
        rotationDelta = Vector3.Lerp(rotationDelta, movementRotation, rotationSpeed * Time.deltaTime);
        transform.eulerAngles = new Vector3(transform.eulerAngles.x, transform.eulerAngles.y, Mathf.Clamp(rotationDelta.x, -60, 60));
    }

    private void CardTilt()
    {
        savedIndex = parentCard.ParentIndex();
        float sine = Mathf.Sin(Time.time + savedIndex) * (parentCard.isHovering ? .2f : 1);
        float cosine = Mathf.Cos(Time.time + savedIndex) * (parentCard.isHovering ? .2f : 1);

        // Get the RectTransform directly
        RectTransform rectTransform = GetComponent<RectTransform>();

        // Calculate offset in screen space
        Vector2 cardScreenPos = rectTransform.position;
        Vector2 mousePos = Input.mousePosition;
        Vector2 offset = modifiable ? cardScreenPos - mousePos : Vector2.one;
        

        float tiltX = parentCard.isHovering ? ((offset.y * -1) * manualTiltAmount * screenTiltFactor) : 0;
        float tiltY = parentCard.isHovering ? ((offset.x) * manualTiltAmount * screenTiltFactor) : 0;
        float tiltZ = parentCard.SiblingAmount();

        var tiltAmount = modifiable ? autoTiltAmount : autoTiltAmountOnSelect;

        float lerpX = Mathf.LerpAngle(tiltParent.eulerAngles.x, tiltX + (sine * tiltAmount), tiltSpeed * Time.deltaTime);
        float lerpY = Mathf.LerpAngle(tiltParent.eulerAngles.y, tiltY + (cosine * tiltAmount), tiltSpeed * Time.deltaTime);
        float lerpZ = Mathf.LerpAngle(tiltParent.eulerAngles.z, tiltZ, tiltSpeed / 2 * Time.deltaTime);

        tiltParent.eulerAngles = new Vector3(lerpX, lerpY, lerpZ);
    }

    public IEnumerator Assign()
    {
        Debug.Log("Assigning Card: " + parentCard.name);

        var canvasGroup = transform.GetComponent<CanvasGroup>();
        Vector3 startPos = transform.localPosition;
        Vector3 targetPos = startPos + Vector3.up * assignMoveAmount;
        Vector3 startScale = transform.localScale;

        float elapsed = 0f;

        while (elapsed < assignTransition)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / assignTransition);
            float scaleT = Mathf.Clamp01(elapsed / assignScaleTransition);
            // Move position
            transform.localPosition = Vector3.Lerp(startPos, targetPos, t);
            // Scale down
            //transform.localScale = Vector3.Lerp(startScale, Vector3.zero, scaleT);

            // Fade out
            if (canvasGroup != null)
                canvasGroup.alpha = Mathf.Lerp(1f, 0f, t);

            yield return null;
        }

        // Ensure final state
        transform.localPosition = targetPos;
        if (canvasGroup != null)
            canvasGroup.alpha = 0f;

        Destroy(parentCard.transform.parent.gameObject);
    }

    private void Select(CardUI card, bool state)
    {
        DOTween.Kill(2, true);
        float dir = state ? 1 : 0;
        shakeParent.DOPunchPosition(shakeParent.up * selectPunchAmount * dir, scaleTransition, 10, 1);
        shakeParent.DOPunchRotation(Vector3.forward * (hoverPunchAngle / 2), hoverTransition, 20, 1).SetId(2);
        SetScalable(!state);
    }

    private void PointerEnter(CardUI card)
    {
        if (!initalize || !modifiable) return;
        if (scaleAnimations)
            transform.DOScale(scaleOnHover, scaleTransition).SetEase(scaleEase);

        DOTween.Kill(2, true);
        shakeParent.DOPunchRotation(Vector3.forward * hoverPunchAngle, hoverTransition, 20, 1).SetId(2);
    }

    private void PointerExit(CardUI card)
    {
        if (!initalize || !modifiable) return;
        transform.DOScale(1, scaleTransition).SetEase(scaleEase);
    }

    private void PointerUp(CardUI card, bool longPress)
    {
        if (!initalize || !modifiable) return;
        if (scaleAnimations)
        {
            transform.DOScale(longPress ? scaleOnHover : scaleOnPressed, scaleTransition).SetEase(scaleEase);
        }
        canvas.overrideSorting = false;

        visualShadow.localPosition = shadowDistance;
        shadowCanvas.overrideSorting = true;
    }

    private void PointerDown(CardUI card)
    {
        if (!initalize || !modifiable) return;
        if (scaleAnimations)
            transform.DOScale(scaleOnPressed, scaleTransition).SetEase(scaleEase);

        visualShadow.localPosition += (-Vector3.up * shadowOffset);
        shadowCanvas.overrideSorting = false;
    }

    public void SetScalable(bool state)
    {
        modifiable = state;
        // Selecting a card will disable scaling
        if (!state)//Card Selected
        {
            if (scaleAnimations)
                transform.DOScale(scaleOnSelect, scaleTransition).SetEase(scaleEase);
            canvas.overrideSorting = true;
            raycaster.enabled = true;

            leftButton.gameObject.SetActive(true);
            rightButton.gameObject.SetActive(true);
        }
        else
        {
            if (scaleAnimations)
                transform.DOScale(1, scaleTransition).SetEase(scaleEase);
            canvas.overrideSorting = false;
            raycaster.enabled = false;
            leftButton.gameObject.SetActive(false);
            rightButton.gameObject.SetActive(false);
        }
    }
}