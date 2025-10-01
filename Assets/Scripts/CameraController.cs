using UnityEngine;

public class CameraController : MonoBehaviour
{
    [Header("Move Settings")]
    public float moveSpeed = 20f;        // WASD / arrow movement
    public float dragSpeed = 2f;         // mouse/finger drag speed

    [Header("Zoom Settings")]
    public float zoomSpeed = 10f;        // zoom speed (scroll / pinch)
    public float minZoom = 30f;          // minimum orthographic size
    public float maxZoom = 100f;         // maximum orthographic size

    private Camera cam;
    private Vector3 dragOrigin;

    void Start()
    {
        cam = Camera.main;
    }

    void Update()
    {
#if UNITY_EDITOR || UNITY_STANDALONE
        HandleKeyboard();
        HandleMouseDrag();
        HandleScrollZoom();
#else
        HandleTouchControls();
#endif
    }

    // --- Desktop keyboard movement ---
    void HandleKeyboard()
    {
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");

        Vector3 move = new Vector3(h, 0, v) * moveSpeed * Time.deltaTime;
        transform.Translate(move, Space.World);
    }

    // --- Mouse drag movement ---
    void HandleMouseDrag()
    {
        if (Input.GetMouseButtonDown(1)) // right mouse button
        {
            dragOrigin = Input.mousePosition;
            return;
        }

        if (!Input.GetMouseButton(1)) return;

        Vector3 pos = cam.ScreenToViewportPoint(Input.mousePosition - dragOrigin);
        Vector3 move = new Vector3(-pos.x * dragSpeed, 0, -pos.y * dragSpeed);

        transform.Translate(move, Space.World);
        dragOrigin = Input.mousePosition;
    }

    // --- Mouse wheel zoom ---
    void HandleScrollZoom()
    {
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (Mathf.Abs(scroll) > 0.01f)
        {
            cam.orthographicSize -= scroll * zoomSpeed;
            cam.orthographicSize = Mathf.Clamp(cam.orthographicSize, minZoom, maxZoom);
        }
    }

    // --- Mobile touch controls ---
    void HandleTouchControls()
    {
        if (Input.touchCount == 1) // drag with one finger
        {
            Touch t = Input.GetTouch(0);

            if (t.phase == TouchPhase.Began)
            {
                dragOrigin = t.position;
            }
            else if (Input.touchCount == 2) // pinch zoom
            {
                Touch t0 = Input.GetTouch(0);
                Touch t1 = Input.GetTouch(1);

                // previous positions of touches
                Vector2 prev0 = t0.position - t0.deltaPosition;
                Vector2 prev1 = t1.position - t1.deltaPosition;

                // distances
                float prevDist = (prev0 - prev1).magnitude;
                float currDist = (t0.position - t1.position).magnitude;

                // difference (positive = zoom out, negative = zoom in)
                float delta = prevDist - currDist;

                cam.orthographicSize += delta * Time.deltaTime * zoomSpeed;
                cam.orthographicSize = Mathf.Clamp(cam.orthographicSize, minZoom, maxZoom);
            }

        }
        else if (Input.touchCount == 2) // pinch zoom
        {
            Touch t0 = Input.GetTouch(0);
            Touch t1 = Input.GetTouch(1);

            Vector2 prev0 = t0.position - t0.deltaPosition;
            Vector2 prev1 = t1.position - t1.deltaPosition;

            float prevDist = (prev0 - prev1).magnitude;
            float currDist = (t0.position - t1.position).magnitude;

            float delta = prevDist - currDist;

            cam.orthographicSize += delta * Time.deltaTime * zoomSpeed;
            cam.orthographicSize = Mathf.Clamp(cam.orthographicSize, minZoom, maxZoom);
        }
    }

    // --- For UI slider hookup ---
    public void SetZoom(float value)
    {
        cam.orthographicSize = Mathf.Clamp(value, minZoom, maxZoom);
    }
}
