using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// ItemDragger with grid snapping: integrates with GridManager to snap items to grid cells.
/// Handles overlap tinting, rollback on invalid placement, and marks grid occupancy.
/// </summary>
[RequireComponent(typeof(Collider))]
[RequireComponent(typeof(Rigidbody))]
public class ItemDragger : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 10f;
    public LayerMask groundLayer;
    public bool enableGridSnapping = true;
    public GridManager gridManager;

    [Header("Overlap Visual")]
    public Color overlapColor = new Color(1f, 0.2f, 0.2f, 1f);
    [Tooltip("If true, lerp current color toward overlapColor (keeps texture visible). If false, set color directly.")]
    public bool lerpTint = true;
    [Range(0f, 1f)] public float tintLerpAmount = 0.75f;

    [Header("Rollback Settings")]
    public bool useSmoothReturn = true;
    [Tooltip("Duration in seconds for the smooth return to last valid position")]
    public float smoothReturnDuration = 0.18f;

    private Camera mainCamera;
    private bool isDragging = false;
    private bool isMoveMode = false;
    private Vector3 dragOffset;
    private float dragHeight;

    // grid position cache
    private Vector2Int lastGridCoord;
    private bool hasGridPosition = false;

    // render/material tint management
    private List<Renderer> segmentRenderers = new List<Renderer>();
    private List<Material[]> originalMaterials = new List<Material[]>();
    private bool isTintApplied = false;
    private HashSet<GameObject> overlappingRoadRoots = new HashSet<GameObject>();

    // UI / raycast helpers
    private GraphicRaycaster uiRaycaster;
    private EventSystem eventSystem;
    private PointerEventData pointerData;
    private List<RaycastResult> raycastResults = new List<RaycastResult>();

    // rollback cache
    private Vector3 lastValidPosition;
    private Quaternion lastValidRotation;
    private bool hasValidPlacement = false;

    public System.Action OnDragEnd;

    // ---------------------------------------------------------------------
    void Awake()
    {
        mainCamera = Camera.main;
        eventSystem = EventSystem.current;

        var canvas = FindObjectOfType<Canvas>();
        if (canvas != null) uiRaycaster = canvas.GetComponent<GraphicRaycaster>();

        // safe defaults
        var col = GetComponent<Collider>();
        if (col != null) col.isTrigger = true;

        var rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = true;
            rb.useGravity = false;
        }

        // auto find grid if not assigned
        if (gridManager == null)
            gridManager = FindObjectOfType<GridManager>();
    }

    void Start()
    {
        RefreshRenderersAndCacheMaterials();
    }

    void Update()
    {
        if (!isMoveMode) return;

        if (!isTintApplied && Time.frameCount % 20 == 0)
            RefreshRenderersAndCacheMaterials();

        HandleDraggingInput();

        // track valid placement when not overlapping
        if (isDragging && overlappingRoadRoots.Count == 0)
        {
            lastValidPosition = transform.position;
            lastValidRotation = transform.rotation;
            hasValidPlacement = true;
        }
    }

    // ---------------------------------------------------------------------
    private void HandleDraggingInput()
    {
        // Begin drag
        if (Input.GetMouseButtonDown(0))
        {
            if (IsPointerOverUI()) return;

            if (Physics.Raycast(mainCamera.ScreenPointToRay(Input.mousePosition), out RaycastHit hit, 1000f, groundLayer))
            {
                dragOffset = transform.position - hit.point;
                dragHeight = transform.position.y - hit.point.y;
            }
        }

        // Drag movement
        if (Input.GetMouseButton(0))
        {
            if (IsPointerOverUI()) return;

            if (!isDragging)
            {
                isDragging = true;
                InputBlocker.IsModelDragging = true;

                // release any previously occupied grid if we start moving
                if (hasGridPosition && gridManager != null)
                    gridManager.SetTileOccupied(lastGridCoord, false);
            }

            if (Physics.Raycast(mainCamera.ScreenPointToRay(Input.mousePosition), out RaycastHit hit, 1000f, groundLayer))
            {
                Vector3 target = hit.point + dragOffset;
                target.y = hit.point.y + dragHeight;

                // Apply grid snapping if available
                if (enableGridSnapping && gridManager != null)
                    target = gridManager.GetNearestGridPosition(target);

                transform.position = Vector3.Lerp(transform.position, target, Time.deltaTime * moveSpeed);
            }
        }

        // Release
        if (Input.GetMouseButtonUp(0))
        {
            if (!isDragging) return;

            isDragging = false;
            InputBlocker.IsModelDragging = false;

            Vector2Int snappedCoord = Vector2Int.zero;

            // handle grid occupancy
            if (enableGridSnapping && gridManager != null)
            {
                snappedCoord = gridManager.GetGridCoordinate(transform.position);
                bool occupied = gridManager.IsTileOccupied(snappedCoord);

                if (occupied)
                {
                    // rollback to last valid
                    if (hasValidPlacement)
                    {
                        if (useSmoothReturn)
                            StartCoroutine(SmoothReturnToValidPosition());
                        else
                            transform.position = lastValidPosition;

                        Debug.Log($"[ItemDragger] Grid tile occupied, reverting placement.");
                    }
                }
                else
                {
                    gridManager.SetTileOccupied(snappedCoord, true);
                    lastGridCoord = snappedCoord;
                    hasGridPosition = true;
                    Debug.Log($"[ItemDragger] Occupied tile set at {snappedCoord}");
                }
            }

            // restore visuals
            overlappingRoadRoots.Clear();
            if (isTintApplied) RestoreOriginalMaterials();
            isTintApplied = false;

            OnDragEnd?.Invoke();
        }
    }

    // ---------------------------------------------------------------------
    private IEnumerator SmoothReturnToValidPosition()
    {
        Vector3 startPos = transform.position;
        Quaternion startRot = transform.rotation;
        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / Mathf.Max(0.0001f, smoothReturnDuration);
            transform.position = Vector3.Lerp(startPos, lastValidPosition, t);
            transform.rotation = Quaternion.Slerp(startRot, lastValidRotation, t);
            yield return null;
        }
    }

    private bool IsPointerOverUI()
    {
        if (eventSystem == null)
            return EventSystem.current != null && EventSystem.current.IsPointerOverGameObject();

        if (uiRaycaster == null)
        {
            var canvas = FindObjectOfType<Canvas>();
            if (canvas != null) uiRaycaster = canvas.GetComponent<GraphicRaycaster>();
        }

        if (uiRaycaster == null)
            return EventSystem.current != null && EventSystem.current.IsPointerOverGameObject();

        pointerData = new PointerEventData(eventSystem) { position = Input.mousePosition };
        raycastResults.Clear();
        uiRaycaster.Raycast(pointerData, raycastResults);
        return raycastResults.Count > 0;
    }

    // ---------------------------------------------------------------------
    private void RefreshRenderersAndCacheMaterials()
    {
        Renderer[] found = GetComponentsInChildren<Renderer>(true);
        segmentRenderers.Clear();
        originalMaterials.Clear();

        foreach (var r in found)
        {
            if (r == null || r.gameObject == gameObject) continue;
            segmentRenderers.Add(r);

            r.materials = r.materials;
            Material[] mats = r.materials;
            Material[] copies = new Material[mats.Length];
            for (int i = 0; i < mats.Length; i++)
                copies[i] = mats[i] != null ? new Material(mats[i]) : null;

            originalMaterials.Add(copies);
        }
    }

    // ---------------------------------------------------------------------
    void OnTriggerEnter(Collider other)
    {
        if (!isDragging || other == null) return;
        if (other.gameObject.layer != LayerMask.NameToLayer("Road")) return;
        GameObject otherRoot = other.transform.root != null ? other.transform.root.gameObject : other.gameObject;
        overlappingRoadRoots.Add(otherRoot);
        ApplyTintToSegments();
    }

    void OnTriggerExit(Collider other)
    {
        if (!isDragging || other == null) return;
        if (other.gameObject.layer != LayerMask.NameToLayer("Road")) return;

        GameObject otherRoot = other.transform.root != null ? other.transform.root.gameObject : other.gameObject;
        overlappingRoadRoots.Remove(otherRoot);

        if (overlappingRoadRoots.Count == 0)
            RestoreOriginalMaterials();
    }

    private void ApplyTintToSegments()
    {
        foreach (var r in segmentRenderers)
        {
            if (r == null) continue;
            foreach (var mat in r.materials)
            {
                if (mat == null) continue;
                if (mat.HasProperty("_BaseColor"))
                    mat.SetColor("_BaseColor", Color.Lerp(mat.GetColor("_BaseColor"), overlapColor, tintLerpAmount));
                else if (mat.HasProperty("_Color"))
                    mat.SetColor("_Color", Color.Lerp(mat.GetColor("_Color"), overlapColor, tintLerpAmount));
            }
        }
        isTintApplied = true;
    }

    private void RestoreOriginalMaterials()
    {
        for (int i = 0; i < segmentRenderers.Count; i++)
        {
            if (segmentRenderers[i] != null && i < originalMaterials.Count)
                segmentRenderers[i].materials = originalMaterials[i];
        }
        isTintApplied = false;
    }

    // ---------------------------------------------------------------------
    public void EnableDraggingExternally(bool enable) => EnableDragging(enable);
    public void EnableDragging(bool enable)
    {
        isMoveMode = enable;
        if (!enable)
        {
            isDragging = false;
            InputBlocker.IsModelDragging = false;
            overlappingRoadRoots.Clear();
            if (isTintApplied) RestoreOriginalMaterials();
            isTintApplied = false;
        }
        Debug.Log($"[ItemDragger] MoveMode={(enable ? "ON" : "OFF")} for {name}");
    }
}
