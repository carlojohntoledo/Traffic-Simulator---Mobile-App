using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(Collider))]
public class ItemDragger : MonoBehaviour
{
    [Header("Settings")]
    public LayerMask groundLayer;
    public float dragHeightOffset = 0.05f;
    public Camera mainCam;

    private bool isDragging = false;
    private bool dragStartedOnObject = false;

    private void Start()
    {
        if (mainCam == null) mainCam = Camera.main;
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

    public void EnableDragging(bool enable)
    {
        isDragging = enable;
        dragStartedOnObject = false;
        Debug.Log($"[ItemDragger] EnableDragging({enable}) on {gameObject.name}");
    }

    private void HandleMouseDrag()
    {
        if (Input.GetMouseButtonDown(0))
        {
            bool overUI = EventSystem.current != null && EventSystem.current.IsPointerOverGameObject();
            bool overCameraPanel = CameraDragPanel.IsPointerOverCameraPanel;

            // Only ignore if it's over UI that is *not* the camera panel
            if (overUI && !overCameraPanel)
            {
                Debug.Log("[ItemDragger] Clicked over UI (not camera panel) — ignoring.");
                dragStartedOnObject = false;
                return;
            }

            Ray ray = mainCam.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit, 300f))
            {
                Debug.Log($"[ItemDragger] Ray hit {hit.transform.name}");
                if (hit.transform == transform || hit.transform.IsChildOf(transform))
                {
                    dragStartedOnObject = true;
                    InputBlocker.IsModelDragging = true;
                    Debug.Log($"[ItemDragger] Started dragging {gameObject.name}");
                }
            }
        }

        if (Input.GetMouseButton(0) && dragStartedOnObject)
        {
            Ray ray = mainCam.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit, 300f, groundLayer))
            {
                transform.position = hit.point + Vector3.up * dragHeightOffset;
            }
        }

        if (Input.GetMouseButtonUp(0))
        {
            if (dragStartedOnObject)
                Debug.Log($"[ItemDragger] Stopped dragging {gameObject.name}");

            dragStartedOnObject = false;
            InputBlocker.IsModelDragging = false;
        }
    }

    private void HandleTouchDrag()
    {
        if (Input.touchCount == 0) return;

        Touch touch = Input.GetTouch(0);
        bool overUI = EventSystem.current != null && EventSystem.current.IsPointerOverGameObject(touch.fingerId);
        bool overCameraPanel = CameraDragPanel.IsPointerOverCameraPanel;

        if (touch.phase == TouchPhase.Began)
        {
            if (overUI && !overCameraPanel)
            {
                Debug.Log("[ItemDragger] Touch over UI (not camera panel) — ignoring.");
                dragStartedOnObject = false;
                return;
            }

            Ray ray = mainCam.ScreenPointToRay(touch.position);
            if (Physics.Raycast(ray, out RaycastHit hit, 300f))
            {
                if (hit.transform == transform || hit.transform.IsChildOf(transform))
                {
                    dragStartedOnObject = true;
                    InputBlocker.IsModelDragging = true;
                    Debug.Log($"[ItemDragger] Started touch dragging {gameObject.name}");
                }
            }
        }

        if (touch.phase == TouchPhase.Moved && dragStartedOnObject)
        {
            Ray ray = mainCam.ScreenPointToRay(touch.position);
            if (Physics.Raycast(ray, out RaycastHit hit, 300f, groundLayer))
            {
                transform.position = hit.point + Vector3.up * dragHeightOffset;
            }
        }

        if (touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled)
        {
            dragStartedOnObject = false;
            InputBlocker.IsModelDragging = false;
            Debug.Log($"[ItemDragger] Stopped touch dragging {gameObject.name}");
        }
    }
}
