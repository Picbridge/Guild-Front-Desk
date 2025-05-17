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
    private void Awake()
    {
        handInTween = GetComponent<UI_TweenHelper>();
        handInTween.PreClose += PreToggleAdventurerId;
        handInTween.PostOpen += ToggleAdventurerId;
    }

    void Start()
    {
        Script_QueueManager.Instance.OnAdventurerEntered += HandleAdventurerEntered;
        Script_QueueManager.Instance.PreAdventurerExit += HandleAdventurerExited;
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
        Transform idCardTransform = handInTween.gameObject.transform.Find(GlobalConstants.adventurerIdDirectory + "IdCard");
        GameObject idCard = idCardTransform != null ? idCardTransform.gameObject : null;

        idCard.transform.Find("Name").GetComponent<TMP_Text>().text = adventurer.data.adventurerName;
        idCard.transform.Find("Class").GetComponent<TMP_Text>().text = adventurer.data.adventurerClass.ToString();
        idCard.transform.Find("Gender").GetComponent<TMP_Text>().text = adventurer.data.gender.ToString();
        idCard.transform.Find("Race").GetComponent<TMP_Text>().text = adventurer.data.race.ToString();

        Color sealingWaxColor = Color.white;

        switch (adventurer.data.rank)
        {
            case Rank.Bronze:
                sealingWaxColor = new Color(0.545f, 0.271f, 0.075f); 
                break;
            case Rank.Silver:
                sealingWaxColor = new Color(0.726f, 0.726f, 0.726f);
                break;
            case Rank.Gold:
                sealingWaxColor = new Color(0.99f, 0.9f, 0.36f);
                break;
            case Rank.Platinum:
                sealingWaxColor = new Color(0.971f, 0.971f, 0.971f);
                break;
            case Rank.Diamond:
                sealingWaxColor = new Color(0.7f, 1, 0.97f);
                break;
            case Rank.Adamantium:
                sealingWaxColor = new Color(0.73f, 0.46f, 1);
                break;
        }

        Transform profileTransform = handInTween.gameObject.transform.Find(GlobalConstants.adventurerIdDirectory + "Profile");
        GameObject profile = profileTransform != null ? profileTransform.gameObject : null;
        profile.GetComponent<Image>().sprite = Resources.Load<Sprite>(GlobalConstants.spriteDirectory + adventurer.data.portrait);

        idCard.transform.Find("SealingWax").GetComponent<Image>().color = sealingWaxColor;

        idCardTransform = adventurerIdTween.gameObject.transform.Find(GlobalConstants.adventurerIdDirectory + "IdCard");
        idCard = idCardTransform != null ? idCardTransform.gameObject : null;
        idCard.transform.Find("Name").GetComponent<TMP_Text>().text = adventurer.data.adventurerName;
        idCard.transform.Find("Class").GetComponent<TMP_Text>().text = adventurer.data.adventurerClass.ToString();
        idCard.transform.Find("Gender").GetComponent<TMP_Text>().text = adventurer.data.gender.ToString();
        idCard.transform.Find("Race").GetComponent<TMP_Text>().text = adventurer.data.race.ToString();

        switch (adventurer.data.rank)
        {
            case Rank.Bronze:
                sealingWaxColor = new Color(0.545f, 0.271f, 0.075f);
                break;
            case Rank.Silver:
                sealingWaxColor = new Color(0.726f, 0.726f, 0.726f);
                break;
            case Rank.Gold:
                sealingWaxColor = new Color(0.99f, 0.9f, 0.36f);
                break;
            case Rank.Platinum:
                sealingWaxColor = new Color(0.971f, 0.971f, 0.971f);
                break;
            case Rank.Diamond:
                sealingWaxColor = new Color(0.7f, 1, 0.97f);
                break;
            case Rank.Adamantium:
                sealingWaxColor = new Color(0.73f, 0.46f, 1);
                break;
        }

        idCard.transform.Find("SealingWax").GetComponent<Image>().color = sealingWaxColor;

        profileTransform = adventurerIdTween.gameObject.transform.Find(GlobalConstants.adventurerIdDirectory + "Profile");
        profile = profileTransform != null ? profileTransform.gameObject : null;
        profile.GetComponent<Image>().sprite = Resources.Load<Sprite>(GlobalConstants.spriteDirectory + adventurer.data.portrait);

        handInTween.ToggleOpenClose();
    }

    public IEnumerator HandleAdventurerExited()
    {
        yield return new WaitForSeconds(0.05f);
        handInTween.ToggleOpenClose();
    }
}
