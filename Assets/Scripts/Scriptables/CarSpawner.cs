using UnityEngine;
using System.Collections;

public class CarSpawner : MonoBehaviour
{
    public GameObject[] carPrefabs;
    public WaypointNode startNode;
    public int maxCars = 10;
    public float spawnInterval = 3f;

    private int spawned = 0;

    public void StartSpawn()
    {
        StartCoroutine(SpawnLoop());
    }

    IEnumerator SpawnLoop()
    {
        while (spawned < maxCars)
        {
            SpawnCar();
            yield return new WaitForSeconds(spawnInterval);
        }
    }

    void SpawnCar()
    {
        if (carPrefabs.Length == 0 || startNode == null) return;

        GameObject prefab = carPrefabs[Random.Range(0, carPrefabs.Length)];
        GameObject car = Instantiate(prefab, startNode.transform.position, startNode.transform.rotation);
        car.GetComponent<CarAI>().currentNode = startNode;
        spawned++;
    }
}
