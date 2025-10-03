using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CameraUIController : MonoBehaviour, IDragHandler, IBeginDragHandler, IEndDragHandler
{
    [Header("References")]
    public Camera cam;
    public RectTransform dragPanel;
    public Slider zoomSlider;
    public Transform ground;

    [Header("Settings")]
    public float dragSpeed = 0.5f;
    public float minZoom = 30f;
    public float maxZoom = 100f;
    public float pinchZoomSpeed = 0.1f;

    [Header("Layers")]
    public LayerMask clickableLayer; // Assign "Road" layer here

    private bool dragging = false;
    private Vector2 lastPos;

    private Vector2 minBounds;
    private Vector2 maxBounds;

    void Awake()
    {
        if (cam == null) cam = Camera.main;
    }

    void Start()
    {
        // Setup zoom slider
        if (zoomSlider != null)
        {
            zoomSlider.minValue = minZoom;
            zoomSlider.maxValue = maxZoom;
            zoomSlider.value = cam.orthographicSize;
            zoomSlider.onValueChanged.AddListener(SetZoom);
        }

        if (ground != null)
            CalculateBounds();
    }

    void Update()
    {
        HandlePinchZoom();
        HandleClick();
    }

    void LateUpdate()
    {
        ClampPosition();
    }

    // --- Dragging ---
    public void OnBeginDrag(PointerEventData eventData)
    {
        dragging = true;
        lastPos = eventData.position;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!dragging) return;

        Vector2 delta = eventData.position - lastPos;
        Vector3 move = new Vector3(-delta.x, 0, -delta.y) * dragSpeed * Time.deltaTime;
        cam.transform.Translate(move, Space.World);
        lastPos = eventData.position;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        dragging = false;
    }

    // --- Zoom ---
    public void SetZoom(float value)
    {
        cam.orthographicSize = Mathf.Clamp(value, minZoom, maxZoom);
    }

    void HandlePinchZoom()
    {
        if (Input.touchCount == 2)
        {
            Touch touch0 = Input.GetTouch(0);
            Touch touch1 = Input.GetTouch(1);

            Vector2 touch0Prev = touch0.position - touch0.deltaPosition;
            Vector2 touch1Prev = touch1.position - touch1.deltaPosition;

            float prevMagnitude = (touch0Prev - touch1Prev).magnitude;
            float currentMagnitude = (touch0.position - touch1.position).magnitude;
            float difference = currentMagnitude - prevMagnitude;

            float newZoom = cam.orthographicSize - difference * pinchZoomSpeed;
            cam.orthographicSize = Mathf.Clamp(newZoom, minZoom, maxZoom);

            if (zoomSlider != null)
                zoomSlider.value = cam.orthographicSize;
        }
    }

    // --- Click detection ---
    private void HandleClick()
    {
        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
            return; // don't click through UI

        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = cam.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, clickableLayer))
            {
                Debug.Log($"[CameraUIController] Raycast hit: {hit.collider.gameObject.name}");

                SelectableItem item = hit.collider.GetComponent<SelectableItem>();
                if (item != null)
                {
                    Debug.Log("[CameraUIController] Found SelectableItem, forwarding click.");
                    item.OnClicked();
                }
                else
                {
                    Debug.LogWarning("[CameraUIController] No SelectableItem on hit object.");
                }
            }
        }

    }


    // --- Bounds ---
    void ClampPosition()
    {
        if (ground == null) return;

        Vector3 pos = cam.transform.position;
        pos.x = Mathf.Clamp(pos.x, minBounds.x, maxBounds.x);
        pos.z = Mathf.Clamp(pos.z, minBounds.y, maxBounds.y);
        cam.transform.position = pos;
    }

    void CalculateBounds()
    {
        Renderer rend = ground.GetComponent<Renderer>();
        if (rend != null)
        {
            Bounds b = rend.bounds;
            minBounds = new Vector2(b.min.x, b.min.z);
            maxBounds = new Vector2(b.max.x, b.max.z);
            return;
        }

        Terrain terrain = ground.GetComponent<Terrain>();
        if (terrain != null)
        {
            Vector3 size = terrain.terrainData.size;
            Vector3 pos = terrain.GetPosition();
            minBounds = new Vector2(pos.x, pos.z);
            maxBounds = new Vector2(pos.x + size.x, pos.z + size.z);
        }
    }
}
