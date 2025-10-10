// using UnityEngine;
// using System.Collections.Generic;

// [RequireComponent(typeof(Collider))]
// public class PrefabSnapHandler : MonoBehaviour
// {
//     [Header("Snap Points")]
//     public Transform StartPoint;
//     public Transform EndPoint;

//     [Header("Snap Settings")]
//     public float snapDistance = 1.0f; // Max distance to snap
//     public LayerMask snapLayer;       // Layer of other root prefabs

//     [Header("Segment References")]
//     public List<Transform> roadSegments = new List<Transform>();

//     private Collider rootCollider;

//     private void Awake()
//     {
//         rootCollider = GetComponent<Collider>();
//         UpdateSnapPoints();
//     }

//     private void Update()
//     {
//         UpdateSnapPoints();
//         TrySnapToNearbyPrefab();
//     }

//     /// <summary>
//     /// Update StartPoint and EndPoint dynamically based on first and last segment
//     /// </summary>
//     public void UpdateSnapPoints()
//     {
//         if (roadSegments.Count == 0) return;

//         // First segment -> StartPoint
//         if (StartPoint != null)
//             StartPoint.position = roadSegments[0].position;

//         // Last segment -> EndPoint
//         if (EndPoint != null)
//         {
//             Transform lastSegment = roadSegments[roadSegments.Count - 1];
//             SnapPointHolder lastSnap = lastSegment.GetComponent<SnapPointHolder>();
//             if (lastSnap != null)
//                 EndPoint.position = lastSnap.End.position;
//             else
//                 EndPoint.position = lastSegment.position; // fallback
//         }

//         UpdateColliderToSegments();
//     }

//     /// <summary>
//     /// Adjust root collider to cover all segments
//     /// </summary>
//     private void UpdateColliderToSegments()
//     {
//         if (rootCollider == null || roadSegments.Count == 0) return;

//         Bounds bounds = new Bounds(roadSegments[0].position, Vector3.zero);
//         foreach (var seg in roadSegments)
//         {
//             Renderer rend = seg.GetComponent<Renderer>();
//             if (rend != null)
//                 bounds.Encapsulate(rend.bounds);
//         }

//         if (rootCollider is BoxCollider box)
//         {
//             box.center = bounds.center - transform.position;
//             box.size = bounds.size;
//         }
//     }

//     /// <summary>
//     /// Check for nearby snap candidates and snap if close
//     /// </summary>
//     private void TrySnapToNearbyPrefab()
//     {
//         if (StartPoint == null || EndPoint == null) return;

//         // Check all colliders in snapLayer within snapDistance
//         Collider[] hits = Physics.OverlapSphere(StartPoint.position, snapDistance, snapLayer);
//         foreach (var hit in hits)
//         {
//             if (hit.gameObject == gameObject) continue; // skip self

//             PrefabSnapHandler other = hit.GetComponent<PrefabSnapHandler>();
//             if (other == null) continue;

//             // Snap Start to other End
//             float distance = Vector3.Distance(StartPoint.position, other.EndPoint.position);
//             if (distance <= snapDistance)
//             {
//                 Vector3 offset = other.EndPoint.position - StartPoint.position;
//                 transform.position += offset;

//                 // Align rotation along connection
//                 Vector3 dir = (EndPoint.position - StartPoint.position).normalized;
//                 if (dir != Vector3.zero)
//                     transform.rotation = Quaternion.LookRotation(dir);

//                 Debug.Log($"[PrefabSnapHandler] Snapped {name} to {other.name}");
//                 return;
//             }

//             // Snap End to other Start
//             distance = Vector3.Distance(EndPoint.position, other.StartPoint.position);
//             if (distance <= snapDistance)
//             {
//                 Vector3 offset = other.StartPoint.position - EndPoint.position;
//                 transform.position += offset;

//                 Vector3 dir = (EndPoint.position - StartPoint.position).normalized;
//                 if (dir != Vector3.zero)
//                     transform.rotation = Quaternion.LookRotation(dir);

//                 Debug.Log($"[PrefabSnapHandler] Snapped {name} to {other.name}");
//                 return;
//             }
//         }
//     }

//     private void OnDrawGizmos()
//     {
//         if (StartPoint != null)
//         {
//             Gizmos.color = Color.green;
//             Gizmos.DrawSphere(StartPoint.position, 0.1f);
//         }

//         if (EndPoint != null)
//         {
//             Gizmos.color = Color.red;
//             Gizmos.DrawSphere(EndPoint.position, 0.1f);
//         }
//     }
// }

// /// <summary>
// /// Optional helper component for segments to define local start/end snap points
// /// </summary>
// public class SnapPointHolder : MonoBehaviour
// {
//     public Transform Start;
//     public Transform End;
// }
