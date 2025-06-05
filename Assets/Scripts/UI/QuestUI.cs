using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using Unity.VisualScripting.Antlr3.Runtime;
using System.Collections.Generic;
using System.Linq;

public class QuestUI : MonoBehaviour
{
    private Script_QuestManager questManager;
    private Script_TimeManager timeManager;
    private Script_GameManager gameManager;
    private Script_QueueManager queueManager;
    private Script_AdventurerManager adventurerManager;
    
    [Header("UI Elements")]
    [SerializeField]
    private GameObject questButtonPrefab;
    [SerializeField]
    private GameObject questPanel;
    [SerializeField] 
    private GameObject descPanel;
    [SerializeField]
    private GameObject listPanel;
    [SerializeField]
    private Button assignButton;

    [Header("Button References")]
    [SerializeField]
    private Button questToggleButton;
    [SerializeField]
    private Button questDescriptionButton;
    [SerializeField]
    private Button activeQuestsButton;
    [SerializeField]
    private Button completedQuestsButton;
    [SerializeField]
    private Button failedQuestsButton;

    [Header("Animation Sprites")]
    [SerializeField]
    private Sprite[] questPanelAnimation;
    [SerializeField]
    private Sprite[] signatureAnimation;
    
    [SerializeField]
    private GameObject listedQuestDescPanel;

    private GameObject questListContent;
    private UI_TweenHelper tweenHelper;
    private int totalQuestCount = 0;
    private int assignedQuestCount = 0;
    private Quest currentQuest;
    private bool isQuestPanelVisible = false;
    private Coroutine animationCoroutine;
    private Button currentButton;
    private Button currOpenedQuest = null;

