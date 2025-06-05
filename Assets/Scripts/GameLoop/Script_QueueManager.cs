using GFD.Adventurer;
using System;
using System.CodeDom.Compiler;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Script_QueueManager : MonoBehaviour
{
    public static Script_QueueManager Instance { get; private set; }

    [Header("Queue Manager Settings")]
    [SerializeField] 
    private GameObject adventurerPrefab;
    [SerializeField] 
    private AdventurerSO Guard;

    [Header("Fade Settings")]
    [SerializeField] 
    private float fadeInDuration = 1f;
    [SerializeField] 
    private float fadeOutDuration = 1f;
    
    private Queue<Adventurer> adventurerQueue = new Queue<Adventurer>();
    private Adventurer currentAdventurer;
    private bool isProcessingQueue = false;
    
    private Script_AdventurerManager adventurerManager;
    private Script_QuestManager questManager;

    public event Func<IEnumerator> PreAdventurerExit;
    public event Action<Adventurer> OnAdventurerEntered;
    public event Action<Adventurer> OnAdventurerExited;

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

    private void Start()
    {
        adventurerManager = Script_AdventurerManager.Instance;
        questManager = Script_QuestManager.Instance;

        questManager.OnDailyQuestUpdated += ResumeQueue;
        Script_TimeManager.Instance.OnDayStarted += AddNPCtoQueue;
        OnAdventurerEntered += HandleAdventurerEntered;
        OnAdventurerExited += HandleAdventurerExited;
        //StartCoroutine(ProcessQueue());
    }
    private void AddNPCtoQueue()
    {

        var lists = questManager.GetDailyDeadInActions();

        if (lists.Count > 0)
        {
            // return guard to inform player that there several adventurers died in quests
            // 
            var generatedGuard = GenerateAdventurer(Guard.data);
            AddAdventurerToQueue(generatedGuard);
        }
    }
    public Adventurer GetNextAvailableAdventurer()
    {
        // Check if there's any adventurers that completed their quest
        var nextAdventurerData = adventurerManager.GetCompletedAdventurer() ?? 
            adventurerManager.GetAvailableAdventurers().FirstOrDefault(
                a => a.currentState != GFD.Adventurer.State.InQuest);

        var nextAdventurer = GenerateAdventurer(nextAdventurerData);
        AddAdventurerToQueue(nextAdventurer);
        return nextAdventurer;
    }
    public void StopQueue()
    {
        isProcessingQueue = false;
    }

    public void ResumeQueue()
    {
        if (!isProcessingQueue)
        {
            if (questManager.GetQuestsOfTheDay().Count > 0 &&
                adventurerQueue.Count <= 0)
            {
                GetNextAvailableAdventurer();
            }
            StartCoroutine(ProcessQueue());
        }
    }
    public Adventurer GetCurrentAdventurer()
    {
        return currentAdventurer;
    }

    public void AddAdventurerToQueue(AdventurerData adventurerData)
    {
        adventurerQueue.Enqueue(GenerateAdventurer(adventurerData));
    }

    public void AddAdventurerToQueue(Adventurer adventurer)
    {
        adventurerQueue.Enqueue(adventurer);
        adventurerManager.RemoveFromAvailableAdventurers(adventurer.data);
        //Debug.Log(adventurer.data.prompt);
    }
    public void AssignQuestToCurrentAdventurer(Quest quest)
    {
        if (currentAdventurer != null)
        {
            questManager.AssignQuestToAdventurer(quest.data.questId, currentAdventurer.data.adventurerId);
            adventurerManager.MarkAdventurerAsBusy(currentAdventurer.data.adventurerId);
        }
    }

    private Adventurer GenerateAdventurer(AdventurerData adventurerData)
    {
        GameObject gameObject = Instantiate(adventurerPrefab);
        SpriteRenderer renderer = gameObject.GetComponent<SpriteRenderer>();
        Adventurer adventurer = gameObject.GetComponent<Adventurer>();
        gameObject.transform.position = new Vector3(-0.21f, 1.1f, 1);
        gameObject.transform.localScale = new Vector3(.4f, .4f, 1);

        // Generate a new adventurer and add to the adventurer database if no adventurer available
        if (adventurerData == null)
        {
            adventurer.GenerateRandomAdventurer();
            adventurerManager.AddAdventurer(adventurer.data);
        }
        else
        {
            adventurerManager.RemoveFromAvailableAdventurers(adventurerData);
            adventurer.CopyFrom(adventurerData);
        }

        string portraitName = adventurer.data.portrait;
        string resourcePath = GlobalConstants.spriteDirectory + portraitName;
        Sprite loadedSprite = Resources.Load<Sprite>(resourcePath);

        if (loadedSprite == null)
        {
            Debug.LogError($"Sprite not found at path: {resourcePath}. Check the portrait name and Resources folder.");
        }
        gameObject.GetComponent<SpriteRenderer>().sprite = loadedSprite;
        gameObject.SetActive(false);

        //Debug.Log("Generated Adventurer: " + adventurer.data.adventurerName);
        return adventurer;
    }
    
    private IEnumerator ProcessQueue()
    {
        isProcessingQueue = true;
        
        while (isProcessingQueue)
        {
            if (adventurerQueue.Count > 0)
            {
                if (currentAdventurer != null)
                {
                    if (PreAdventurerExit != null)
                    {
                        foreach (Func<IEnumerator> handler in PreAdventurerExit.GetInvocationList())
                        {
                            yield return StartCoroutine(handler());
                        }
                    }

                    // Fade out current adventurer
                    yield return StartCoroutine(FadeAdventurer(false));
                    // Notify that adventurer has exited
                    OnAdventurerExited?.Invoke(currentAdventurer);
                }

                // Get next adventurer from the queue
                currentAdventurer = adventurerQueue.Dequeue();
                currentAdventurer.gameObject.SetActive(true);
                // Fade in
                yield return StartCoroutine(FadeAdventurer(true));
                
                // Notify that adventurer has entered
                OnAdventurerEntered?.Invoke(currentAdventurer);
            }
            else
            {
                if (currentAdventurer != null)
                {
                    if (PreAdventurerExit != null)
                    {
                        foreach (Func<IEnumerator> handler in PreAdventurerExit.GetInvocationList())
                        {
                            yield return StartCoroutine(handler());
                        }
                    }

                    yield return StartCoroutine(FadeAdventurer(false));
                    OnAdventurerExited?.Invoke(currentAdventurer);
                }
                else
                {
                    Debug.Log("No available adventurers");
                    isProcessingQueue = false;
                }

            }
            yield return new WaitForSeconds(0.1f);
        }
    }
    
    private IEnumerator FadeAdventurer(bool fadeIn)
    {
        float duration = fadeIn ? fadeInDuration : fadeOutDuration;
        float startAlpha = fadeIn ? 0f : 1f;
        float endAlpha = fadeIn ? 1f : 0f;
        float elapsedTime = 0f;
        
        SpriteRenderer adventurerVisual = currentAdventurer.GetComponent<SpriteRenderer>();

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float alpha = Mathf.Lerp(startAlpha, endAlpha, elapsedTime / duration);
            adventurerVisual.color = new Color(adventurerVisual.color.r, adventurerVisual.color.g, adventurerVisual.color.b, alpha);
            yield return null;
        }
    }

    private void HandleAdventurerEntered(Adventurer adventurer)
    {
        //Debug.Log(adventurer.data.adventurerName + "has entered");
        StopQueue();
    }

    private void HandleAdventurerExited(Adventurer adventurer)
    {
        //Debug.Log(adventurer.data.adventurerName + "has exited");
        currentAdventurer = null;
        Destroy(adventurer.gameObject);
    }
}
