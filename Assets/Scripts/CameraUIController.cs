using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CameraUIController : MonoBehaviour, IDragHandler, IBeginDragHandler, IEndDragHandler
{
    [Header("References")]
    public Camera cam;              // assign Main Camera
    public RectTransform dragPanel; // assign panel for drag
    public Slider zoomSlider;       // assign zoom slider (optional)
    public Transform ground;        // assign Ground object (plane/terrain)

    [Header("Settings")]
    public float dragSpeed = 0.5f;
    public float minZoom = 30f;
    public float maxZoom = 100f;

    private bool dragging = false;
    private Vector2 lastPos;

    private Vector2 minBounds;
    private Vector2 maxBounds;

    void Start()
    {
        if (cam == null) cam = Camera.main;

        // Setup slider
        if (zoomSlider != null)
        {
            zoomSlider.minValue = minZoom;
            zoomSlider.maxValue = maxZoom;
            zoomSlider.value = cam.orthographicSize;
            zoomSlider.onValueChanged.AddListener(SetZoom);
        }

        // Calculate bounds based on ground
        if (ground != null)
            CalculateBounds();
    }

    void LateUpdate()
    {
        ClampPosition();
    }

    // --- Panel drag begin ---
    public void OnBeginDrag(PointerEventData eventData)
    {
        dragging = true;
        lastPos = eventData.position;
    }

    // --- Panel dragging ---
    public void OnDrag(PointerEventData eventData)
    {
        if (!dragging) return;

        Vector2 delta = eventData.position - lastPos;
        Vector3 move = new Vector3(-delta.x, 0, -delta.y) * dragSpeed * Time.deltaTime;
        cam.transform.Translate(move, Space.World);
        lastPos = eventData.position;
    }

    // --- Panel drag end ---
    public void OnEndDrag(PointerEventData eventData)
    {
        dragging = false;
    }

    // --- Zoom from slider ---
    public void SetZoom(float value)
    {
        cam.orthographicSize = Mathf.Clamp(value, minZoom, maxZoom);
    }

    // --- Clamp position inside ground ---
    void ClampPosition()
    {
        if (ground == null) return;

        Vector3 pos = cam.transform.position;
        pos.x = Mathf.Clamp(pos.x, minBounds.x, maxBounds.x);
        pos.z = Mathf.Clamp(pos.z, minBounds.y, maxBounds.y);
        cam.transform.position = pos;
    }

    // --- Calculate ground size from renderer/terrain ---
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
