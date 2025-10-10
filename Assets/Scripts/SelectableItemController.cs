using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Collider))]
[DisallowMultipleComponent]
public class SelectableItemController : MonoBehaviour
{
    [Header("Highlight")]
    public float brightnessBoost = 1.3f;
    public float highlightOpacity = 0.7f;

    [Header("Lift")]
    public float liftHeight = 0.5f;
    public float liftSpeed = 4f;

    [Header("Material")]
    public bool useSharedMaterials = false;

    private Renderer[] renderers;
    private Color[][] originalColors;
    private ItemDragger itemDragger;
    private EditRoadItem editRoad;

    private bool isSelected;
    private bool isMoveMode;
    private bool isLifting;

    private Vector3 basePosition;
    private Vector3 lastAppliedPosition;
    private Quaternion lastAppliedRotation;

    private Coroutine liftRoutine;

    private void Awake()
    {
        itemDragger = GetComponent<ItemDragger>() ?? gameObject.AddComponent<ItemDragger>();
        editRoad = GetComponent<EditRoadItem>();

        itemDragger.OnDragEnd = OnDragEnd;

        renderers = GetComponentsInChildren<Renderer>(true);
        CacheOriginalMaterialSettings();

        basePosition = transform.position;
        lastAppliedPosition = transform.position;
        lastAppliedRotation = transform.rotation;

        itemDragger.EnableDragging(false);
    }

    // ================== Selection ==================
    public void Select()
    {
        foreach (var o in FindObjectsOfType<SelectableItemController>())
            if (o != this) o.Deselect();

        isSelected = true;
        ApplyHighlight();
        StartLift(basePosition + Vector3.up * liftHeight);
    }

    public void Deselect()
    {
        if (!isSelected) return;

        isSelected = false;
        RestoreOriginalMaterials();
        StartLift(basePosition);
        SetMoveActive(false);

        FindObjectOfType<SelectableControllerUI>()?.ResetMoveButtonVisual();
    }

    // ================== Drag / Snap ==================
    private void OnDragEnd()
    {
        StopLift();

        // Update base/applied position
        basePosition = transform.position;
        lastAppliedPosition = transform.position;

        // Auto-snap to other roots
        foreach (var other in FindObjectsOfType<SelectableItemController>())
        {
            if (other == this) continue;
            if (editRoad.TrySnapTo(other.GetComponent<EditRoadItem>()))
                break;
        }

        Debug.Log($"[SelectableItemController] Drag ended for {name}");
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
    }

    public void ToggleMove() => SetMoveActive(!isMoveMode);
    public void RotateLeft() => transform.Rotate(Vector3.up, -90f, Space.World);
    public void RotateRight() => transform.Rotate(Vector3.up, 90f, Space.World);

    // ================== Apply / Revert / Remove ==================
    public void Revert()
    {
        transform.position = lastAppliedPosition;
        transform.rotation = lastAppliedRotation;
    }

    public void Apply()
    {
        lastAppliedPosition = transform.position;
        lastAppliedRotation = transform.rotation;
        basePosition = transform.position;
        Deselect();
    }

    public void Remove() => Destroy(gameObject);

    // ================== Lift ==================
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

    // ================== Highlight ==================
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
                if (mat.HasProperty("_Color")) mat.color = originalColors[i][j];
                SetMaterialRenderingMode(mat, RenderingMode.Opaque);
            }
        }
    }

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
