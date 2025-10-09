using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections.Generic;

[RequireComponent(typeof(Collider))]
public class ItemDragger : MonoBehaviour
{
    [Header("Settings")]
    public float moveSpeed = 10f;            // smooth follow speed
    public LayerMask groundLayer;            // layer for raycast placement

    [Header("References (Auto-filled)")]
    public Camera mainCamera;
    public GraphicRaycaster uiRaycaster;
    public EventSystem eventSystem;

    // ✅ Callback for notifying SelectableItemController
    public System.Action OnDragEnd;

    private bool isDragging = false;
    private bool isMoveMode = false;
    private float dragStartTime;
    private PointerEventData pointerData;
    private List<RaycastResult> raycastResults = new List<RaycastResult>();

    void Awake()
    {
        // Auto-assign references
        if (mainCamera == null)
            mainCamera = Camera.main;

        if (eventSystem == null)
            eventSystem = EventSystem.current;

        if (uiRaycaster == null)
        {
            Canvas canvas = FindObjectOfType<Canvas>();
            if (canvas != null)
                uiRaycaster = canvas.GetComponent<GraphicRaycaster>();
        }

        if (uiRaycaster == null)
            Debug.LogWarning("[ItemDragger] No GraphicRaycaster found — UI click protection may fail!");
    }

    public void EnableDragging(bool enable)
    {
        isMoveMode = enable;

        if (!enable)
        {
            isDragging = false;
            InputBlocker.IsModelDragging = false;
        }

        Debug.Log($"[ItemDragger] MoveMode={(enable ? "ON" : "OFF")} for {name}");
    }

    void Update()
    {
        if (!isMoveMode) return;

        // --- Begin drag ---
        if (Input.GetMouseButtonDown(0))
        {
            if (IsPointerOverUI())
            {
                Debug.Log($"[ItemDragger] Click ignored — pointer is over UI for {name}");
                return;
            }

            dragStartTime = Time.time;
        }

        // --- Dragging (while mouse held) ---
        if (Input.GetMouseButton(0))
        {
            if (IsPointerOverUI()) return;

            if (!isDragging)
            {
                isDragging = true;
                InputBlocker.IsModelDragging = true;
                Debug.Log($"[ItemDragger] Drag started for {name}");
            }

            if (Physics.Raycast(mainCamera.ScreenPointToRay(Input.mousePosition), out RaycastHit hit, 1000f, groundLayer))
            {
                Vector3 target = hit.point;
                transform.position = Vector3.Lerp(transform.position, target, Time.deltaTime * moveSpeed);
            }
        }

        // --- Drag End ---
        if (Input.GetMouseButtonUp(0))
        {
            if (isDragging)
            {
                isDragging = false;
                InputBlocker.IsModelDragging = false;
                Debug.Log($"[ItemDragger] Drag ended for {name}");

                // ✅ Notify controller to update basePosition
                OnDragEnd?.Invoke();
            }
        }
    }

    // --- Detect if pointer is over UI ---
    private bool IsPointerOverUI()
    {
        if (eventSystem == null)
            return EventSystem.current != null && EventSystem.current.IsPointerOverGameObject();

        if (uiRaycaster == null)
        {
            Canvas canvas = FindObjectOfType<Canvas>();
            if (canvas != null)
                uiRaycaster = canvas.GetComponent<GraphicRaycaster>();
        }

        if (uiRaycaster == null) return false;

        pointerData = new PointerEventData(eventSystem)
        {
            position = Input.mousePosition
        };

        raycastResults.Clear();
        uiRaycaster.Raycast(pointerData, raycastResults);
        return raycastResults.Count > 0;
    }
}
