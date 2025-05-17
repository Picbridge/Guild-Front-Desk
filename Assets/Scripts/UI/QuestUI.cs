using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using Unity.VisualScripting.Antlr3.Runtime;

public class QuestUI : MonoBehaviour
{
    private Script_QuestManager questManager;

    [SerializeField]
    private RectTransform questList;
    [SerializeField]
    private GameObject questButtonPrefab;
    [SerializeField]
    private GameObject questPanel;
    [SerializeField]
    private Sprite[] questPanelAnimation;
    [SerializeField]
    private UI_TweenHelper tweenHelper;
    [SerializeField]
    private GameObject questDescriptionPanel;

    [ContextMenu("Toggle Open Close")]
    public void ToggleOpenClose()
    {
        if (isQuestPanelVisible)
        {
            HideQuestPanel();
        }
        else
        {
            ShowQuestPanel();
        }
    }
    private bool isQuestPanelVisible = false;
    private Coroutine animationCoroutine;
    Button currOpenedQuest = null;

    void Start()
    {
        questManager = Script_QuestManager.Instance;
        questManager.OnQuestPopulated += PopulateQuestList;
        questList.gameObject.SetActive(false);
        tweenHelper.PreClose += HideQuestPanel;
        tweenHelper.PostOpen += ShowQuestPanel;
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
    void PopulateQuestList()
    {
        //Debug.Log("Populating Quest List");

        foreach (Transform child in questList)
        {
            Destroy(child.gameObject); // Clear existing
        }

        foreach (var quest in questManager.GetAllQuests())
        {
            GameObject buttonGO = Instantiate(questButtonPrefab, questList);
            buttonGO.name = $"Button_{quest.data.questId}";

            TMP_Text text = buttonGO.GetComponentInChildren<TMP_Text>();
            if (text != null)
                text.text = quest.data.questName;

            Button button = buttonGO.GetComponent<Button>();
            if (button != null)
            {
                button.onClick.AddListener(() =>
                {
                    //Debug.Log($"Clicked Quest: {quest.data.questName}");

                    bool isSameQuest = currOpenedQuest == button;

                    if (isSameQuest)
                    {
                        currOpenedQuest = null;
                        questDescriptionPanel.GetComponent<UI_TweenHelper>().ToggleOpenClose();
                    }
                    else
                    {
                        StartCoroutine(OpenQuestPanelAfterClose(button, quest));
                    }
                });
            }


        }
    }
    private IEnumerator OpenQuestPanelAfterClose(Button button, Quest quest)
    {
        if (currOpenedQuest != null)
        {
            questDescriptionPanel.GetComponent<UI_TweenHelper>().ToggleOpenClose();

            float duration = questDescriptionPanel.GetComponent<UI_TweenHelper>().AnimationDuration; 
            yield return new WaitForSeconds(duration + 0.05f);
        }

        currOpenedQuest = button;

        var panelTransform = questDescriptionPanel.transform;
        panelTransform.Find("Name").GetComponent<TMP_Text>().text = quest.data.questName;
        panelTransform.Find("Description").GetComponent<TMP_Text>().text = quest.data.description;
        panelTransform.Find("Type").GetComponent<TMP_Text>().text = quest.data.questType.ToString();
        panelTransform.Find("Difficulty").GetComponent<TMP_Text>().text = quest.data.difficulty.ToString();

        questDescriptionPanel.GetComponent<UI_TweenHelper>().ToggleOpenClose();
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
            questDescriptionPanel.GetComponent<UI_TweenHelper>().ToggleOpenClose();
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
            questList.gameObject.SetActive(false);
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
            questList.gameObject.SetActive(true);
        }
    }

}
