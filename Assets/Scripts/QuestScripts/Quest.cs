using UnityEngine;
using GFD.Utilities;
using GFD.Quest;
using Random = UnityEngine.Random;
using GFD.Adventurer;

public class Quest
{
    public QuestData data;

    public void CopyData(QuestData og)
    {
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

    public void GenerateRandomQuest()
    {
        // Define quest generation parameters
        QuestParameters questParameters = new QuestParameters
        {
            preferredType = GetRandom<QuestType>(),
            difficulty = GetRandom<QuestDifficulty>(),
        };

        // Call the generator with parameters and a callback to handle the result
        GeminiQuestGenerator.GenerateQuest(questParameters, OnQuestGenerated);
    }

    // This function will be called when quest generation completes
    private void OnQuestGenerated(QuestData generatedQuest)
    {
        CopyData(generatedQuest);
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
