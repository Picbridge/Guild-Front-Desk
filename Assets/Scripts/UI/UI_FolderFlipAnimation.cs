using UnityEngine;
using System.Collections;

public class UI_FolderFlipAnimation : MonoBehaviour
{
    public RectTransform FrontFolder; // A
    public RectTransform BackFolder;  // B
    public float Duration = 0.4f;
    public float MoveOffset = 50f;

    public IEnumerator SwapCoroutine(Transform frontButton, Transform backButton)
    {
        Transform backButtonParentOG = backButton.parent;
        backButton.SetParent(BackFolder);
        Transform frontButtonParentOG = frontButton.parent;
        frontButton.SetParent(FrontFolder);

        // Cache original positions and sibling indices
        Vector3 aOriginalPos = FrontFolder.localPosition;
        Vector3 bOriginalPos = BackFolder.localPosition;
        int aIndex = FrontFolder.GetSiblingIndex();
        int bIndex = BackFolder.GetSiblingIndex();

        float elapsed = 0f;

        // Phase 1: move FrontFolder right, BackFolder left
        while (elapsed < Duration)
        {
            float t = elapsed / Duration;
            FrontFolder.localPosition = Vector3.Lerp(
                aOriginalPos,
                aOriginalPos + Vector3.right * MoveOffset,
                t
            );
            BackFolder.localPosition = Vector3.Lerp(
                bOriginalPos,
                bOriginalPos + Vector3.left * MoveOffset,
                t
            );

            elapsed += Time.deltaTime;
            yield return null;
        }

        // Snap to exact separated positions
        FrontFolder.localPosition = aOriginalPos + Vector3.right * MoveOffset;
        BackFolder.localPosition = bOriginalPos + Vector3.left * MoveOffset;

        // Temporarily bring BackFolder to front
        BackFolder.SetSiblingIndex(aIndex);

        // Phase 2: move folders back to original positions
        elapsed = 0f;
        while (elapsed < Duration)
        {
            float t = elapsed / Duration;
            FrontFolder.localPosition = Vector3.Lerp(
                aOriginalPos + Vector3.right * MoveOffset,
                aOriginalPos,
                t
            );
            BackFolder.localPosition = Vector3.Lerp(
                bOriginalPos + Vector3.left * MoveOffset,
                bOriginalPos,
                t
            );

            elapsed += Time.deltaTime;
            yield return null;
        }

        // Snap back to original
        FrontFolder.localPosition = aOriginalPos;
        BackFolder.localPosition = bOriginalPos;

        // Restore original sibling order
        FrontFolder.SetSiblingIndex(aIndex);
        BackFolder.SetSiblingIndex(bIndex);
        // Set the button back to its original parent
        backButton.transform.SetParent(backButtonParentOG, worldPositionStays: false);
        backButton.transform.SetAsFirstSibling();
        frontButton.transform.SetParent(frontButtonParentOG, worldPositionStays: false);
        frontButton.transform.SetAsFirstSibling();
    }

}
