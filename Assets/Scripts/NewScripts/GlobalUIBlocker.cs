using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections.Generic;

public class GlobalUIBlocker : MonoBehaviour
{
    public static GlobalUIBlocker Instance { get; private set; }

    [Header("Required References")]
    public GraphicRaycaster uiRaycaster;
    public EventSystem eventSystem;

    [Header("Allowed Click-Through UI Elements")]
    public List<RectTransform> allowClickThrough = new List<RectTransform>();

    private PointerEventData pointerData;
    private readonly List<RaycastResult> raycastResults = new List<RaycastResult>();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    /// <summary>
    /// Returns true if the current pointer (mouse or touch) is over a blocking UI element.
    /// </summary>
    public bool IsPointerOverBlockingUI()
    {
        if (eventSystem == null || uiRaycaster == null)
            return false;

        pointerData = new PointerEventData(eventSystem)
        {
            position = Input.mousePosition
        };

        raycastResults.Clear();
        uiRaycaster.Raycast(pointerData, raycastResults);

        foreach (var result in raycastResults)
        {
            RectTransform rt = result.gameObject.GetComponent<RectTransform>();
            if (rt != null)
            {
                // Skip elements that are allowed click-through
                if (allowClickThrough.Contains(rt))
                    continue;

                return true; // Block this click
            }
        }

        return false;
    }

    /// <summary>
    /// Adds a RectTransform to the allowed click-through list.
    /// </summary>
    public void AddAllowedClickThrough(RectTransform rt)
    {
        if (!allowClickThrough.Contains(rt))
            allowClickThrough.Add(rt);
    }

    /// <summary>
    /// Removes a RectTransform from the allowed click-through list.
    /// </summary>
    public void RemoveAllowedClickThrough(RectTransform rt)
    {
        if (allowClickThrough.Contains(rt))
            allowClickThrough.Remove(rt);
    }
}
