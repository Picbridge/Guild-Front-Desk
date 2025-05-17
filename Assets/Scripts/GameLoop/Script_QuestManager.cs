using GFD.Quest;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System.IO;
using Newtonsoft.Json;
using System.Collections;

public class Script_QuestManager : MonoBehaviour
{
    [Serializable]
    private class RawQuest
    {
        public string name;
        public string description;
        public string difficulty;
        public string type;
        public int reward;
        public int duration;
    }

    [SerializeField] private TextAsset questJsonFile;

    public static Script_QuestManager Instance { get; private set; }

    private Dictionary<string, Quest> questsById = new Dictionary<string, Quest>();

    private List<Quest> allQuests = new List<Quest>();
    private List<Quest> activeQuests = new List<Quest>();
    private List<Quest> completedQuests = new List<Quest>();
    private List<Quest> failedQuests = new List<Quest>();
    private List<Quest> pendingQuests = new List<Quest>();

    public event Action<Quest> OnQuestCreated;
    public event Action<Quest> OnQuestStatusChanged;
    public event Action OnQuestPopulated;

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

    public void LoadQuestsAsync()
    {
        StartCoroutine(LoadAndProcessQuestsCoroutine());
    }

    private IEnumerator LoadAndProcessQuestsCoroutine()
    {
        if (questJsonFile == null)
        {
            Debug.LogError("No quest JSON file assigned.");
            yield break;
        }

        string jsonContent = questJsonFile.text;
        List<RawQuest> rawQuests = null;
        bool done = false;

        // Run parsing in background thread
        System.Threading.Tasks.Task.Run(() =>
        {
            rawQuests = JsonConvert.DeserializeObject<List<RawQuest>>(jsonContent);
            done = true;
        });

        // Wait until JSON is parsed
        while (!done)
            yield return null;

        // Process parsed quests in coroutine batches
        yield return StartCoroutine(ProcessParsedQuests(rawQuests));
    }

    private IEnumerator ProcessParsedQuests(List<RawQuest> rawQuests)
    {
        const int batchSize = 10;
        int processed = 0;

        foreach (var raw in rawQuests)
        {
            var difficulty = ParseDifficulty(raw.difficulty);

            var data = new QuestData
            {
                questId = Guid.NewGuid().ToString(),
                questName = raw.name,
                description = raw.description,
                questType = ParseQuestType(raw.type),
                difficulty = difficulty,
                status = QuestStatus.Pending,
                assignedAdventurerId = string.Empty,
                reward = CalculateReward(difficulty),
                duration = GetMaxDuration(difficulty)
            };

            Quest quest = new Quest { data = data };
            AddQuest(quest);

            processed++;
            if (processed % batchSize == 0)
                yield return null;
        }

        //Debug.Log($"[Hybrid Load] Loaded {processed} quests.");
        OnQuestPopulated?.Invoke();
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

    public void AddQuestToLists(Quest quest)
    {
        allQuests.Add(quest);
        questsById[quest.data.questId] = quest;
        UpdateQuestCategory(quest);

        OnQuestCreated?.Invoke(quest);
    }

    private void UpdateQuestCategory(Quest quest)
    {
        activeQuests.Remove(quest);
        completedQuests.Remove(quest);
        failedQuests.Remove(quest);
        pendingQuests.Remove(quest);

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

    private static QuestType ParseQuestType(string typeString)
    {
        if (Enum.TryParse<QuestType>(typeString, true, out var result))
            return result;

        return QuestType.Elimination;
    }

    private static QuestDifficulty ParseDifficulty(string difficultyString)
    {
        if (Enum.TryParse<QuestDifficulty>(difficultyString, true, out var result))
            return result;

        return QuestDifficulty.Normal;
    }

    private static int CalculateReward(QuestDifficulty difficulty)
    {
        return difficulty switch
        {
            QuestDifficulty.Easy => UnityEngine.Random.Range(5, 20),
            QuestDifficulty.Normal => UnityEngine.Random.Range(15, 30),
            QuestDifficulty.Hard => UnityEngine.Random.Range(25, 50),
            QuestDifficulty.Deadly => UnityEngine.Random.Range(45, 70),
            _ => 100
        };
    }

    private static int GetMaxDuration(QuestDifficulty difficulty)
    {
        return difficulty switch
        {
            QuestDifficulty.Easy => UnityEngine.Random.Range(1, 3),
            QuestDifficulty.Normal => UnityEngine.Random.Range(2, 4),
            QuestDifficulty.Hard => UnityEngine.Random.Range(3, 6),
            QuestDifficulty.Deadly => UnityEngine.Random.Range(4, 8),
            _ => 2
        };
    }
}
