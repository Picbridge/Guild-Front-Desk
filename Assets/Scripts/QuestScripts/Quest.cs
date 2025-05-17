using UnityEngine;
using GFD.Utilities;
using GFD.Quest;
using Random = UnityEngine.Random;
using GFD.Adventurer;
using UnityEditor.PackageManager.Requests;
using System;

public class Quest
{
    public QuestData data;

    public void CopyData(QuestData og)
    {
        if (og == null)
        {
            Debug.LogError("Cannot copy from null QuestData");
            return;
        }

        // Initialize data if it's null
        if (data == null)
            data = new QuestData();

        data.questId = og.questId;
        data.questName = og.questName;
        data.description = og.description;
        data.questType = og.questType;
        data.difficulty = og.difficulty;
        data.reward = og.reward;
        data.duration = og.duration;
        data.status = og.status;
        data.assignedAdventurerId = og.assignedAdventurerId;
    }

    // In your Quest.cs file
    [Obsolete]
    public void GenerateRandomQuest(MonoBehaviour coroutineRunner = null)
    {
        // If data is null, initialize it
        if (data == null)
            data = new QuestData();

        // Define quest generation parameters
        QuestParameters questParameters = new QuestParameters
        {
            preferredType = GetRandom<QuestType>(),
            difficulty = GetRandom<QuestDifficulty>(),
        };

        Debug.Log("Parameters defined for new quest creation");

        // Find a MonoBehaviour to run the coroutine if none is provided
        if (coroutineRunner == null)
        {
            coroutineRunner = GameObject.FindObjectOfType<Script_QuestManager>();
            if (coroutineRunner == null)
            {
                Debug.LogError("No MonoBehaviour found to run the coroutine!");
                // Create a basic quest as fallback
                data.questId = Guid.NewGuid().ToString();
                data.questName = "Fallback Quest";
                data.description = "A basic quest created when no coroutine runner was available";
                data.questType = questParameters.preferredType;
                data.difficulty = questParameters.difficulty;
                data.status = QuestStatus.Pending;
                return;
            }
        }

        // Start the coroutine properly
        coroutineRunner.StartCoroutine(GeminiQuestGenerator.GenerateQuest(
            questParameters,
            generatedQuest => {
                if (generatedQuest != null)
                {
                    CopyData(generatedQuest);
                    Debug.Log($"Quest generated: {data.questName} \n" +
                        $"Description: {data.description} \n" +
                        $"Type: {data.questType} \n" +
                        $"Difficulty: {data.difficulty} \n" +
                        $"Status: {data.status} \n" +
                        $"Assigned Adventurer: {data.assignedAdventurerId} \n" +
                        $"Reward: {data.reward} \n" +
                        $"Duration: {data.duration} \n");

                }
                else
                {
                    Debug.LogError("Failed to generate quest - result was null");
                    // Create fallback quest
                    data.questId = Guid.NewGuid().ToString();
                    data.questName = $"Emergency {questParameters.preferredType} Quest";
                    data.description = $"A standard {questParameters.difficulty.ToString().ToLower()} mission.";
                    data.questType = questParameters.preferredType;
                    data.difficulty = questParameters.difficulty;
                    data.status = QuestStatus.Pending;
                }

                // Add to quest manager
                if (Script_QuestManager.Instance != null)
                    Script_QuestManager.Instance.AddQuestToLists(this);
            }
        ));
    }


    // This function will be called when quest generation completes
    private void OnQuestGenerated(QuestData generatedQuest)
    {
        CopyData(generatedQuest);

        Script_QuestManager.Instance.AddQuestToLists(this);
        Debug.Log(data);
    }

    public void AssignAdventurer(string adventurerId)
    {
        data.assignedAdventurerId = adventurerId;
        data.status = QuestStatus.Active;
    }
    public void FailQuest()
    {
        Script_AdventurerManager.Instance.MarkAdventurerAsAvailable(data.assignedAdventurerId);
        var adventurer = Script_AdventurerManager.Instance.GetAdventurer(data.assignedAdventurerId);
        
        EndQuest(adventurer);
        data.status = QuestStatus.Failed;
    }
    public void CompleteQuest()
    {
        Script_AdventurerManager.Instance.MarkAdventurerAsAvailable(data.assignedAdventurerId);
        var adventurer = Script_AdventurerManager.Instance.GetAdventurer(data.assignedAdventurerId);

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
        adventurer.currentState = GFD.Adventurer.State.Resting;

        if (reward > 0)
        {
            adventurer.experience += reward;

            if (adventurer.experience >= 100 * ((int)adventurer.rank + 1))
            {
                adventurer.experience = 0;
                if (adventurer.rank < Rank.Adamantium)
                    adventurer.rank++;
            }
        }
    }

    private T GetRandom<T>()
    {
        var values = System.Enum.GetValues(typeof(T));
        return (T)values.GetValue(Random.Range(0, values.Length));
    }
}
