using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(Rigidbody))]
[DisallowMultipleComponent]
public class CarAI : MonoBehaviour
{
    [Header("Movement Settings")]
    public float maxSpeed = 8f;
    public float acceleration = 4f;
    public float deceleration = 6f;
    public float turnSpeed = 5f;

    [Header("Waypoint Settings")]
    public WaypointNode currentNode;

    [Header("Car Detection Settings")]
    public float detectRange = 10f;        // forward detection distance
    public float stopDistance = 3f;        // stop if closer than this
    public float sideDetectRange = 3.5f;   // side ray length
    public float sideAvoidStrength = 0.5f; // steering adjustment
    public float sensorHeight = 0.2f;      // << adjustable detection height
    public float frontOffset = 1.2f;       // how far from car pivot to start front rays
    public LayerMask carLayer;             // assign to "Car" layer

    [Header("Decision Settings")]
    public float decisionStopTime = 1.5f;

    private Rigidbody rb;
    private float currentSpeed = 0f;
    private bool isStopped = false;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.useGravity = false;
        rb.isKinematic = false;
    }

    void FixedUpdate()
    {
        if (currentNode == null || isStopped) return;

        // Detect surrounding cars
        bool frontDetected = DetectCarInDirection(transform.forward, detectRange, out float frontDist);
        bool leftDetected = DetectCarInDirection(-transform.right, sideDetectRange, out float leftDist);
        bool rightDetected = DetectCarInDirection(transform.right, sideDetectRange, out float rightDist);

        // Speed control
        float targetSpeed = maxSpeed;
        if (frontDetected)
        {
            if (frontDist <= stopDistance)
                targetSpeed = 0f;
            else
                targetSpeed = Mathf.Lerp(0f, maxSpeed, (frontDist - stopDistance) / (detectRange - stopDistance));
        }

        // Side avoidance
        Vector3 steeringOffset = Vector3.zero;
        if (leftDetected && !rightDetected)
            steeringOffset += transform.right * sideAvoidStrength;
        else if (rightDetected && !leftDetected)
            steeringOffset -= transform.right * sideAvoidStrength;

        MoveTowardsNode(targetSpeed, steeringOffset);
    }

    void MoveTowardsNode(float targetSpeed, Vector3 steeringOffset)
    {
        if (currentSpeed < targetSpeed)
            currentSpeed += acceleration * Time.fixedDeltaTime;
        else
            currentSpeed -= deceleration * Time.fixedDeltaTime;

        currentSpeed = Mathf.Clamp(currentSpeed, 0f, maxSpeed);

        Vector3 dir = (currentNode.transform.position - transform.position).normalized + steeringOffset;
        dir.Normalize();

        Quaternion targetRot = Quaternion.LookRotation(dir, Vector3.up);
        transform.rotation = Quaternion.Lerp(transform.rotation, targetRot, turnSpeed * Time.fixedDeltaTime);

        rb.MovePosition(transform.position + transform.forward * currentSpeed * Time.fixedDeltaTime);

        if (Vector3.Distance(transform.position, currentNode.transform.position) < 0.6f)
            ChooseNextNode();
    }

    void ChooseNextNode()
    {
        if (currentNode.connectedNodes == null || currentNode.connectedNodes.Count == 0)
        {
            Destroy(gameObject);
            return;
        }

        if (currentNode.isDecisionPoint)
            StartCoroutine(HandleDecision());
        else
            currentNode = currentNode.connectedNodes[Random.Range(0, currentNode.connectedNodes.Count)];
    }

    IEnumerator HandleDecision()
    {
        isStopped = true;
        float prevSpeed = currentSpeed;
        currentSpeed = 0f;

        yield return new WaitForSeconds(decisionStopTime);

        isStopped = false;
        currentSpeed = prevSpeed;
        currentNode = currentNode.connectedNodes[Random.Range(0, currentNode.connectedNodes.Count)];
    }

    bool DetectCarInDirection(Vector3 direction, float range, out float hitDistance)
    {
        hitDistance = range;
        bool detected = false;

        // Origin at sensor height and front offset
        Vector3 baseOrigin = transform.position + Vector3.up * sensorHeight + transform.forward * frontOffset;

        Vector3[] offsets = new Vector3[]
        {
            Vector3.zero,
            transform.right * 0.7f,
            -transform.right * 0.7f
        };

        foreach (var offset in offsets)
        {
            Vector3 origin = baseOrigin + offset;
            if (Physics.Raycast(origin, direction, out RaycastHit hit, range, carLayer))
            {
                if (hit.collider && hit.collider.gameObject != gameObject)
                {
                    detected = true;
                    hitDistance = Mathf.Min(hitDistance, hit.distance);
                    Debug.DrawLine(origin, hit.point, Color.red);
                }
            }
            else
            {
                Debug.DrawRay(origin, direction * range, Color.green);
            }
        }

        return detected;
    }
}
