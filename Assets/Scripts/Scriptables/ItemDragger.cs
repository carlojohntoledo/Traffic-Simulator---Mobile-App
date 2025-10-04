using UnityEngine;

[RequireComponent(typeof(Collider))]
public class ItemDragger : MonoBehaviour
{
    [Header("Settings")]
    public LayerMask groundLayer; // assign your ground layer here
    public float dragHeightOffset = 0.05f; // small height to avoid clipping

    private Camera mainCam;
    private bool isDragging = false;
    private Vector3 offset;

    private void Start()
    {
        mainCam = Camera.main;
    }

    private void Update()
    {
        if (!isDragging) return;

#if UNITY_EDITOR || UNITY_STANDALONE
        HandleMouseDrag();
#elif UNITY_ANDROID || UNITY_IOS
        HandleTouchDrag();
#endif
    }

    private void HandleMouseDrag()
    {
        if (Input.GetMouseButton(0))
        {
            Ray ray = mainCam.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit, 200f, groundLayer))
            {
                Vector3 targetPos = hit.point + Vector3.up * dragHeightOffset;
                transform.position = targetPos;
            }
        }
    }

    private void HandleTouchDrag()
    {
        if (Input.touchCount == 1)
        {
            Touch touch = Input.GetTouch(0);
            Ray ray = mainCam.ScreenPointToRay(touch.position);
            if (Physics.Raycast(ray, out RaycastHit hit, 200f, groundLayer))
            {
                Vector3 targetPos = hit.point + Vector3.up * dragHeightOffset;
                transform.position = targetPos;
            }
        }
    }

    public void EnableDragging(bool enable)
    {
        isDragging = enable;
    }
}
