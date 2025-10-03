using System.Collections.Generic;
using UnityEngine;

public class BuildItem : MonoBehaviour
{
    [Header("Item Data Reference")]
    public ItemData data;

    [Header("Roads")]
    public int length = 1;
    public List<GameObject> roadSegments = new List<GameObject>();

    [Header("Vehicles")]
    public float vehicleSpeed;

    [Header("Pedestrians")]
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
    }
}
