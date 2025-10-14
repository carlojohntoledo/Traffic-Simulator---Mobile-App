using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(CarSpawnerManager))]
public class EditSpawnerItem : MonoBehaviour
{
    [Header("Item Data Reference")]
    public ItemData data; // Linked ItemData ScriptableObject

    [Header("Spawner Components")]
    public CarSpawnerManager spawnerManager;

    [Header("Editable Settings")]
    public SpawnerType spawnerType;
    public float spawnInterval = 2f;
    public int maxSpawnCount = 5;

    private void Awake()
    {
        spawnerManager = GetComponent<CarSpawnerManager>();

        if (spawnerManager == null)
            Debug.LogError("[EditSpawnerItem] Missing CarSpawnerManager component!");
    }

    private void Start()
    {
        if (data != null)
            LoadFromData();
        else
            CacheCurrentValues();
    }

    // ============================================================
    // INITIALIZATION
    // ============================================================

    public void Initialize(ItemData itemData)
    {
        data = itemData;
        LoadFromData();
    }

    private void CacheCurrentValues()
    {
        if (spawnerManager != null)
        {
            spawnInterval = spawnerManager.spawnInterval;
            maxSpawnCount = spawnerManager.maxSpawnCount;
        }
    }

    private void LoadFromData()
    {
        if (data == null) return;

        spawnerType = data.spawnerType;
        spawnInterval = data.spawnInterval;
        maxSpawnCount = data.maxSpawnCount;

        ApplyToSpawner();

        Debug.Log($"[EditSpawnerItem] Loaded from ItemData: {data.itemName} | Interval={spawnInterval}, Max={maxSpawnCount}");
    }

    // ============================================================
    // APPLY & SAVE CHANGES
    // ============================================================

    public void ApplyToSpawner()
    {
        if (spawnerManager == null) return;

        spawnerManager.spawnInterval = spawnInterval;
        spawnerManager.maxSpawnCount = maxSpawnCount;
        spawnerManager.data = data;

        Debug.Log($"[EditSpawnerItem] Applied changes to {spawnerManager.name} → Interval={spawnInterval}, Max={maxSpawnCount}");
    }

    public void SaveBackToData()
    {
        if (data == null) return;

        data.spawnInterval = spawnInterval;
        data.maxSpawnCount = maxSpawnCount;
        data.spawnerType = spawnerType;

        Debug.Log($"[EditSpawnerItem] Saved changes to ItemData asset ({data.name})");
    }

    // ============================================================
    // INTEGRATION WITH CONFIRM EDIT PANEL
    // ============================================================

    public void OpenEditPanel(ConfirmEditPanel panel)
    {
        if (panel == null)
        {
            Debug.LogWarning("[EditSpawnerItem] Tried to open edit panel but reference is null!");
            return;
        }

        if (data == null)
        {
            Debug.LogWarning("[EditSpawnerItem] Cannot open ConfirmEditPanel — ItemData not assigned!");
            return;
        }

        // ✅ Use existing ConfirmEditPanel.Open() system
        panel.Open(data, gameObject);
        Debug.Log("[EditSpawnerItem] Opened ConfirmEditPanel for spawner edit.");
    }

    public void ConfirmEdit(float newInterval, int newMaxCount)
    {
        spawnInterval = newInterval;
        maxSpawnCount = newMaxCount;

        ApplyToSpawner();
        SaveBackToData();

        Debug.Log($"[EditSpawnerItem] Confirmed edit → Interval={newInterval}, Max={newMaxCount}");
    }
}
