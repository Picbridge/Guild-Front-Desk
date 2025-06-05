using UnityEngine;
using GFD.Utilities;
using GFD.Quest;
using Random = UnityEngine.Random;
using GFD.Adventurer;
using UnityEditor.PackageManager.Requests;
using System;
using Unity.VisualScripting;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEditor.PackageManager;

public class Quest
{
    public QuestData data;
    private Script_AdventurerManager adventurerManager = Script_AdventurerManager.Instance;
    private Script_QuestManager questManager = Script_QuestManager.Instance;

    private float assignedAdventurerEES;
    private float assignedAdventurerRiskPerDay;

    #region Quest Calculations
    private float CalculateEES(AdventurerData adventurer)
    {
        // Get raw stats
        int str = adventurer.strength;
        int agi = adventurer.agility;
        int intel = adventurer.intelligence;
        int end = adventurer.endurance;

        // Get class and weights
        string cls = adventurer.adventurerClass.ToString();
        var (clsStr, clsAgi, clsInt, clsEnd) = QuestWeights.ClassWeights.ContainsKey(cls) &&
            QuestWeights.ClassWeights[cls].ContainsKey(data.questType)
            ? QuestWeights.ClassWeights[cls][data.questType]
            : (0.25f, 0.25f, 0.25f, 0.25f); // fallback

        // Extract verbs from quest.description (use regex)
        var verbs = ExtractVerbs(data.description);
        var (verbStr, verbAgi, verbInt, verbEnd) = GetVerbStatWeights(verbs);

        // Blend 50/50: class-based and verb-based
        float finalStrWeight = 0.5f * clsStr + 0.5f * verbStr;
        float finalAgiWeight = 0.5f * clsAgi + 0.5f * verbAgi;
        float finalIntWeight = 0.5f * clsInt + 0.5f * verbInt;
        float finalEndWeight = 0.5f * clsEnd + 0.5f * verbEnd;

        // Compute execution efficiency score (EES)
        return finalStrWeight * str +
                     finalAgiWeight * agi +
                     finalIntWeight * intel +
                     finalEndWeight * end;
    }
    private int CalculateDuration(AdventurerData adventurer)
    {
        // Adjust duration based on EES
        int adjustment = GetDurationRankModifier(adventurer.rank);

        return Math.Max(1, data.duration + adjustment);
    }
    private List<string> ExtractVerbs(string description)
    {
        // Look for base verbs from sentence start (imperative mood)
        var verbCandidates = new List<string>();
        var lines = description.ToLower().Split('\n');
        foreach (var line in lines)
        {
            var matches = Regex.Matches(line, @"\b(track|eliminate|escort|investigate|retrieve|navigate|chart|burn|defend|descend|infiltrate|report|survey|recover|ignite)\b");
            foreach (Match match in matches)
            {
                verbCandidates.Add(match.Value);
            }
        }
        return verbCandidates;
    }
    private (float Int, float Agi, float Str, float End) GetVerbStatWeights(List<string> verbs)
    {
        float intSum = 0, agiSum = 0, strSum = 0, endSum = 0;

        foreach (var verb in verbs)
        {
            if (QuestWeights.VerbStatWeights.TryGetValue(verb, out var w))
            {
                intSum += w.intel;
                agiSum += w.agi;
                strSum += w.str;
                endSum += w.end;
            }
        }

        float total = intSum + agiSum + strSum + endSum;
        return total > 0
            ? (intSum / total, agiSum / total, strSum / total, endSum / total)
            : (0.25f, 0.25f, 0.25f, 0.25f); // neutral fallback
    }
    private int GetDurationRankModifier(Rank rank)
    {
        return rank switch
        {
            Rank.Bronze => 1,                      // Always penalized
            Rank.Silver => assignedAdventurerEES >= 9 ? 0 : 1,       // Slightly better than Bronze
            Rank.Gold => assignedAdventurerEES >= 10 ? -1 : 0,     // Gets bonuses at high EES
            Rank.Diamond => assignedAdventurerEES >= 9 ? -1 : 0,
            Rank.Adamantine => assignedAdventurerEES >= 8 ? -2 : -1,
            _ => 0
        };
    }
    private float GetDailyDeathProb(float duration)
    { 
        float baseRisk = GetBaseRisk(data.questType, data.difficulty.ToString());
        float rankModifier = GetRiskRankModifier(adventurerManager.GetAdventurer(data.assignedAdventurerId).rank);
        float eesModifier = GetEESModifier();

        float missionRisk = baseRisk * rankModifier * eesModifier;
        float dailyDeathProb = 1.0f - Mathf.Pow(1.0f - missionRisk, 1.0f / duration);

        return Math.Clamp(dailyDeathProb, 0.0f, 0.95f);
    }
    private float GetEESModifier()
    {
        if (assignedAdventurerEES >= 13) return 0.5f;
        if (assignedAdventurerEES >= 10) return 0.75f;
        if (assignedAdventurerEES >= 7) return 1.0f;
        if (assignedAdventurerEES >= 5) return 1.5f;
        /* ees < 5 */
        return 2.0f;
    }
    private float GetBaseRisk(QuestType type, string difficulty)
    {
        return (type, difficulty) switch
        {
            (QuestType.Elimination, "Easy") => 0.05f,
            (QuestType.Elimination, "Normal") => 0.10f,
            (QuestType.Elimination, "Hard") => 0.20f,
            (QuestType.Elimination, "Deadly") => 0.40f,

            (QuestType.Assassination, "Easy") => 0.07f,
            (QuestType.Assassination, "Normal") => 0.15f,
            (QuestType.Assassination, "Hard") => 0.25f,
            (QuestType.Assassination, "Deadly") => 0.50f,

            (QuestType.Escort, "Easy") => 0.03f,
            (QuestType.Escort, "Normal") => 0.05f,
            (QuestType.Escort, "Hard") => 0.10f,
            (QuestType.Escort, "Deadly") => 0.25f,

            (QuestType.Retrieval, "Easy") => 0.01f,
            (QuestType.Retrieval, "Normal") => 0.03f,
            (QuestType.Retrieval, "Hard") => 0.06f,
            (QuestType.Retrieval, "Deadly") => 0.15f,

            (QuestType.Exploration, "Easy") => 0.02f,
            (QuestType.Exploration, "Normal") => 0.04f,
            (QuestType.Exploration, "Hard") => 0.08f,
            (QuestType.Exploration, "Deadly") => 0.20f,

            _ => 0.0f
        };
    }
    private float GetRiskRankModifier(Rank rank)
    {
        return rank switch
        {
            Rank.Bronze => 1.25f,
            Rank.Silver => 1.0f,
            Rank.Gold => 0.85f,
            Rank.Diamond => 0.7f,
            Rank.Adamantine => 0.5f,
            _ => 1.0f
        };
    }

