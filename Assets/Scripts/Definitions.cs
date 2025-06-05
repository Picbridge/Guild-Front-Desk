using GFD.Adventurer;
using GFD.Quest;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
public enum TimePhase { Morning, Afternoon, Evening }

public static class QuestWeights
{
    public static readonly Dictionary<string, Dictionary<QuestType, (float str, float agi, float intel, float end)>> ClassWeights =
    new()
    {
        ["warrior"] = new()
        {
            [QuestType.Elimination] = (0.5f, 0.2f, 0.1f, 0.2f),
            [QuestType.Assassination] = (0.4f, 0.3f, 0.1f, 0.2f),
            [QuestType.Exploration] = (0.2f, 0.3f, 0.2f, 0.3f),
            [QuestType.Retrieval] = (0.3f, 0.3f, 0.2f, 0.2f),
            [QuestType.Escort] = (0.4f, 0.2f, 0.1f, 0.3f),
        },
        ["mage"] = new()
        {
            [QuestType.Elimination] = (0.0f, 0.1f, 0.6f, 0.3f),
            [QuestType.Assassination] = (0.0f, 0.2f, 0.5f, 0.3f),
            [QuestType.Exploration] = (0.0f, 0.2f, 0.5f, 0.3f),
            [QuestType.Retrieval] = (0.0f, 0.3f, 0.4f, 0.3f),
            [QuestType.Escort] = (0.0f, 0.2f, 0.4f, 0.4f),
        },
        ["rogue"] = new()
        {
            [QuestType.Elimination] = (0.1f, 0.5f, 0.2f, 0.2f),
            [QuestType.Assassination] = (0.0f, 0.6f, 0.2f, 0.2f),
            [QuestType.Exploration] = (0.0f, 0.5f, 0.3f, 0.2f),
            [QuestType.Retrieval] = (0.0f, 0.5f, 0.3f, 0.2f),
            [QuestType.Escort] = (0.1f, 0.4f, 0.2f, 0.3f),
        },
        ["ranger"] = new()
        {
            [QuestType.Elimination] = (0.2f, 0.4f, 0.2f, 0.2f),
            [QuestType.Assassination] = (0.1f, 0.5f, 0.2f, 0.2f),
            [QuestType.Exploration] = (0.1f, 0.4f, 0.3f, 0.2f),
            [QuestType.Retrieval] = (0.1f, 0.4f, 0.3f, 0.2f),
            [QuestType.Escort] = (0.2f, 0.4f, 0.2f, 0.2f),
        }
    };

    public static readonly Dictionary<string, (float str, float agi, float intel, float end)> VerbStatWeights = new()
    {
        ["anticipate"] = (0.2f, 0.2f, 0.2f, 0.2f),
        ["approach"] = (0.1f, 0.6f, 0.1f, 0.2f),
        ["ascend"] = (0.1f, 0.6f, 0.1f, 0.2f),
        ["assemble"] = (0.2f, 0.2f, 0.2f, 0.2f),
        ["assess"] = (0.0f, 0.2f, 0.7f, 0.1f),
        ["assist"] = (0.2f, 0.2f, 0.3f, 0.3f),
        ["bait"] = (0.1f, 0.6f, 0.2f, 0.1f),
        ["brave"] = (0.2f, 0.2f, 0.2f, 0.2f),
        ["breach"] = (0.2f, 0.2f, 0.2f, 0.2f),
        ["bring"] = (0.2f, 0.2f, 0.3f, 0.3f),
        ["bypass"] = (0.1f, 0.6f, 0.2f, 0.1f),
        ["chart"] = (0.0f, 0.2f, 0.7f, 0.1f),
        ["clear"] = (0.2f, 0.2f, 0.2f, 0.2f),
        ["climb"] = (0.1f, 0.6f, 0.1f, 0.2f),
        ["collect"] = (0.2f, 0.2f, 0.2f, 0.2f),
        ["confirm"] = (0.0f, 0.2f, 0.7f, 0.1f),
        ["defend"] = (0.5f, 0.2f, 0.1f, 0.2f),
        ["descend"] = (0.1f, 0.6f, 0.1f, 0.2f),
        ["destroy"] = (0.5f, 0.2f, 0.1f, 0.2f),
        ["determine"] = (0.0f, 0.2f, 0.7f, 0.1f),
        ["disable"] = (0.2f, 0.2f, 0.2f, 0.2f),
        ["dismantle"] = (0.5f, 0.2f, 0.1f, 0.2f),
        ["disrupt"] = (0.5f, 0.2f, 0.1f, 0.2f),
        ["document"] = (0.0f, 0.2f, 0.7f, 0.1f),
        ["eliminate"] = (0.5f, 0.2f, 0.1f, 0.2f),
        ["engage"] = (0.2f, 0.2f, 0.2f, 0.2f),
        ["enter"] = (0.1f, 0.6f, 0.1f, 0.2f),
        ["escort"] = (0.1f, 0.6f, 0.1f, 0.2f),
        ["evade"] = (0.1f, 0.6f, 0.1f, 0.2f),
        ["evaluate"] = (0.0f, 0.2f, 0.7f, 0.1f),
        ["explore"] = (0.0f, 0.2f, 0.7f, 0.1f),
        ["flush"] = (0.2f, 0.2f, 0.2f, 0.2f),
        ["follow"] = (0.1f, 0.6f, 0.1f, 0.2f),
        ["guard"] = (0.5f, 0.2f, 0.1f, 0.2f),
        ["guide"] = (0.1f, 0.6f, 0.1f, 0.2f),
        ["harvest"] = (0.2f, 0.2f, 0.2f, 0.2f),
        ["identify"] = (0.0f, 0.2f, 0.7f, 0.1f),
        ["ignite"] = (0.3f, 0.2f, 0.3f, 0.2f),
        ["infiltrate"] = (0.1f, 0.6f, 0.2f, 0.1f),
        ["investigate"] = (0.0f, 0.2f, 0.7f, 0.1f),
        ["join"] = (0.2f, 0.2f, 0.3f, 0.3f),
        ["launch"] = (0.2f, 0.2f, 0.2f, 0.2f),
        ["lead"] = (0.1f, 0.6f, 0.1f, 0.2f),
        ["locate"] = (0.2f, 0.2f, 0.2f, 0.2f),
        ["lure"] = (0.1f, 0.6f, 0.2f, 0.1f),
        ["map"] = (0.0f, 0.2f, 0.7f, 0.1f),
        ["navigate"] = (0.1f, 0.6f, 0.1f, 0.2f),
        ["pacify"] = (0.3f, 0.2f, 0.3f, 0.2f),
        ["protect"] = (0.5f, 0.2f, 0.1f, 0.2f),
        ["recover"] = (0.2f, 0.2f, 0.3f, 0.3f),
        ["repel"] = (0.5f, 0.2f, 0.1f, 0.2f),
        ["retrieve"] = (0.2f, 0.2f, 0.3f, 0.3f),
        ["scale"] = (0.1f, 0.6f, 0.1f, 0.2f),
        ["scout"] = (0.2f, 0.2f, 0.2f, 0.2f),
        ["search"] = (0.0f, 0.2f, 0.7f, 0.1f),
        ["shield"] = (0.5f, 0.2f, 0.1f, 0.2f),
        ["sketch"] = (0.2f, 0.2f, 0.2f, 0.2f),
        ["survey"] = (0.0f, 0.2f, 0.7f, 0.1f),
        ["take"] = (0.2f, 0.2f, 0.3f, 0.3f),
        ["track"] = (0.1f, 0.6f, 0.1f, 0.2f),
        ["travel"] = (0.1f, 0.6f, 0.1f, 0.2f),
        ["traverse"] = (0.1f, 0.6f, 0.1f, 0.2f),
        ["uncover"] = (0.0f, 0.2f, 0.7f, 0.1f),
        ["verify"] = (0.0f, 0.2f, 0.7f, 0.1f),
    };

}



