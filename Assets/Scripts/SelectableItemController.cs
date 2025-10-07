using UnityEngine;
using System.Collections;
using System.Reflection;

[RequireComponent(typeof(Collider))]
public class SelectableItemController : MonoBehaviour
{
    [Header("Highlight Settings")]
    [Range(1f, 2f)] public float brightnessBoost = 1.3f;
    [Range(0f, 1f)] public float highlightOpacity = 0.7f;
    [Tooltip("Y-axis lift when selected")]
    public float liftHeight = 0.5f;
    [Tooltip("Lift/Land speed for Y-axis transition")]
    public float liftSpeed = 4f;

    [Tooltip("When true, modifies shared materials (affects all instances). False = makes unique material copies.")]
    public bool useSharedMaterials = false;

    private Renderer[] renderers;
    private Color[][] originalColors;
    private int[] originalRenderModes;

    private bool isSelected = false;
    private bool isMoveMode = false;
    private bool isLifting = false;

    private Vector3 lastAppliedPosition;
    private Quaternion lastAppliedRotation;
    private Vector3 basePosition;

    private ItemDragger itemDragger;

    private void Awake()
    {
        // Ensure ItemDragger exists
        itemDragger = GetComponent<ItemDragger>();
        if (itemDragger == null)
            itemDragger = gameObject.AddComponent<ItemDragger>();

        renderers = GetComponentsInChildren<Renderer>(true);
        CacheOriginalMaterialSettings();

        basePosition = transform.position;
        lastAppliedPosition = transform.position;
        lastAppliedRotation = transform.rotation;

        itemDragger.EnableDragging(false);
    }

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
            Material[] mats = useSharedMaterials ? renderers[i].sharedMaterials : renderers[i].materials;
            originalColors[i] = new Color[mats.Length];

            for (int j = 0; j < mats.Length; j++)
            {
                if (mats[j].HasProperty("_Color"))
                    originalColors[i][j] = mats[j].color;
            }
        }
    }

    // --- Selection ---
    public void Select()
    {
        var others = FindObjectsOfType<SelectableItemController>();
        foreach (var o in others)
            if (o != this) o.Deselect();

        isSelected = true;
        ApplyHighlight();
        StartCoroutine(SmoothLift(basePosition + Vector3.up * liftHeight));
        Debug.Log("[SelectableItemController] Selected: " + name);
    }

    public void Deselect()
    {
        if (!isSelected) return;

        isSelected = false;
        RestoreOriginalMaterials();
        StartCoroutine(SmoothLift(basePosition));

        isMoveMode = false;
        itemDragger.EnableDragging(false);
        Debug.Log("[SelectableItemController] Deselected: " + name);

        var ui = FindObjectOfType<SelectableControllerUI>();
        ui?.ResetMoveButtonVisual();
    }

    // --- Highlight (brighten + transparency) ---
    private void ApplyHighlight()
    {
        for (int i = 0; i < renderers.Length; i++)
        {
            Material[] mats = useSharedMaterials ? renderers[i].sharedMaterials : renderers[i].materials;

            for (int j = 0; j < mats.Length; j++)
            {
                if (!mats[j].HasProperty("_Color")) continue;

                Material mat = mats[j];
                Color baseColor = originalColors[i][j];
                Color brighter = baseColor * brightnessBoost;
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
            Material[] mats = useSharedMaterials ? renderers[i].sharedMaterials : renderers[i].materials;

            for (int j = 0; j < mats.Length; j++)
            {
                Material mat = mats[j];
                if (mat.HasProperty("_Color"))
                    mat.color = originalColors[i][j];

                SetMaterialRenderingMode(mat, RenderingMode.Opaque);
            }
        }
    }

    // --- Smooth Lift Animation ---
    private IEnumerator SmoothLift(Vector3 targetPos)
    {
        if (isLifting) yield break;
        isLifting = true;

        while (Vector3.Distance(transform.position, targetPos) > 0.01f)
        {
            transform.position = Vector3.Lerp(transform.position, targetPos, Time.deltaTime * liftSpeed);
            yield return null;
        }

        transform.position = targetPos;
        isLifting = false;
    }

    // --- Move / UI logic ---
    public void SetMoveActive(bool enable)
    {
        if (enable && !isSelected)
        {
            Debug.LogWarning($"[SelectableItemController] Attempt to enable Move on {name} while not selected. Ignoring.");
            return;
        }

        isMoveMode = enable;
        if (itemDragger != null)
        {
            itemDragger.EnableDragging(enable);
            Debug.Log($"[SelectableItemController] SetMoveActive({enable}) on {name}");
        }

        var ui = FindObjectOfType<SelectableControllerUI>();
        ui?.SetMoveButtonActive(enable);
    }

    public void ToggleMove() => SetMoveActive(!isMoveMode);
    public void RotateLeft() => transform.Rotate(Vector3.up, -90f, Space.World);
    public void RotateRight() => transform.Rotate(Vector3.up, 90f, Space.World);

    public void Revert()
    {
        transform.position = lastAppliedPosition;
        transform.rotation = lastAppliedRotation;
        Debug.Log("[SelectableItemController] Reverted " + name);
    }

    public void Apply()
    {
        lastAppliedPosition = transform.position;
        lastAppliedRotation = transform.rotation;
        basePosition = transform.position; // Update base to new height
        Debug.Log("[SelectableItemController] Applied transform for " + name);
        Deselect();
    }

    public void Remove()
    {
        Debug.Log("[SelectableItemController] Removed " + name);
        Destroy(gameObject);
    }

    // --- Utility for changing Standard shader mode ---
    public enum RenderingMode { Opaque, Cutout, Fade, Transparent }

    public static void SetMaterialRenderingMode(Material material, RenderingMode mode)
    {
        if (material == null) return;

        switch (mode)
        {
            case RenderingMode.Opaque:
                material.SetFloat("_Mode", 0);
                material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
                material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.Zero);
                material.SetInt("_ZWrite", 1);
                material.DisableKeyword("_ALPHATEST_ON");
                material.DisableKeyword("_ALPHABLEND_ON");
                material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                material.renderQueue = -1;
                break;

            case RenderingMode.Transparent:
                material.SetFloat("_Mode", 3);
                material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                material.SetInt("_ZWrite", 0);
                material.DisableKeyword("_ALPHATEST_ON");
                material.DisableKeyword("_ALPHABLEND_ON");
                material.EnableKeyword("_ALPHAPREMULTIPLY_ON");
                material.renderQueue = 3000;
                break;
        }
    }
}
