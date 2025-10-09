using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Collider))]
[DisallowMultipleComponent]
public class SelectableItemController : MonoBehaviour
{
    [Header("Highlight Settings")]
    [Range(1f, 2f)] public float brightnessBoost = 1.3f;
    [Range(0f, 1f)] public float highlightOpacity = 0.7f;

    [Header("Lift Settings")]
    [Tooltip("Y-axis lift when selected")]
    public float liftHeight = 0.5f;
    [Tooltip("Lift/Land speed for Y-axis transition")]
    public float liftSpeed = 4f;

    [Header("Material Settings")]
    [Tooltip("True = uses shared materials (affects all instances). False = makes unique copies.")]
    public bool useSharedMaterials = false;

    // --- Cached references ---
    private Renderer[] renderers;
    private Color[][] originalColors;
    private ItemDragger itemDragger;

    // --- State ---
    private bool isSelected;
    private bool isMoveMode;
    private bool isLifting;

    private Vector3 basePosition;
    private Vector3 lastAppliedPosition;
    private Quaternion lastAppliedRotation;

    private Coroutine liftRoutine;

    // ====================================================================================================
    // LIFECYCLE
    // ====================================================================================================
    private void Awake()
    {
        // Ensure we have ItemDragger
        itemDragger = GetComponent<ItemDragger>();
        if (itemDragger == null)
            itemDragger = gameObject.AddComponent<ItemDragger>();

        // Hook drag end event
        itemDragger.OnDragEnd = OnDragEnd;

        // Cache renderers and original material settings
        renderers = GetComponentsInChildren<Renderer>(true);
        CacheOriginalMaterialSettings();

        // Initialize transform tracking
        basePosition = transform.position;
        lastAppliedPosition = transform.position;
        lastAppliedRotation = transform.rotation;

        // Disable move mode by default
        itemDragger.EnableDragging(false);
    }

    // ====================================================================================================
    // SELECTION LOGIC
    // ====================================================================================================
    public void Select()
    {
        // Deselect others
        foreach (var o in FindObjectsOfType<SelectableItemController>())
            if (o != this) o.Deselect();

        isSelected = true;
        ApplyHighlight();

        StartLift(basePosition + Vector3.up * liftHeight);

        Debug.Log($"[SelectableItemController] Selected: {name}");
    }

    public void Deselect()
    {
        if (!isSelected) return;

        isSelected = false;
        RestoreOriginalMaterials();
        StartLift(basePosition);

        SetMoveActive(false);
        Debug.Log($"[SelectableItemController] Deselected: {name}");

        // Reset UI
        FindObjectOfType<SelectableControllerUI>()?.ResetMoveButtonVisual();
    }

    // ====================================================================================================
    // DRAG / MOVE LOGIC
    // ====================================================================================================
    private void OnDragEnd()
    {
        StopLift();

        basePosition = transform.position;
        lastAppliedPosition = transform.position;

        Debug.Log($"[SelectableItemController] Base position updated after drag: {basePosition}");
    }

    public void SetMoveActive(bool enable)
    {
        if (enable && !isSelected)
        {
            Debug.LogWarning($"[SelectableItemController] Attempt to enable Move on {name} while not selected. Ignored.");
            return;
        }

        isMoveMode = enable;
        itemDragger.EnableDragging(enable);

        FindObjectOfType<SelectableControllerUI>()?.SetMoveButtonActive(enable);
        Debug.Log($"[SelectableItemController] Move mode {(enable ? "ENABLED" : "DISABLED")} for {name}");
    }

    public void ToggleMove() => SetMoveActive(!isMoveMode);

    public void RotateLeft() => transform.Rotate(Vector3.up, -90f, Space.World);
    public void RotateRight() => transform.Rotate(Vector3.up, 90f, Space.World);

    // ====================================================================================================
    // APPLY / REVERT / REMOVE
    // ====================================================================================================
    public void Revert()
    {
        transform.position = lastAppliedPosition;
        transform.rotation = lastAppliedRotation;
        Debug.Log($"[SelectableItemController] Reverted {name}");
    }

