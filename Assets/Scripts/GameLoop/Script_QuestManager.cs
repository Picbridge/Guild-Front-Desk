using GFD.Quest;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Script_QuestManager : MonoBehaviour
{
    public static Script_QuestManager Instance { get; private set; }

    // Using a dictionary for faster quest lookups by ID
    private Dictionary<string, Quest> questsById = new Dictionary<string, Quest>();

    // Maintaining categorized lists for faster filtering
    private List<Quest> allQuests = new List<Quest>();
    private List<Quest> activeQuests = new List<Quest>();
    private List<Quest> completedQuests = new List<Quest>();
    private List<Quest> failedQuests = new List<Quest>();
    private List<Quest> pendingQuests = new List<Quest>();

    public event Action<Quest> OnQuestCreated;
    public event Action<Quest> OnQuestStatusChanged;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public Quest CreateNewQuest()
    {
        Quest newQuest = new Quest();
        newQuest.GenerateRandomQuest();
        AddQuestToLists(newQuest);
        return newQuest;
    }

    public Quest AddQuest(Quest questData)
    {
        if (questData == null)
            return null;

        if (string.IsNullOrEmpty(questData.data.questId))
            questData.data.questId = Guid.NewGuid().ToString();

        AddQuestToLists(questData);
        return questData;
    }

    private void AddQuestToLists(Quest quest)
    {
        allQuests.Add(quest);
        questsById[quest.data.questId] = quest;
        UpdateQuestCategory(quest);

        OnQuestCreated?.Invoke(quest);
    }

    private void UpdateQuestCategory(Quest quest)
    {
        // Remove from all category lists first
        activeQuests.Remove(quest);
        completedQuests.Remove(quest);
        failedQuests.Remove(quest);
        pendingQuests.Remove(quest);

        // Add to the appropriate list based on status
        switch (quest.data.status)
        {
            case QuestStatus.Active:
                activeQuests.Add(quest);
                break;
            case QuestStatus.Completed:
                completedQuests.Add(quest);
                break;
            case QuestStatus.Failed:
                failedQuests.Add(quest);
                break;
            case QuestStatus.Pending:
                pendingQuests.Add(quest);
                break;
        }
    }

    public void AssignQuestToAdventurer(string questId, string adventurerId)
    {
        if (string.IsNullOrEmpty(questId) || string.IsNullOrEmpty(adventurerId))
            return;

        if (questsById.TryGetValue(questId, out Quest quest))
        {
            quest.AssignAdventurer(adventurerId);
            UpdateQuestCategory(quest);
            OnQuestStatusChanged?.Invoke(quest);
        }
    }

    public void CompleteQuest(string questId)
    {
        if (string.IsNullOrEmpty(questId))
            return;

        if (questsById.TryGetValue(questId, out Quest quest))
        {
            quest.CompleteQuest();
            UpdateQuestCategory(quest);
            OnQuestStatusChanged?.Invoke(quest);
        }
    }

    public void FailQuest(string questId)
    {
        if (string.IsNullOrEmpty(questId))
            return;

        if (questsById.TryGetValue(questId, out Quest quest))
        {
            quest.FailQuest();
            UpdateQuestCategory(quest);
            OnQuestStatusChanged?.Invoke(quest);
        }
    }

    public void RemoveQuest(string questId)
    {
        if (string.IsNullOrEmpty(questId) || !questsById.TryGetValue(questId, out Quest quest))
            return;

        allQuests.Remove(quest);
        activeQuests.Remove(quest);
        completedQuests.Remove(quest);
        failedQuests.Remove(quest);
        pendingQuests.Remove(quest);
        questsById.Remove(questId);
    }

    // Fast getters for different quest lists
    public IReadOnlyList<Quest> GetAllQuests() => allQuests;
    public IReadOnlyList<Quest> GetActiveQuests() => activeQuests;
    public IReadOnlyList<Quest> GetCompletedQuests() => completedQuests;
    public IReadOnlyList<Quest> GetFailedQuests() => failedQuests;
    public IReadOnlyList<Quest> GetPendingQuests() => pendingQuests;

    public Quest GetQuestById(string id)
    {
        if (string.IsNullOrEmpty(id))
            return null;

        questsById.TryGetValue(id, out Quest quest);
        return quest;
    }

    // Advanced filtering methods
    public List<Quest> GetQuestsByType(QuestType type)
    {
        return allQuests.Where(q => q.data.questType == type).ToList();
    }

    public List<Quest> GetQuestsByDifficulty(QuestDifficulty difficulty)
    {
        return allQuests.Where(q => q.data.difficulty == difficulty).ToList();
    }

    public List<Quest> GetQuestsByAdventurer(string adventurerId)
    {
        if (string.IsNullOrEmpty(adventurerId))
            return new List<Quest>();

        return allQuests.Where(q => q.data.assignedAdventurerId == adventurerId).ToList();
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }
}
