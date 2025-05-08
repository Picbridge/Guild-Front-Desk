using System;
using UnityEngine;
using TMPro;

public class Script_TimeManager : MonoBehaviour
{
    [SerializeField]
    private RectTransform dial;
    [SerializeField]
    private TMP_Text text;

    [SerializeField]
    private float totalDayDuration = 60f;

    public event Action OnDayEnded;
    private float currentTime = 0f;
    private bool isDayRunning = false;

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

        text.text = currentPhase.ToString();


        if (currentTime >= (totalDayDuration * 0.5f))
        {
            EndDay();
        }
    }

    public void StartDay()
    {
        currentTime = 0f;
        isDayRunning = true;
        dial.localRotation = Quaternion.Euler(0, 0, 0);
    }
    public void EndDay()
    {
        isDayRunning = false;
        Debug.Log("Day has ended!");
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
