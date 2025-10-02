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
    public float roadLength;

    // --- Spawner ---
    public SpawnerType spawnerType;
    public int maxSpawnCount;
    public float spawnInterval;

    // --- Pedestrians ---
    public float pedestrianDefaultSpeed;

    // --- Rules ---
    public TrafficRuleType trafficRuleType;

    // Traffic Sign
    public int signPriority;

    // Traffic Light
    public float stopTime = 5f;
    public float slowdownTime = 2f;
    public float goTime = 5f;
    public bool hazardMode = false;
    public bool flashingMode = false;

    // --- Vehicles ---
    public VehicleType vehicleType;
    public float vehicleDefaultSpeed;
}
