using UnityEngine;
using System.Collections.Generic;

[DisallowMultipleComponent]
[RequireComponent(typeof(BoxCollider))]
public class CarSpawnerManager : MonoBehaviour
{
    [Header("Item Data Reference")]
    public ItemData data;

    [Header("Spawner Settings (Runtime)")]
    public float spawnInterval = 2f;
    public int maxSpawnCount = 5;
    public GameObject vehiclePrefab;
    public List<GameObject> activeVehicles = new List<GameObject>();

    [Header("Snap Settings")]
    [Tooltip("Distance within which this spawner can snap to a road.")]
    public float snapDistance = 1.5f;
    public LayerMask roadLayer;

    private float timer;
    private bool isDragging;

    private Transform snapPoint;  // Used to attach to road
    private Transform startPoint; // Where cars spawn

    private void Awake()
    {
        // Auto-assign child points if they exist
        snapPoint = transform.Find("SnapPoint");
        startPoint = transform.Find("StartPoint");

        if (snapPoint == null)
            Debug.LogWarning($"[CarSpawner] No SnapPoint child found in {name}!");
        if (startPoint == null)
            Debug.LogWarning($"[CarSpawner] No StartPoint child found in {name}!");

        // Prevent physics movement
        var rb = GetComponent<Rigidbody>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody>();
            rb.isKinematic = true;
        }
    }

    private void Start()
    {
        if (data != null)
            InitializeFromData();
    }

    private void Update()
    {
        if (!isDragging)
            TrySnapToNearbyRoad();

        timer += Time.deltaTime;

        if (timer >= spawnInterval && activeVehicles.Count < maxSpawnCount)
        {
            SpawnVehicle();
            timer = 0f;
        }

        activeVehicles.RemoveAll(c => c == null);
    }

    // ============================================================
    // INITIALIZATION
    // ============================================================

    public void Initialize(ItemData itemData)
    {
        data = itemData;
        InitializeFromData();
    }

    private void InitializeFromData()
    {
        if (data == null) return;

        spawnInterval = data.spawnInterval;
        maxSpawnCount = data.maxSpawnCount;

        // Optional: Assign default car prefab based on spawner type
        if (data.spawnerType == SpawnerType.Car && data.itemPrefab != null)
            vehiclePrefab = data.itemPrefab;
    }

    // ============================================================
    // SPAWNING
    // ============================================================

    private void SpawnVehicle()
    {
        if (vehiclePrefab == null || startPoint == null)
        {
            Debug.LogWarning($"[CarSpawner] Cannot spawn vehicle — prefab or StartPoint missing on {name}!");
            return;
        }

        GameObject car = Instantiate(vehiclePrefab, startPoint.position, startPoint.rotation);
        activeVehicles.Add(car);
    }

    // ============================================================
    // SNAPPING
    // ============================================================

    private void TrySnapToNearbyRoad()
    {
        if (snapPoint == null) return;

        Collider[] hits = Physics.OverlapSphere(snapPoint.position, snapDistance, roadLayer);
        foreach (Collider hit in hits)
        {
            if (hit.gameObject == gameObject) continue;

            EditRoadItem road = hit.GetComponent<EditRoadItem>();
            if (road == null || road.snapPoints == null)
                continue;

            foreach (Transform roadSnap in road.snapPoints)
            {
                float dist = Vector3.Distance(snapPoint.position, roadSnap.position);
                if (dist <= snapDistance)
                {
                    // ✅ Snap only if facing opposite directions
                    float dot = Vector3.Dot(snapPoint.forward, -roadSnap.forward);
                    if (dot > 0.9f)
                    {
                        PerformSnap(roadSnap.position, snapPoint.position);
                        Debug.Log($"[CarSpawner] Snapped {name} → {road.name} at {roadSnap.name}");
                        return;
                    }
                }
            }
        }
    }

    private void PerformSnap(Vector3 targetPos, Vector3 snapOrigin)
    {
        Vector3 offset = targetPos - snapOrigin;
        transform.position += offset;
    }

    // ============================================================
    // DRAGGING (from your ItemDragger)
    // ============================================================

    public void SetDraggingState(bool dragging)
    {
        isDragging = dragging;
    }

    // ============================================================
    // DEBUG VISUALIZATION
    // ============================================================

    private void OnDrawGizmos()
    {
        if (snapPoint != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawSphere(snapPoint.position, 0.1f);
            Gizmos.DrawRay(snapPoint.position, snapPoint.forward * 0.5f);
        }

        if (startPoint != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawSphere(startPoint.position, 0.1f);
            Gizmos.DrawRay(startPoint.position, startPoint.forward * 0.5f);
        }
    }
}