    public void AdjustFavorability()
    {
        if (data.assignedAdventurerId == null) return;

        var adventurer = adventurerManager.GetAdventurer(data.assignedAdventurerId);
        if (adventurer == null) return;

        int favorabilityChange = 0;

        // Base favorability changes
        if (data.status == QuestStatus.Completed)
        {
            favorabilityChange += 5;  // Base increase for completing a quest
        }
        else if (data.status == QuestStatus.Failed)
        {
            favorabilityChange -= 3;  // Base decrease for failing a quest
        }

        // === RANK-BASED FAVORABILITY ===
        favorabilityChange += GetRankFavorabilityModifier(adventurer.rank);

        // === RACE-BASED FAVORABILITY ===
        favorabilityChange += GetRaceFavorabilityModifier(adventurer.race);

        // === PERSONALITY-BASED FAVORABILITY ===
        favorabilityChange += GetPersonalityFavorabilityModifier(adventurer.personality);

        // === EES-BASED FAVORABILITY ===
        favorabilityChange += GetEESFavorabilityModifier();

        // Apply the favorability change
        ApplyFavorabilityChange(adventurer, favorabilityChange);

        Debug.Log($"Adjusted favorability for {adventurer.adventurerName} by {favorabilityChange}. " +
                  $"New friendship level: {adventurer.friendshipLevel}");
    }

