using UnityEngine;

public class SelectableItem : MonoBehaviour
{
    [Header("Parent Build Reference")]
    public BuildItem parentBuild;   // set when segment is spawned

    private void OnMouseDown()
    {
        // (Optional) Fallback if CameraUIController raycast is not used
        HandleClick();
    }

    public void OnClicked()
    {
        Debug.Log($"[SelectableItem] Clicked on {gameObject.name}");

        if (parentBuild != null)
        {
            Debug.Log($"[SelectableItem] Forwarding to parent: {parentBuild.gameObject.name}");
            ConfirmBuildPanel.Instance.EditItem(parentBuild);
        }
        else
        {
            Debug.LogWarning("[SelectableItem] No parentBuild assigned!");
        }
    }

    private void HandleClick()
    {
        // This ensures clicks register even without CameraUIController forwarding
        OnClicked();
    }
}
