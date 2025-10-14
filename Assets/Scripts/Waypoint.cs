using UnityEngine;
using System.Collections.Generic;

public enum LaneType { RightLane, LeftLane }

public class Waypoint : MonoBehaviour
{
    [Header("Lane Settings")]
    public LaneType laneType = LaneType.RightLane;

    [Header("Next Waypoints")]
    public List<Waypoint> nextWaypoints = new List<Waypoint>();

    [Header("Gizmo Settings")]
    public float gizmoRadius = 0.3f;
    public Color gizmoColor = Color.yellow;

    private void OnDrawGizmos()
    {
        Gizmos.color = gizmoColor;
        Gizmos.DrawSphere(transform.position, gizmoRadius);

        Gizmos.color = Color.green;
        foreach (Waypoint wp in nextWaypoints)
        {
            if (wp != null)
                Gizmos.DrawLine(transform.position, wp.transform.position);
        }
    }
}
