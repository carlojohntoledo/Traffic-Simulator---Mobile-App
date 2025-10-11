using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(BoxCollider))]
[DisallowMultipleComponent]
public class EditRoadItem : MonoBehaviour
{
    [Header("Item Data Reference")]
    public ItemData data;

    [Header("Road Settings")]
    public int length = 1;
    public GameObject roadSegmentPrefab;
    public List<Transform> roadSegments = new List<Transform>();

    [Header("Snap Points")]
    [Tooltip("Assign all snap points manually in the Inspector. Supports multiple directions.")]
    public Transform[] snapPoints;

    [Header("Snap Settings")]
    public float snapDistance = 1.0f;
    public LayerMask snapLayer;

    private BoxCollider rootCollider;

    private void Awake()
    {
        rootCollider = GetComponent<BoxCollider>();
    }

    private void Start()
    {
        if (data != null)
            InitializeFromData();
    }

    private void Update()
    {
        UpdateSnapPoints();

        // Only auto-snap when in move mode
        var selectable = GetComponent<SelectableItemController>();
        if (selectable != null && selectable.IsMoveModeActive())
            TrySnapToNearbyPrefab();
    }

    // ============================================================
    // Initialization
    // ============================================================

    public void Initialize(ItemData itemData)
    {
        data = itemData;
        InitializeFromData();
    }

    private void InitializeFromData()
    {
        if (data == null) return;

        length = Mathf.RoundToInt(data.roadLength);
        if (roadSegmentPrefab == null)
            roadSegmentPrefab = data.itemPrefab;

        RebuildRoadSegments();
    }

    public void ApplyEditChanges(ItemData newData)
    {
        if (newData == null) return;
        data = newData;
        length = Mathf.RoundToInt(data.roadLength);
        RebuildRoadSegments();
    }

    public void RebuildRoadSegments()
    {
        // Clear old segments
        foreach (var seg in roadSegments)
            if (seg != null) Destroy(seg.gameObject);

        roadSegments.Clear();

        if (roadSegmentPrefab == null)
        {
            Debug.LogWarning($"[EditRoadItem] No roadSegmentPrefab assigned for {name}!");
            return;
        }

        float segmentLength = 10f; // base mesh length along X
        for (int i = 0; i < length; i++)
        {
            GameObject seg = Instantiate(roadSegmentPrefab, transform);
            seg.transform.localPosition = new Vector3(i * segmentLength, 0f, 0f);
            seg.transform.localRotation = Quaternion.identity;
            roadSegments.Add(seg.transform);
        }

        UpdateColliderToSegments();
        UpdateSnapPoints();
    }

    // ============================================================
    // Collider & Snap Points Update
    // ============================================================

    private void UpdateColliderToSegments()
    {
        if (rootCollider == null || roadSegments.Count == 0) return;

        Bounds bounds = new Bounds(roadSegments[0].position, Vector3.zero);
        foreach (var seg in roadSegments)
        {
            Renderer rend = seg.GetComponentInChildren<Renderer>();
            if (rend != null)
                bounds.Encapsulate(rend.bounds);
        }

        Vector3 localCenter = transform.InverseTransformPoint(bounds.center);
        rootCollider.center = localCenter;
        rootCollider.size = bounds.size;
    }

    public void UpdateSnapPoints()
    {
        if (roadSegments.Count == 0 || snapPoints == null || snapPoints.Length == 0)
            return;

        float segmentLength = 10f;
        Renderer firstRenderer = roadSegments[0].GetComponentInChildren<Renderer>();
        if (firstRenderer != null)
            segmentLength = firstRenderer.bounds.size.x;

        if (snapPoints.Length == 2)
        {
            Vector3 startPos = roadSegments[0].position - roadSegments[0].right * (segmentLength * 0.5f);
            snapPoints[0].position = startPos;

            Transform lastSeg = roadSegments[roadSegments.Count - 1];
            Vector3 endPos = lastSeg.position + lastSeg.right * (segmentLength * 0.5f);
            snapPoints[1].position = endPos;
        }

        UpdateColliderToSegments();
    }

    // ============================================================
    // Continuous Snap (multi-point)
    // ============================================================

    private void TrySnapToNearbyPrefab()
    {
        if (snapPoints == null || snapPoints.Length == 0) return;

        foreach (Transform mySnap in snapPoints)
        {
            Collider[] hits = Physics.OverlapSphere(mySnap.position, snapDistance, snapLayer);

            foreach (Collider hit in hits)
            {
                if (hit.gameObject == gameObject) continue;

                EditRoadItem other = hit.GetComponent<EditRoadItem>();
                if (other == null || other == this || other.snapPoints == null) continue;

                var otherSelectable = other.GetComponent<SelectableItemController>();
                if (otherSelectable != null && !otherSelectable.IsMoveModeActive())
                {
                    foreach (Transform otherSnap in other.snapPoints)
                    {
                        float dist = Vector3.Distance(mySnap.position, otherSnap.position);
                        if (dist > snapDistance) continue;

                        // ✅ Snap if the two snap points face opposite directions
                        float dot = Vector3.Dot(mySnap.forward, -otherSnap.forward);
                        if (dot > 0.9f)
                        {
                            PerformSnap(otherSnap.position, mySnap.position);
                            Debug.Log($"[EditRoadItem] Snapped {name} ({mySnap.name}) → {other.name} ({otherSnap.name})");
                            return;
                        }
                    }
                }
            }
        }
    }

    // ============================================================
    // Manual Snap (used by SelectableItemController)
    // ============================================================

    public bool TrySnapTo(EditRoadItem other, SelectableItemController movingController = null)
    {
        if (snapPoints == null || other == null || other.snapPoints == null)
            return false;

        // Prevent stationary object from moving
        var thisController = GetComponent<SelectableItemController>();
        if (movingController != null && movingController != thisController)
            return false;

        foreach (Transform mySnap in snapPoints)
        {
            foreach (Transform otherSnap in other.snapPoints)
            {
                float dist = Vector3.Distance(mySnap.position, otherSnap.position);
                if (dist <= snapDistance)
                {
                    float dot = Vector3.Dot(mySnap.forward, -otherSnap.forward);
                    if (dot > 0.9f)
                    {
                        PerformSnap(otherSnap.position, mySnap.position);
                        return true;
                    }
                }
            }
        }

        return false;
    }

    private void PerformSnap(Vector3 targetPos, Vector3 localSnapPos)
    {
        Vector3 offset = targetPos - localSnapPos;
        transform.position += offset;

        UpdateSnapPoints();
        UpdateColliderToSegments();
    }

    // ============================================================
    // Debug Visualization
    // ============================================================

    private void OnDrawGizmos()
    {
        if (snapPoints == null) return;

        foreach (Transform snap in snapPoints)
        {
            if (snap == null) continue;

            Gizmos.color = Color.cyan;
            Gizmos.DrawSphere(snap.position, 0.1f);

            Gizmos.color = Color.yellow;
            Gizmos.DrawRay(snap.position, snap.forward * 0.5f);
        }
    }
}

// ============================================================
// 🔹 Helper Extension for Move Mode Check
// ============================================================
public static class SelectableExtensions
{
    public static bool IsMoveModeActive(this SelectableItemController controller)
    {
        if (controller == null) return false;

        // Use reflection-safe access since isMoveMode is private
        var field = typeof(SelectableItemController).GetField("isMoveMode", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        return field != null && (bool)field.GetValue(controller);
    }
}
