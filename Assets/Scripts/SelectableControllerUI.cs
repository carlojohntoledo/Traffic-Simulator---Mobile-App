using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SelectableControllerUI : MonoBehaviour
{
    [Header("UI References")]
    public Button moveButton;
    public Button rotateLeftButton;
    public Button rotateRightButton;
    public Button applyButton;
    public Button revertButton;
    public Button removeButton;
    public TMP_Text itemNameText;

    [Header("External References")]
    public CameraUIController cameraController; // assign your CameraUIController in inspector

    private SelectableItemController currentTarget;
    private bool moveModeActive = false;

    void Awake()
    {
        if (moveButton != null) moveButton.onClick.AddListener(OnMoveButtonClicked);
        if (rotateLeftButton != null) rotateLeftButton.onClick.AddListener(() => RotateCurrent(-90f));
        if (rotateRightButton != null) rotateRightButton.onClick.AddListener(() => RotateCurrent(90f));
        if (applyButton != null) applyButton.onClick.AddListener(OnApplyClicked);
        if (revertButton != null) revertButton.onClick.AddListener(OnRevertClicked);
        if (removeButton != null) removeButton.onClick.AddListener(OnRemoveClicked);
    }

    // --- SHOW / HIDE PANEL ---
    public void Show(SelectableItemController target)
    {
        if (target == null)
        {
            Debug.LogWarning("[SelectableControllerUI] Show() called with null target.");
            return;
        }

        currentTarget = target;
        itemNameText.text = target.name;
        gameObject.SetActive(true);

        Debug.Log($"[SelectableControllerUI] Show() called. Target = {target.name}");
    }

    public void Hide()
    {
        moveModeActive = false;
        UpdateMoveButtonVisual();

        if (cameraController != null && cameraController.dragPanel != null)
        {
            var img = cameraController.dragPanel.GetComponent<Image>();
            if (img != null)
                img.raycastTarget = true; // restore camera drag
        }

        currentTarget = null;
        gameObject.SetActive(false);
    }

    // --- MOVE BUTTON ---
    private void OnMoveButtonClicked()
    {
        if (currentTarget == null)
        {
            Debug.LogWarning("[SelectableControllerUI] Move button clicked but no target selected!");
            return;
        }

        moveModeActive = !moveModeActive;

        Debug.Log($"[SelectableControllerUI] Move button clicked. Current target: {currentTarget.name}");
        Debug.Log($"[SelectableControllerUI] Setting moveModeActive={moveModeActive} for {currentTarget.name}");

        currentTarget.SetMoveActive(moveModeActive);
        UpdateMoveButtonVisual();

        // Dynamically disable camera drag panel raycast while in move mode
        if (cameraController != null && cameraController.dragPanel != null)
        {
            var img = cameraController.dragPanel.GetComponent<Image>();
            if (img != null)
            {
                img.raycastTarget = !moveModeActive;
                Debug.Log($"[SelectableControllerUI] Camera drag panel raycastTarget={img.raycastTarget}");
            }
        }
    }

    public void SetMoveButtonActive(bool active)
    {
        moveModeActive = active;
        UpdateMoveButtonVisual();
    }

    public void ResetMoveButtonVisual()
    {
        moveModeActive = false;
        UpdateMoveButtonVisual();

        if (cameraController != null && cameraController.dragPanel != null)
        {
            var img = cameraController.dragPanel.GetComponent<Image>();
            if (img != null)
                img.raycastTarget = true;
        }
    }

    private void UpdateMoveButtonVisual()
    {
        if (moveButton == null) return;

        var colors = moveButton.colors;
        colors.normalColor = moveModeActive ? new Color(0.5f, 0.9f, 0.5f) : Color.white;
        moveButton.colors = colors;
    }

    // --- ROTATION ---
    private void RotateCurrent(float degrees)
    {
        if (currentTarget == null) return;

        if (degrees > 0)
            currentTarget.RotateRight();
        else
            currentTarget.RotateLeft();
    }

    // --- APPLY / REVERT / REMOVE ---
    private void OnApplyClicked()
    {
        if (currentTarget == null) return;
        currentTarget.Apply();
        Hide();
    }

    private void OnRevertClicked()
    {
        if (currentTarget == null) return;
        currentTarget.Revert();
    }

    private void OnRemoveClicked()
    {
        if (currentTarget == null) return;
        currentTarget.Remove();
        Hide();
    }
}
