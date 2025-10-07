using System.Collections.Generic;
using UnityEngine;

public class BuildItem : MonoBehaviour
{
    [Header("Item Data Reference")]
    public ItemData data;

    [Header("Immutable Snapshot")]
    public string staticName;
    public Sprite staticImage;

    [Header("Road Settings")]
    public int length = 1;
    public GameObject roadSegmentPrefab;
    public List<GameObject> roadSegments = new List<GameObject>();

    [Header("Vehicle Settings")]
    public float vehicleSpeed;

    [Header("Pedestrian Settings")]
    public float pedestrianSpeed;

    [Header("Traffic Light Settings")]
    public float stopTime;
    public float slowdownTime;
    public float goTime;
    public bool hazardMode;
    public bool flashingMode;

    [Header("Traffic Sign Settings")]
    public int signPriority;

    [Header("Spawner Settings")]
    public int maxSpawnCount;
    public float spawnInterval;

    public void Initialize(ItemData itemData)
    {
        data = itemData;
        staticName = data.itemName;
        staticImage = data.previewImage;

        ApplyData();
    }

    public void ApplyData()
    {
        if (data == null) return;

        length = Mathf.RoundToInt(data.roadLength);
        vehicleSpeed = data.vehicleDefaultSpeed;
        pedestrianSpeed = data.pedestrianDefaultSpeed;
        stopTime = data.stopTime;
        slowdownTime = data.slowdownTime;
        goTime = data.goTime;
        hazardMode = data.hazardMode;
        flashingMode = data.flashingMode;
        signPriority = data.signPriority;
        maxSpawnCount = data.maxSpawnCount;
        spawnInterval = data.spawnInterval;
    }

    public void OnAttributesChanged()
    {
        ApplyData();

        // Modular road rebuild
        if (data.type == ItemType.Roads)
        {
            RebuildRoadSegments();
        }
    }

    private void RebuildRoadSegments()
    {
        // Remove old segments
        foreach (var seg in roadSegments)
        {
            if (seg != null) Destroy(seg);
        }
        roadSegments.Clear();

        if (roadSegmentPrefab == null)
        {
            Debug.LogWarning("[BuildItem] No roadSegmentPrefab assigned!");
            return;
        }

        float segmentLength = 1f;
        for (int i = 0; i < length; i++)
        {
            Vector3 pos = transform.position + transform.forward * i * segmentLength;
            GameObject seg = Instantiate(roadSegmentPrefab, pos, transform.rotation, transform);
            roadSegments.Add(seg);
        }

        Debug.Log($"[BuildItem] Rebuilt road with {length} segments for {name}");
    }
}
