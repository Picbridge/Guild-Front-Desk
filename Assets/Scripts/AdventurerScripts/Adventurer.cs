using UnityEngine;
using System;
using Random = UnityEngine.Random;
using GFD.Adventurer;
using System.Collections.Generic;
using UnityEngine.UIElements;
using UnityEngine.Localization.Settings;
using System.Linq;


public class Adventurer : MonoBehaviour
{

    public AdventurerData data;

    private void Awake()
    {
        data = new AdventurerData();
    }
    public void GenerateRandomAdventurer()
    {
        var race = GetRandom<Race>();
        var gender = GetRandom<Gender>();
        var rank = GetRandom<Rank>();
        var background = GetRandom<AdventurerBackground>();

        var adventurerClass = GetWeightedRandom(raceClassWeights[race]);
        var personality = Random.value < 0.9f? GetWeightedRandom(backgroundPersonalityWeights[background]) : GetRandom<Personality>();
        var (str, agi, intl, endu) = GenerateAbilities(adventurerClass, rank, race);

        Sprite[] portraits = Resources.LoadAll<Sprite>(GlobalConstants.spriteDirectory);
        Sprite randomPortrait = portraits.Length > 0 ? portraits[Random.Range(0, portraits.Length)] : null;

        data.adventurerId = Guid.NewGuid().ToString();
        data.adventurerName = GenerateRandomName();
        data.gender = gender;
        data.race = race;
        data.adventurerClass = adventurerClass;
        data.personality = personality;
        data.rank = rank;
        data.background = background;
        data.currentState = State.Resting;
        data.strength = str;
        data.agility = agi;
        data.intelligence = intl;
        data.endurance = endu;
        data.experience = Random.Range(0, 100);
        data.numOfVisits = Random.Range(0, 100);
        data.isSponsored = false;
        data.questCount = 0;
        data.currQuestID = -1;
        data.friendshipLevel = Random.Range(-100, 100);
        data.portrait = randomPortrait?.name ?? "default";
        data.adventurerType = AdventurerType.Adventurer;
        data.questDuration = 0;
        data.prompt = GetPrompt();
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
            if (data.rank < Rank.Adamantine)
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

    private string GetPrompt()
    {
        string friendlinessDescriptor = data.friendshipLevel switch
        {
            >= 70 => "trusting",
            >= 30 => "friendly",
            >= 0 => "neutral",
            _ => "hostile"
        };


        return
        $"You are now roleplaying as **{data.adventurerName}**, a {data.gender} {data.race} {data.adventurerClass}.\n" +
        $"You are a visitor to the guild. The receptionist is speaking to you.\n" +
        $"Speak naturally, as if this were a real conversation.\n" +
        $"\n" +
        $"**Adventurer Profile**\n" +
        $"Personality: {data.personality.ToString().ToLower()}\n" +
        $"Rank: {data.rank}\n" +
        $"Background: {data.background.ToString().ToLower()}\n" +
        $"Relationship: {friendlinessDescriptor}\n" +
        $"Number of Visits: {++data.numOfVisits}\n" +
        $"\n" +
        $"**Response Instructions**\n" +
        $"- Respond with {LocalizationSettings.SelectedLocale} spoken dialogue only.\n" +
        $"- Do NOT include narration, inner thoughts, tone descriptions, or stage directions.\n" +
        $"- Do NOT explain your personality, or background.\n" +
        $"- Do NOT include any notes, formatting instructions, or non-dialogue content.\n" +
        $"- Your tone and word choice must clearly reflect your personality, and relationship({friendlinessDescriptor}).\n" +
        $"\n" +
        $"- **Must wrap your spoken dialogue in quotation marks, like: \"...\"**\n";
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
        data.portrait = availableAdventurer.portrait;
        data.questDuration = availableAdventurer.questDuration;
    }


    private readonly Dictionary<AdventurerBackground, Dictionary<Personality, int>> backgroundPersonalityWeights = new()
    {
        [AdventurerBackground.Noble] = new()
        {
            [Personality.Arrogant] = 30,
            [Personality.Serious] = 20,
            [Personality.Brave] = 15,
            [Personality.Shy] = 10,
            [Personality.Academic] = 10,
            [Personality.Cheerful] = 10,
            [Personality.Sarcastic] = 5
        },

        [AdventurerBackground.Ghetto] = new()
        {
            [Personality.Sarcastic] = 30,
            [Personality.Brave] = 25,
            [Personality.Shy] = 15,
            [Personality.Arrogant] = 10,
            [Personality.Cheerful] = 10,
            [Personality.Serious] = 5,
            [Personality.Academic] = 5
        },

        [AdventurerBackground.Mercenary] = new()
        {
            [Personality.Brave] = 30,
            [Personality.Serious] = 20,
            [Personality.Sarcastic] = 20,
            [Personality.Arrogant] = 15,
            [Personality.Cheerful] = 5,
            [Personality.Academic] = 5,
            [Personality.Shy] = 5
        },

        [AdventurerBackground.Farmer] = new()
        {
            [Personality.Cheerful] = 30,
            [Personality.Brave] = 25,
            [Personality.Shy] = 15,
            [Personality.Serious] = 10,
            [Personality.Academic] = 10,
            [Personality.Sarcastic] = 5,
            [Personality.Arrogant] = 5
        },

        [AdventurerBackground.Merchant] = new()
        {
            [Personality.Arrogant] = 25,
            [Personality.Cheerful] = 20,
            [Personality.Academic] = 20,
            [Personality.Sarcastic] = 15,
            [Personality.Brave] = 10,
            [Personality.Serious] = 5,
            [Personality.Shy] = 5
        }
    };

    private static readonly Dictionary<Race, Dictionary<Class, int>> raceClassWeights = new()
    {
        [Race.Human] = new()
        {
            [Class.Warrior] = 30,
            [Class.Mage] = 25,
            [Class.Rogue] = 25,
            [Class.Ranger] = 20
        },

        [Race.Elf] = new()
        {
            [Class.Ranger] = 40,
            [Class.Mage] = 30,
            [Class.Rogue] = 20,
            [Class.Warrior] = 10
        },

        [Race.Dwarf] = new()
        {
            [Class.Warrior] = 50,
            [Class.Ranger] = 25,
            [Class.Rogue] = 15,
            [Class.Mage] = 10
        },

        [Race.Orc] = new()
        {
            [Class.Warrior] = 60,
            [Class.Rogue] = 20,
            [Class.Ranger] = 15,
            [Class.Mage] = 5
        }
    };

    private static T GetWeightedRandom<T>(Dictionary<T, int> weights)
    {
        int total = weights.Values.Sum();
        int roll = Random.Range(0, total);
        foreach (var kv in weights)
        {
            if (roll < kv.Value)
                return kv.Key;
            roll -= kv.Value;
        }

        return weights.Keys.First(); // Fallback
    }

    private static readonly Dictionary<Race, (int str, int agi, int intl, int endu)> raceStatModifiers = new()
    {
        [Race.Human] = (0, 0, 0, 0),             // Balanced, no bonuses
        [Race.Elf] = (-1, 2, 2, -1),           // Agile and smart, but fragile
        [Race.Dwarf] = (2, -1, 0, 2),            // Strong and tough, but slow
        [Race.Orc] = (3, 0, -2, 1),            // Strong and durable, but not very bright
    };

    private static (int str, int agi, int intl, int endu) GenerateAbilities(Class cls, Rank rank, Race race)
    {
        int baseMin = (int)rank * 3;
        int baseMax = baseMin + 10;

        (int str, int agi, int intl, int endu) baseStats = cls switch
        {
            Class.Warrior => (
                Random.Range(baseMax, baseMax + 5),
                Random.Range(baseMin, baseMin + 3),
                Random.Range(baseMin, baseMin + 2),
                Random.Range(baseMax, baseMax + 5)
            ),

            Class.Mage => (
                Random.Range(baseMin, baseMin + 2),
                Random.Range(baseMin, baseMin + 3),
                Random.Range(baseMax, baseMax + 6),
                Random.Range(baseMin, baseMin + 3)
            ),

            Class.Rogue => (
                Random.Range(baseMin, baseMin + 3),
                Random.Range(baseMax, baseMax + 5),
                Random.Range(baseMin, baseMin + 3),
                Random.Range(baseMin, baseMin + 2)
            ),

            Class.Ranger => (
                Random.Range(baseMin + 2, baseMax),
                Random.Range(baseMax, baseMax + 3),
                Random.Range(baseMin, baseMin + 3),
                Random.Range(baseMin, baseMin + 3)
            ),

            _ => (10, 10, 10, 10),
        };

        var raceBonus = raceStatModifiers[race];

        return (
            baseStats.str + raceBonus.str,
            baseStats.agi + raceBonus.agi,
            baseStats.intl + raceBonus.intl,
            baseStats.endu + raceBonus.endu
        );
    }

}


