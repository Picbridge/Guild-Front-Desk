using UnityEngine;
using GFD.Adventurer;
using System;

[CreateAssetMenu(fileName = "New Adventurer", menuName = "Guild/Adventurer")]
public class AdventurerSO : ScriptableObject
{
    public AdventurerData data;

    private void OnEnable()
    {

            data.adventurerId = Guid.NewGuid().ToString();
#if UNITY_EDITOR
            UnityEditor.EditorUtility.SetDirty(this);
#endif
    }

    // Instead of showing a hidden integer field, let's add a read-only display
#if UNITY_EDITOR
    private void OnValidate()
    {
        data.adventurerId = Guid.NewGuid().ToString();
        UnityEditor.EditorUtility.SetDirty(this);
    }
#endif
}
