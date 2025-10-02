using UnityEngine;

[DisallowMultipleComponent]
public class SnapPoint : MonoBehaviour
{
    [Tooltip("Is this snap point already connected to another road?")]
    public bool isOccupied = false;
    public SnapPoint connectedTo;

    [HideInInspector] public RoadPiece parentRoad;

    void Awake()
    {
        parentRoad = GetComponentInParent<RoadPiece>();
    }

    public void Occupy()
    {
        isOccupied = true;
    }

    public void Release()
    {
        isOccupied = false;
    }

    // Visual debug in the Scene view
    void OnDrawGizmos()
    {
        Gizmos.color = isOccupied ? Color.red : Color.green;
        Gizmos.DrawSphere(transform.position, 0.1f);
        Gizmos.color = Color.blue;
        Gizmos.DrawRay(transform.position, transform.forward * 0.5f);
    }
}
