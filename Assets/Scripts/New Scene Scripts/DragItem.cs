using UnityEngine;

[RequireComponent(typeof(Collider))]
public class DragItem : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 15f;
    public LayerMask groundLayer = ~0;

    [Header("Grid")]
    public bool useGridSnapping = true;
    public float gridSize = 1f;

    private Camera mainCamera;
    private bool isMoving = false;

    private void Awake()
    {
        mainCamera = Camera.main;
        var col = GetComponent<Collider>();
        if (col != null) col.isTrigger = true;
    }

    private void Update()
    {
        if (!isMoving) return;
        HandleDrag();
    }

    private void HandleDrag()
    {
        // ✅ Skip movement if pointer is over UI
        if (InputManager.Instance.IsPointerOverUI())
            return;

        Vector2 screenPos = Input.touchCount > 0
            ? (Vector2)Input.GetTouch(0).position
            : (Vector2)Input.mousePosition;

        Ray ray = mainCamera.ScreenPointToRay(screenPos);
        if (Physics.Raycast(ray, out RaycastHit hit, 1000f, groundLayer))
        {
            Vector3 target = hit.point;
            if (useGridSnapping && gridSize > 0f)
            {
                target.x = Mathf.Round(target.x / gridSize) * gridSize;
                target.z = Mathf.Round(target.z / gridSize) * gridSize;
            }

            transform.position = Vector3.Lerp(transform.position, target, Time.deltaTime * moveSpeed);
        }
    }

    public void ToggleMoveMode(bool enable)
    {
        isMoving = enable;
    }

    public void ToggleRotate90()
    {
        transform.rotation = Quaternion.Euler(0f, 90f, 0f) * transform.rotation;
    }

    public void Place() => ToggleMoveMode(false);
    public void Remove() => Destroy(gameObject);
}
