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
    
    public event Action<AdventurerData> OnAdventurerCreated;
    public event Action<AdventurerData> OnAdventurerStatusChanged;
    
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
            OnAdventurerCreated?.Invoke(adventurerData);
        }
    }
    
    public void MarkAdventurerAsBusy(string adventurerId)
    {
        if (adventurerDatabase.TryGetValue(adventurerId, out AdventurerData adventurer))
        {
            adventurer.currentState = GFD.Adventurer.State.InQuest;
            availableAdventurers.Remove(adventurer);
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
            adventurer.injuryStatus = InjuryStatus.Dead;
            availableAdventurers.Remove(adventurer);
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

    public void UpdateAvailableAdventurers()
    {
        availableAdventurers = adventurerDatabase.Values.Where(adventurer => adventurer.currentState == GFD.Adventurer.State.Resting).ToList();
    }
}
