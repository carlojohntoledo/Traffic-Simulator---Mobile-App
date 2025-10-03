using UnityEngine;

public enum ItemType
{
    Roads,
    Spawner,
    Pedestrians,
    Rules,
    Vehicles
}

public enum SpawnerType
{
    Car,
    Pedestrian
}

public enum VehicleType
{
    Motorcycle,
    LightVehicle,
    HeavyVehicle
}

public enum TrafficRuleType
{
    TrafficSign,
    TrafficLight
}

[CreateAssetMenu(fileName = "NewItemData", menuName = "TrafficManager/ItemData", order = 0)]
public class ItemData : ScriptableObject
{
    [Header("General Info")]
    public ItemType type;
    public string itemName;
    [TextArea] public string description;

    [Header("Visuals")]
    public Sprite previewImage;

    [Header("Prefab Reference")]
    public GameObject itemPrefab;

    // --- Roads ---
    [Header("Road Settings")]
    [Tooltip("Desired road length in meters (each prefab piece = 0.5m).")]
    public float roadLength = 1f;

    // --- Spawner ---
    [Header("Spawner Settings")]
    public SpawnerType spawnerType;
    [Tooltip("Maximum number of objects this spawner can create.")]
    public int maxSpawnCount = 10;
    [Tooltip("Time between spawns (seconds).")]
    public float spawnInterval = 1f;

    // --- Pedestrians ---
    [Header("Pedestrian Settings")]
    public float pedestrianDefaultSpeed = 1.5f;

    // --- Rules ---
    [Header("Traffic Rule Settings")]
    public TrafficRuleType trafficRuleType;

    // Traffic Sign
    [Tooltip("Priority for signs (lower = higher priority).")]
    public int signPriority = 1;

    // Traffic Light
    [Tooltip("Time light stays red.")]
    public float stopTime = 5f;
    [Tooltip("Time light stays yellow.")]
    public float slowdownTime = 2f;
    [Tooltip("Time light stays green.")]
    public float goTime = 5f;
    public bool hazardMode = false;
    public bool flashingMode = false;

    // --- Vehicles ---
    [Header("Vehicle Settings")]
    public VehicleType vehicleType;
    public float vehicleDefaultSpeed = 10f;
}
