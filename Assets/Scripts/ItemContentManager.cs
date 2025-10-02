using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ItemContentManager : MonoBehaviour
{
    [Header("Roads Items")]
    public ItemData[] roads;

    [Header("Vehicle Items")]
    public ItemData[] vehicles;

    [Header("Pedestrian Items")]
    public ItemData[] pedestrians;

    [Header("Rules Items")]
    public ItemData[] rules;

    [Header("Spawner Items")]
    public ItemData[] spawners;

    [Header("References")]
    public GameObject itemPanelPrefab;   // assign prefab in inspector
    public Transform contentParent;      // assign Content panel
    public ConfirmBuildPanel confirmBuildPanel; // assign in inspector

    [Header("Menu Buttons")]
    public Button roadsButton;
    public Button vehiclesButton;
    public Button pedestriansButton;
    public Button rulesButton;
    public Button spawnersButton;

    private ItemType currentFilter = ItemType.Roads;

    void Start()
    {
        // Hook up menu buttons
        roadsButton.onClick.AddListener(() => ShowItems(ItemType.Roads));
        vehiclesButton.onClick.AddListener(() => ShowItems(ItemType.Vehicles));
        pedestriansButton.onClick.AddListener(() => ShowItems(ItemType.Pedestrians));
        rulesButton.onClick.AddListener(() => ShowItems(ItemType.Rules));
        spawnersButton.onClick.AddListener(() => ShowItems(ItemType.Spawner));

        // Default tab = Roads
        ShowItems(ItemType.Roads);
    }

    public void ShowItems(ItemType type)
    {
        currentFilter = type;

        // Clear old content
        foreach (Transform child in contentParent)
            Destroy(child.gameObject);

        // Choose correct array
        ItemData[] items = null;
        switch (type)
        {
            case ItemType.Roads: items = roads; break;
            case ItemType.Vehicles: items = vehicles; break;
            case ItemType.Pedestrians: items = pedestrians; break;
            case ItemType.Rules: items = rules; break;
            case ItemType.Spawner: items = spawners; break;
        }

        if (items == null) return;

        // Spawn new items
        foreach (var item in items)
        {
            if (item == null) continue;

            GameObject clone = Instantiate(itemPanelPrefab, contentParent);

            // Try to find components on prefab
            TMP_Text tmpText = clone.GetComponentInChildren<TMP_Text>();
            if (tmpText != null) tmpText.text = item.itemName;

            Text uiText = clone.GetComponentInChildren<Text>();
            if (uiText != null) uiText.text = item.itemName;

            Image img = clone.GetComponentInChildren<Image>();
            if (img != null) img.sprite = item.previewImage;

            // Add button listener to open confirm panel
            Button btn = clone.GetComponentInChildren<Button>();
            if (btn != null)
            {
                ItemData captured = item; // avoid closure bug
                btn.onClick.AddListener(() =>
                {
                    confirmBuildPanel.Show(captured);
                });
            }
        }
    }
}
