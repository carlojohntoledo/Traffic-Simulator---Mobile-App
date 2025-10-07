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
    public Button editButton;
    public TMP_Text itemNameText;

    [Header("External References")]
    public CameraUIController cameraController; // assign in inspector if possible

    private SelectableItemController currentTarget;
    private bool moveModeActive = false;

    private ConfirmEditPanel confirmEditPanel;

    void Awake()
    {
        if (moveButton != null) moveButton.onClick.AddListener(OnMoveButtonClicked);
        if (rotateLeftButton != null) rotateLeftButton.onClick.AddListener(() => RotateCurrent(-90f));
        if (rotateRightButton != null) rotateRightButton.onClick.AddListener(() => RotateCurrent(90f));
        if (applyButton != null) applyButton.onClick.AddListener(OnApplyClicked);
        if (revertButton != null) revertButton.onClick.AddListener(OnRevertClicked);
        if (removeButton != null) removeButton.onClick.AddListener(OnRemoveClicked);
        if (editButton != null) editButton.onClick.AddListener(OnEditClicked);
    }

    void Start()
    {
        // Dynamically find ConfirmEditPanel (since prefabs can’t be pre-linked)
        confirmEditPanel = FindObjectOfType<ConfirmEditPanel>(true);

        if (cameraController == null)
            cameraController = FindObjectOfType<CameraUIController>();
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

        RestoreCameraDragRaycast();

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

        // Manage drag panel raycast dynamically
        SetCameraDragRaycast(!moveModeActive);
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
        RestoreCameraDragRaycast();
    }

    private void UpdateMoveButtonVisual()
    {
        if (moveButton == null) return;

        var colors = moveButton.colors;
        colors.normalColor = moveModeActive ? new Color(0.6f, 1f, 0.6f) : Color.white;
        colors.selectedColor = moveModeActive ? new Color(0.6f, 1f, 0.6f) : Color.white;
        moveButton.colors = colors;
    }

    // --- CAMERA RAYCAST MANAGEMENT ---
    private void SetCameraDragRaycast(bool enabled)
    {
        if (cameraController == null || cameraController.dragPanel == null)
            return;

        var img = cameraController.dragPanel.GetComponent<Image>();
        if (img != null)
        {
            img.raycastTarget = enabled;
            Debug.Log($"[SelectableControllerUI] Camera drag panel raycastTarget={enabled}");
        }
    }

    private void RestoreCameraDragRaycast() => SetCameraDragRaycast(true);

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

    // --- EDIT BUTTON ---
    private void OnEditClicked()
    {
        if (currentTarget == null)
        {
            Debug.LogWarning("[SelectableControllerUI] Edit button clicked but no target selected!");
            return;
        }

        if (confirmEditPanel == null)
        {
            confirmEditPanel = FindObjectOfType<ConfirmEditPanel>(true);
            if (confirmEditPanel == null)
            {
                Debug.LogError("[SelectableControllerUI] No ConfirmEditPanel found in scene!");
                return;
            }
        }

        var buildItem = currentTarget.GetComponent<BuildItem>();
        if (buildItem != null && buildItem.data != null)
        {
            confirmEditPanel.Open(buildItem.data);
            Debug.Log($"[SelectableControllerUI] Opened ConfirmEditPanel for {buildItem.data.itemName}");
        }
        else
        {
            Debug.LogWarning("[SelectableControllerUI] Current target has no BuildItem or data.");
        }
    }
}
