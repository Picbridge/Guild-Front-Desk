using UnityEngine;
using GFD.Quest;
using System;

[CreateAssetMenu(fileName = "New Quest", menuName = "Guild/Quest")]
public class QuestSO : ScriptableObject
{
    public QuestData data;
    private void OnEnable()
    {

        data.questId = Guid.NewGuid().ToString();
#if UNITY_EDITOR
        UnityEditor.EditorUtility.SetDirty(this);
#endif
    }

    // Instead of showing a hidden integer field, let's add a read-only display
#if UNITY_EDITOR
    private void OnValidate()
    {
        data.questId = Guid.NewGuid().ToString();
        UnityEditor.EditorUtility.SetDirty(this);
    }
#endif
}
