using UnityEngine;
using System.Collections.Generic;

public class PlacementManager : MonoBehaviour
{
    public static PlacementManager Instance;

    // runtime state
    private GameObject previewObject;
    private RoadPiece currentPiece;
    private Camera mainCam;
    public LayerMask groundMask;
    public LayerMask roadMask; // set in inspector to the "Road" layer mask

    private bool isPlacing = false;

    [Header("Snapping Settings")]
    public float snapRange = 1.5f;

    [Header("Preview visuals (editable in Inspector)")]
    public Color validColor = new Color(0.2f, 1f, 0.2f); // green
    public Color invalidColor = new Color(1f, 0.2f, 0.2f); // red
    [Range(0f, 1f)] public float previewAlpha = 0.6f;

    [Header("Overlap tuning")]
    [Range(0.4f, 1f)] public float overlapShrink = 0.85f; // shrink overlap box to avoid edge-touch false positives

    // list of placed roads
    private List<RoadPiece> placedRoads = new List<RoadPiece>();

    // material management
    private Dictionary<Renderer, Material[]> originalSharedMaterials = new Dictionary<Renderer, Material[]>();
    private Dictionary<Renderer, Material[]> previewInstanceMaterials = new Dictionary<Renderer, Material[]>();

    void Awake() => Instance = this;
    void Start() => mainCam = Camera.main;

    void Update()
    {
        if (!isPlacing || previewObject == null) return;

#if UNITY_EDITOR || UNITY_STANDALONE
        HandleMouse();
#else
        HandleTouch();
#endif

        // rotate preview on desktop
        if (Input.GetKeyDown(KeyCode.Q)) previewObject.transform.Rotate(Vector3.up, -currentPiece.rotationStep);
        if (Input.GetKeyDown(KeyCode.E)) previewObject.transform.Rotate(Vector3.up, currentPiece.rotationStep);

        // preview color: red if overlapping, green if free
        bool overlapping = IsOverlapping(previewObject);
        UpdatePreviewMaterialColor(overlapping ? invalidColor : validColor, previewAlpha);
    }

    // ---- Public API ----
    private bool firstFrameAfterSpawn = false;

