using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(MeshRenderer))]
public class GridManager : MonoBehaviour
{
    [Header("Grid Settings")]
    public int gridWidth = 5;
    public int gridHeight = 5;
    public float cellSize = 1f;

    [Header("Visuals")]
    public bool showGrid = true;
    public Color gridColor = Color.green;
    public Color occupiedColor = new Color(1f, 0.3f, 0.3f, 0.6f);

    private Dictionary<Vector2Int, bool> occupiedTiles = new Dictionary<Vector2Int, bool>();
    private Vector3 origin;

    private void Awake()
    {
        origin = transform.position - new Vector3((gridWidth * cellSize) / 2f, 0f, (gridHeight * cellSize) / 2f);
        InitializeGrid();
    }

    private void InitializeGrid()
    {
        occupiedTiles.Clear();

        for (int x = 0; x < gridWidth; x++)
        {
            for (int z = 0; z < gridHeight; z++)
            {
                Vector2Int coord = new Vector2Int(x, z);
                occupiedTiles.Add(coord, false);
            }
        }
    }

    // Convert world position to nearest grid center
    public Vector3 GetNearestGridPosition(Vector3 worldPos)
    {
        float localX = (worldPos.x - origin.x) / cellSize;
        float localZ = (worldPos.z - origin.z) / cellSize;

        int x = Mathf.Clamp(Mathf.RoundToInt(localX), 0, gridWidth - 1);
        int z = Mathf.Clamp(Mathf.RoundToInt(localZ), 0, gridHeight - 1);

        return GetWorldPosition(new Vector2Int(x, z));
    }

    // Get world position of a given tile coordinate
    public Vector3 GetWorldPosition(Vector2Int coord)
    {
        return new Vector3(
            origin.x + coord.x * cellSize + (cellSize / 2f),
            transform.position.y,
            origin.z + coord.y * cellSize + (cellSize / 2f)
        );
    }

    // Get grid coordinate from world position
    public Vector2Int GetGridCoordinate(Vector3 worldPos)
    {
        int x = Mathf.FloorToInt((worldPos.x - origin.x) / cellSize);
        int z = Mathf.FloorToInt((worldPos.z - origin.z) / cellSize);
        return new Vector2Int(x, z);
    }

    // Occupancy methods
    public bool IsTileOccupied(Vector2Int coord)
    {
        if (!occupiedTiles.ContainsKey(coord)) return true;
        return occupiedTiles[coord];
    }

    public void SetTileOccupied(Vector2Int coord, bool occupied)
    {
        if (occupiedTiles.ContainsKey(coord))
            occupiedTiles[coord] = occupied;
    }

    // Draw grid and occupancy in Scene view
    private void OnDrawGizmos()
    {
        if (!showGrid) return;

        Gizmos.color = gridColor;

        Vector3 start = transform.position - new Vector3((gridWidth * cellSize) / 2f, 0f, (gridHeight * cellSize) / 2f);

        for (int x = 0; x <= gridWidth; x++)
        {
            Vector3 from = start + new Vector3(x * cellSize, 0, 0);
            Vector3 to = from + new Vector3(0, 0, gridHeight * cellSize);
            Gizmos.DrawLine(from, to);
        }

        for (int z = 0; z <= gridHeight; z++)
        {
            Vector3 from = start + new Vector3(0, 0, z * cellSize);
            Vector3 to = from + new Vector3(gridWidth * cellSize, 0, 0);
            Gizmos.DrawLine(from, to);
        }

        // Draw occupied tiles
        if (occupiedTiles != null && occupiedTiles.Count > 0)
        {
            foreach (var kvp in occupiedTiles)
            {
                if (kvp.Value)
                {
                    Gizmos.color = occupiedColor;
                    Vector3 center = GetWorldPosition(kvp.Key);
                    Gizmos.DrawCube(center, new Vector3(cellSize * 0.9f, 0.01f, cellSize * 0.9f));
                }
            }
        }
    }
}
