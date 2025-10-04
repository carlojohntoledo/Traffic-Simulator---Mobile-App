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
    }
}
