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
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            //GameObject adventurerObject = Instantiate(adventurerPrefab);

            //adventurerObject.transform.position = new Vector3(0, 0.9f, 1);
            //adventurerObject.transform.localScale = new Vector3(2f, 2f, 0);

            //var adventurerScript = adventurerObject.GetComponent<Adventurer>();
            //var builder = adventurerObject.GetComponent<AdventurerRenderer>();

            //adventurerScript.Init();
            //builder.BuildFrom(adventurerScript.instance);

            //foreach (var sr in adventurerObject.GetComponentsInChildren<SpriteRenderer>())
            //{
            //    sr.color = new Color(1, 1, 1, 0);
            //    StartCoroutine(FadeIn(sr, 0.1f));
            //}
            AddNewAdventurerToQueue();
        }
        if (Input.GetKeyDown(KeyCode.Q))
        {
            Script_QueueManager.Instance.ResumeQueue();
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
