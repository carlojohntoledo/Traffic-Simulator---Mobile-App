using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public class WaypointNode : MonoBehaviour
{
    public List<WaypointNode> connectedNodes = new List<WaypointNode>();
    public bool isDecisionPoint = false;

    private void OnDrawGizmos()
    {
        Gizmos.color = isDecisionPoint ? Color.yellow : Color.green;
        Gizmos.DrawSphere(transform.position, 0.3f);
        foreach (var node in connectedNodes)
        {
            if (node != null)
                Gizmos.DrawLine(transform.position, node.transform.position);
        }
    }
}
