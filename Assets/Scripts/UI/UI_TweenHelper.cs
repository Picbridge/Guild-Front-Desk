using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class UI_TweenHelper : MonoBehaviour
{
    public enum AnimateToDirection
    {
        Top,
        Bottom,
        Left,
        Right
    }

    [Header("Tween Settings")]
    [SerializeField] private GameObject targetObject;
    [SerializeField] private RectTransform targetRectTransform;
    [SerializeField] private CanvasGroup targetCanvasGroup;

    [Header("Animation Settings")]
    [SerializeField] private AnimateToDirection openDirection = AnimateToDirection.Bottom;
    [SerializeField] private AnimateToDirection closeDirection = AnimateToDirection.Top;
    [Space]
    [SerializeField] private Vector2 distanceToAnimate = new Vector2(1, 1);
    [SerializeField] private AnimationCurve easingCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    [Range(0, 1f)][SerializeField] private float animationDuration = 0.5f;

    public event Func<IEnumerator> PreClose;
    public event Func<IEnumerator> PreOpen;
    public event Action OnOpen;
    public event Action OnClose;
    public event Action PostOpen;
    public event Action PostClose;

    private bool isOpen = false;

    private Vector2 initialPosition;
    private Vector2 currPosition;

    private Vector2 upOffset;
    private Vector2 downOffset;
    private Vector2 leftOffset;
    private Vector2 rightOffset;

    private Coroutine animationCoroutine;
    public float AnimationDuration => animationDuration;

    public void ToggleOpenClose()
    {
        if (isOpen) Close();
        else Open();
    }

    private void Close()
    {
        if (!isOpen) return;
        isOpen = false;

        OnClose?.Invoke();
        if (animationCoroutine != null)
            StopCoroutine(animationCoroutine);

        animationCoroutine = StartCoroutine(AnimateUI(false));
    }

    private void Open()
    {
        if (isOpen) return;
        isOpen = true;

        OnOpen?.Invoke();
        if (animationCoroutine != null)
            StopCoroutine(animationCoroutine);

        animationCoroutine = StartCoroutine(AnimateUI(true));
    }

    private IEnumerator AnimateUI(bool open)
    {
        if (open)
        {
            if (PreOpen != null)
            {
                foreach (Func<IEnumerator> handler in PreOpen.GetInvocationList())
                {
                    yield return StartCoroutine(handler());
                }
            }
            if (targetCanvasGroup != null)
                targetObject.SetActive(true);
        }
        if (!open)
        {
            if (PreClose != null)
            {
                foreach (Func<IEnumerator> handler in PreClose.GetInvocationList())
                {
                    yield return StartCoroutine(handler());
                }
            }
        }

        currPosition = targetRectTransform.anchoredPosition;
        float elapsedTime = 0f;
        Vector2 targetPosition = currPosition + GetOffset(open ? openDirection : closeDirection);

        while (elapsedTime < animationDuration)
        {
            float t = easingCurve.Evaluate(elapsedTime / animationDuration);
            targetRectTransform.anchoredPosition = Vector2.Lerp(currPosition, targetPosition, t);

            if (targetCanvasGroup != null)
                targetCanvasGroup.alpha = open ? Mathf.Lerp(0, 1, t) : Mathf.Lerp(1, 0, t);

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        targetRectTransform.anchoredPosition = targetPosition;

        if (targetCanvasGroup != null)
        {
            targetCanvasGroup.alpha = open ? 1 : 0;
            targetCanvasGroup.interactable = open;
            targetCanvasGroup.blocksRaycasts = open;
        }

        if (!open)
        {
            if (targetCanvasGroup != null)
                targetObject.SetActive(false);

            targetRectTransform.anchoredPosition = initialPosition;
            PostClose?.Invoke();
        }
        else
            PostOpen?.Invoke();
    }

    private Vector2 GetOffset(AnimateToDirection direction)
    {
        return direction switch
        {
            AnimateToDirection.Top => upOffset,
            AnimateToDirection.Bottom => downOffset,
            AnimateToDirection.Left => leftOffset,
            AnimateToDirection.Right => rightOffset,
            _ => Vector2.zero,
        };
    }

    private void OnValidate()
    {
        if (targetObject != null)
        {
            targetRectTransform = targetObject.GetComponent<RectTransform>();
            targetCanvasGroup = targetObject.GetComponent<CanvasGroup>();
        }

        distanceToAnimate.x = Mathf.Max(0, distanceToAnimate.x);
        distanceToAnimate.y = Mathf.Max(0, distanceToAnimate.y);
    }

    void Start()
    {
        initialPosition = targetRectTransform.anchoredPosition;
        StartCoroutine(DelayedOffsetInit());
    }

    private IEnumerator DelayedOffsetInit()
    {
        yield return null; // Wait one frame so layout is valid

        LayoutRebuilder.ForceRebuildLayoutImmediate(targetRectTransform);

        float height = targetRectTransform.rect.height * distanceToAnimate.y;
        float width = targetRectTransform.rect.width * distanceToAnimate.x;

        upOffset = new Vector2(0, height);
        downOffset = new Vector2(0, -height);
        leftOffset = new Vector2(-width, 0);
        rightOffset = new Vector2(width, 0);
    }

    void Update()
    {

    }
}
