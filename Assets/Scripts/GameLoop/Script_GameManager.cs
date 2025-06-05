using System.Linq;
using UnityEngine;
using GFD.Adventurer;
using GFD.Quest;

public class Script_GameManager : MonoBehaviour
{
    public static Script_GameManager Instance { get; private set; }

    private Script_TimeManager timeManager;
    private Script_QuestManager questManager;
    private Script_QueueManager queueManager;
    private Script_AdventurerManager adventurerManager;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    void Start()
    {
        timeManager = Script_TimeManager.Instance;
        questManager = Script_QuestManager.Instance;
        queueManager = Script_QueueManager.Instance;
        adventurerManager = Script_AdventurerManager.Instance;

        timeManager.OnDayEnded += HandleDayEnded;
        questManager.OnQuestPopulated += HandleQuestPopulated;
        questManager.LoadQuestsAsync();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            queueManager.ResumeQueue();
            
        }
        if (Input.GetKeyDown(KeyCode.Q))
        {
            timeManager.StartDay();
        }

    }

    // This point is the entry point for the game loop
    private void HandleQuestPopulated()
    {
        timeManager.StartDay();
    }

    private void HandleDayEnded()
    {
        Debug.Log("Day has ended!");    
    }

    public void AddNewAdventurerToQueue()
    {
        Adventurer newAdventurer = queueManager.GetNextAvailableAdventurer();
    }
   
}
