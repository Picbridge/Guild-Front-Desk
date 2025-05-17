using UnityEngine;
public enum TimePhase { Morning, Afternoon, Evening }

public static class GlobalConstants
{
    public const string spriteDirectory = "Sprites/Adventurers/";
    public const string adventurerIdDirectory = "AdventurerId/";
}

namespace GFD.Adventurer
{
    public enum Perspective
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
        public Background background;

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
        public bool isSponsored;
        public State currentState;
        public InjuryStatus injuryStatus;
        public Mood mood;

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