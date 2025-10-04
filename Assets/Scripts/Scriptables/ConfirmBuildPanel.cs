using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ConfirmBuildPanel : MonoBehaviour
{
    public static ConfirmBuildPanel Instance { get; private set; }

    [Header("Panel UI")]
    public GameObject panel;
    public TMP_Text itemNameText;
    public TMP_Text itemTypeText;

    private BuildItem editingItem;

    private SelectableItemController currentController;

    public void Open(SelectableItemController controller)
    {
        currentController = controller;
        gameObject.SetActive(true);

        // TODO: populate UI fields with currentController's editable attributes
    }

    public void Close()
    {
        gameObject.SetActive(false);
    }

    // Example: Apply edited values
    public void ApplyChanges()
    {
        if (currentController == null) return;

        // Apply changes from UI fields to the currentController's prefab here
    }

    void Awake()
    {
        if (Instance != null && Instance != this) Destroy(gameObject);
        Instance = this;
    }

    public void Show(BuildItem item)
    {
        editingItem = item;
        panel.SetActive(true);
        itemNameText.text = item.staticName;
        itemTypeText.text = item.data.type.ToString();
        // You can add more editable fields here
    }

    public void ApplyEdits()
    {
        // Save changes from input fields to editingItem
        panel.SetActive(false);
    }

    public void Cancel()
    {
        panel.SetActive(false);
    }
}