    public void Apply()
    {
        lastAppliedPosition = transform.position;
        lastAppliedRotation = transform.rotation;
        basePosition = transform.position;
        Debug.Log($"[SelectableItemController] Applied transform for {name}");
        Deselect();
    }

    public void Remove()
    {
        Debug.Log($"[SelectableItemController] Removed {name}");
        Destroy(gameObject);
    }

    // ====================================================================================================
    // LIFT ANIMATION
    // ====================================================================================================
    private void StartLift(Vector3 targetPos)
    {
        StopLift();
        liftRoutine = StartCoroutine(LiftRoutine(targetPos));
    }

    private void StopLift()
    {
        if (liftRoutine != null)
        {
            StopCoroutine(liftRoutine);
            liftRoutine = null;
        }
        isLifting = false;
    }

    private IEnumerator LiftRoutine(Vector3 targetPos)
    {
        isLifting = true;

        while (Vector3.Distance(transform.position, targetPos) > 0.01f)
        {
            transform.position = Vector3.Lerp(transform.position, targetPos, Time.deltaTime * liftSpeed);
            yield return null;
        }

        transform.position = targetPos;
        isLifting = false;
    }

    // ====================================================================================================
    // HIGHLIGHT LOGIC
    // ====================================================================================================
    private void CacheOriginalMaterialSettings()
    {
        if (renderers == null || renderers.Length == 0)
        {
            originalColors = new Color[0][];
            return;
        }

        originalColors = new Color[renderers.Length][];
        for (int i = 0; i < renderers.Length; i++)
        {
            var mats = useSharedMaterials ? renderers[i].sharedMaterials : renderers[i].materials;
            originalColors[i] = new Color[mats.Length];

            for (int j = 0; j < mats.Length; j++)
                if (mats[j].HasProperty("_Color"))
                    originalColors[i][j] = mats[j].color;
        }
    }

    private void ApplyHighlight()
    {
        for (int i = 0; i < renderers.Length; i++)
        {
            var mats = useSharedMaterials ? renderers[i].sharedMaterials : renderers[i].materials;
            for (int j = 0; j < mats.Length; j++)
            {
                var mat = mats[j];
                if (!mat.HasProperty("_Color")) continue;

                var baseColor = originalColors[i][j];
                var brighter = baseColor * brightnessBoost;
                brighter.a = highlightOpacity;

                SetMaterialRenderingMode(mat, RenderingMode.Transparent);
                mat.color = brighter;
            }
        }
    }

    private void RestoreOriginalMaterials()
    {
        for (int i = 0; i < renderers.Length; i++)
        {
            var mats = useSharedMaterials ? renderers[i].sharedMaterials : renderers[i].materials;
            for (int j = 0; j < mats.Length; j++)
            {
                var mat = mats[j];
                if (mat.HasProperty("_Color"))
                    mat.color = originalColors[i][j];

                SetMaterialRenderingMode(mat, RenderingMode.Opaque);
            }
        }
    }

    // ====================================================================================================
    // MATERIAL RENDER MODE UTILITY
    // ====================================================================================================
    public enum RenderingMode { Opaque, Transparent }

    public static void SetMaterialRenderingMode(Material mat, RenderingMode mode)
    {
        if (mat == null) return;

        switch (mode)
        {
            case RenderingMode.Opaque:
                mat.SetFloat("_Mode", 0);
                mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
                mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.Zero);
                mat.SetInt("_ZWrite", 1);
                mat.DisableKeyword("_ALPHATEST_ON");
                mat.DisableKeyword("_ALPHABLEND_ON");
                mat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                mat.renderQueue = -1;
                break;

            case RenderingMode.Transparent:
                mat.SetFloat("_Mode", 3);
                mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                mat.SetInt("_ZWrite", 0);
                mat.DisableKeyword("_ALPHATEST_ON");
                mat.DisableKeyword("_ALPHABLEND_ON");
                mat.EnableKeyword("_ALPHAPREMULTIPLY_ON");
                mat.renderQueue = 3000;
                break;
        }
    }
}
