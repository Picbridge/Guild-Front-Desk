using System.Linq;
using UnityEngine;

public class Script_GameManager : MonoBehaviour
{
    [SerializeField]
    private Script_TimeManager timeManager;
    [SerializeField] 
    private GameObject adventurerPrefab;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            GameObject adventurerObject = Instantiate(adventurerPrefab);

            adventurerObject.transform.position = new Vector3(0, 0.9f, 1);
            adventurerObject.transform.localScale = new Vector3(2f, 2f, 0);

            var adventurerScript = adventurerObject.GetComponent<Script_Adventurer>();
            var builder = adventurerObject.GetComponent<AdventurerRenderer>();

            adventurerScript.Init();
            builder.BuildFrom(adventurerScript.instance);

            foreach (var sr in adventurerObject.GetComponentsInChildren<SpriteRenderer>())
            {
                sr.color = new Color(1, 1, 1, 0);
                StartCoroutine(FadeIn(sr, 0.1f));
            }
        }

    }

    private System.Collections.IEnumerator FadeIn(SpriteRenderer sr, float duration)
    {
        float elapsed = 0f;
        Color color = sr.color;

        while (elapsed < duration)
        {
            float alpha = Mathf.Clamp01(elapsed / duration);
            sr.color = new Color(color.r, color.g, color.b, alpha);
            elapsed += Time.deltaTime;
            yield return null;
        }

        sr.color = new Color(color.r, color.g, color.b, 1f); // Ensure fully opaque
    }
}
