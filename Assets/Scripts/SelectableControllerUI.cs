using UnityEngine;
using UnityEngine.UI;

public class SelectableControllerUI : MonoBehaviour
{
    [Header("Buttons")]
    public Button moveButton;
    public Button removeButton;
    public Button rotateLeftButton;
    public Button rotateRightButton;
    public Button revertButton;
    public Button applyButton;

    private SelectableItemController currentTarget;
    private bool moveModeActive = false;

    [Header("Visuals")]
    public Color normalColor = Color.white;
    public Color activeColor = new Color(0.75f, 0.75f, 0.75f);

    private void Start()
    {
        gameObject.SetActive(false);

        if (moveButton != null)
            moveButton.onClick.AddListener(OnMoveButtonClicked);

        if (removeButton != null)
            removeButton.onClick.AddListener(OnRemoveButtonClicked);

        if (rotateLeftButton != null)
            rotateLeftButton.onClick.AddListener(() => currentTarget?.RotateLeft());

        if (rotateRightButton != null)
            rotateRightButton.onClick.AddListener(() => currentTarget?.RotateRight());

        if (revertButton != null)
            revertButton.onClick.AddListener(() => currentTarget?.Revert());

        if (applyButton != null)
            applyButton.onClick.AddListener(OnApplyButtonClicked);

        UpdateMoveButtonVisual();
    }

    private void OnMoveButtonClicked()
    {
        Debug.Log("[SelectableControllerUI] Move button clicked. Current target: " + (currentTarget ? currentTarget.name : "NONE"));

        if (currentTarget == null)
        {
            Debug.LogWarning("[SelectableControllerUI] No target to toggle move for.");
            return;
        }

        // toggle local state, then instruct target
        moveModeActive = !moveModeActive;
        Debug.Log($"[SelectableControllerUI] Setting moveModeActive={moveModeActive} for {currentTarget.name}");

        currentTarget.SetMoveActive(moveModeActive);

        UpdateMoveButtonVisual();
    }

    private void OnRemoveButtonClicked()
    {
        Debug.Log("[SelectableControllerUI] Remove clicked for: " + (currentTarget ? currentTarget.name : "NONE"));
        currentTarget?.Remove();
        Hide();
    }

    private void OnApplyButtonClicked()
    {
        Debug.Log("[SelectableControllerUI] Apply clicked for: " + (currentTarget ? currentTarget.name : "NONE"));
        currentTarget?.Apply();
        Hide();
    }

    public void Show(SelectableItemController target)
    {
        currentTarget = target;
        moveModeActive = false;
        UpdateMoveButtonVisual();
        gameObject.SetActive(true);

        Debug.Log("[SelectableControllerUI] Show() called. Target = " + (target ? target.name : "NONE"));
        target?.Select();
    }

    public void Hide()
    {
        if (currentTarget != null)
        {
            Debug.Log("[SelectableControllerUI] Hide() called. Deselecting: " + currentTarget.name);
            currentTarget.Deselect();
            currentTarget = null;
        }
        moveModeActive = false;
        UpdateMoveButtonVisual();
        gameObject.SetActive(false);
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
    }

    private void UpdateMoveButtonVisual()
    {
        if (moveButton == null) return;
        var img = moveButton.GetComponent<Image>();
        if (img != null)
            img.color = moveModeActive ? activeColor : normalColor;
    }
}
