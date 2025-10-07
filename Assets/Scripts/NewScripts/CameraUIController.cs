using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections.Generic;

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
    public LayerMask clickableLayer; // Assign your interactable layer

    [Header("UI Blocking Settings")]
    public GraphicRaycaster uiRaycaster;
    public EventSystem eventSystem;
    public List<RectTransform> uiClickThrough = new List<RectTransform>();

    private bool dragging = false;
    private Vector2 lastPos;
    private Vector2 minBounds;
    private Vector2 maxBounds;

    private PointerEventData pointerData;
    private List<RaycastResult> raycastResults = new List<RaycastResult>();

    void Awake()
    {
        if (cam == null) cam = Camera.main;
    }

    void Start()
    {
        // Zoom slider setup
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

    // --- Camera Dragging ---
    public void OnBeginDrag(PointerEventData eventData)
    {
        if (InputBlocker.IsModelDragging)
        {
            Debug.Log("[CameraUIController] Skipping camera drag — model is being dragged.");
            return;
        }

        if (IsPointerOverBlockingUI())
        {
            Debug.Log("[CameraUIController] Drag blocked by UI.");
            return;
        }

        dragging = true;
        InputBlocker.IsCameraDragging = true;
        lastPos = eventData.position;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!dragging || InputBlocker.IsModelDragging)
            return;

        Vector2 delta = eventData.position - lastPos;
        Vector3 move = new Vector3(-delta.x, 0, -delta.y) * dragSpeed * Time.deltaTime;
        cam.transform.Translate(move, Space.World);
        lastPos = eventData.position;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        dragging = false;
        InputBlocker.IsCameraDragging = false;
    }

    // --- Zoom ---
    public void SetZoom(float value)
    {
        cam.orthographicSize = Mathf.Clamp(value, minZoom, maxZoom);
    }

    private void HandlePinchZoom()
    {
        if (Input.touchCount == 2)
        {
            Touch touch0 = Input.GetTouch(0);
            Touch touch1 = Input.GetTouch(1);

            Vector2 prevPos0 = touch0.position - touch0.deltaPosition;
            Vector2 prevPos1 = touch1.position - touch1.deltaPosition;

            float prevMag = (prevPos0 - prevPos1).magnitude;
            float currentMag = (touch0.position - touch1.position).magnitude;
            float diff = currentMag - prevMag;

            float newZoom = cam.orthographicSize - diff * pinchZoomSpeed;
            cam.orthographicSize = Mathf.Clamp(newZoom, minZoom, maxZoom);

            if (zoomSlider != null)
                zoomSlider.value = cam.orthographicSize;
        }
    }

    // --- World Click Detection ---
    private void HandleClick()
    {
        if (Input.GetMouseButtonDown(0))
        {
            if (InputBlocker.IsModelDragging)
                return;

            if (IsPointerOverBlockingUI())
            {
                Debug.Log("[CameraUIController] Click blocked by UI.");
                return;
            }

            Ray ray = cam.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, clickableLayer))
            {
                Debug.Log($"[CameraUIController] Raycast hit: {hit.collider.gameObject.name}");
            }
        }
    }

    // --- UI Blocking Logic ---
    private bool IsPointerOverBlockingUI()
    {
        if (eventSystem == null || uiRaycaster == null)
            return false;

        pointerData = new PointerEventData(eventSystem)
        {
            position = Input.mousePosition
        };

        raycastResults.Clear();
        uiRaycaster.Raycast(pointerData, raycastResults);

        foreach (var result in raycastResults)
        {
            RectTransform rt = result.gameObject.GetComponent<RectTransform>();
            if (rt != null)
            {
                // ✅ Allow camera drag panel and its children to be click-through
                if (IsInClickThroughList(rt))
                    continue;

                // ✅ Allow special marker flag (if you're using CameraDragPanel helper)
                if (CameraDragPanel.IsPointerOverCameraPanel)
                    continue;

                return true; // otherwise it's blocking
            }
        }

        return false;
    }

    // helper
    private bool IsInClickThroughList(RectTransform target)
    {
        foreach (var allowed in uiClickThrough)
        {
            if (target == allowed)
                return true;

            if (target.IsChildOf(allowed))
                return true;
        }
        return false;
    }


    // --- Camera Bounds ---
    private void ClampPosition()
    {
        if (ground == null) return;

        Vector3 pos = cam.transform.position;
        pos.x = Mathf.Clamp(pos.x, minBounds.x, maxBounds.x);
        pos.z = Mathf.Clamp(pos.z, minBounds.y, maxBounds.y);
        cam.transform.position = pos;
    }

    private void CalculateBounds()
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
