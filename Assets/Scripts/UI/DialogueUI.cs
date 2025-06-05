using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem; 
public class DialogueUI : MonoBehaviour
{
    [SerializeField]
    private TMPro.TextMeshProUGUI dialogueText;

    private Queue<string> paragraphs = new Queue<string>();
    private bool isConversationEnded = false;

    private string p;
    public void DisplayNextParagraph(string[] dialogue)
    {
        if (paragraphs.Count == 0)
        {
            if (!isConversationEnded)
            {
                StartConversation(dialogue);
            }
            else
            {
                EndConversation();
            }
        } 

        p = paragraphs.Dequeue();

        dialogueText.text = p;

        if (paragraphs.Count == 0)
        {
            isConversationEnded = true;
        }
    }

    private void StartConversation(string[] dialogue)
    {
        if (!gameObject.activeSelf)
        {
            // Activate the dialogue UI if it is not already active
            gameObject.SetActive(true);
        }
        for (int i = 0; i < dialogue.Length; i++)
        {
            paragraphs.Enqueue(dialogue[i]);
        }
    }
    private void EndConversation()
    {
        // Clear the dialogue queue
        paragraphs.Clear();

        isConversationEnded = false;
        if (gameObject.activeSelf)
        {
            gameObject.SetActive(false);
        }
    }
}
