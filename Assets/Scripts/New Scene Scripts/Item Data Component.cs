using UnityEngine;

public enum ItemModelType
{
    Road,
    Vehicle,
    Pedestrian,
    Spawner,
    Signage,
    TrafficLight
}

public enum RoadType
{
    RoadWay,
    HighWay,
    Intersection,
    HighwayIntersection
}

public enum CarType
{
    Motorcycle,
    LightWeight,
    HeavyWeight
}

public enum PedestrianType
{
    Human
}

public enum SpawnType
{
    Vehicle,
    Pedestrian
}

public enum SpawnMode
{
    Single,
    Random
}

public enum TrafficLightStart
{
    Go,
    Slow,
    Stop
}

[CreateAssetMenu(fileName = "NewItemData", menuName = "TrafficManager/Item Data", order = 1)]
public class ItemDataComponent : ScriptableObject
{
    // =======================
    // Item Details
    // =======================
    [Header("Item Details (Read-Only in Play Mode)")]
    public string itemName;
    public ItemModelType itemModelType;
    public Sprite itemImagePreview;
    public GameObject itemModelPrefab;
    [TextArea(2, 5)] public string itemDescription;

    // =======================
    // Item Components
    // =======================
    [Header("Item Components")]

    // ---- Road ----
    [HideInInspector] public RoadType roadType;
    [HideInInspector] public int roadLength;

    // ---- Vehicle ----
    [HideInInspector] public CarType carType;
    [HideInInspector] public int vehicleSpeed;
    [HideInInspector] public int vehicleMaxSpeed;
    [HideInInspector] public int vehicleMinSpeed;

    // ---- Pedestrian ----
    [HideInInspector] public PedestrianType pedestrianType;
    [HideInInspector] public int pedestrianSpeed;

    // ---- Spawner ----
    [HideInInspector] public SpawnType spawnType;
    [HideInInspector] public int maxSpawn;
    [HideInInspector] public int spawnInterval;
    [HideInInspector] public GameObject[] spawnModelPrefabs;
    [HideInInspector] public SpawnMode spawnMode;

    // ---- Signage ----
    [HideInInspector][Range(0, 1)] public int rulePriority;

    // ---- Traffic Light ----
    [HideInInspector] public TrafficLightStart startLight;
    [HideInInspector] public int goTime;
    [HideInInspector] public int slowTime;
    [HideInInspector] public int stopTime;
    [HideInInspector] public bool hazardMode;
}
