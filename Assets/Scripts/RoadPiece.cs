using UnityEngine;
using System.Collections.Generic;

public enum RoadType
{
    Straight,
    Corner,
    Intersection
}

[DisallowMultipleComponent]
public class RoadPiece : MonoBehaviour
{
    [Header("Road Settings")]
    public RoadType roadType = RoadType.Straight;
    public int laneCount = 1;

    [Tooltip("Snap size in world units (e.g., 1, 2, 4).")]
    public float snapSize = 1f;

    [Tooltip("Rotation step for snapping (e.g., 90 for corners).")]
    public float rotationStep = 90f;

    [Tooltip("Assign SnapPoint transforms (ends/connection points). If left empty the script will try to auto-find children named 'SnapPoint'")]
    public Transform[] snapPoints;

    void Reset()
    {
        // Try to auto-find snap points in children when the component is added in the editor
        AutoCollectSnapPoints();
    }

    void Awake()
    {
        if (snapPoints == null || snapPoints.Length == 0)
            AutoCollectSnapPoints();
    }

    void AutoCollectSnapPoints()
    {
        var list = new List<Transform>();
        foreach (Transform t in GetComponentsInChildren<Transform>(true))
        {
            if (t == this.transform) continue;
            // match children named SnapPoint (case-insensitive) or with a "SnapPoint" tag/name pattern
            if (t.name.ToLower().Contains("snappoint"))
                list.Add(t);
        }

        if (list.Count > 0)
            snapPoints = list.ToArray();
    }

    /// <summary>
    /// Called when road is placed permanently.
    /// Extend this for registering in a road network, adding colliders, updating navmesh, etc.
    /// </summary>
    public void OnPlaced()
    {
        Debug.Log($"Placed {roadType} with {laneCount} lanes at {transform.position}");
    }

    /// <summary>
    /// Return the closest snap point of this road to worldPos within maxDist. Returns null if none.
    /// </summary>
    public Transform GetClosestSnapPoint(Vector3 worldPos, float maxDist)
    {
        if (snapPoints == null || snapPoints.Length == 0) return null;

        Transform best = null;
        float bestDist = maxDist;

        foreach (var sp in snapPoints)
        {
            if (sp == null) continue;
            float d = Vector3.Distance(worldPos, sp.position);
            if (d < bestDist)
            {
                bestDist = d;
                best = sp;
            }
        }

        return best;
    }
}
