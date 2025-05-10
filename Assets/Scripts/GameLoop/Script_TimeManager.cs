using System;
using UnityEngine;
using TMPro;

public class Script_TimeManager : MonoBehaviour
{
    public static Script_TimeManager Instance { get; private set; }

    [SerializeField]
    private RectTransform dial;
    [SerializeField]
    private TMP_Text timeOfDayText;
    [SerializeField]
    private TMP_Text dayCountText;

    [SerializeField]
    private float totalDayDuration = 60f;

    public event Action OnDayEnded;
    private float currentTime = 0f;
    private bool isDayRunning = false;
    private int dayCount = 0;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject); 
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject); 
    }

    void Start()
    {
        currentTime = 0f;
        isDayRunning = true;
    }

    // Update is called once per frame
    public void Update()
    {
        if (!isDayRunning)
            return;

        currentTime += Time.deltaTime;

        float progress = Mathf.Clamp01(currentTime / totalDayDuration);
        dial.localRotation = Quaternion.Euler(0, 0, Mathf.Lerp(0, -360, progress));

        // Update text field with current time phase
        TimePhase currentPhase = GetCurrentTimePhase();

        timeOfDayText.text = currentPhase.ToString();
        dayCountText.text = "Day " + dayCount.ToString();

        if (currentTime >= (totalDayDuration * 0.5f))
        {
            EndDay();
        }
    }

    public void StartDay()
    {
        dayCount++;
        currentTime = 0f;
        isDayRunning = true;
        dial.localRotation = Quaternion.Euler(0, 0, 0);
        Script_AdventurerManager.Instance.UpdateAvailableAdventurers();
    }
    public void EndDay()
    {
        isDayRunning = false;
        OnDayEnded?.Invoke();
    }

    public TimePhase GetCurrentTimePhase()
    {
        if (currentTime < totalDayDuration * 0.125f)
            return TimePhase.Morning;
        else if (currentTime < totalDayDuration * 0.375f)
            return TimePhase.Afternoon;
        else
            return TimePhase.Evening;
    }

    public void SpendTime(float seconds)
    {
        currentTime += seconds;
    }
}
