using UnityEngine;
using TMPro;
using UnityEngine.Localization;
using UnityEngine.Localization.SmartFormat.PersistentVariables;
using System.Collections.Generic;
using System;

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
    private TimePhase currentPhase = TimePhase.Morning;
    private TimePhase lastDisplayedPhase = TimePhase.Morning;

    // Localization
    private LocalizedString localizedDayFormat;
    private IntVariable dayVariable;
    private Dictionary<TimePhase, LocalizedString> timePhaseStrings = new Dictionary<TimePhase, LocalizedString>();
    private LocalizedString currentTimePhaseString;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        SetupDayLocalization();
        SetupTimePhaseLocalization();
    }

    private void SetupDayLocalization()
    {
        dayVariable = new IntVariable();
        localizedDayFormat = new LocalizedString("UI Texts", "ui.main.dayCounter");

        localizedDayFormat.Arguments = new object[] { new { day = dayVariable } };
        localizedDayFormat.StringChanged += UpdateDayCountText;
    }


    private void SetupTimePhaseLocalization()
    {
        // Preload time phase strings but DO NOT subscribe yet
        foreach (TimePhase phase in Enum.GetValues(typeof(TimePhase)))
        {
            LocalizedString localizedString = new LocalizedString("UI Texts", GetTimePhaseKey(phase));
            timePhaseStrings[phase] = localizedString;
        }
    }


    private string GetTimePhaseKey(TimePhase phase)
    {
        return phase switch
        {
            TimePhase.Morning => "ui.main.time.morning",
            TimePhase.Afternoon => "ui.main.time.afternoon",
            TimePhase.Evening => "ui.main.time.evening",
            _ => "ui.main.time.unknown"
        };
    }

    private void UpdateDayCountText(string localizedValue)
    {
        dayCountText.text = localizedValue;
    }

    private void UpdateTimePhaseText(string localizedValue)
    {
        timeOfDayText.text = localizedValue;
    }

    void Start()
    {
        currentTime = 0f;
        isDayRunning = true;
        UpdateTimeDisplay();
        UpdateDayDisplay();
    }

    public void Update()
    {
        if (!isDayRunning)
            return;

        currentTime += Time.deltaTime;

        float progress = Mathf.Clamp01(currentTime / totalDayDuration);
        dial.localRotation = Quaternion.Euler(0, 0, Mathf.Lerp(0, -360, progress));

        // Update time phase if it changed
        currentPhase = GetCurrentTimePhase();
        if (currentPhase != lastDisplayedPhase)
        {
            lastDisplayedPhase = currentPhase;
            UpdateTimeDisplay();
        }

        if (currentTime >= (totalDayDuration * 0.5f))
        {
            EndDay();
        }
    }

    private void UpdateTimeDisplay()
    {
        // Unsubscribe from previous stringChanged event
        if (currentTimePhaseString != null)
        {
            currentTimePhaseString.StringChanged -= UpdateTimePhaseText;
        }

        if (timePhaseStrings.TryGetValue(currentPhase, out LocalizedString localizedString))
        {
            currentTimePhaseString = localizedString;
            currentTimePhaseString.StringChanged += UpdateTimePhaseText;
            currentTimePhaseString.RefreshString();
        }
    }


    private void UpdateDayDisplay()
    {
        dayVariable.Value = dayCount;
        localizedDayFormat.RefreshString();
    }

    public void StartDay()
    {
        dayCount++;
        currentTime = 0f;
        isDayRunning = true;
        dial.localRotation = Quaternion.Euler(0, 0, 0);
        Script_AdventurerManager.Instance.UpdateAvailableAdventurers();

        // Update UI with new day
        UpdateDayDisplay();
        UpdateTimeDisplay();
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

        // Update time phase if it changed after spending time
        TimePhase newPhase = GetCurrentTimePhase();
        if (newPhase != lastDisplayedPhase)
        {
            currentPhase = newPhase;
            lastDisplayedPhase = newPhase;
            UpdateTimeDisplay();
        }
    }
}
