using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class ItemComponentContentManager : MonoBehaviour
{
    [Header("Item Collections (assign in inspector)")]
    public ItemDataComponent[] roads;
    public ItemDataComponent[] vehicles;
    public ItemDataComponent[] pedestrians;
    public ItemDataComponent[] signages;
    public ItemDataComponent[] spawners;
    public ItemDataComponent[] trafficLights;

    [Header("UI")]
    public GameObject itemButtonPrefab; // prefab with a Button, TMP_Text named "ItemName", Image named "Icon"
    public Transform contentParent;

    [Header("Spawn & UI")]
    public Transform spawnParent;               // where previews/placed objects are parented (optional)
    public ItemPreviewUI itemPreviewUI;         // assign your preview UI
    public GridManager gridManager;             // optional - will auto-find Ground grid if null

    private Dictionary<ItemModelType, ItemDataComponent[]> itemMap;

    private void Awake()
    {
        itemMap = new Dictionary<ItemModelType, ItemDataComponent[]>
        {
            { ItemModelType.Road, roads },
            { ItemModelType.Vehicle, vehicles },
            { ItemModelType.Pedestrian, pedestrians },
            { ItemModelType.Signage, signages },
            { ItemModelType.Spawner, spawners },
            { ItemModelType.TrafficLight, trafficLights }
        };

        if (gridManager == null)
        {
            var ground = GameObject.Find("Ground");
            if (ground != null) gridManager = ground.GetComponent<GridManager>();
        }

        if (itemPreviewUI == null)
            itemPreviewUI = FindObjectOfType<ItemPreviewUI>(true);
    }

    private void Start()
    {
        // default populate roads
        ShowItems(ItemModelType.Road);
    }

    /// <summary>
    /// Clear content and populate buttons for the given type
    /// </summary>
    public void ShowItems(ItemModelType type)
    {
        foreach (Transform t in contentParent) Destroy(t.gameObject);

        if (!itemMap.TryGetValue(type, out var items) || items == null || items.Length == 0)
        {
            Debug.LogWarning($"No items found for {type}");
            return;
        }

        foreach (var item in items)
        {
            if (item == null) continue;

            GameObject btnObj = Instantiate(itemButtonPrefab, contentParent);
            var btn = btnObj.GetComponentInChildren<Button>();
            var nameText = btnObj.transform.Find("ItemName")?.GetComponent<TMP_Text>();
            var iconImg = btnObj.transform.Find("Icon")?.GetComponent<Image>();

            if (nameText) nameText.text = item.itemName;
            if (iconImg) iconImg.sprite = item.itemImagePreview;

            if (btn != null)
            {
                ItemDataComponent captured = item;
                btn.onClick.AddListener(() => OnItemButtonClicked(captured));
            }
        }
    }

    private void OnItemButtonClicked(ItemDataComponent item)
    {
        if (item == null || item.itemModelPrefab == null)
        {
            Debug.LogWarning("[ItemComponentContentManager] Invalid item or missing prefab.");
            return;
        }

        // Destroy any existing preview (only one preview at a time)
        var existingPreview = FindObjectOfType<SelectedItem>();
        if (existingPreview != null && existingPreview.IsPreviewMode)
        {
            Destroy(existingPreview.gameObject);
        }

        // Instantiate preview in front of camera or raycast center
        Vector3 spawnPos = Vector3.zero;
        Camera cam = Camera.main;
        if (cam != null)
        {
            Ray ray = cam.ScreenPointToRay(new Vector3(Screen.width / 2f, Screen.height / 2f, 0f));
            if (Physics.Raycast(ray, out RaycastHit hit))
                spawnPos = hit.point;
            else
                spawnPos = cam.transform.position + cam.transform.forward * 4f;
        }

        // Snap to grid center if available
        if (gridManager != null)
            spawnPos = gridManager.GetNearestGridPosition(spawnPos);

        GameObject preview = Instantiate(item.itemModelPrefab, spawnPos, Quaternion.identity, spawnParent);
        preview.name = item.itemName + "_Preview";

        // Add SelectedItem component (if not present) and initialize
        var sel = preview.GetComponent<SelectedItem>() ?? preview.AddComponent<SelectedItem>();
        sel.Initialize(item, itemPreviewUI, gridManager);

        // Show the preview UI for this item
        itemPreviewUI.Show(sel);
    }
}
