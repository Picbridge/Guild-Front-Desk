using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using System.Linq;

public class ZoomableMapUI : MonoBehaviour
{
    [Header("Map")]
    [SerializeField]
    private RectTransform mapViewport;
    [SerializeField]
    private RectTransform mapContent;
    [SerializeField]
    private float zoomSpeed = 0.1f;
    [SerializeField]
    private float minZoom = 0.5f;
    [SerializeField]
    private float maxZoom = 2f;
    [SerializeField]
    private float zoomSmoothTime = 0.2f;

    [Header("Zoom Display")]
    [SerializeField]
    private TextMeshProUGUI zoomText;
    [SerializeField]
    private float zoomOutOffset = 0.22f;

    [Header("Zoom and Focus Settings")]
    [SerializeField, Range(0f, 1f)]
    private float listFocusZoomPercent = 0.35f; // Previously 0.35f
    [SerializeField, Range(0f, 1f)]
    private float directFocusZoomPercent = 0.6f; // Previously 0.6f
    [SerializeField, Range(0f, 1f)]
    private float directFocusOffsetY = 0.57f; // Previously 0.57f
    [SerializeField]
    private float smoothPanAnimDuration = 0.8f; // Previously 0.8f
    [SerializeField]
    private float directPanAnimDuration = 0.5f; // Previously 0.5f
    [SerializeField]
    private float panEaseMultiplier = 1.6667f; // Previously 1.6667f

    [Header("MapToggle")]
    [SerializeField]
    private Button mapToggleButton;
    [SerializeField]
    private Button closeButton;

    [Header("Location Buttons")]
    [SerializeField]
    private Transform locationButtonsParent;
    [SerializeField]
    private Transform locationListParent;

    [Header("List Button Transition Colors")]
    [SerializeField]
    private ColorBlock listButtonTransition;

    // Threshold constants
    private const float ZOOM_EPSILON = 0.001f;
    private const float SCROLL_THRESHOLD = 0.001f;
    private const float HALF_FACTOR = 0.5f;
    private const float PERCENT_MULTIPLIER = 100f;

    private float minPanIntentDistance = 15.0f;
    private float minPanThreshold = 3.0f;
    private float positionSimilarityThreshold = 1.0f;

    // Basic map variables
    private Vector2 dragOrigin;
    private bool dragging;
    private float currentZoom = 1f;
    private float targetZoom = 1f;
    private float zoomVelocity;
    private Vector2 zoomPivot;
    private bool isZooming;
    private Camera uiCamera;
    private bool mapIsActive = false;
    private bool previousMapIsActive = false;
    // Animation helpers
    private UI_TweenHelper buttonTween;
    private UI_TweenHelper tweenHelper;

    // Event that fires when zoom changes
    public event System.Action<float> OnZoomChanged;
    private Coroutine currentPanAndZoomCoroutine;
    private Vector2? currentTargetPosition = null;
    private bool isInteractingWithUI = false;
    private Vector2 panStartPosition;

    private void Start()
    {
        StartCoroutine(InitAfterInspector());
    }

