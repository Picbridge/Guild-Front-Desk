using UnityEngine;
using System;

public class Script_TimeManager : MonoBehaviour
{
    public static Script_TimeManager Instance { get; private set; }
    private Script_QuestManager questManager;

    public event Action OnDayEnded;
    private float currentTime = 0f;
    private int dayCount = 0;
    private TimePhase currentPhase = TimePhase.Morning;

    private float totalDayDuration = 60f;
    private float totalWorkHours;

    public event Action OnDayStarted;
    public event Action<float> OnSpendTime;

    private bool isDayRunning = false;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);


        totalWorkHours = 0.5f * totalDayDuration; // 50% of the day is work time
    }

    void Start()
    {
        currentTime = 0f;
        questManager = Script_QuestManager.Instance;
        questManager.OnDailyQuestUpdated += SetWorkHours;
    }

    public void Update()
    {
        //currentTime += Time.deltaTime;
        if (currentTime >= totalWorkHours)
        {
            EndDay();
        }
    }
    public void SetWorkHours()
    {
        var workHours = questManager.CurrentDailyQuestCount;
        totalWorkHours = workHours;
        totalDayDuration = workHours * 2f;
    }
    public void StartDay()
    {
        if (isDayRunning)
            return;

        dayCount++;
        currentTime = 0f;
        isDayRunning = true;
        OnDayStarted?.Invoke();
    }

    public void EndDay()
    {
        if (!isDayRunning)
            return;
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
    public float GetCurrentTime()
    {
        return currentTime;
    }
    public int GetCurrentDayCount()
    {
        return dayCount;
    }

    public void SpendTime()
    {
        if (currentTime >= totalWorkHours)
            return;
        currentTime ++;
        // Update time phase
        currentPhase = GetCurrentTimePhase();
        float progress = Mathf.Clamp01(currentTime / totalWorkHours) * 180f;
        OnSpendTime?.Invoke(progress);
    }
}
