using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

[System.Serializable]
public class UIPage
{
    [Tooltip("Button that opens this panel")]
    public Button openButton;

    [Tooltip("The panel GameObject to show/hide")]
    public GameObject panel;

    [Tooltip("Optional cancel button inside the panel")]
    public Button cancelButton;

    [Tooltip("If true, clicking the panel background also closes it")]
    public bool closeOnBackgroundClick = true;
}

public class UIPageSystem : MonoBehaviour
{
    [Header("All UI Pages (Button + Panel Pairings)")]
    public List<UIPage> pages = new List<UIPage>();

    [Header("Options")]
    public bool hideAllOnStart = true;
    public int defaultPageIndex = -1;

    private UIPage currentPage;

    void Awake()
    {
        // Assign listeners for all pages
        foreach (var page in pages)
        {
            if (page.openButton != null)
                page.openButton.onClick.AddListener(() => TogglePage(page));

            if (page.cancelButton != null)
                page.cancelButton.onClick.AddListener(() => HidePage(page));

            // Optional: background click handler
            if (page.closeOnBackgroundClick && page.panel != null)
            {
                AddBackgroundCloseListener(page);
            }
        }

        // Hide all initially
        if (hideAllOnStart)
        {
            foreach (var page in pages)
            {
                if (page.panel != null) page.panel.SetActive(false);
            }
        }

        // Show default page if defined
        if (defaultPageIndex >= 0 && defaultPageIndex < pages.Count)
        {
            ShowPage(pages[defaultPageIndex]);
        }
    }

    /// <summary>
    /// Adds a background click listener to a panel.
    /// </summary>
    private void AddBackgroundCloseListener(UIPage page)
    {
        // Add an invisible full-screen Button if none exists
        Button backgroundButton = page.panel.GetComponent<Button>();
        if (backgroundButton == null)
        {
            backgroundButton = page.panel.AddComponent<Button>();
            backgroundButton.transition = Selectable.Transition.None;
            backgroundButton.targetGraphic = null; // no highlight visuals
        }

        backgroundButton.onClick.AddListener(() =>
        {
            // Only close if this page is the current one
            if (currentPage == page)
            {
                HidePage(page);
            }
        });
    }

    /// <summary>
    /// Toggle a panel on/off.
    /// </summary>
    private void TogglePage(UIPage page)
    {
        if (currentPage == page)
        {
            HidePage(page);
        }
        else
        {
            ShowPage(page);
        }
    }

    /// <summary>
    /// Show a specific panel, hiding all others.
    /// </summary>
    public void ShowPage(UIPage page)
    {
        HideAll();
        if (page.panel != null)
        {
            page.panel.SetActive(true);
            currentPage = page;
        }
    }

    /// <summary>
    /// Hide a specific page.
    /// </summary>
    public void HidePage(UIPage page)
    {
        if (page.panel != null)
            page.panel.SetActive(false);

        if (currentPage == page)
            currentPage = null;
    }

    /// <summary>
    /// Hide all UI panels.
    /// </summary>
    public void HideAll()
    {
        foreach (var page in pages)
        {
            if (page.panel != null)
                page.panel.SetActive(false);
        }
        currentPage = null;
    }
}
