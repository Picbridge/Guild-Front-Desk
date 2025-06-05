using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using GFD.Quest;
using Random = UnityEngine.Random;
using GFD.Adventurer;

public class Script_AdventurerManager : MonoBehaviour
{
    public static Script_AdventurerManager Instance { get; private set; }
    
    private Dictionary<string, AdventurerData> adventurerDatabase = new Dictionary<string, AdventurerData>();
    private List<AdventurerData> availableAdventurers = new List<AdventurerData>();
    private List<AdventurerData> busyAdventurers = new List<AdventurerData>();
    private List<AdventurerData> deadAdventurers = new List<AdventurerData>();

    public event Action<AdventurerData> OnAdventurerCreated;
    public event Action<AdventurerData> OnAdventurerStatusChanged;

    private Script_QuestManager questManager;
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
    
    private void Start()
    {
        Script_TimeManager.Instance.OnDayStarted += UpdateAvailableAdventurers;
        questManager = Script_QuestManager.Instance;

    }

    public AdventurerData GetAdventurer(string adventurerId)
    {
        if (adventurerDatabase.TryGetValue(adventurerId, out AdventurerData adventurer))
        {
            return adventurer;
        }
        return null;
    }

    public void AddAdventurer(AdventurerData adventurerData)
    {
        if (!adventurerDatabase.ContainsKey(adventurerData.adventurerId))
        {
            adventurerDatabase.Add(adventurerData.adventurerId, adventurerData);
            MarkAdventurerAsAvailable(adventurerData.adventurerId);
            OnAdventurerCreated?.Invoke(adventurerData);
        }
    }
    
    public void MarkAdventurerAsBusy(string adventurerId)
    {
        if (adventurerDatabase.TryGetValue(adventurerId, out AdventurerData adventurer))
        {
            adventurer.currentState = GFD.Adventurer.State.InQuest;
            availableAdventurers.Remove(adventurer);
            busyAdventurers.Add(adventurer);
            OnAdventurerStatusChanged?.Invoke(adventurer);
        }
    }
    
    public void MarkAdventurerAsAvailable(string adventurerId)
    {
        if (adventurerDatabase.TryGetValue(adventurerId, out AdventurerData adventurer))
        {
            adventurer.currentState = GFD.Adventurer.State.Resting;
            if (!availableAdventurers.Contains(adventurer))
            {
                availableAdventurers.Add(adventurer);
            }
            OnAdventurerStatusChanged?.Invoke(adventurer);
        }
    }
    
    public void MarkAdventurerAsDead(string adventurerId)
    {
        if (adventurerDatabase.TryGetValue(adventurerId, out AdventurerData adventurer))
        {
            adventurer.currentState = State.Dead;
            availableAdventurers.Remove(adventurer);
            deadAdventurers.Add(adventurer);
            OnAdventurerStatusChanged?.Invoke(adventurer);
        }
    }
    public AdventurerData GetAdventurerById(string id)
    {
        return adventurerDatabase.TryGetValue(id, out AdventurerData adventurer) ? adventurer : null;
    }

    public List<AdventurerData> GetAvailableAdventurers()
    {
        return availableAdventurers;
    }

    public void RemoveFromAvailableAdventurers(AdventurerData adventurer)
    {
        if (availableAdventurers.Contains(adventurer))
        {
            availableAdventurers.Remove(adventurer);
        }
    }

    public AdventurerData GetCompletedAdventurer()
    {
        // Find the first adventurer in the busyAdventurers list with duration <= 0
        AdventurerData completedAdventurer = busyAdventurers.FirstOrDefault(adventurer => adventurer.questDuration <= 0);
        if (completedAdventurer != null)
        {
            busyAdventurers.Remove(completedAdventurer);
            return completedAdventurer;
        }
        return null;
    }

    // Update the status of adventurers at the start of each day
    public void UpdateAvailableAdventurers()
    {
        foreach (var adventurer in busyAdventurers.ToList())
        {
            adventurer.questDuration -= 1f;
        }
        availableAdventurers = adventurerDatabase.Values.Where(adventurer => adventurer.currentState == GFD.Adventurer.State.Resting).ToList();
        busyAdventurers = adventurerDatabase.Values.Where(adventurer => adventurer.currentState == GFD.Adventurer.State.InQuest).ToList();
        deadAdventurers = adventurerDatabase.Values.Where(adventurer => adventurer.currentState == GFD.Adventurer.State.Dead).ToList();
    }
}
