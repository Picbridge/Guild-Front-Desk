using UnityEngine;
using LLMUnity;
using TMPro;
using System;
using System.Collections;
using GFD.Adventurer;

public class AdventurerChat : MonoBehaviour
{
    [Header("Adventurer Chat Settings")]
    [SerializeField]
    private LLMCharacter character;
    [SerializeField]
    private DialogueUI dialogueUI;

    private string response;

    public event Action OnReplyComplete;
    public event Action OnQuestAssigned;
    // Update is called once per frame
    void Start()
    {
        Script_QueueManager.Instance.OnAdventurerEntered += onAdventurerEnter;
        Script_QueueManager.Instance.PreAdventurerExit += onAdventurerExit;

        character.stop.Add("(");
    }

    private void onAdventurerEnter(Adventurer adventurer)
    {
        if (character == null) return;
        character.playerName = adventurer.data.adventurerName;
        character.SetPrompt(adventurer.data.prompt, true);
        character.gameObject.transform.Find("Canvas").gameObject.SetActive(true);
        character.ClearChat();
        string receptionistName = adventurer.data.numOfVisits > 1 ? "Hans" : "you've never met before";
        string reasonClause = adventurer.data.currentState == GFD.Adventurer.State.InQuest ?
        "You are here to report your quest success and claim your reward." :
        "You are here to ask for new missions.";

        string message =
        $"You have entered the guild office. A male receptionist {receptionistName} is present behind the desk.\n" +
        $"{reasonClause}" +
        "You may say whatever and how many paragraphs you want. It depends entirely on your personality and your relationship.\n" +
        "Respond as your character would in this moment. Only reply in-character with spoken dialogue.";


        response = "...";
        _ = character.Chat(message, SetAIText, AIReplyComplete);
    }

    private IEnumerator onAdventurerExit()
    {
        while (character != null)
        {
            character.CancelRequests();
            if (character.gameObject.transform.Find("Canvas").gameObject.activeSelf)
            {
                character.gameObject.transform.Find("Canvas").gameObject.SetActive(false);
                break;
            }
            yield return null;
        }
    }

    private void onRevealingPerposeOfVisit(Adventurer adventurer)
    {
        if (character == null) return;

        string receptionistName = "receptionist ";
        string perposeOfVisit = "are here to ";
        string adventureInjuryStatus = "";
        if (adventurer.data.numOfVisits > 1)
        {
            receptionistName += "Hans";
        }

        if (adventurer.data.currentState == GFD.Adventurer.State.InQuest)
        {
            perposeOfVisit += "You are here to report your quest success and claim your reward.";
        }
        else
        {
            perposeOfVisit += "You are here to ask for new missions.";
        }

        string message =
        $"{receptionistName} asks for your purpose of visit. You are {adventureInjuryStatus}{perposeOfVisit}.\n" +
        "Speak as your character would in this moment. Only respond in-character.";

        response = "...";
        _ = character.Chat(message, SetAIText, AIReplyComplete);
    }

    private void onConversation(string message)
    {
        response = "...";
        _ = character.Chat(message, SetAIText, AIReplyComplete);
    }

    private void onQuestAssigned(string message)
    {
        response = "...";
        _ = character.Chat(message, SetAIText, AIQuestAssignComplete);
    }

    private void SetAIText(string text)
    {
        int firstQuoteIndex = text.IndexOf('\"');
        int secondQuoteIndex = text.IndexOf('\"', firstQuoteIndex + 1);

        if (firstQuoteIndex >= 0 && secondQuoteIndex > firstQuoteIndex)
        {
            string result = text.Substring(firstQuoteIndex + 1, secondQuoteIndex - firstQuoteIndex - 1);
            response = result;

            character.CancelRequests();
            return;
        }

        if (firstQuoteIndex >= 0 && secondQuoteIndex == -1)
        {
            string result = text.Substring(firstQuoteIndex + 1);
            response = result;
            return;
        }
    }

    private void AIReplyComplete()
    {
        OnReplyComplete?.Invoke();
        Debug.Log("AI Reply Complete: " + response);
        dialogueUI.DisplayNextParagraph(SplitToParagraph());
    }

    private string[] SplitToParagraph()
    {
        if (string.IsNullOrEmpty(response))
        {
            return new string[] { "" };
        }
        // Split the response into paragraphs based on new lines
        string[] paragraphs = response.Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);
        // Display each paragraph in the AIText component
        response = string.Join("\n", paragraphs);
        return paragraphs;
    }

    private void AIQuestAssignComplete()
    {
        OnQuestAssigned?.Invoke();
    }
    private void OnDestroy()
    {
        // Unsubscribe from all events to prevent callbacks after this component is destroyed
        if (Script_QueueManager.Instance != null)
        {
            Script_QueueManager.Instance.OnAdventurerEntered -= onAdventurerEnter;
            Script_QueueManager.Instance.PreAdventurerExit -= onAdventurerExit;
        }
    }

}