    private int GetRankFavorabilityModifier(Rank rank)
    {
        // Higher ranks expect more challenging quests
        int modifier = 0;

        switch (data.difficulty)
        {
            case QuestDifficulty.Easy:
                modifier = rank switch
                {
                    Rank.Bronze => 2,       // Bronze adventurers appreciate easy quests
                    Rank.Silver => 1,       // Silver adventurers are okay with easy quests
                    Rank.Gold => -1,        // Gold adventurers find easy quests beneath them
                    Rank.Diamond => -2,     // Diamond adventurers are insulted by easy quests
                    Rank.Adamantine => -3,  // Adamantine adventurers are very insulted by easy quests
                    _ => 0
                };
                break;

            case QuestDifficulty.Normal:
                modifier = rank switch
                {
                    Rank.Bronze => 1,       // Bronze adventurers like normal quests
                    Rank.Silver => 2,       // Silver adventurers prefer normal quests
                    Rank.Gold => 1,         // Gold adventurers consider normal quests acceptable
                    Rank.Diamond => -1,     // Diamond adventurers find normal quests too simple
                    Rank.Adamantine => -2,  // Adamantine adventurers find normal quests very simple
                    _ => 0
                };
                break;

            case QuestDifficulty.Hard:
                modifier = rank switch
                {
                    Rank.Bronze => -1,      // Bronze adventurers find hard quests challenging
                    Rank.Silver => 1,       // Silver adventurers appreciate hard quests
                    Rank.Gold => 2,         // Gold adventurers prefer hard quests
                    Rank.Diamond => 2,      // Diamond adventurers enjoy hard quests
                    Rank.Adamantine => 1,   // Adamantine adventurers find hard quests acceptable
                    _ => 0
                };
                break;

            case QuestDifficulty.Deadly:
                modifier = rank switch
                {
                    Rank.Bronze => -2,      // Bronze adventurers are scared of deadly quests
                    Rank.Silver => -1,      // Silver adventurers find deadly quests very challenging
                    Rank.Gold => 1,         // Gold adventurers appreciate deadly quests
                    Rank.Diamond => 2,      // Diamond adventurers prefer deadly quests
                    Rank.Adamantine => 3,   // Adamantine adventurers love deadly quests
                    _ => 0
                };
                break;
        }

        return modifier;
    }

    private int GetRaceFavorabilityModifier(Race race)
    {
        // Different races may have preferences for certain quest types
        return (race, data.questType) switch
        {
            // Humans are versatile but prefer social quests
            (Race.Human, QuestType.Escort) => 2,

            // Elves prefer exploration and ranged combat
            (Race.Elf, QuestType.Exploration) => 2,
            (Race.Elf, QuestType.Retrieval) => 1,
            (Race.Elf, QuestType.Elimination) => -1,

            // Dwarves prefer mining quests and dislike sneaking
            (Race.Dwarf, QuestType.Exploration) => 1,
            (Race.Dwarf, QuestType.Assassination) => -2,

            // Orcs prefer direct combat and dislike delicate missions
            (Race.Orc, QuestType.Elimination) => 2,
            (Race.Orc, QuestType.Assassination) => 1,
            (Race.Orc, QuestType.Escort) => -1,
            (Race.Orc, QuestType.Retrieval) => -1,

            // Default case: no particular preference
            _ => 0
        };
    }

    private int GetPersonalityFavorabilityModifier(Personality personality)
    {
        // Different personalities prefer different types of quests
        return (personality, data.questType) switch
        {
            // Cheerful adventurers enjoy helping others
            (Personality.Cheerful, QuestType.Escort) => 2,
            (Personality.Cheerful, QuestType.Assassination) => -2,

            // Brave adventurers like challenging combat
            (Personality.Brave, QuestType.Elimination) => 2,
            (Personality.Brave, QuestType.Exploration) => 1,

            // Sarcastic adventurers enjoy missions with a twist
            (Personality.Sarcastic, QuestType.Retrieval) => 1,
            (Personality.Sarcastic, QuestType.Escort) => -1,

            // Shy adventurers prefer solo missions
            (Personality.Shy, QuestType.Exploration) => 2,
            (Personality.Shy, QuestType.Escort) => -2,

            // Serious adventurers appreciate any well-structured mission
            (Personality.Serious, _) => 1,

            // Arrogant adventurers like missions that bring prestige
            (Personality.Arrogant, QuestType.Elimination) => 1,
            (Personality.Arrogant, QuestType.Assassination) => 2,
            (Personality.Arrogant, QuestType.Escort) => -1,

            // Academic adventurers prefer intellectual challenges
            (Personality.Academic, QuestType.Exploration) => 2,
            (Personality.Academic, QuestType.Elimination) => -1,

            // Default case: no particular preference
            _ => 0
        };
    }

