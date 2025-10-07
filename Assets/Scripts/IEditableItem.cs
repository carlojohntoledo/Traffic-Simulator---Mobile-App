using UnityEngine;

public interface IEditableItem
{
    /// <summary>
    /// Called whenever the item's attributes (from ItemData) are changed.
    /// Implement this to update visuals, parameters, or behavior.
    /// </summary>
    void OnAttributesChanged();
}
