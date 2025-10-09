using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public class EditRoadItem : MonoBehaviour
{
    [Header("Item Data Reference")]
    public ItemData data;

    [Header("Road Settings")]
    public int length = 1;
    public GameObject roadSegmentPrefab;
    public List<GameObject> roadSegments = new List<GameObject>();

    private BoxCollider boxCollider;

    private void Awake()
    {
        boxCollider = GetComponent<BoxCollider>();
    }

    private void Start()
    {
        if (data != null)
        {
            InitializeFromData();
        }
        else
        {
            Debug.LogWarning($"[EditRoadItem] No ItemData found on {name} — please assign via Initialize() before Start!");
        }
    }

    public void Initialize(ItemData itemData)
    {
        data = itemData;
        if (data != null)
        {
            InitializeFromData();
            Debug.Log($"[EditRoadItem] Initialized with data: {data.itemName}");
        }
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

        Debug.Log($"[EditRoadItem] Applied edit changes and rebuilt road with {length} segments.");
    }

    public void RebuildRoadSegments()
    {
        // Clear old segments
        foreach (var seg in roadSegments)
        {
            if (seg != null)
                Destroy(seg);
        }
        roadSegments.Clear();

        if (roadSegmentPrefab == null)
        {
            Debug.LogWarning($"[EditRoadItem] No roadSegmentPrefab assigned for {name}!");
            return;
        }

        // --- Spawn segments ---
        float segmentLength = 5f; // Use your road mesh length in local Z units
        for (int i = 0; i < length; i++)
        {
            GameObject seg = Instantiate(roadSegmentPrefab, transform);
            seg.transform.localPosition = new Vector3(0f, 0f, i * segmentLength);
            seg.transform.localRotation = Quaternion.identity;
            roadSegments.Add(seg);
        }

        Debug.Log($"[EditRoadItem] Rebuilt road with {length} segments for {name}");

        // --- Update collider ---
        UpdateColliderToFitSegments();
    }

    /// <summary>
    /// Updates the root BoxCollider to encompass all segment meshes automatically.
    /// </summary>
    private void UpdateColliderToFitSegments()
    {
        if (boxCollider == null)
            boxCollider = GetComponent<BoxCollider>();

        if (roadSegments.Count == 0)
            return;

        Bounds combinedBounds = new Bounds(roadSegments[0].transform.position, Vector3.zero);
        foreach (var seg in roadSegments)
        {
            Renderer rend = seg.GetComponentInChildren<Renderer>();
            if (rend != null)
                combinedBounds.Encapsulate(rend.bounds);
        }

        // Move bounds to local space of root
        Vector3 localCenter = transform.InverseTransformPoint(combinedBounds.center);
        boxCollider.center = localCenter;
        boxCollider.size = combinedBounds.size;

        Debug.Log($"[EditRoadItem] Updated collider for {name}: Center={boxCollider.center}, Size={boxCollider.size}");
    }

}