    public void StartPlacingRoad(GameObject prefab)
    {
        if (previewObject != null) DestroyPreviewImmediate();

        // spawn preview
        previewObject = Instantiate(prefab);
        currentPiece = previewObject.GetComponent<RoadPiece>();

        // set preview collider trigger
        var col = previewObject.GetComponentInChildren<Collider>();
        if (col != null) col.isTrigger = true;

        // collect preview materials
        CollectOriginalAndCreatePreviewMaterials(previewObject, validColor, previewAlpha);

        // place at mouse/touch right away
        Ray ray = mainCam.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit, 100f, groundMask))
            previewObject.transform.position = SnapToGrid(hit.point);
        else
            previewObject.transform.position = Vector3.zero; // fallback

        isPlacing = true;
        firstFrameAfterSpawn = true; // prevents instant auto-snap
    }

    public void FinishPlacingRoad()
    {
        if (!isPlacing || previewObject == null) return;

        // strict overlap check
        if (IsOverlapping(previewObject))
        {
            Debug.Log("Placement canceled: overlapping another road.");
            DestroyPreviewImmediate();
            return;
        }

        // make collider solid
        var col = previewObject.GetComponentInChildren<Collider>();
        if (col != null) col.isTrigger = false;

        // restore original materials
        RestoreOriginalMaterials(previewObject);

        // mark snap points as occupied
        foreach (var sp in currentPiece.snapPoints)
        {
            if (sp != null && sp.connectedTo != null)
            {
                sp.Occupy();
                sp.connectedTo.Occupy();
            }
        }

        // register road
        placedRoads.Add(currentPiece);

        // reset state
        previewObject = null;
        currentPiece = null;
        isPlacing = false;
        originalSharedMaterials.Clear();
        previewInstanceMaterials.Clear();
    }

    // ---- Input handlers ----
    void HandleMouse()
    {
        Ray ray = mainCam.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit, 100f, groundMask))
            TrySnapOrGrid(hit.point);

        if (Input.GetMouseButtonUp(0))
            FinishPlacingRoad();
    }

    void HandleTouch()
    {
        if (Input.touchCount == 0) return;
        Touch t = Input.GetTouch(0);

        Ray ray = mainCam.ScreenPointToRay(t.position);
        if (Physics.Raycast(ray, out RaycastHit hit, 100f, groundMask))
            TrySnapOrGrid(hit.point);

        if (t.phase == TouchPhase.Ended)
            FinishPlacingRoad();
    }

    // ---- Snapping / grid logic ----
    void TrySnapOrGrid(Vector3 hitPos)
    {
        if (previewObject == null || currentPiece == null) return;

        Vector3 targetPos = SnapToGrid(hitPos);
        Quaternion targetRot = previewObject.transform.rotation;

        SnapPoint bestTarget = null;
        SnapPoint bestMyPoint = null;
        float bestDist = snapRange;
        // disable snapping on first frame to prevent instant auto-lock
        if (firstFrameAfterSpawn)
        {
            previewObject.transform.position = targetPos;
            return;
        }

        foreach (var road in placedRoads)
        {
            if (road == null) continue;

            foreach (var candidate in road.snapPoints)
            {
                if (candidate == null || candidate.isOccupied) continue;

                foreach (var myPoint in currentPiece.snapPoints)
                {
                    if (myPoint == null) continue;

                    float distBetween = Vector3.Distance(candidate.transform.position, myPoint.transform.position);
                    if (distBetween < bestDist)
                    {
                        float dot = Vector3.Dot(myPoint.transform.forward, candidate.transform.forward);
                        if (dot > 0.9f) continue;

                        bestDist = distBetween;
                        bestTarget = candidate;
                        bestMyPoint = myPoint;
                    }
                }
            }
        }

        // apply soft snap
        if (bestTarget != null && bestMyPoint != null)
        {
            Vector3 delta = bestMyPoint.transform.position - previewObject.transform.position;
            targetPos = bestTarget.transform.position - delta;

            Quaternion rotation = Quaternion.FromToRotation(bestMyPoint.transform.forward, -bestTarget.transform.forward);
            targetRot = rotation * previewObject.transform.rotation;
        }

        previewObject.transform.position = targetPos;
        previewObject.transform.rotation = targetRot;
    }

    Vector3 SnapToGrid(Vector3 pos)
    {
        if (currentPiece == null) return pos;
        float grid = currentPiece.snapSize;
        float x = Mathf.Round(pos.x / grid) * grid;
        float z = Mathf.Round(pos.z / grid) * grid;
        return new Vector3(x, 0f, z);
    }

    // ---- Overlap detection ----
    private bool IsOverlapping(GameObject preview)
    {
        if (preview == null) return false;
        if (placedRoads.Count == 0) return false;

        Collider previewCol = preview.GetComponentInChildren<Collider>();
        if (previewCol == null) return false;

        // physics-based
        Vector3 center = previewCol.bounds.center;
        Vector3 halfExtents = previewCol.bounds.extents * overlapShrink;
        Collider[] hits = Physics.OverlapBox(center, halfExtents, preview.transform.rotation, roadMask);
        foreach (var hit in hits)
        {
            if (hit.transform.IsChildOf(preview.transform)) continue;
            if (hit.GetComponentInParent<RoadPiece>() != null)
                return true;
        }

        // manual bounds check
        Bounds pBounds = previewCol.bounds;
        pBounds.extents *= overlapShrink;
        foreach (var road in placedRoads)
        {
            if (road == null) continue;

            Collider[] roadCols = road.GetComponentsInChildren<Collider>();
            foreach (var rc in roadCols)
            {
                if (rc == null || !rc.enabled) continue;
                if (rc.transform.IsChildOf(preview.transform)) continue;

                if (pBounds.Intersects(rc.bounds))
                    return true;
            }
        }

        return false;
    }

    // ---- Preview materials ----
    private void CollectOriginalAndCreatePreviewMaterials(GameObject preview, Color baseColor, float alpha)
    {
        originalSharedMaterials.Clear();
        previewInstanceMaterials.Clear();

        var renderers = preview.GetComponentsInChildren<Renderer>(true);
        foreach (var r in renderers)
        {
            if (r == null) continue;

            Material[] shared = r.sharedMaterials;
            originalSharedMaterials[r] = shared;

            Material[] previewMats = new Material[shared.Length];
            for (int i = 0; i < shared.Length; i++)
            {
                Material src = shared[i] != null ? shared[i] : new Material(Shader.Find("Standard"));
                Material inst = new Material(src);
                if (inst.HasProperty("_Color"))
                {
                    Color c = baseColor;
                    c.a = alpha;
                    inst.color = c;
                }
                previewMats[i] = inst;
            }

            r.materials = previewMats;
            previewInstanceMaterials[r] = previewMats;
        }
    }

    private void UpdatePreviewMaterialColor(Color baseColor, float alpha)
    {
        foreach (var kv in previewInstanceMaterials)
        {
            var mats = kv.Value;
            if (mats == null) continue;
            for (int i = 0; i < mats.Length; i++)
            {
                Material m = mats[i];
                if (m == null) continue;
                if (m.HasProperty("_Color"))
                {
                    Color c = baseColor;
                    c.a = alpha;
                    m.color = c;
                }
            }
        }
    }

    private void RestoreOriginalMaterials(GameObject preview)
    {
        var renderers = preview.GetComponentsInChildren<Renderer>(true);
        foreach (var r in renderers)
        {
            if (r == null) continue;
            if (originalSharedMaterials.TryGetValue(r, out Material[] orig))
                r.sharedMaterials = orig;
        }

        foreach (var kv in previewInstanceMaterials)
        {
            var mats = kv.Value;
            if (mats == null) continue;
            for (int i = 0; i < mats.Length; i++)
                if (mats[i] != null) Destroy(mats[i]);
        }

        originalSharedMaterials.Clear();
        previewInstanceMaterials.Clear();
    }

    private void DestroyPreviewImmediate()
    {
        foreach (var kv in previewInstanceMaterials)
        {
            var mats = kv.Value;
            if (mats == null) continue;
            for (int i = 0; i < mats.Length; i++)
                if (mats[i] != null) Destroy(mats[i]);
        }
        previewInstanceMaterials.Clear();
        originalSharedMaterials.Clear();

        if (previewObject != null) Destroy(previewObject);
        previewObject = null;
        currentPiece = null;
        isPlacing = false;
    }

    // ---- Utility ----
    public static void AlignToSnapPoint(Transform piece, Transform mySnap, Transform targetSnap)
    {
        if (piece == null || mySnap == null || targetSnap == null) return;

        Vector3 delta = mySnap.position - piece.position;
        piece.position = targetSnap.position - delta;

        Quaternion rotation = Quaternion.FromToRotation(mySnap.forward, -targetSnap.forward);
        piece.rotation = rotation * piece.rotation;
    }
}
