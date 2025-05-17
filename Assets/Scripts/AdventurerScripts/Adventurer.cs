using UnityEngine;
using System;
using Random = UnityEngine.Random;
using GFD.Adventurer;

public class Adventurer : MonoBehaviour
{
    public AdventurerData data;
    private void Awake()
    {
        data = new AdventurerData();
    }
    public void GenerateRandomAdventurer()
    {
        Sprite[] availablePortraits = Resources.LoadAll<Sprite>(GlobalConstants.spriteDirectory);
        Sprite randomPortrait;
        if (availablePortraits != null && availablePortraits.Length > 0)
        {
            randomPortrait = availablePortraits[Random.Range(0, availablePortraits.Length)];
        }
        else
        {
            Debug.LogWarning("No portraits found in Resources/Sprites/Portraits folder!");
            randomPortrait = null;
        }

        data.adventurerId = Guid.NewGuid().ToString();
        data.adventurerName = GenerateRandomName();

        data.gender = GetRandom<Gender>();
        data.race = GetRandom<Race>();
        data.adventurerClass = GetRandom<Class>();
        data.personality = GetRandom<Personality>();
        data.rank = GetRandom<Rank>();
        data.background = GetRandom<Background>();
        data.mood = GetRandom<Mood>();
        data.injuryStatus = InjuryStatus.Healthy;
        data.currentState = State.Resting;

        data.strength = Random.Range(1, 20);
        data.agility = Random.Range(1, 20);
        data.intelligence = Random.Range(1, 20);
        data.endurance = Random.Range(1, 20);
        data.experience = Random.Range(0, 100);

        data.isSponsored = false;
        data.questCount = 0;
        data.currQuestID = -1;

        data.portrait = randomPortrait.name;
    }

    public void EndQuest(int reward = 0)
    {
        data.questCount++;
        GetReward(reward);
    }

    private void GetReward(int reward)
    {
        data.experience += reward;

        if (data.experience >= 100 * ((int)data.rank + 1))
        {
            data.experience = 0;
            if (data.rank < Rank.Adamantium)
                data.rank++;
        }
    }
    private string GenerateRandomName()
    {
        // Implement your name generation logic here
        return "Adventurer " + Random.Range(1, 1000);
    }

    private T GetRandom<T>()
    {
        var values = System.Enum.GetValues(typeof(T));
        return (T)values.GetValue(Random.Range(0, values.Length));
    }

    public string GetPrompt()
    {
        return $"You are {data.adventurerName}, a {data.gender} {data.race} {data.adventurerClass}" +
               $"Personality: {data.personality}" +
               $"Rank: {data.rank}" +
               $"Background: {data.background}" +
               $"Feelings: {data.mood}" +
               $"Abilities:" +
               $"- Strength: {data.strength}" +
               $"- Agility: {data.agility}" +
               $"- Intelligence: {data.intelligence}" +
               $"- Endurance: {data.endurance}" +
               $"ONLY respond with spoken dialogue in quotation marks (\"\")" +
               $"and do not include any other text. Do not use any special characters or emojis." +
               $"Do not include any narration, actions, or inner thoughts." +
               $"Speak in a way that reflects your personality, background, and current feelings. " +
               $"Avoid being generic. Stay in character.";
    }

    public void CopyFrom(AdventurerData availableAdventurer)
    {
        data.adventurerId = availableAdventurer.adventurerId;
        data.adventurerName = availableAdventurer.adventurerName;
        data.gender = availableAdventurer.gender;
        data.race = availableAdventurer.race;
        data.adventurerClass = availableAdventurer.adventurerClass;
        data.personality = availableAdventurer.personality;
        data.rank = availableAdventurer.rank;
        data.background = availableAdventurer.background;
        data.strength = availableAdventurer.strength;
        data.agility = availableAdventurer.agility;
        data.intelligence = availableAdventurer.intelligence;
        data.endurance = availableAdventurer.endurance;
        data.experience = availableAdventurer.experience;
        data.isSponsored = availableAdventurer.isSponsored;
        data.questCount = availableAdventurer.questCount;
        data.currQuestID = availableAdventurer.currQuestID;
        data.currentState = availableAdventurer.currentState;
        data.injuryStatus = availableAdventurer.injuryStatus;
        data.mood = availableAdventurer.mood;
        data.portrait = availableAdventurer.portrait;
    }
}


