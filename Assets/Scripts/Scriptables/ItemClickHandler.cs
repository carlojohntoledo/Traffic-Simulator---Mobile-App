using UnityEngine;

public class ItemClickHandler : MonoBehaviour
{
    private SelectableItemController selectableItem;
    private SelectableControllerUI selectableUI;

    void Awake()
    {
        selectableItem = GetComponent<SelectableItemController>();
        selectableUI = FindObjectOfType<SelectableControllerUI>(true);
    }

    void OnMouseDown()
    {
        Debug.Log($"[ItemClickHandler] OnMouseDown() triggered on {name}");

        // Ensure we’re not blocked by editor/placement state
        if (InputBlocker.IsModelDragging || InputBlocker.IsCameraDragging)
        {
            Debug.Log("[ItemClickHandler] Click ignored — model or camera currently dragging.");
            return;
        }

        // If this is a placed item, allow re-selection
        if (selectableItem != null)
        {
            Debug.Log($"[ItemClickHandler] Attempting to select {name}");

            selectableItem.Select();

            if (selectableUI != null)
            {
                selectableUI.Show(selectableItem);
                Debug.Log($"[ItemClickHandler] UI panel shown for {name}");
            }
            else
            {
                Debug.LogWarning("[ItemClickHandler] No SelectableControllerUI found in scene!");
            }
        }
        else
        {
            Debug.LogWarning($"[ItemClickHandler] No SelectableItemController found on {name}");
        }

        Debug.Log($"[ItemClickHandler] Finished OnMouseDown() for {name}");
    }
}
