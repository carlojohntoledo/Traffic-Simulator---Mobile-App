using UnityEngine;
using System.Collections.Generic;

public class GroundGrid : MonoBehaviour
{
    [Header("Grid Settings")]
    public float cellSize = 2f;        // e.g. road piece size
    public int gridWidth = 50;
    public int gridHeight = 50;
    public Vector3 origin = Vector3.zero;

    private Dictionary<Vector2Int, GameObject> occupiedCells = new Dictionary<Vector2Int, GameObject>();

    public static GroundGrid Instance { get; private set; }

    void Awake()
    {
        Instance = this;
    }

    // Convert world position to grid coordinate
    public Vector2Int WorldToGrid(Vector3 worldPos)
    {
        Vector3 local = worldPos - origin;
        int x = Mathf.RoundToInt(local.x / cellSize);
        int z = Mathf.RoundToInt(local.z / cellSize);
        return new Vector2Int(x, z);
    }

    // Convert grid coordinate to world position (center of cell)
    public Vector3 GridToWorld(Vector2Int gridPos)
    {
        return origin + new Vector3(gridPos.x * cellSize, 0, gridPos.y * cellSize);
    }

    public bool IsOccupied(Vector2Int gridPos)
    {
        return occupiedCells.ContainsKey(gridPos);
    }

    public void OccupyCell(Vector2Int gridPos, GameObject occupant)
    {
        if (!occupiedCells.ContainsKey(gridPos))
            occupiedCells.Add(gridPos, occupant);
    }

    public void FreeCell(Vector2Int gridPos)
    {
        if (occupiedCells.ContainsKey(gridPos))
            occupiedCells.Remove(gridPos);
    }

    public void FreeCells(GameObject occupant)
    {
        List<Vector2Int> toRemove = new List<Vector2Int>();
        foreach (var kvp in occupiedCells)
        {
            if (kvp.Value == occupant)
                toRemove.Add(kvp.Key);
        }
        foreach (var cell in toRemove)
            occupiedCells.Remove(cell);
    }

    public bool CanPlaceAt(Vector2Int gridPos)
    {
        return !IsOccupied(gridPos);
    }
}
