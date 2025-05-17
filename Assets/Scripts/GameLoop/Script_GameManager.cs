using System.Linq;
using UnityEngine;
using GFD.Adventurer;
using GFD.Quest;

public class Script_GameManager : MonoBehaviour
{
    [SerializeField] 
    private GameObject adventurerPrefab;

    void Start()
    {
        Script_TimeManager.Instance.StartDay();
        Script_TimeManager.Instance.OnDayEnded += HandleDayEnded;
        Script_QuestManager.Instance.LoadQuestsAsync();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Script_QueueManager.Instance.ResumeQueue();
            
        }
        if (Input.GetKeyDown(KeyCode.Q))
        {
            AddNewAdventurerToQueue();
        }

    }

    private void HandleDayEnded()
    {
        Debug.Log("Day has ended!");    
    }

    public void AddNewAdventurerToQueue()
    {
        Adventurer newAdventurer = Script_QueueManager.Instance.GetNextAvailableAdventurer();
        Script_QueueManager.Instance.AddAdventurerToQueue(newAdventurer);
    }
    
    public void AssignQuestToCurrentAdventurer(QuestData quest)
    {
        var currentAdventurer = Script_QueueManager.Instance.GetCurrentAdventurer();
        if (currentAdventurer != null)
        {
            Script_QuestManager.Instance.AssignQuestToAdventurer(quest.questId, currentAdventurer.data.adventurerId);
            Script_AdventurerManager.Instance.MarkAdventurerAsBusy(currentAdventurer.data.adventurerId);
        }
    }

}
