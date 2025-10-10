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
    public Transform StartPoint;
    public Transform EndPoint;

    [Header("Snap Settings")]
    public float snapDistance = 1.0f;
    public LayerMask snapLayer;

    private BoxCollider rootCollider;

    private void Awake()
    {
        rootCollider = GetComponent<BoxCollider>();

        if (StartPoint == null)
        {
            StartPoint = new GameObject("StartPoint").transform;
            StartPoint.SetParent(transform);
        }

        if (EndPoint == null)
        {
            EndPoint = new GameObject("EndPoint").transform;
            EndPoint.SetParent(transform);
        }
    }

    private void Start()
    {
        if (data != null)
            InitializeFromData();
    }

    private void Update()
    {
        UpdateSnapPoints();
        TrySnapToNearbyPrefab(); // ✅ continuous snapping like PrefabSnapHandler
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
        foreach (var seg in roadSegments)
            if (seg != null) Destroy(seg.gameObject);

        roadSegments.Clear();

        if (roadSegmentPrefab == null)
        {
            Debug.LogWarning($"[EditRoadItem] No roadSegmentPrefab assigned for {name}!");
            return;
        }

        float segmentLength = 5f; // adjust to your mesh length
        for (int i = 0; i < length; i++)
        {
            GameObject seg = Instantiate(roadSegmentPrefab, transform);
            seg.transform.localPosition = new Vector3(0f, 0f, i * segmentLength);
            seg.transform.localRotation = Quaternion.identity;
            roadSegments.Add(seg.transform);
        }

        UpdateColliderToSegments();
        UpdateSnapPoints();
    }

    // ============================================================
    // Collider & Snap Points
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
        if (roadSegments.Count == 0) return;

        // Estimate segment length using first segment’s renderer
        float segmentLength = 5f; // default fallback
        Renderer firstRenderer = roadSegments[0].GetComponentInChildren<Renderer>();
        if (firstRenderer != null)
            segmentLength = firstRenderer.bounds.size.z;

        // Position StartPoint slightly before first segment
        Vector3 startPos = roadSegments[0].position - roadSegments[0].forward * (segmentLength * 0.5f);
        StartPoint.position = startPos;

        // Position EndPoint slightly after last segment
        Transform lastSeg = roadSegments[roadSegments.Count - 1];
        Vector3 endPos = lastSeg.position + lastSeg.forward * (segmentLength * 0.5f);
        EndPoint.position = endPos;

        UpdateColliderToSegments();
    }


    // ============================================================
    // Continuous Snap (rebuilt PrefabSnapHandler logic)
    // ============================================================

    private void TrySnapToNearbyPrefab()
    {
        if (StartPoint == null || EndPoint == null) return;

        // Look for nearby colliders within snap distance
        Collider[] hits = Physics.OverlapSphere(StartPoint.position, snapDistance, snapLayer);
        foreach (var hit in hits)
        {
            if (hit.gameObject == gameObject) continue;

            EditRoadItem other = hit.GetComponent<EditRoadItem>();
            if (other == null || other == this) continue;

            // Snap Start -> Other End
            float dist = Vector3.Distance(StartPoint.position, other.EndPoint.position);
            if (dist <= snapDistance)
            {
                PerformSnap(other.EndPoint.position, StartPoint.position);
                Debug.Log($"[EditRoadItem] Snapped {name} Start → {other.name} End");
                return;
            }

            // Snap End -> Other Start
            dist = Vector3.Distance(EndPoint.position, other.StartPoint.position);
            if (dist <= snapDistance)
            {
                PerformSnap(other.StartPoint.position, EndPoint.position);
                Debug.Log($"[EditRoadItem] Snapped {name} End → {other.name} Start");
                return;
            }
        }
    }

    // ============================================================
    // Manual Snap (for SelectableItemController)
    // ============================================================

    public bool TrySnapTo(EditRoadItem other)
    {
        if (other == null) return false;

        float distStartEnd = Vector3.Distance(StartPoint.position, other.EndPoint.position);
        float distEndStart = Vector3.Distance(EndPoint.position, other.StartPoint.position);

        if (distStartEnd <= snapDistance)
        {
            PerformSnap(other.EndPoint.position, StartPoint.position);
            return true;
        }

        if (distEndStart <= snapDistance)
        {
            PerformSnap(other.StartPoint.position, EndPoint.position);
            return true;
        }

        return false;
    }

    private void PerformSnap(Vector3 targetPos, Vector3 localSnapPos)
    {
        Vector3 offset = targetPos - localSnapPos;
        transform.position += offset;

        Vector3 dir = (EndPoint.position - StartPoint.position).normalized;
        if (dir != Vector3.zero)
            transform.rotation = Quaternion.LookRotation(dir);

        UpdateSnapPoints();
        UpdateColliderToSegments();
    }

    // ============================================================
    // Debug Visualization
    // ============================================================

    private void OnDrawGizmos()
    {
        if (StartPoint != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawSphere(StartPoint.position, 0.1f);
        }

        if (EndPoint != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(EndPoint.position, 0.1f);
        }
    }
}

public class SnapPointHolder : MonoBehaviour
{
    public Transform Start;
    public Transform End;
}
