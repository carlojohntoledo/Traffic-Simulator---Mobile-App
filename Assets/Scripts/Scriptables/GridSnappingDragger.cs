using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(Collider))]
public class GridSnappingDragger : MonoBehaviour
{
    public float dragHeight = 0.1f;
    public Color validColor = Color.white;
    public Color invalidColor = Color.red;

    private bool isDragging;
    private Camera mainCam;
    private Renderer[] renderers;
    private Vector3 lastValidPosition;
    private bool canPlace = true;

    void Start()
    {
        mainCam = Camera.main;
        renderers = GetComponentsInChildren<Renderer>();
        lastValidPosition = transform.position;
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            // start drag if clicked on self
            if (Physics.Raycast(mainCam.ScreenPointToRay(Input.mousePosition), out var hit))
            {
                if (hit.transform == transform || hit.transform.IsChildOf(transform))
                    isDragging = true;
            }
        }

        if (Input.GetMouseButton(0) && isDragging)
        {
            if (Physics.Raycast(mainCam.ScreenPointToRay(Input.mousePosition), out var hit))
            {
                Vector3 worldPos = hit.point;
                var grid = GroundGrid.Instance;
                if (grid == null) return;

                // snap to nearest grid cell
                Vector2Int cell = grid.WorldToGrid(worldPos);
                Vector3 snapped = grid.GridToWorld(cell);
                snapped.y += dragHeight;

                // preview color depending on occupancy
                canPlace = grid.CanPlaceAt(cell);
                SetTint(canPlace ? validColor : invalidColor);

                transform.position = snapped;

                if (canPlace)
                    lastValidPosition = snapped;
            }
        }

        if (Input.GetMouseButtonUp(0) && isDragging)
        {
            isDragging = false;

            var grid = GroundGrid.Instance;
            if (grid == null) return;

            Vector2Int cell = grid.WorldToGrid(transform.position);
            if (canPlace)
            {
                grid.OccupyCell(cell, gameObject);
                SetTint(validColor);
            }
            else
            {
                // rollback
                transform.position = lastValidPosition;
                SetTint(validColor);
            }
        }
    }

    void SetTint(Color c)
    {
        foreach (var r in renderers)
        {
            foreach (var mat in r.materials)
            {
                if (mat.HasProperty("_Color"))
                    mat.color = c;
                else if (mat.HasProperty("_BaseColor"))
                    mat.SetColor("_BaseColor", c);
            }
        }
    }
}
