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
    public LayerMask clickableLayer;

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
        if (InputManager.Instance.IsModelDragging) return;
        if (InputManager.Instance.IsPointerOverUI()) return;

        dragging = true;
        InputManager.Instance.SetCameraDragging(true);
        lastPos = eventData.position;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!dragging || InputManager.Instance.IsModelDragging) return;

        Vector2 delta = eventData.position - lastPos;
        Vector3 move = new Vector3(-delta.x, 0, -delta.y) * dragSpeed * Time.deltaTime;
        cam.transform.Translate(move, Space.World);
        lastPos = eventData.position;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        dragging = false;
        InputManager.Instance.SetCameraDragging(false);
    }

    // --- Zoom ---
    public void SetZoom(float value)
    {
        cam.orthographicSize = Mathf.Clamp(value, minZoom, maxZoom);
    }

    private void HandlePinchZoom()
    {
        if (Input.touchCount != 2) return;

        Touch t0 = Input.GetTouch(0);
        Touch t1 = Input.GetTouch(1);

        Vector2 prev0 = t0.position - t0.deltaPosition;
        Vector2 prev1 = t1.position - t1.deltaPosition;

        float prevMag = (prev0 - prev1).magnitude;
        float currMag = (t0.position - t1.position).magnitude;

        float diff = currMag - prevMag;
        float newZoom = cam.orthographicSize - diff * pinchZoomSpeed;

        cam.orthographicSize = Mathf.Clamp(newZoom, minZoom, maxZoom);

        if (zoomSlider != null)
            zoomSlider.value = cam.orthographicSize;
    }

    // --- World Click Detection ---
    private void HandleClick()
    {
        if (Input.GetMouseButtonDown(0))
        {
            if (InputManager.Instance.AnyActive) return;
            if (InputManager.Instance.IsPointerOverUI()) return;

            Ray ray = cam.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, clickableLayer))
            {
                var handler = hit.collider.GetComponentInParent<ItemClickHandler>();
                if (handler != null)
                {
                    handler.SendMessage("OnMouseDown", SendMessageOptions.DontRequireReceiver);
                    return;
                }

                var selectable = hit.collider.GetComponentInParent<SelectableItemController>();
                if (selectable != null)
                {
                    selectable.Select();

                    var ui = FindObjectOfType<SelectableControllerUI>();
                    if (ui != null)
                        ui.Show(selectable);
                }
            }
        }
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
        }
    }
}
