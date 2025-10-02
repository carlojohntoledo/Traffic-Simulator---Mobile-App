using UnityEngine;
using System.Collections;

public class ShowUIPanel : MonoBehaviour
{
    [Header("References")]
    public RectTransform panel; // assign your hidden panel

    [Header("Settings")]
    public float animationDuration = 0.5f; // seconds to animate
    [Range(0f, 1f)] public float minAnchorY = 0f;   // collapsed position
    [Range(0f, 1f)] public float maxAnchorY = 0.5f; // expanded position

    private Coroutine currentAnim;
    private bool isVisible = false; // track state

    // --- Toggle panel ---
    public void toggleMyPanel()
    {
        if (panel == null) return;
        if (currentAnim != null) StopCoroutine(currentAnim);

        if (!isVisible) // show
        {
            panel.gameObject.SetActive(true);

            // Reset collapsed
            panel.anchorMin = new Vector2(panel.anchorMin.x, minAnchorY);
            panel.anchorMax = new Vector2(panel.anchorMax.x, minAnchorY);

            currentAnim = StartCoroutine(AnimatePanel(minAnchorY, maxAnchorY, false));
            isVisible = true;
        }
        else // hide
        {
            currentAnim = StartCoroutine(AnimatePanel(panel.anchorMax.y, minAnchorY, true));
            isVisible = false;
        }
    }

    // --- Coroutine animates between two Y values ---
    IEnumerator AnimatePanel(float startY, float targetY, bool disableAtEnd)
    {
        float elapsed = 0f;

        Vector2 startMin = new Vector2(panel.anchorMin.x, minAnchorY);
        Vector2 startMax = new Vector2(panel.anchorMax.x, startY);
        Vector2 targetMin = new Vector2(panel.anchorMin.x, minAnchorY);
        Vector2 targetMax = new Vector2(panel.anchorMax.x, targetY);

        while (elapsed < animationDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / animationDuration);

            panel.anchorMin = Vector2.Lerp(startMin, targetMin, t);
            panel.anchorMax = Vector2.Lerp(startMax, targetMax, t);

            yield return null;
        }

        // Snap to final state
        panel.anchorMin = targetMin;
        panel.anchorMax = targetMax;

        // Disable if hiding
        if (disableAtEnd)
            panel.gameObject.SetActive(false);

        currentAnim = null;
    }
}
