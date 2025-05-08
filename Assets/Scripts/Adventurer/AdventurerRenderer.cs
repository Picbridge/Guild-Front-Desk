using UnityEngine;

public class AdventurerRenderer : MonoBehaviour
{
    [Header("Layered Sprite Renderers")]
    public SpriteRenderer bodyRenderer;
    public SpriteRenderer headRenderer;
    public SpriteRenderer hairRenderer;
    public SpriteRenderer eyesRenderer;
    public SpriteRenderer mouthRenderer;

    // Builds the character from instance data
    public void BuildFrom(AdventurerInstance instance)
    {
        AdventurerData data = instance.data;

        string basePath = $"CharacterParts/{data.perspective}";

        // Load and assign sprites
        bodyRenderer.sprite = LoadSprite($"{basePath}/Body/{data.gender}/{data.bodySprite}");
        headRenderer.sprite = LoadSprite($"{basePath}/Head/{data.headSprite}");
        hairRenderer.sprite = LoadSprite($"{basePath}/Hair/{data.hairSprite}");
        eyesRenderer.sprite = LoadSprite($"{basePath}/Eyes/{data.eyesSprite}");
        mouthRenderer.sprite = LoadSprite($"{basePath}/Mouth/{data.mouthSprite}");

        // Tint with race/hair color
        //bodyRenderer.color = data.skinColor;
        //headRenderer.color = data.skinColor;
        //mouthRenderer.color = data.skinColor;
        hairRenderer.color = data.hairColor;

        ApplyLayeringOrder();
    }
    private void ApplyLayeringOrder()
    {
        if (bodyRenderer != null) bodyRenderer.sortingOrder = 0;
        if (headRenderer != null) headRenderer.sortingOrder = 1;
        if (eyesRenderer != null) eyesRenderer.sortingOrder = 2;
        if (mouthRenderer != null) mouthRenderer.sortingOrder = 3;
        if (hairRenderer != null) hairRenderer.sortingOrder = 4;
    }

    private Sprite LoadSprite(string path)
    {
        Sprite sprite = Resources.Load<Sprite>(path);
        if (sprite == null)
        {
            Debug.LogError($"Failed to load sprite at path: {path}");
            return null;
        }
        return sprite;
    }

}
