using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// ItemDragger: draggable root controller that tints child segment materials red while overlapping other "Road" objects,
/// restores the original materials when there are no overlaps anymore, and rolls back to last valid placement if released while overlapping.
/// Robust to runtime-instantiated/destroyed segments (safe null checks + refresh).
/// </summary>
[RequireComponent(typeof(Collider))]
[RequireComponent(typeof(Rigidbody))]
public class ItemDragger : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 10f;
    public LayerMask groundLayer;

    [Header("Overlap Visual")]
    public Color overlapColor = new Color(1f, 0.2f, 0.2f, 1f);
    [Tooltip("If true, lerp current color toward overlapColor (keeps texture visible). If false, set color directly.")]
    public bool lerpTint = true;
    [Range(0f, 1f)] public float tintLerpAmount = 0.75f;

    [Header("Rollback Settings")]
    public bool useSmoothReturn = true;
    [Tooltip("Duration in seconds for the smooth return to last valid position")]
    public float smoothReturnDuration = 0.18f;

    // runtime
    private Camera mainCamera;
    private bool isDragging = false;
    private bool isMoveMode = false;
    private Vector3 dragOffset;
    private float dragHeight;

    // dynamic renderer/material storage (child segments)
    private List<Renderer> segmentRenderers = new List<Renderer>();
    private List<Material[]> originalMaterials = new List<Material[]>(); // snapshots to restore
    private bool isTintApplied = false;

    // overlapping tracking
    private HashSet<GameObject> overlappingRoadRoots = new HashSet<GameObject>();

    // UI / raycast helpers
    private GraphicRaycaster uiRaycaster;
    private EventSystem eventSystem;
    private PointerEventData pointerData;
    private List<RaycastResult> raycastResults = new List<RaycastResult>();

    // last valid placement (non-overlapping) for rollback
    private Vector3 lastValidPosition;
    private Quaternion lastValidRotation;
    private bool hasValidPlacement = false;

    // public hook
    public System.Action OnDragEnd;

    // ---------------------------------------------------------------------
    void Awake()
    {
        mainCamera = Camera.main;
        eventSystem = EventSystem.current;

        var canvas = FindObjectOfType<Canvas>();
        if (canvas != null) uiRaycaster = canvas.GetComponent<GraphicRaycaster>();

        // Ensure collider is trigger and rigidbody is kinematic (safe defaults)
        var col = GetComponent<Collider>();
        if (col != null) col.isTrigger = true;

        var rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = true;
            rb.useGravity = false;
        }
    }

    void Start()
    {
        RefreshRenderersAndCacheMaterials();
    }

    void Update()
    {
        if (!isMoveMode) return;

        // refresh occasionally to catch runtime-instantiated segments (do not overwrite originals while tinted)
        if (!isTintApplied && Time.frameCount % 20 == 0)
            RefreshRenderersAndCacheMaterials();

        HandleDraggingInput();

        // while dragging, if not overlapping, record last valid transform
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
        // Begin drag (pointer down)
        if (Input.GetMouseButtonDown(0))
        {
            if (IsPointerOverUI()) return;

            if (Physics.Raycast(mainCamera.ScreenPointToRay(Input.mousePosition), out RaycastHit hit, 1000f, groundLayer))
            {
                dragOffset = transform.position - hit.point;
                dragHeight = transform.position.y - hit.point.y;
            }
        }

        // While dragging (pointer held)
        if (Input.GetMouseButton(0))
        {
            if (IsPointerOverUI()) return;

            if (!isDragging)
            {
                isDragging = true;
                InputBlocker.IsModelDragging = true;
            }

            if (Physics.Raycast(mainCamera.ScreenPointToRay(Input.mousePosition), out RaycastHit hit, 1000f, groundLayer))
            {
                Vector3 target = hit.point + dragOffset;
                target.y = hit.point.y + dragHeight;
                transform.position = Vector3.Lerp(transform.position, target, Time.deltaTime * moveSpeed);
            }
        }

        // End drag
        if (Input.GetMouseButtonUp(0))
        {
            if (!isDragging) return;

            isDragging = false;
            InputBlocker.IsModelDragging = false;

            // If released while overlapping and we have a saved valid placement, rollback
            if (overlappingRoadRoots.Count > 0)
            {
                if (hasValidPlacement)
                {
                    Debug.Log($"[ItemDragger] Released while overlapping — reverting to last valid placement for '{name}'");

                    if (useSmoothReturn)
                        StartCoroutine(SmoothReturnToValidPosition());
                    else
                    {
                        transform.position = lastValidPosition;
                        transform.rotation = lastValidRotation;
                    }
                }
                else
                {
                    Debug.Log($"[ItemDragger] Released while overlapping but no valid placement saved for '{name}'");
                }
            }

            // clear overlaps and restore visuals
            overlappingRoadRoots.Clear();
            if (isTintApplied)
                RestoreOriginalMaterials();
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

        transform.position = lastValidPosition;
        transform.rotation = lastValidRotation;
    }

    // ---------------------------------------------------------------------
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
    /// <summary>Finds child renderers and creates a snapshot of their current materials for restoration later.</summary>
    private void RefreshRenderersAndCacheMaterials()
    {
        Renderer[] found = GetComponentsInChildren<Renderer>(true);

        segmentRenderers.Clear();
        originalMaterials.Clear();

        foreach (var r in found)
        {
            if (r == null) continue;
            if (r.gameObject == this.gameObject) continue; // skip any renderer on root itself
            segmentRenderers.Add(r);

            // ensure unique material instances
            try
            {
                r.materials = r.materials;
            }
            catch { /* renderer might be destroyed mid-call; ignore */ }

            // snapshot of current materials (create new material instances to keep texture/settings safe)
            Material[] mats = r.materials;
            Material[] copies = new Material[mats.Length];
            for (int i = 0; i < mats.Length; i++)
            {
                copies[i] = mats[i] != null ? new Material(mats[i]) : null;
            }
            originalMaterials.Add(copies);
        }

        Debug.Log($"[ItemDragger] Refreshed renderers: found {segmentRenderers.Count} child renderers under '{name}'");
    }

    // ---------------------------------------------------------------------
    void OnTriggerEnter(Collider other)
    {
        if (!isDragging) return;
        if (other == null) return;
        if (other.gameObject == this.gameObject) return;
        if (other.gameObject.layer != LayerMask.NameToLayer("Road")) return;

        GameObject otherRoot = other.transform.root != null ? other.transform.root.gameObject : other.gameObject;
        overlappingRoadsAddAndApply(otherRoot);
    }

    void OnTriggerExit(Collider other)
    {
        if (!isDragging) return;
        if (other == null) return;
        if (other.gameObject == this.gameObject) return;
        if (other.gameObject.layer != LayerMask.NameToLayer("Road")) return;

        GameObject otherRoot = other.transform.root != null ? other.transform.root.gameObject : other.gameObject;
        overlappingRoadsRemoveAndMaybeRestore(otherRoot);
    }

    // ---------------------------------------------------------------------
    private void overlappingRoadsAddAndApply(GameObject otherRoot)
    {
        if (otherRoot == null) return;

        overlappingRoadRoots.Add(otherRoot);
        Debug.Log($"[ItemDragger] COLLISION START: '{name}' <-> '{otherRoot.name}'  (total overlaps = {overlappingRoadRoots.Count})");

        // if we haven't applied tint yet, refresh renderers and snapshot originals first
        if (!isTintApplied)
        {
            RefreshRenderersAndCacheMaterials();
            StoreOriginalMaterialsSnapshot();
        }

        ApplyTintToSegments();
    }

    private void overlappingRoadsRemoveAndMaybeRestore(GameObject otherRoot)
    {
        if (otherRoot == null) return;

        overlappingRoadRoots.Remove(otherRoot);
        Debug.Log($"[ItemDragger] COLLISION END: '{name}' <-> '{otherRoot.name}'  (remaining overlaps = {overlappingRoadRoots.Count})");

        if (overlappingRoadRoots.Count == 0)
        {
            // no more overlaps => restore originals
            RestoreOriginalMaterials();
            isTintApplied = false;
        }
    }

    // ---------------------------------------------------------------------
    private void StoreOriginalMaterialsSnapshot()
    {
        // Ensure renderers fresh
        if (segmentRenderers.Count == 0)
        {
            originalMaterials.Clear();
            return;
        }

        originalMaterials.Clear();
        foreach (var r in segmentRenderers)
        {
            if (r == null)
            {
                originalMaterials.Add(new Material[0]);
                continue;
            }

            Material[] mats = r.materials;
            Material[] copies = new Material[mats.Length];
            for (int i = 0; i < mats.Length; i++)
            {
                copies[i] = mats[i] != null ? new Material(mats[i]) : null;
            }
            originalMaterials.Add(copies);
        }

        Debug.Log($"[ItemDragger] Stored snapshot of original materials ({originalMaterials.Count} renderers).");
    }

    // ---------------------------------------------------------------------
    private void ApplyTintToSegments()
    {
        if (segmentRenderers.Count == 0)
        {
            Debug.LogWarning($"[ItemDragger] No child renderers found to tint on '{name}'");
            return;
        }

        Debug.Log($"[ItemDragger] Applying tint to {segmentRenderers.Count} renderers on '{name}'");

        for (int rIndex = 0; rIndex < segmentRenderers.Count; rIndex++)
        {
            Renderer r = segmentRenderers[rIndex];
            if (r == null) continue;

            Material[] mats = r.materials;
            for (int m = 0; m < mats.Length; m++)
            {
                Material mat = mats[m];
                if (mat == null) continue;

                if (mat.HasProperty("_BaseColor"))
                {
                    Color current = mat.GetColor("_BaseColor");
                    Color target = lerpTint ? Color.Lerp(current, overlapColor, tintLerpAmount) : overlapColor;
                    mat.SetColor("_BaseColor", target);
                }
                else if (mat.HasProperty("_Color"))
                {
                    Color current = mat.GetColor("_Color");
                    Color target = lerpTint ? Color.Lerp(current, overlapColor, tintLerpAmount) : overlapColor;
                    mat.SetColor("_Color", target);
                }
                else
                {
                    // If shader doesn't expose common color props, skip — uncommon for Standard/URP.
                }
            }
        }

        isTintApplied = true;
    }

    // ---------------------------------------------------------------------
    private void RestoreOriginalMaterials()
    {
        if (segmentRenderers.Count == 0 || originalMaterials.Count == 0)
        {
            return;
        }

        Debug.Log($"[ItemDragger] Restoring original materials for {segmentRenderers.Count} renderers on '{name}'");

        int count = Mathf.Min(segmentRenderers.Count, originalMaterials.Count);
        for (int i = 0; i < count; i++)
        {
            Renderer r = segmentRenderers[i];
            Material[] originals = originalMaterials[i];

            if (r == null || originals == null) continue;

            try
            {
                Material[] current = r.materials;

                if (originals.Length == current.Length)
                {
                    // direct assignment (restores textures + all properties)
                    r.materials = originals;
                }
                else
                {
                    // fallback: restore color properties where possible
                    int loop = Mathf.Min(current.Length, originals.Length);
                    for (int m = 0; m < loop; m++)
                    {
                        if (originals[m] == null) continue;
                        Material matInstance = current[m];
                        if (matInstance == null) continue;

                        if (matInstance.HasProperty("_BaseColor") && originals[m].HasProperty("_BaseColor"))
                        {
                            matInstance.SetColor("_BaseColor", originals[m].GetColor("_BaseColor"));
                        }
                        else if (matInstance.HasProperty("_Color") && originals[m].HasProperty("_Color"))
                        {
                            matInstance.SetColor("_Color", originals[m].GetColor("_Color"));
                        }
                    }
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogWarning($"[ItemDragger] Exception while restoring materials for renderer '{r?.name}': {ex.Message}");
            }
        }

        // free snapshot
        originalMaterials.Clear();
        isTintApplied = false;
    }

    // ---------------------------------------------------------------------
    // Compatibility helper
    public void EnableDraggingExternally(bool enable) => EnableDragging(enable);

    public void EnableDragging(bool enable)
    {
        isMoveMode = enable;

        if (!enable)
        {
            // stop drag and restore if necessary
            isDragging = false;
            InputBlocker.IsModelDragging = false;
            overlappingRoadRoots.Clear();
            if (isTintApplied) RestoreOriginalMaterials();
            isTintApplied = false;
        }

        Debug.Log($"[ItemDragger] MoveMode={(enable ? "ON" : "OFF")} for {name}");
    }
}
