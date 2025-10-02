using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class ShowUIPanel : MonoBehaviour
{
    [Header("References")]
    public RectTransform panel; // assign the panel2 (the one to expand/collapse)
    public RectTransform parentPanel; // assign the parent container (with vertical layout)

    [Header("Settings")]
    public float animationDuration = 0.5f; // time to expand/collapse
    [Range(0f, 1f)] public float targetHeightPercent = 0.4f; // 40% of parent

    private LayoutElement layoutElement;
    private Coroutine currentAnim;
    private bool isVisible = false; // track state

    void Awake()
    {
        if (panel != null)
        {
            // Ensure LayoutElement exists
            layoutElement = panel.GetComponent<LayoutElement>();
            if (layoutElement == null)
                layoutElement = panel.gameObject.AddComponent<LayoutElement>();

            // Start collapsed
            layoutElement.preferredHeight = 0;
        }
    }

    // --- Toggle function ---
    public void toggleMyPanel()
    {
        if (panel == null || parentPanel == null) return;

        if (currentAnim != null) StopCoroutine(currentAnim);

        float parentHeight = parentPanel.rect.height;
        float targetHeight = isVisible ? 0f : parentHeight * targetHeightPercent;

        currentAnim = StartCoroutine(AnimateHeight(layoutElement.preferredHeight, targetHeight));
        isVisible = !isVisible;
    }

    // --- Coroutine to animate height ---
    IEnumerator AnimateHeight(float start, float target)
    {
        float elapsed = 0f;

        while (elapsed < animationDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / animationDuration);
            layoutElement.preferredHeight = Mathf.Lerp(start, target, t);
            yield return null;
        }

        layoutElement.preferredHeight = target;
        currentAnim = null;
    }
}
