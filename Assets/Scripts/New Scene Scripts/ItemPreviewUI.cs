using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ItemPreviewUI : MonoBehaviour
{
    [Header("Buttons")]
    public Button moveButton;       // toggle
    public Button rotateButton;     // toggle 0 <-> 90
    public Button editButton;       // open edit page (PageSystem)
    public Button removeButton;     // remove item
    public Button placeButton;      // apply/place (confirm)
    public Button cancelButton;     // cancel (destroy preview)

    [Header("Info")]
    public TMP_Text itemNameText;

    [Header("External refs")]
    public GridManager gridManager;
    public UIPageSystem pageSystem; // using your existing page system
    public UIPage editPage;         // assign the page used to edit (Button+Panel pair)

    private SelectedItem currentItem;

    private void Awake()
    {
        // ensure buttons exist - listeners are set when Show() is called
    }

    /// <summary>
    /// Show and wire UI for the selected item
    /// </summary>
    public void Show(SelectedItem item)
    {
        if (item == null)
        {
            Hide();
            return;
        }

        currentItem = item;
        gameObject.SetActive(true);

        if (itemNameText != null)
            itemNameText.text = item.data != null ? item.data.itemName : item.gameObject.name;

        // clear previous listeners to avoid duplicates
        moveButton?.onClick.RemoveAllListeners();
        rotateButton?.onClick.RemoveAllListeners();
        editButton?.onClick.RemoveAllListeners();
        removeButton?.onClick.RemoveAllListeners();
        placeButton?.onClick.RemoveAllListeners();
        cancelButton?.onClick.RemoveAllListeners();

        // Move toggle
        moveButton?.onClick.AddListener(() =>
        {
            bool next = !currentItem.IsMoving;
            currentItem.ToggleMoveMode(next);
            UpdateMoveButtonVisual();
        });
        UpdateMoveButtonVisual();

        // Rotate toggle
        rotateButton?.onClick.AddListener(() =>
        {
            currentItem.ToggleRotate();
            UpdateRotateButtonVisual();
        });
        UpdateRotateButtonVisual();

        // Edit button: only for placed items
        editButton?.onClick.AddListener(() =>
        {
            if (currentItem.IsPreviewMode)
            {
                Debug.Log("[ItemPreviewUI] Edit not available during preview.");
                return;
            }

            if (pageSystem != null && editPage != null)
            {
                pageSystem.ShowPage(editPage);
            }
            else
            {
                Debug.LogWarning("[ItemPreviewUI] pageSystem or editPage not assigned.");
            }
        });

        // Remove
        removeButton?.onClick.AddListener(() =>
        {
            currentItem.Remove();
            Hide();
        });

        // Place (confirm) - only during preview
        placeButton?.onClick.AddListener(() =>
        {
            if (!currentItem.IsPreviewMode)
            {
                Debug.Log("[ItemPreviewUI] Item already placed.");
                return;
            }

            // ensure gridManager reference (fallback)
            if (gridManager == null && currentItem != null)
            {
                var gm = GameObject.Find("Ground");
                if (gm != null) gridManager = gm.GetComponent<GridManager>();
            }

            currentItem.ConfirmPlacement();
            UpdateButtonsForPlacedState();
        });

        // Cancel (preview)
        cancelButton?.onClick.AddListener(() =>
        {
            if (!currentItem.IsPreviewMode)
            {
                Debug.Log("[ItemPreviewUI] Cancel only works during preview.");
                return;
            }

            currentItem.CancelPlacement();
            Hide();
        });

        // initial visibility
        UpdateButtonsForPlacedState();
    }

    private void UpdateMoveButtonVisual()
    {
        if (moveButton == null || currentItem == null) return;
        var colors = moveButton.colors;
        colors.normalColor = currentItem.IsMoving ? new Color(0.6f, 1f, 0.6f) : Color.white;
        moveButton.colors = colors;
    }

    private void UpdateRotateButtonVisual()
    {
        if (rotateButton == null || currentItem == null) return;
        var colors = rotateButton.colors;
        colors.normalColor = currentItem.IsRotated ? new Color(0.9f, 0.8f, 0.6f) : Color.white;
        rotateButton.colors = colors;
    }

    private void UpdateButtonsForPlacedState()
    {
        if (currentItem == null) return;

        bool preview = currentItem.IsPreviewMode;

        // preview: show Place + Cancel, hide Edit + Remove (or choose to show Remove)
        if (placeButton) placeButton.gameObject.SetActive(preview);
        if (cancelButton) cancelButton.gameObject.SetActive(preview);

        if (editButton) editButton.gameObject.SetActive(!preview);
        if (removeButton) removeButton.gameObject.SetActive(!preview);

        if (rotateButton) rotateButton.gameObject.SetActive(true);
        if (moveButton) moveButton.gameObject.SetActive(true);

        UpdateMoveButtonVisual();
        UpdateRotateButtonVisual();
    }

    public void Hide()
    {
        // remove listeners
        moveButton?.onClick.RemoveAllListeners();
        rotateButton?.onClick.RemoveAllListeners();
        editButton?.onClick.RemoveAllListeners();
        removeButton?.onClick.RemoveAllListeners();
        placeButton?.onClick.RemoveAllListeners();
        cancelButton?.onClick.RemoveAllListeners();

        currentItem = null;
        gameObject.SetActive(false);
    }
}
