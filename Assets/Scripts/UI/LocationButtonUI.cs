using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

public class LocationButtonUI : MonoBehaviour
{
    [SerializeField]
    private RectTransform descriptionRect;
    [SerializeField]
    private float duration = 0.2f;

    static int topSortingOrder = 100;        // global counter

    Canvas descCanvas;
    bool isOpen;
    float maxHeight = 60f;
    private Coroutine animationCoroutine = null;  // Track active coroutine

    private const float SCROLL_THRESHOLD = 0.001f;
    void Awake()
    {
        // add (or cache) a Canvas on the description once
        descCanvas = descriptionRect.GetComponent<Canvas>();
        if (descCanvas == null)
        {
            descCanvas = descriptionRect.gameObject.AddComponent<Canvas>();
            descCanvas.overrideSorting = true;
            descriptionRect.gameObject.AddComponent<UnityEngine.UI.GraphicRaycaster>();
        }

        GetComponent<UnityEngine.UI.Button>().onClick.AddListener(OnButtonClick);
    }

    public void ToggleDescription()
    {
        // If animation is in progress, ignore this click
        if (animationCoroutine != null)
            return;

        isOpen = !isOpen;

        if (isOpen)
        {
            // bring this panel to the very top
            descCanvas.sortingOrder = ++topSortingOrder;
        }

        animationCoroutine = StartCoroutine(AnimateHeight(isOpen ? maxHeight : 0));
    }

    void OnButtonClick()
    {
        
    }

    IEnumerator AnimateHeight(float target)
    {
        float speed = maxHeight / duration;

        while (!Mathf.Approximately(descriptionRect.sizeDelta.y, target))
        {
            float newH = Mathf.MoveTowards(descriptionRect.sizeDelta.y, target, speed * Time.deltaTime);
            descriptionRect.sizeDelta = new Vector2(descriptionRect.sizeDelta.x, newH);
            yield return null;
        }

        descriptionRect.sizeDelta = new Vector2(descriptionRect.sizeDelta.x, target);

        animationCoroutine = null;
    }

    void Update()
    {
        if (!isOpen) return;

        // Close the description if the mouse scrolls or clicks outside of it
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if ((Mathf.Abs(scroll) > SCROLL_THRESHOLD || Input.GetMouseButtonDown(0)) &&
            !RectTransformUtility.RectangleContainsScreenPoint(descriptionRect, Input.mousePosition) &&
            animationCoroutine == null)  // Only respond if no animation is running
        {
            isOpen = false;
            animationCoroutine = StartCoroutine(AnimateHeight(0));
        }
    }
}
