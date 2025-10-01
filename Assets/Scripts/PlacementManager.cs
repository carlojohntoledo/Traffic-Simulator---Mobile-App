using UnityEngine;
using System.Collections.Generic;

public class PlacementManager : MonoBehaviour
{
    public static PlacementManager Instance;

    private GameObject previewObject;
    private RoadPiece currentPiece;
    private Camera mainCam;
    public LayerMask groundMask;

    private bool isPlacing = false;

    [Header("Snapping Settings")]
    public float snapRange = 1.5f; // distance threshold to detect nearby snap points

    private List<RoadPiece> placedRoads = new List<RoadPiece>();

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        mainCam = Camera.main;
    }

    void Update()
    {
        if (!isPlacing) return;

#if UNITY_EDITOR || UNITY_STANDALONE
        HandleMouse();
#else
        HandleTouch();
#endif

        if (previewObject == null) return;

        // Rotate manually (desktop)
        if (Input.GetKeyDown(KeyCode.Q)) previewObject.transform.Rotate(Vector3.up, -currentPiece.rotationStep);
        if (Input.GetKeyDown(KeyCode.E)) previewObject.transform.Rotate(Vector3.up, currentPiece.rotationStep);
    }

    public void StartPlacingRoad(GameObject prefab)
    {
        if (previewObject != null) Destroy(previewObject);

        previewObject = Instantiate(prefab);
        currentPiece = previewObject.GetComponent<RoadPiece>();

        // Ghost setup
        previewObject.GetComponent<Collider>().enabled = false;
        SetMaterialAlpha(previewObject, 0.5f);

        isPlacing = true;
    }

    public void FinishPlacingRoad()
    {
        if (!isPlacing || previewObject == null) return;

        // Solidify
        previewObject.GetComponent<Collider>().enabled = true;
        SetMaterialAlpha(previewObject, 1f);

        // Fire event
        currentPiece.OnPlaced();

        // Add to placed list for future snapping
        placedRoads.Add(currentPiece);

        // Reset preview
        previewObject = null;
        currentPiece = null;
        isPlacing = false;
    }

    void HandleMouse()
    {
        Ray ray = mainCam.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out RaycastHit hit, 100f, groundMask))
        {
            TrySnapOrGrid(hit.point);
        }

        if (Input.GetMouseButtonUp(0))
        {
            FinishPlacingRoad();
        }
    }

    void HandleTouch()
    {
        if (Input.touchCount == 0) return;

        Touch t = Input.GetTouch(0);
        Ray ray = mainCam.ScreenPointToRay(t.position);

        if (Physics.Raycast(ray, out RaycastHit hit, 100f, groundMask))
        {
            TrySnapOrGrid(hit.point);
        }

        if (t.phase == TouchPhase.Ended)
        {
            FinishPlacingRoad();
        }
    }

    void TrySnapOrGrid(Vector3 hitPos)
    {
        if (previewObject == null || currentPiece == null) return;

        // Try snapping
        Transform bestTarget = null;
        Transform bestMyPoint = null;
        float bestDist = snapRange;

        foreach (var road in placedRoads)
        {
            if (road == null) continue;

            Transform candidate = road.GetClosestSnapPoint(hitPos, snapRange);
            if (candidate != null)
            {
                foreach (var myPoint in currentPiece.snapPoints)
                {
                    float d = Vector3.Distance(candidate.position, myPoint.position);
                    if (d < bestDist)
                    {
                        bestDist = d;
                        bestTarget = candidate;
                        bestMyPoint = myPoint;
                    }
                }
            }
        }

        if (bestTarget != null && bestMyPoint != null)
        {
            // Align preview road
            Vector3 offset = previewObject.transform.position - bestMyPoint.position;
            previewObject.transform.position = bestTarget.position + offset;

            // Rotate so forward vectors align (roads connect smoothly)
            Vector3 dirA = bestMyPoint.forward;
            Vector3 dirB = -bestTarget.forward;
            float angle = Vector3.SignedAngle(dirA, dirB, Vector3.up);
            previewObject.transform.Rotate(Vector3.up, angle, Space.World);
        }
        else
        {
            // No snap found → just snap to ground grid
            previewObject.transform.position = SnapToGrid(hitPos);
        }
    }

    Vector3 SnapToGrid(Vector3 pos)
    {
        if (currentPiece == null) return pos;

        float grid = currentPiece.snapSize;
        float x = Mathf.Round(pos.x / grid) * grid;
        float z = Mathf.Round(pos.z / grid) * grid;
        return new Vector3(x, 0f, z);
    }

    void SetMaterialAlpha(GameObject obj, float alpha)
    {
        Renderer[] rends = obj.GetComponentsInChildren<Renderer>();
        foreach (Renderer rend in rends)
        {
            foreach (Material mat in rend.materials)
            {
                Color c = mat.color;
                c.a = alpha;
                mat.color = c;

                if (alpha < 1f)
                {
                    mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                    mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                    mat.SetInt("_ZWrite", 0);
                    mat.DisableKeyword("_ALPHATEST_ON");
                    mat.EnableKeyword("_ALPHABLEND_ON");
                    mat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                    mat.renderQueue = 3000;
                }
                else
                {
                    mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
                    mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.Zero);
                    mat.SetInt("_ZWrite", 1);
                    mat.DisableKeyword("_ALPHATEST_ON");
                    mat.DisableKeyword("_ALPHABLEND_ON");
                    mat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                    mat.renderQueue = -1;
                }
            }
        }
    }

    public static void AlignToSnapPoint(Transform piece, Transform mySnap, Transform targetSnap)
    {
        // Move the piece so snap points overlap
        Vector3 offset = mySnap.position - piece.position;
        piece.position = targetSnap.position - offset;

        // Rotate so forward directions oppose each other
        Quaternion rotation = Quaternion.FromToRotation(mySnap.forward, -targetSnap.forward);
        piece.rotation = rotation * piece.rotation;
    }

}
