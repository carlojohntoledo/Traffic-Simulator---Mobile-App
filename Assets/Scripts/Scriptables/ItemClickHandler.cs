using UnityEngine;
using System.Collections;

public class ItemClickHandler : MonoBehaviour
{
    private SelectableItemController selectableItem;
    private SelectableControllerUI selectableUI;

    [Header("Hold Settings")]
    [Tooltip("How long the player must hold before selecting this item.")]
    public float holdDuration = 0.5f; // seconds

    private bool isHolding = false;
    private bool hasSelected = false;
    private float holdTimer = 0f;

    private Coroutine holdCoroutine;

    void Awake()
    {
        selectableItem = GetComponent<SelectableItemController>();
        selectableUI = FindObjectOfType<SelectableControllerUI>(true);
    }

    void OnMouseDown()
    {
        Debug.Log($"[ItemClickHandler] OnMouseDown() triggered on {name}");

        if (InputBlocker.IsModelDragging || InputBlocker.IsCameraDragging)
        {
            Debug.Log("[ItemClickHandler] Hold ignored — model or camera currently dragging.");
            return;
        }

        // Start hold detection
        if (holdCoroutine != null)
            StopCoroutine(holdCoroutine);

        holdCoroutine = StartCoroutine(HoldToSelect());
    }

    void OnMouseUp()
    {
        Debug.Log($"[ItemClickHandler] OnMouseUp() released on {name}");

        if (holdCoroutine != null)
            StopCoroutine(holdCoroutine);

        // Reset flags
        isHolding = false;
        holdTimer = 0f;
        hasSelected = false;
    }

    private IEnumerator HoldToSelect()
    {
        isHolding = true;
        holdTimer = 0f;
        hasSelected = false;

        while (isHolding)
        {
            holdTimer += Time.deltaTime;

            // When hold time reached, trigger selection
            if (!hasSelected && holdTimer >= holdDuration)
            {
                hasSelected = true;
                PerformSelection();
                yield break;
            }

            yield return null;
        }
    }

    private void PerformSelection()
    {
        if (selectableItem == null)
        {
            Debug.LogWarning($"[ItemClickHandler] No SelectableItemController found on {name}");
            return;
        }

        Debug.Log($"[ItemClickHandler] Hold complete — selecting {name}");

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

    void OnMouseExit()
    {
        // Cancel if the cursor leaves the item while holding
        if (isHolding)
        {
            Debug.Log($"[ItemClickHandler] Hold canceled (mouse exited {name})");
            isHolding = false;
            holdTimer = 0f;
            hasSelected = false;

            if (holdCoroutine != null)
                StopCoroutine(holdCoroutine);
        }
    }
}
