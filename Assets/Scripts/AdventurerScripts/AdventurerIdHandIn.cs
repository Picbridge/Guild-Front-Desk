using GFD.Adventurer;
using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AdventurerIdHandIn : MonoBehaviour
{
    [SerializeField]
    private UI_TweenHelper adventurerIdTween;

    private UI_TweenHelper handInTween;

    private Script_QueueManager queueManager;
    private void Awake()
    {
        handInTween = GetComponent<UI_TweenHelper>();
        handInTween.PreClose += PreToggleAdventurerId;
        handInTween.PostOpen += ToggleAdventurerId;
    }

    void Start()
    {
        queueManager = Script_QueueManager.Instance;
        queueManager.OnAdventurerEntered += HandleAdventurerEntered;
        queueManager.PreAdventurerExit += HandleAdventurerExited;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void ToggleAdventurerId()
    {
        adventurerIdTween.ToggleOpenClose();
    }

    public IEnumerator PreToggleAdventurerId()
    {
        yield return new WaitForSeconds(0.05f);
        adventurerIdTween.ToggleOpenClose();
    }

    private void HandleAdventurerEntered(Adventurer adventurer)
    {
        // Only adventurers have adventurer IDs
        if (adventurer.data.adventurerType != AdventurerType.Adventurer)
            return;

        UpdateIdCardPanel(handInTween.gameObject, adventurer);
        UpdateIdCardPanel(adventurerIdTween.gameObject, adventurer);

        handInTween.ToggleOpenClose();
    }

    private void UpdateIdCardPanel(GameObject parent, Adventurer adventurer)
    {
        Transform idCardTransform = parent.transform.Find(GlobalConstants.adventurerIdDirectory + "IdCard");
        if (idCardTransform == null) return;

        GameObject idCard = idCardTransform.gameObject;

        SetTMPText(idCard, "Name", adventurer.data.adventurerName);
        SetTMPText(idCard, "Class", adventurer.data.adventurerClass.ToString());
        SetTMPText(idCard, "Gender", adventurer.data.gender.ToString());
        SetTMPText(idCard, "Race", adventurer.data.race.ToString());

        Image sealingWaxImage = idCard.transform.Find("SealingWax")?.GetComponent<Image>();
        if (sealingWaxImage != null)
        {
            sealingWaxImage.color = GetSealingWaxColor(adventurer.data.rank);
        }

        Transform profileTransform = parent.transform.Find(GlobalConstants.adventurerIdDirectory + "Profile");
        if (profileTransform != null)
        {
            Image profileImage = profileTransform.GetComponent<Image>();
            if (profileImage != null)
            {
                profileImage.sprite = Resources.Load<Sprite>(GlobalConstants.spriteDirectory + adventurer.data.portrait);
            }
        }
    }

    private void SetTMPText(GameObject parent, string childName, string value)
    {
        TMP_Text textComponent = parent.transform.Find(childName)?.GetComponent<TMP_Text>();
        if (textComponent != null)
        {
            textComponent.text = value;
        }
    }

    private Color GetSealingWaxColor(Rank rank)
    {
        return rank switch
        {
            Rank.Bronze => new Color(0.545f, 0.271f, 0.075f),
            Rank.Silver => new Color(0.726f, 0.726f, 0.726f),
            Rank.Gold => new Color(0.99f, 0.9f, 0.36f),
            Rank.Diamond => new Color(0.7f, 1f, 0.97f),
            Rank.Adamantine => new Color(0.73f, 0.46f, 1f),
            _ => Color.white
        };
    }

    public IEnumerator HandleAdventurerExited()
    {
        yield return new WaitForSeconds(0.05f);
        handInTween.ToggleOpenClose();
    }
}
