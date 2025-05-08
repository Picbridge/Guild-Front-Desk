using UnityEngine;
using GFD.Adventurer;
using GFD.Quest;
public enum TimePhase { Morning, Afternoon, Evening }

namespace GFD.Adventurer
{
    public enum  Perspective
    {
        Front,
        Side
    }
    public enum Gender
    {
        Male,
        Female
    }
    public enum State
    {
        Idle,
        InQuest,
        Resting,
        Dead
    }
    public enum Class
    {
        Warrior,
        Mage,
        Rogue,
        Healer,
        Ranger,
        Paladin
    }

    public enum Personality
    {
        Brave,
        Arrogant,
        Shy,
        Suspicious,
        Cheerful,
        Talkative
    }

    public enum Rank
    {
        Bronze,
        Silver,
        Gold,
        Platinum,
        Diamond,
        Adamantium
    }

    public enum Race
    {
        Human,
        Elf,
        Dwarf,
        Orc,
        Beastman,
        Demon,
        Angel,
        Dragon
    }

    public enum Mood
    {
        Happy,
        Calm,
        Anxious,
        Angry,
        Depressed,
        Fearful,
        Curious
    }
    public enum Background
    {
        Noble,
        Orphan,
        Exiled,
        Mercenary,
        Academic,
        Cultist,
        Farmer,
        Criminal
    }
    public enum InjuryStatus
    {
        Healthy,
        Wounded,
        SeverelyInjured,
        Crippled,
        Dead
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
}

[System.Serializable]
public class QuestData
{
    public string questName;
    public string description;
    public QuestType questType;
    public QuestDifficulty difficulty;
    public int reward;
    public int duration;
    public int maxAdventurers;
    public int minAdventurers;

    [Header("Flags")]
    public bool isActive;
    public bool isCompleted;
}

[System.Serializable]
public class AdventurerData
{
    [Header("Identity")]
    public string adventurerName;
    public Class adventurerClass;
    public Gender gender;
    public Perspective perspective;
    public Race race;
    public Personality personality;
    public Rank rank;
    public Background background;

    public VisualData visualData;

    [Header("Visual")]// filename without path
    public string bodySprite;
    public string headSprite;
    public string hairSprite;
    public string eyesSprite;
    public string mouthSprite;

    public Color skinColor;
    public Color hairColor;

    [Header("Stats")]
    public int strength;
    public int agility;
    public int intelligence;
    public int endurance;

    [Header("Flags")]
    public int questCount;
    public bool isSponsored;
    public State currentState;
    public InjuryStatus injuryStatus;
    public Mood mood;

    public int currQuestID;
}

public struct VisualData
{
    public string bodySprite;
    public string headSprite;
    public string eyesSprite;
    public string mouthSprite;
    public string hairSprite;
    public Color skinColor;
    public Color hairColor;
}


