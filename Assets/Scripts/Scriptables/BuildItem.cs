using UnityEngine;

public class BuildItem : MonoBehaviour
{
    [HideInInspector] public ItemData data;

    // Runtime editable values
    public float length;                 // Roads
    public float vehicleSpeed;           // Vehicles
    public float pedestrianSpeed;        // Pedestrians

    // Rules
    public float stopTime;
    public float slowdownTime;
    public float goTime;
    public bool hazardMode;
    public bool flashingMode;
    public int signPriority;

    // Spawner
    public int maxSpawnCount;
    public float spawnInterval;

    public void Initialize(ItemData itemData)
    {
        data = itemData;

        // Copy defaults from ItemData
        switch (data.type)
        {
            case ItemType.Roads:
                length = data.roadLength;
                break;

            case ItemType.Vehicles:
                vehicleSpeed = data.vehicleDefaultSpeed;
                break;

            case ItemType.Pedestrians:
                pedestrianSpeed = data.pedestrianDefaultSpeed;
                break;

            case ItemType.Rules:
                if (data.trafficRuleType == TrafficRuleType.TrafficLight)
                {
                    stopTime = data.stopTime;
                    slowdownTime = data.slowdownTime;
                    goTime = data.goTime;
                    hazardMode = data.hazardMode;
                    flashingMode = data.flashingMode;
                }
                else if (data.trafficRuleType == TrafficRuleType.TrafficSign)
                {
                    signPriority = data.signPriority;
                }
                break;

            case ItemType.Spawner:
                maxSpawnCount = data.maxSpawnCount;
                spawnInterval = data.spawnInterval;
                break;
        }
    }
}