    [ContextMenu("Toggle Open Close")]
    public void ToggleOpenClose()
    {
        if (isQuestPanelVisible)
        {
            StartCoroutine(HideQuestPanel());
        }
        else
        {
            ShowQuestPanel();
        }
    }
    void Start()
    {
        questManager = Script_QuestManager.Instance;
        timeManager = Script_TimeManager.Instance;
        gameManager = Script_GameManager.Instance;
        queueManager = Script_QueueManager.Instance;
        adventurerManager = Script_AdventurerManager.Instance;

        questListContent =listPanel.transform.Find("Viewport/Content").gameObject;
        tweenHelper = gameObject.GetComponent<UI_TweenHelper>();
        currentButton = questDescriptionButton;

        questManager.OnDailyQuestUpdated += PopulateQuestList;
        tweenHelper.PreClose += HideQuestPanel;
        tweenHelper.PostOpen += ShowQuestPanel;

        TogglePanelVisibility(listPanel, false);
        TogglePanelVisibility(descPanel, false);
        TogglePanelVisibility(listedQuestDescPanel, false);

        assignButton.onClick.AddListener(OnAssignButtonClick);

        questToggleButton.onClick.AddListener(() =>
        {
            if (queueManager.GetCurrentAdventurer().data.currentState != GFD.Adventurer.State.InQuest)
                tweenHelper.ToggleOpenClose();
        });

        questDescriptionButton.onClick.AddListener(() =>
        {
            Debug.Log("Quest Description button clicked.");
            if (currentButton == questDescriptionButton)
                return;
            StartCoroutine(HideSwapShowSequence(currentButton.transform, questDescriptionButton.transform));
            currentButton = questDescriptionButton;
        });
        activeQuestsButton.onClick.AddListener(() =>
        {
            Debug.Log("Active Quests button clicked.");
            if (currentButton == activeQuestsButton)
                return;
            StartCoroutine(HideSwapShowSequence(currentButton.transform, activeQuestsButton.transform));
            currentButton = activeQuestsButton;
        });
        completedQuestsButton.onClick.AddListener(() =>
        {
            Debug.Log("Completed Quests button clicked.");
            if (currentButton == completedQuestsButton)
                return;
            StartCoroutine(HideSwapShowSequence(currentButton.transform, completedQuestsButton.transform));
            currentButton = completedQuestsButton;
        });
        failedQuestsButton.onClick.AddListener(() =>
        {
            Debug.Log("Failed Quests button clicked.");
            if (currentButton == failedQuestsButton)
                return;
            StartCoroutine(HideSwapShowSequence(currentButton.transform, failedQuestsButton.transform));
            currentButton = failedQuestsButton;
        });
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.O))
        {
            ToggleOpenClose();
        }
        if (Input.GetKeyDown(KeyCode.P))
        {
            tweenHelper.ToggleOpenClose();
        }
    }

    private void FillQuestList(Button currButton)
    {
        if (currButton == questDescriptionButton)
        {
            // Fill quest description
            UpdateQuestData();
        }
        else
        {

            // clear previous entries
            foreach (Transform child in questListContent.transform)
                Destroy(child.gameObject);

            IEnumerable<Quest> source = currButton == activeQuestsButton
                ? questManager.GetActiveQuests()
                : currButton == completedQuestsButton
                    ? questManager.GetCompletedQuests()
                    : questManager.GetFailedQuests();
            
            Debug.Log($"Filling quest list with {source.Count()} quests.");

            foreach (var quest in source)
            {
                Debug.Log($"Quest {quest.data.questName} added to list.");
                GameObject buttonGO = Instantiate(questButtonPrefab, questListContent.transform);
                buttonGO.name = $"Button_{quest.data.questId}";

                TMP_Text text = buttonGO.GetComponentInChildren<TMP_Text>();
                if (text != null)
                    text.text = quest.data.questName;

                Button button = buttonGO.GetComponent<Button>();
                if (button != null)
                {
                    button.onClick.AddListener(() =>
                    {
                        Debug.Log($"Clicked Quest: {quest.data.questName}");

                        bool isSameQuest = currOpenedQuest == button;

                        if (isSameQuest)
                        {
                            currOpenedQuest = null;
                            listedQuestDescPanel.GetComponent<UI_TweenHelper>().ToggleOpenClose();
                        }
                        else
                        {
                            StartCoroutine(OpenQuestPanelAfterClose(button, quest));
                        }
                    });
                }
            }
        }
    }

    private IEnumerator OpenQuestPanelAfterClose(Button button, Quest quest)
    {
        if (currOpenedQuest != null)
        {
            listedQuestDescPanel.GetComponent<UI_TweenHelper>().ToggleOpenClose();

            float duration = listedQuestDescPanel.GetComponent<UI_TweenHelper>().AnimationDuration;
            yield return new WaitForSeconds(duration + 0.05f);
        }

        currOpenedQuest = button;

        var assignedAdventurer = adventurerManager.GetAdventurerById(quest.data.assignedAdventurerId);

        var panelTransform = listedQuestDescPanel.transform;
        panelTransform.Find("Name").GetComponent<TMP_Text>().text = quest.data.questName;
        panelTransform.Find("Description").GetComponent<TMP_Text>().text = quest.data.description;
        panelTransform.Find("Type").GetComponent<TMP_Text>().text = quest.data.questType.ToString();
        panelTransform.Find("Difficulty").GetComponent<TMP_Text>().text = quest.data.difficulty.ToString();

        if (!string.IsNullOrEmpty(quest.data.assignedAdventurerId))
            panelTransform.Find("Assigned").GetComponent<TMP_Text>().text = assignedAdventurer.adventurerName;
        listedQuestDescPanel.GetComponent<UI_TweenHelper>().ToggleOpenClose();
    }

    private void TogglePanelVisibility(GameObject panel, bool isVisible)
    {
        if (panel.GetComponent<CanvasGroup>() == null)
        {
            panel.AddComponent<CanvasGroup>();
        }
        panel.GetComponent<CanvasGroup>().alpha = isVisible ? 1 : 0;
        panel.GetComponent<CanvasGroup>().interactable = isVisible;
        panel.GetComponent<CanvasGroup>().blocksRaycasts = isVisible;
    }

    private IEnumerator HideSwapShowSequence(Transform prevButton, Transform currButton)
    {
        TogglePanelVisibility(listPanel, false);

        yield return StartCoroutine(HideQuestPanel());

        yield return StartCoroutine(gameObject.GetComponent<UI_FolderFlipAnimation>().SwapCoroutine(prevButton, currButton));

        FillQuestList(currButton.GetComponent<Button>());
        
        ShowQuestPanel();

        TogglePanelVisibility(listPanel, true);
    }

    private void OnAssignButtonClick()
    {
        if (assignedQuestCount >= totalQuestCount)
        {
            Debug.Log("All quests assigned for the day.");
            return;
        }
        assignedQuestCount++;
        timeManager.SpendTime();
        tweenHelper.ToggleOpenClose();
        queueManager.AssignQuestToCurrentAdventurer(currentQuest);

        UpdateQuestData();
        queueManager.ResumeQueue();
    }

    private void PopulateQuestList()
    {
        totalQuestCount = questManager.CurrentDailyQuestCount;
        assignedQuestCount = 0;
        UpdateQuestData();
    }

    private void UpdateQuestData()
    {
        currentQuest = questManager.GetNextQuestOfTheDay();
        var panelTransform = descPanel.transform;
        if (currentQuest == null)
        {
            panelTransform.Find("Name").GetComponent<TMP_Text>().text = "";
            panelTransform.Find("Description").GetComponent<TMP_Text>().text = "";
            panelTransform.Find("Type").GetComponent<TMP_Text>().text = "";
            panelTransform.Find("Difficulty").GetComponent<TMP_Text>().text = "";
            Debug.Log("No quests available for the day.");
            return;
        }
        
        panelTransform.Find("Name").GetComponent<TMP_Text>().text = currentQuest.data.questName;
        panelTransform.Find("Description").GetComponent<TMP_Text>().text = currentQuest.data.description;
        panelTransform.Find("Type").GetComponent<TMP_Text>().text = currentQuest.data.questType.ToString();
        panelTransform.Find("Difficulty").GetComponent<TMP_Text>().text = currentQuest.data.difficulty.ToString();
    }

    public void ShowQuestPanel()
    {
        if (isQuestPanelVisible)
            return;

        isQuestPanelVisible = true;
        if (animationCoroutine != null)
        {
            StopCoroutine(animationCoroutine);
        }
        StartCoroutine(AnimateQuestPanel(true));
    }
    public IEnumerator HideQuestPanel()
    {
        if (!isQuestPanelVisible)
            yield break;

        if (currOpenedQuest != null)
        {
            listedQuestDescPanel.GetComponent<UI_TweenHelper>().ToggleOpenClose();
            currOpenedQuest = null;
        }

        isQuestPanelVisible = false;
        if (animationCoroutine != null)
        {
            StopCoroutine(animationCoroutine);
        }
        yield return AnimateQuestPanel(false);
    }

    private IEnumerator AnimateQuestPanel(bool open)
    {
        Image panelImage = questPanel.GetComponent<Image>();
        
        if (!open)
        {
            TogglePanelVisibility(descPanel, false);
            TogglePanelVisibility(listPanel, false);
        }

        if (open)
        {
            panelImage.sprite = questPanelAnimation[0];
            for (int i = 1; i < questPanelAnimation.Length; i++)
            {
                panelImage.sprite = questPanelAnimation[i];
                yield return new WaitForSeconds(0.05f);
            }
            
        }
        else
        {
            for (int i = questPanelAnimation.Length - 1; i >= 0; i--)
            {
                panelImage.sprite = questPanelAnimation[i];
                yield return new WaitForSeconds(0.05f);
            }
        }

        if (open)
        {
            UpdateActiveSubPanel();
        }
    }
    private void UpdateActiveSubPanel()
    {
        bool showDesc = currentButton == questDescriptionButton;
        TogglePanelVisibility(listPanel, !showDesc);
        TogglePanelVisibility(descPanel, showDesc);
    }

}
