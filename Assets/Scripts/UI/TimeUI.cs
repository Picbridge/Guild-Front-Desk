using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Localization.SmartFormat.PersistentVariables;
using UnityEngine.Localization;
using System;
using System.Collections;

public class TimeUI : MonoBehaviour
{
    [SerializeField]
    private RectTransform dial;
    [SerializeField]
    private TMP_Text timeOfDayText;
    [SerializeField]
    private TMP_Text dayCountText;

    // Localization
    private LocalizedString localizedDayFormat;
    private IntVariable dayVariable;
    private Dictionary<TimePhase, LocalizedString> timePhaseStrings = new Dictionary<TimePhase, LocalizedString>();
    private LocalizedString currentTimePhaseString;
    private TimePhase lastDisplayedPhase = TimePhase.Morning;

    private Script_TimeManager timeManager;
    private Coroutine rotationCoroutine;

    void Start()
    {
        timeManager = Script_TimeManager.Instance;

        timeManager.OnDayStarted += OnDayStart;
        timeManager.OnSpendTime += OnSpendTime;
        SetupDayLocalization();
        SetupTimePhaseLocalization();

        UpdateTimeDisplay();
        UpdateDayDisplay();
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
    private void OnDayStart()
    {
        dial.localRotation = Quaternion.Euler(0, 0, 0);
        // Update UI with new day
        UpdateDayDisplay();
        UpdateTimeDisplay();
    }

    private void OnSpendTime(float targetDegrees)
    {
        // Stop any previous animation
        if (rotationCoroutine != null)
            StopCoroutine(rotationCoroutine);

        // Start new animation
        rotationCoroutine = StartCoroutine(RotateDialSmoothly(targetDegrees));

        // Update text phase only when needed
        var currentTimePhase = timeManager.GetCurrentTimePhase();
        if (currentTimePhase != lastDisplayedPhase)
        {
            lastDisplayedPhase = currentTimePhase;
            UpdateTimeDisplay();
        }
    }

    private IEnumerator RotateDialSmoothly(float targetDegrees)
    {
        float duration = 0.2f;
        float elapsed = 0f;

        Quaternion startRotation = dial.localRotation;
        Quaternion endRotation = Quaternion.Euler(0, 0, -targetDegrees);

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            dial.localRotation = Quaternion.Lerp(startRotation, endRotation, t);
            yield return null;
        }

        // Ensure final value
        dial.localRotation = endRotation;
    }

    private void UpdateDayCountText(string localizedValue)
    {
        dayCountText.text = localizedValue;
    }

    private void UpdateTimePhaseText(string localizedValue)
    {
        timeOfDayText.text = localizedValue;
    }
    private void UpdateTimeDisplay()
    {
        // Unsubscribe from previous stringChanged event
        if (currentTimePhaseString != null)
        {
            currentTimePhaseString.StringChanged -= UpdateTimePhaseText;
        }

        if (timePhaseStrings.TryGetValue(timeManager.GetCurrentTimePhase(), out LocalizedString localizedString))
        {
            currentTimePhaseString = localizedString;
            currentTimePhaseString.StringChanged += UpdateTimePhaseText;
            currentTimePhaseString.RefreshString();
        }
    }

    private void UpdateDayDisplay()
    {
        dayVariable.Value = timeManager.GetCurrentDayCount();
        localizedDayFormat.RefreshString();
    }
}
