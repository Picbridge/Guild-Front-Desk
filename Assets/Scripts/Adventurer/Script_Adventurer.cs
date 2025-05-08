using UnityEngine;

public class Script_Adventurer : MonoBehaviour
{
    public AdventurerInstance instance;

    public void Init()
    {
        instance = new AdventurerInstance();
        Debug.Log(instance.GetPrompt());

        // Optionally assign sprite to UI here
        // or use instance.portrait for visual
    }
}
