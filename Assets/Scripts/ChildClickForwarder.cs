using UnityEngine;

public class ChildClickForwarder : MonoBehaviour
{
    private void OnMouseDown()
    {
        Transform current = transform;
        while (current.parent != null)
        {
            var selectable = current.parent.GetComponent<SelectableItemController>();
            if (selectable != null)
            {
                Debug.Log($"[ChildClickForwarder] Forwarding click from {name} to {current.parent.name}");
                selectable.SendMessage("OnMouseDown", SendMessageOptions.DontRequireReceiver);
                return;
            }
            current = current.parent;
        }

        Debug.LogWarning($"[ChildClickForwarder] No SelectableItemController found up the hierarchy for {name}");
    }
}
