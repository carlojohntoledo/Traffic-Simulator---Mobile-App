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
    public ConfirmBuildPanel confirmBuildPanel; // optional, assign later
    public Transform spawnParent;        // optional parent for spawned 3D models
    public Camera mainCamera;            // reference to main camera
    public SelectableControllerUI selectableControllerUI; // assign selectable UI panel

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

        ShowItems(ItemType.Roads);
    }

    public void ShowItems(ItemType type)
    {
        currentFilter = type;

        // Clear old content
        foreach (Transform child in contentParent)
            Destroy(child.gameObject);

        // Select array
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

        foreach (var item in items)
        {
            if (item == null) continue;

            GameObject clone = Instantiate(itemPanelPrefab, contentParent);

            // --- Set Item Name ---
            TMP_Text[] texts = clone.GetComponentsInChildren<TMP_Text>(true);
            foreach (var t in texts)
            {
                if (t.name == "Item Name Text")
                {
                    t.text = item.itemName;
                    break;
                }
            }

            // --- Set Item Image ---
            Image[] images = clone.GetComponentsInChildren<Image>(true);
            foreach (var img in images)
            {
                if (img.name == "Item Image")
                {
                    img.sprite = item.previewImage;
                    break;
                }
            }

            // --- Add Button click ---
            Button btn = clone.GetComponentInChildren<Button>();
            if (btn != null)
            {
                ItemData capturedItem = item; // capture for closure
                btn.onClick.AddListener(() =>
                {
                    Debug.Log("[ItemContentManager] Clicked: " + capturedItem.itemName);
                    SpawnPrefab(capturedItem);
                });
            }
            else
            {
                Debug.LogWarning("[ItemContentManager] No Button found on cloned panel for: " + item.itemName);
            }
        }
    }

    private void SpawnPrefab(ItemData itemData)
    {
        if (itemData == null || itemData.itemPrefab == null)
        {
            Debug.LogWarning("[ItemContentManager] ItemData or prefab is null");
            return;
        }

        if (mainCamera == null)
        {
            Debug.LogError("[ItemContentManager] MainCamera reference is missing!");
            return;
        }

        // Spawn 5 units in front of camera
        Vector3 spawnPos = mainCamera.transform.position + mainCamera.transform.forward * 5f;

        GameObject spawned = Instantiate(itemData.itemPrefab, spawnPos, Quaternion.identity, spawnParent);

        // Add SelectableItemController if not present
        SelectableItemController controller = spawned.GetComponent<SelectableItemController>();
        if (controller == null)
            controller = spawned.AddComponent<SelectableItemController>();

        controller.Select(); // set selected mode

        // Show selectable UI
        if (selectableControllerUI != null)
        {
            selectableControllerUI.Show(controller);
        }

        Debug.Log("[ItemContentManager] Spawned prefab: " + spawned.name);
    }
}
