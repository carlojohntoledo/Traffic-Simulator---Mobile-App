using UnityEngine;

public enum DestinationType
{
    Forward,
    Decision
}

[DisallowMultipleComponent]
public class DestinationPoint : MonoBehaviour
{
    [Header("Destination Settings")]
    public DestinationType destinationType = DestinationType.Forward;

    [Header("Detection Settings")]
    public float detectionRadius = 10f; // how far car can look for next points

    private void OnDrawGizmos()
    {
        Gizmos.color = destinationType == DestinationType.Forward ? Color.green : Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
        Gizmos.DrawRay(transform.position, transform.forward * 2);
    }
}
