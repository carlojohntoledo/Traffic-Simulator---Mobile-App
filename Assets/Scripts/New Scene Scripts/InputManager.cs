using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections.Generic;

/// <summary>
/// InputManager: Centralized input state and UI blocking controller.
/// Prevents camera vs. model drag conflicts, and adds on-screen debug overlay.
/// </summary>
public class InputManager : MonoBehaviour
{
    public static InputManager Instance { get; private set; }

    [Header("UI References")]
    public GraphicRaycaster uiRaycaster;
    public EventSystem eventSystem;

    [Header("Debug Overlay Settings")]
    public bool showDebugOverlay = true;
    public Vector2 overlayPosition = new Vector2(10f, 10f);
    public int fontSize = 14;

    // --- State Flags (backing fields, not properties, so [Header] is valid) ---
    [Header("Runtime States")]
    [SerializeField] private bool isCameraDragging = false;
    [SerializeField] private bool isModelDragging = false;

    private PointerEventData pointerData;
    private readonly List<RaycastResult> results = new List<RaycastResult>();

    // Cached debug GUI style
    private GUIStyle debugStyle;

    // Public read-only accessors
    public bool IsCameraDragging => isCameraDragging;
    public bool IsModelDragging => isModelDragging;
    public bool AnyActive => isCameraDragging || isModelDragging;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        debugStyle = new GUIStyle
        {
            fontSize = fontSize,
            normal = new GUIStyleState { textColor = Color.white }
        };
    }

    private void Update()
    {
        // Toggle debug overlay
        if (Input.GetKeyDown(KeyCode.F1))
        {
            showDebugOverlay = !showDebugOverlay;
        }
    }

    /// <summary>
    /// Check if pointer or touch is currently over a UI element (blocks camera/item input).
    /// </summary>
    public bool IsPointerOverUI()
    {
        if (uiRaycaster == null || eventSystem == null)
            return false;

        pointerData = new PointerEventData(eventSystem)
        {
            position = Input.mousePosition
        };

        results.Clear();
        uiRaycaster.Raycast(pointerData, results);
        return results.Count > 0;
    }

    public void SetCameraDragging(bool active) => isCameraDragging = active;
    public void SetModelDragging(bool active) => isModelDragging = active;

    // --- Debug Overlay ---
    private void OnGUI()
    {
        if (!showDebugOverlay) return;

        string info = $"<b><color=cyan>Input Debug Overlay</color></b>\n" +
                      $"Pointer Over UI: {(IsPointerOverUI() ? "<color=red>YES</color>" : "<color=green>NO</color>")}\n" +
                      $"Camera Dragging: {(isCameraDragging ? "<color=yellow>ACTIVE</color>" : "off")}\n" +
                      $"Model Dragging: {(isModelDragging ? "<color=yellow>ACTIVE</color>" : "off")}\n" +
                      $"Any Active: {(AnyActive ? "<color=orange>TRUE</color>" : "false")}";

        Rect rect = new Rect(overlayPosition.x, overlayPosition.y, 260, 120);
        GUI.Box(rect, GUIContent.none);
        GUI.Label(rect, info, debugStyle);
    }
}