    IEnumerator InitAfterInspector()
    {
        yield return null;
        tweenHelper = GetComponent<UI_TweenHelper>();
        buttonTween = mapToggleButton.GetComponent<UI_TweenHelper>();

        Canvas canvas = GetComponentInParent<Canvas>();
        if (canvas != null && canvas.renderMode != RenderMode.ScreenSpaceOverlay)
        {
            uiCamera = canvas.worldCamera;
        }

        targetZoom = currentZoom;

        buttonTween.PostOpen += tweenHelper.ToggleOpenClose;
        tweenHelper.PostClose += buttonTween.ToggleOpenClose;
        tweenHelper.OnOpen += () => mapIsActive = true;
        tweenHelper.PostClose += () => mapIsActive = false;
        mapToggleButton.onClick.AddListener(() =>
        { 
            buttonTween.ToggleOpenClose();

        });

        closeButton.onClick.AddListener(() =>
        {
            CloseMap();
        });

        // Setup location buttons
        if (locationButtonsParent == null)
            locationButtonsParent = mapContent;

        InitializeLocationButtons();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.L))
        {
            tweenHelper.ToggleOpenClose();
        }

        if (previousMapIsActive != mapIsActive)
        {
            mapViewport.gameObject.SetActive(mapIsActive);
            previousMapIsActive = mapIsActive;
        }
        // Check for clicks outside viewport to close map
        if (mapIsActive && Input.GetMouseButtonDown(0))
        {
            bool clickedInsideViewport = RectTransformUtility.RectangleContainsScreenPoint(
                mapViewport, Input.mousePosition, uiCamera);

            if (!clickedInsideViewport)
            {
                CloseMap();
                return;
            }
        }

        HandleZoomInput();
        HandleSmoothZoom();
        HandlePan();
        UpdateZoomText();
    }

    #region Map Basic Functionality
    private bool IsPointerOverUIElement()
    {
        // Check if the pointer is over any UI element
        if (EventSystem.current.IsPointerOverGameObject())
        {
            // Get the object under the pointer
            PointerEventData eventData = new PointerEventData(EventSystem.current);
            eventData.position = Input.mousePosition;
            List<RaycastResult> results = new List<RaycastResult>();
            EventSystem.current.RaycastAll(eventData, results);

            // Check if any of the hit objects are interactive UI elements (buttons, input fields, etc.)
            foreach (RaycastResult result in results)
            {
                // Check for common interactive UI components
                if (result.gameObject.GetComponent<Button>() != null ||
                    result.gameObject.GetComponent<InputField>() != null ||
                    result.gameObject.GetComponent<Dropdown>() != null ||
                    result.gameObject.GetComponent<Toggle>() != null ||
                    result.gameObject.GetComponent<Scrollbar>() != null ||
                    result.gameObject.GetComponent<ScrollRect>() != null ||
                    result.gameObject.GetComponent<TMP_InputField>() != null)
                {
                    return true;
                }
            }
        }
        return false;
    }
    private void CloseMap()
    {
        tweenHelper.ToggleOpenClose();
    }

    void HandleZoomInput()
    {
        // Don't handle zoom if interacting with UI
        if (isInteractingWithUI || IsPointerOverUIElement())
            return;

        if (!RectTransformUtility.RectangleContainsScreenPoint(mapViewport, Input.mousePosition, uiCamera))
            return;

        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (Mathf.Abs(scroll) > SCROLL_THRESHOLD)
        {
            targetZoom = Mathf.Clamp(targetZoom + scroll * zoomSpeed * currentZoom, minZoom, maxZoom);
            zoomPivot = Input.mousePosition;
            isZooming = true;
        }
    }

    void HandleSmoothZoom()
    {
        if (Mathf.Approximately(currentZoom, targetZoom))
        {
            isZooming = false;
            return;
        }

        currentZoom = Mathf.SmoothDamp(currentZoom, targetZoom, ref zoomVelocity, zoomSmoothTime);

        if (Mathf.Abs(currentZoom - targetZoom) < ZOOM_EPSILON)
        {
            currentZoom = targetZoom;
            isZooming = false;
        }

        if (isZooming)
        {
            ApplyZoomAroundPivot();
        }
    }

    void ApplyZoomAroundPivot()
    {
        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(
            mapViewport, zoomPivot, uiCamera, out Vector2 mouseViewportPos))
            return;

        Vector2 mouseContentPosBefore = mouseViewportPos - mapContent.anchoredPosition;
        float oldZoom = mapContent.localScale.x;

        mapContent.localScale = Vector3.one * currentZoom;

        float scaleFactor = currentZoom / oldZoom;
        Vector2 mouseDelta = mouseContentPosBefore * (scaleFactor - 1);

        mapContent.anchoredPosition -= mouseDelta;

        ClampToViewport();

        // Notify listeners that zoom has changed
        OnZoomChanged?.Invoke(currentZoom);
    }

    void HandlePan()
    {
        // Track UI interaction state when mouse is pressed down
        if (Input.GetMouseButtonDown(0))
        {
            isInteractingWithUI = IsPointerOverUIElement();

            // If interacting with UI, don't start dragging
            if (isInteractingWithUI)
                return;

            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
                mapViewport, Input.mousePosition, uiCamera, out dragOrigin))
            {
                dragging = true;
                // Store the starting position to track total pan distance
                panStartPosition = mapContent.anchoredPosition;
            }
        }
        else if (Input.GetMouseButtonUp(0))
        {
            // Check if we've panned enough to count as intentional movement
            if (dragging)
            {
                float totalPanDistance = Vector2.Distance(panStartPosition, mapContent.anchoredPosition);
                if (totalPanDistance > minPanIntentDistance)
                {
                    // Only clear the target position if we've moved a significant amount
                    currentTargetPosition = null;
                }
            }

            dragging = false;
            isInteractingWithUI = false;  // Reset UI interaction state
        }

        // Only handle pan if we're dragging and not interacting with UI
        if (dragging && Input.GetMouseButton(0) && !isInteractingWithUI)
        {
            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
                mapViewport, Input.mousePosition, uiCamera, out Vector2 currentMousePos))
            {
                Vector2 delta = currentMousePos - dragOrigin;

                // Only consider it a pan if movement exceeds threshold
                if (delta.magnitude > minPanThreshold)
                {
                    mapContent.anchoredPosition += delta;
                    dragOrigin = currentMousePos;

                    ClampToViewport();
                }
            }
        }
    }

    void ClampToViewport()
    {
        Vector2 viewportSize = mapViewport.rect.size;
        Vector2 contentSize = mapContent.rect.size * currentZoom;

        if (contentSize.x < viewportSize.x)
        {
            mapContent.anchoredPosition = new Vector2(0, mapContent.anchoredPosition.y);
        }
        else
        {
            float maxOffsetX = (contentSize.x - viewportSize.x) * HALF_FACTOR;
            mapContent.anchoredPosition = new Vector2(
                Mathf.Clamp(mapContent.anchoredPosition.x, -maxOffsetX, maxOffsetX),
                mapContent.anchoredPosition.y);
        }

        if (contentSize.y < viewportSize.y)
        {
            mapContent.anchoredPosition = new Vector2(mapContent.anchoredPosition.x, 0);
        }
        else
        {
            float maxOffsetY = (contentSize.y - viewportSize.y) * HALF_FACTOR;
            mapContent.anchoredPosition = new Vector2(
                mapContent.anchoredPosition.x,
                Mathf.Clamp(mapContent.anchoredPosition.y, -maxOffsetY, maxOffsetY));
        }
    }

    void UpdateZoomText()
    {
        if (zoomText != null)
        {
            float percent = (currentZoom - minZoom) / (maxZoom - minZoom) * PERCENT_MULTIPLIER;
            zoomText.text = $"Zoom: {percent:0}%";
        }
    }
    #endregion

    #region Location Button Functionality

    public void InitializeLocationButtons()
    {
        // for children of locationButtonsParent, create a button for each location
        if (locationButtonsParent == null || locationListParent == null)
        {
            Debug.LogError("Location buttons parent or list parent is not set.");
            return;
        }

        // Create buttons for each location
        foreach (Transform locationPanel in locationButtonsParent)
        {
            var buttonObj = locationPanel.GetComponentInChildren<LocationButtonUI>().gameObject.transform;
            var pos = ConvertToNormalizedPositionFromAnchored(locationPanel.GetComponent<RectTransform>().anchoredPosition);
            buttonObj.GetComponentInChildren<TextMeshProUGUI>().text = locationPanel.GetComponent<LocationDescription>().LocationDetail.locationName;
            var description = locationPanel.GetComponentInChildren<ScrollRect>().gameObject.transform;
            description.GetComponentInChildren<TextMeshProUGUI>().text = locationPanel.GetComponent<LocationDescription>().LocationDetail.locationDescription;

            var name = locationPanel.Find("Description/Name");
            name.GetComponentInChildren<TextMeshProUGUI>().text = locationPanel.GetComponent<LocationDescription>().LocationDetail.locationName;

            // Map button - direct focus without two-phase zoom
            buttonObj.GetComponent<Button>().onClick.AddListener(() =>
            {
                buttonObj.GetComponent<LocationButtonUI>().ToggleDescription();
                DirectFocusOnLocation(pos);
            });

            var listButton = Instantiate(buttonObj, locationListParent);
            var originalColors = buttonObj.GetComponent<Button>().colors;
            // Set button transition normal color to transparent
            listButton.GetComponent<Button>().colors = listButtonTransition;

            // List button - focus with two-phase zoom animation
            listButton.gameObject.GetComponent<Button>().onClick.AddListener(() =>
            {
                FocusOnLocation(pos);
            });
        }
    }

    public Vector2 ConvertToNormalizedPositionFromAnchored(Vector2 anchoredPosition)
    {
        Vector2 mapSize = mapContent.rect.size;

        // We don't need to adjust for zoom here as these are local coordinates
        // relative to the unscaled RectTransform
        return new Vector2(
            (anchoredPosition.x + mapSize.x * HALF_FACTOR) / mapSize.x,
            (anchoredPosition.y + mapSize.y * HALF_FACTOR) / mapSize.y
        );
    }

    public void FocusOnLocation(Vector2 normalizedPosition)
    {
        // If an animation is running, ignore this request
        if (currentPanAndZoomCoroutine != null)
        {
            Debug.Log("Animation in progress, ignoring focus request");
            return;
        }

        Vector2 mapSize = mapContent.rect.size;
        float targetZoomLevel = minZoom + (maxZoom - minZoom) * listFocusZoomPercent;

        Vector2 targetPos = new Vector2(
            (normalizedPosition.x * mapSize.x) - (mapSize.x * HALF_FACTOR),
            (normalizedPosition.y * mapSize.y) - (mapSize.y * HALF_FACTOR)
        );

        // Apply the inverse of the target position and multiply by the TARGET zoom
        Vector2 newAnchoredPos = -targetPos * targetZoomLevel;

        // Check if we're already at this position and haven't panned
        if (currentTargetPosition.HasValue)
        {
            // Check if this is a focus request for the same position (with some tolerance)
            if (Vector2.Distance(newAnchoredPos, currentTargetPosition.Value) < positionSimilarityThreshold)
            {
                Debug.Log("Already focused on this location. Pan to enable focusing here again.");
                return;
            }
        }

        // This is a new target position, so we'll allow focusing
        currentTargetPosition = newAnchoredPos;

        // Start combined pan and zoom animation
        currentPanAndZoomCoroutine = StartCoroutine(SmoothPanAndZoom(newAnchoredPos, targetZoomLevel));
    }

    public void DirectFocusOnLocation(Vector2 normalizedPosition)
    {
        // If an animation is running, ignore this request
        if (currentPanAndZoomCoroutine != null)
        {
            Debug.Log("Animation in progress, ignoring focus request");
            return;
        }

        Vector2 mapSize = mapContent.rect.size;
        float targetZoomLevel = minZoom + (maxZoom - minZoom) * directFocusZoomPercent;

        Vector2 targetPos = new Vector2(
            (normalizedPosition.x * mapSize.x) - (mapSize.x * HALF_FACTOR),
            (normalizedPosition.y * mapSize.y) - (mapSize.y * directFocusOffsetY)
        );

        // Apply the inverse of the target position and multiply by the TARGET zoom
        Vector2 newAnchoredPos = -targetPos * targetZoomLevel;

        // This is a new target position, so we'll allow focusing
        currentTargetPosition = newAnchoredPos;

        // Start direct pan and zoom animation without two-phase zoom effect
        currentPanAndZoomCoroutine = StartCoroutine(DirectSmoothPanAndZoom(newAnchoredPos, targetZoomLevel));
    }

    // Smooth panning and zooming animation with slight zoom out effect
    private IEnumerator SmoothPanAndZoom(Vector2 targetPosition, float targetZoomLevel)
    {
        try
        {
            Vector2 startPosition = mapContent.anchoredPosition;
            float startZoom = currentZoom;

            // Only zoom out if not at minZoom
            float arcB = (startZoom > minZoom)
                ? Mathf.Clamp(startZoom + zoomOutOffset, minZoom, maxZoom)
                : startZoom;

            float elapsedTime = 0f;

            zoomPivot = new Vector2(Screen.width * HALF_FACTOR, Screen.height * HALF_FACTOR);

            while (elapsedTime < smoothPanAnimDuration)
            {
                elapsedTime += Time.deltaTime;
                float t = Mathf.Clamp01(elapsedTime / smoothPanAnimDuration);

                // Quadratic Bézier for convex arc: (startZoom, arcB, targetZoomLevel)
                float oneMinusT = 1f - t;
                float bezierZoom = oneMinusT * oneMinusT * startZoom + 2f * oneMinusT * t * arcB + t * t * targetZoomLevel;
                currentZoom = bezierZoom;

                mapContent.localScale = Vector3.one * currentZoom;

                // Pan uses a smooth ease
                float panT = 1 - Mathf.Pow(1 - t, 2) * panEaseMultiplier;
                panT = Mathf.Clamp01(panT);
                Vector2 intermediatePosition = Vector2.Lerp(startPosition, targetPosition, panT);
                mapContent.anchoredPosition = intermediatePosition;

                ClampToViewport();
                OnZoomChanged?.Invoke(currentZoom);

                yield return null;
            }

            // Ensure we end at exact values
            currentZoom = targetZoomLevel;
            mapContent.localScale = Vector3.one * currentZoom;
            mapContent.anchoredPosition = targetPosition;
            ClampToViewport();
            targetZoom = targetZoomLevel;
            OnZoomChanged?.Invoke(currentZoom);
        }
        finally
        {
            currentPanAndZoomCoroutine = null;
        }
    }


    private IEnumerator DirectSmoothPanAndZoom(Vector2 targetPosition, float targetZoomLevel)
    {
        try
        {
            Vector2 startPosition = mapContent.anchoredPosition;
            float startZoom = currentZoom;
            float elapsedTime = 0f;

            // Set the center of the zoom pivot to screen center
            zoomPivot = new Vector2(Screen.width * HALF_FACTOR, Screen.height * HALF_FACTOR);

            while (elapsedTime < directPanAnimDuration)
            {
                elapsedTime += Time.deltaTime;
                float t = Mathf.Clamp01(elapsedTime / directPanAnimDuration);

                // Cubic ease-out for smooth deceleration
                float easedT = 1 - Mathf.Pow(1 - t, 3);

                // Direct interpolation to target zoom without phases
                currentZoom = Mathf.Lerp(startZoom, targetZoomLevel, easedT);

                // Apply the zoom
                mapContent.localScale = Vector3.one * currentZoom;

                // Use the same easing for position
                Vector2 intermediatePosition = Vector2.Lerp(startPosition, targetPosition, easedT);
                mapContent.anchoredPosition = intermediatePosition;

                // Ensure the map stays within bounds
                ClampToViewport();

                // Notify zoom changed
                OnZoomChanged?.Invoke(currentZoom);

                yield return null;
            }

            // Ensure we end at exact values
            currentZoom = targetZoomLevel;
            mapContent.localScale = Vector3.one * currentZoom;
            mapContent.anchoredPosition = targetPosition;
            ClampToViewport();

            // Update target zoom to match our new zoom level
            targetZoom = targetZoomLevel;

            // Final notification of zoom change
            OnZoomChanged?.Invoke(currentZoom);
        }
        finally
        {
            // Clear the coroutine reference
            currentPanAndZoomCoroutine = null;
        }
    }
    #endregion
}
