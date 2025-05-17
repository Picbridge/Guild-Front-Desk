//using UnityEngine;
//using LLMUnity;
//using TMPro;
//using System;
//using System.Collections;

//public class AdventurerChat : MonoBehaviour
//{
//    [SerializeField]
//    private LLMCharacter character;
//    [SerializeField]
//    private TMP_Text AIText;
//    public event Action OnReplyComplete;
//    public event Action OnQuestAssigned;

//    // Update is called once per frame
//    void Start()
//    {
//        Script_QueueManager.Instance.OnAdventurerEntered += onAdventurerEnter;
//        Script_QueueManager.Instance.PreAdventurerExit += onAdventurerExit;
//    }

//    void onAdventurerEnter(Adventurer adventurer)
//    {
//        Debug.Log("Adventurer entered: " + adventurer.data.adventurerName);
//        if (character == null) return;
//        Debug.Log("Starting conversation with: " + character.AIName);
//        character.gameObject.transform.Find("Canvas").gameObject.SetActive(true);
//        string message = "You are stepping into the guild and sees the receptionist.";
//        AIText.text = "...";
//        _ = character.Chat(message, SetAIText, AIReplyComplete);
//    }

//    private IEnumerator onAdventurerExit()
//    {
//        while (character != null)
//        {
//            if (character.gameObject.transform.Find("Canvas").gameObject.activeSelf)
//            {
//                character.gameObject.transform.Find("Canvas").gameObject.SetActive(false);
//                break;
//            }
//            yield return null;
//        }
//    }

//    void onConversation(string message)
//    {
//        AIText.text = "...";
//        _ = character.Chat(message, SetAIText, AIReplyComplete);
//    }

//    void onQuestAssigned(string message)
//    {
//        AIText.text = "...";
//        _ = character.Chat(message, SetAIText, AIQuestAssignComplete);
//    }

//    private void SetAIText(string text)
//    {
//        AIText.text = text;
//    }

//    private void AIReplyComplete()
//    {
//        OnReplyComplete?.Invoke();
//    }

//    private void AIQuestAssignComplete()
//    {
//        OnQuestAssigned?.Invoke();
//    }
//    private void OnDestroy()
//    {
//        // Unsubscribe from all events to prevent callbacks after this component is destroyed
//        if (Script_QueueManager.Instance != null)
//        {
//            Script_QueueManager.Instance.OnAdventurerEntered -= onAdventurerEnter;
//            Script_QueueManager.Instance.PreAdventurerExit -= onAdventurerExit;
//        }
//    }

//}