    private int GetEESFavorabilityModifier()
    {
        // If the adventurer is well-suited for the quest (high EES), they'll appreciate it more
        if (assignedAdventurerEES >= 13) return 3;      // Perfect match
        if (assignedAdventurerEES >= 10) return 2;      // Very good match
        if (assignedAdventurerEES >= 7) return 1;       // Good match
        if (assignedAdventurerEES >= 5) return 0;       // Neutral match
        return -1;                                     // Poor match
    }
    private void ApplyFavorabilityChange(AdventurerData adventurer, int change)
    {
        // Apply favorability change with limits
        int newFriendshipLevel = adventurer.friendshipLevel + change;

        // Cap friendship level between 0 and 100
        adventurer.friendshipLevel = Math.Clamp(newFriendshipLevel, -30, 100);
    }
    #endregion

    #region Quest management

    public bool IsAdventurerDead()
    {
        bool isDead = Random.value < assignedAdventurerRiskPerDay;
        if (isDead) 
        {
            var adventurer = adventurerManager.GetAdventurer(data.assignedAdventurerId);
            if (adventurer != null)
            {
                Debug.Log($"Adventurer {adventurer.adventurerName} has died during quest {data.questName}. Risk: {assignedAdventurerRiskPerDay:P2}");
                adventurerManager.MarkAdventurerAsDead(adventurer.adventurerId);
            }
        }

        return isDead;
    }

    public void AssignAdventurer(string adventurerId)
    {
        data.assignedAdventurerId = adventurerId;
        data.status = QuestStatus.Active;
        var adventurer = adventurerManager.GetAdventurer(adventurerId);
        assignedAdventurerEES = CalculateEES(adventurer);
        adventurer.questDuration = CalculateDuration(adventurer);
        assignedAdventurerRiskPerDay = GetDailyDeathProb(adventurer.questDuration);
        Debug.Log($"Assigned adventurer {adventurer.adventurerName} to quest {data.questName}. Duration: {adventurer.questDuration}, Daily Death Risk: {assignedAdventurerRiskPerDay:P2}");
    }
    public void FailQuest()
    {
        AdventurerData adventurer = adventurerManager.GetAdventurer(data.assignedAdventurerId);

        if (adventurer.currentState != GFD.Adventurer.State.Dead)
        {
            adventurerManager.MarkAdventurerAsAvailable(data.assignedAdventurerId);
            EndQuest(adventurer);
        }
            
        data.status = QuestStatus.Failed;
    }
    public void CompleteQuest()
    {
        adventurerManager.MarkAdventurerAsAvailable(data.assignedAdventurerId);
        var adventurer = adventurerManager.GetAdventurer(data.assignedAdventurerId);
        adventurer.questDuration = 0;

        var reward = data.difficulty switch
        {
            QuestDifficulty.Easy => 10,
            QuestDifficulty.Normal => 20,
            QuestDifficulty.Hard => 30,
            QuestDifficulty.Deadly => 40,
            _ => 0
        };

        EndQuest(adventurer, reward);
        data.status = QuestStatus.Completed;
    }

    private void EndQuest(AdventurerData adventurer, int reward = 0)
    {
        AdjustFavorability();
        adventurer.currentState = GFD.Adventurer.State.Resting;

        if (reward > 0)
        {
            adventurer.experience += reward;

            if (adventurer.experience >= 100 * ((int)adventurer.rank + 1))
            {
                adventurer.experience = 0;
                if (adventurer.rank < Rank.Adamantine)
                    adventurer.rank++;
            }
        }
    }
    #endregion

}