public static class GlobalConstants
{
    public const string spriteDirectory = "Sprites/Adventurers/";
    public const string adventurerIdDirectory = "AdventurerId/";
}

namespace GFD.Adventurer
{
    public enum Gender
    {
        Male,
        Female
    }
    public enum State
    {
        InQuest,
        Resting,
        Dead
    }
    public enum Class
    {
        Warrior,
        Mage,
        Rogue,
        Ranger,
    }
    public enum Personality
    {
        Cheerful,     
        Brave,        
        Sarcastic,    
        Shy,          
        Serious,      
        Arrogant,
        Academic
    }
    public enum Rank
    {
        Bronze,
        Silver,
        Gold,
        Diamond,
        Adamantine
    }
    public enum Race
    {
        Human,
        Elf,
        Dwarf,
        Orc
    }
    public enum AdventurerBackground
    {
        Noble,
        Ghetto,
        Mercenary,
        Farmer,
        Merchant
    }

    public enum AdventurerType
    {
        Adventurer,
        GuildMember,
        Guardian,
        Client
    }

    [System.Serializable]
    public class AdventurerData
    {
        [Header("Identity")]
        public string adventurerId;
        public string adventurerName;
        public Class adventurerClass;
        public Gender gender;
        public Race race;
        public Personality personality;
        public Rank rank;
        public AdventurerBackground background;
        public AdventurerType adventurerType;

        [Header("Visual")]// filename without path
        public string portrait;

        [Header("Stats")]
        public int strength;
        public int agility;
        public int intelligence;
        public int endurance;
        public int experience;

        [Header("Flags")]
        public int questCount;
        public int numOfVisits;
        public bool isSponsored;
        public State currentState;
        public string prompt;
        public int friendshipLevel;
        public float questDuration;

        public int currQuestID;
    }
}

namespace GFD.Quest
{
    public enum QuestType
    {
        Elimination,
        Exploration,
        Escort,
        Retrieval,
        Assassination
    }

    public enum QuestDifficulty
    {
        Easy,
        Normal,
        Hard,
        Deadly
    }

    public enum QuestStatus
    {
        Pending,
        Active,
        Completed,
        Failed
    }

    [System.Serializable]
    public class QuestData
    {
        [Header("Identity")]
        public string questId;
        public string questName;
        public string description;
        public string assignedAdventurerId;
        public QuestType questType;
        public QuestDifficulty difficulty;
        public QuestStatus status;
        public int reward;
        public int duration;
    }
}

namespace GFD.Map
{
    [System.Serializable]
    public class LocationDetail
    {
        public string locationName;
        public string locationDescription;
    }
}
