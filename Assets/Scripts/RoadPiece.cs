using UnityEngine;

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

    [Tooltip("SnapPoint components (ends/connection points). If left empty the script will auto-find children with SnapPoint component.")]
    public SnapPoint[] snapPoints = new SnapPoint[0];

    void Reset()
    {
        AutoCollectSnapPoints();
    }

    void Awake()
    {
        if (snapPoints == null || snapPoints.Length == 0)
            AutoCollectSnapPoints();
    }

    void AutoCollectSnapPoints()
    {
        var points = GetComponentsInChildren<SnapPoint>(true);
        snapPoints = points;
        foreach (var sp in snapPoints)
        {
            if (sp != null) sp.parentRoad = this;
        }
    }

    public void OnPlaced()
    {
        Debug.Log($"Placed {roadType} with {laneCount} lanes at {transform.position}");
    }

    /// <summary>
    /// Returns the closest snap point of this road to worldPos within maxDist. Null if none.
    /// </summary>
    public SnapPoint GetClosestSnapPoint(Vector3 worldPos, float maxDist)
    {
        if (snapPoints == null || snapPoints.Length == 0) return null;

        SnapPoint best = null;
        float bestDist = maxDist;

        foreach (var sp in snapPoints)
        {
            if (sp == null) continue;
            float d = Vector3.Distance(worldPos, sp.transform.position);
            if (d < bestDist)
            {
                bestDist = d;
                best = sp;
            }
        }
        return best;
    }
}
