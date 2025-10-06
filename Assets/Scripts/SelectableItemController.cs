using UnityEngine;

[RequireComponent(typeof(Collider))]
public class SelectableItemController : MonoBehaviour
{
    [Header("Highlight Settings")]
    [Range(0f, 2f)] public float brightnessBoost = 1.2f;
    [Range(0f, 2f)] public float emissionIntensity = 0.5f;
    [Range(0.05f, 1f)] public float highlightFadeSpeed = 0.2f;

    private Renderer[] renderers;
    private Color[][] originalColors;
    private Color[][] currentEmissionColors;
    private bool isSelected = false;
    private bool isMoveMode = false;

    private Vector3 lastAppliedPosition;
    private Quaternion lastAppliedRotation;

    private ItemDragger itemDragger;

    private void Awake()
    {
        itemDragger = GetComponent<ItemDragger>();
        if (itemDragger == null)
            itemDragger = gameObject.AddComponent<ItemDragger>();

        renderers = GetComponentsInChildren<Renderer>(true);
        CacheOriginalColors();

        lastAppliedPosition = transform.position;
        lastAppliedRotation = transform.rotation;

        itemDragger.EnableDragging(false);
    }

    private void CacheOriginalColors()
    {
        if (renderers == null || renderers.Length == 0)
        {
            originalColors = new Color[0][];
            currentEmissionColors = new Color[0][];
            return;
        }

        originalColors = new Color[renderers.Length][];
        currentEmissionColors = new Color[renderers.Length][];

        for (int i = 0; i < renderers.Length; i++)
        {
            Material[] mats = renderers[i].materials;
            originalColors[i] = new Color[mats.Length];
            currentEmissionColors[i] = new Color[mats.Length];

            for (int j = 0; j < mats.Length; j++)
            {
                if (mats[j].HasProperty("_Color"))
                    originalColors[i][j] = mats[j].color;

                if (mats[j].HasProperty("_EmissionColor"))
                {
                    mats[j].EnableKeyword("_EMISSION");
                    currentEmissionColors[i][j] = mats[j].GetColor("_EmissionColor");
                }
            }
        }
    }

    private void Update()
    {
        // Smoothly blend emission for highlight fade
        for (int i = 0; i < renderers.Length; i++)
        {
            Material[] mats = renderers[i].materials;
            for (int j = 0; j < mats.Length; j++)
            {
                if (!mats[j].HasProperty("_EmissionColor")) continue;

                Color target = isSelected ? originalColors[i][j] * emissionIntensity : Color.black;
                Color current = mats[j].GetColor("_EmissionColor");
                Color newColor = Color.Lerp(current, target, Time.deltaTime / highlightFadeSpeed);
                mats[j].SetColor("_EmissionColor", newColor);
            }
        }
    }

    public void Select()
    {
        // Deselect others
        var others = FindObjectsOfType<SelectableItemController>();
        foreach (var o in others)
            if (o != this) o.Deselect();

        isSelected = true;
        ApplyHighlight();
        Debug.Log("[SelectableItemController] Selected: " + name);
    }

    public void Deselect()
    {
        if (!isSelected) return;

        isSelected = false;
        RestoreOriginalColors();

        isMoveMode = false;
        itemDragger.EnableDragging(false);
        Debug.Log("[SelectableItemController] Deselected: " + name);

        var ui = FindObjectOfType<SelectableControllerUI>();
        ui?.ResetMoveButtonVisual();
    }

    private void ApplyHighlight()
    {
        if (renderers == null) return;

        for (int i = 0; i < renderers.Length; i++)
        {
            Material[] mats = renderers[i].materials;
            for (int j = 0; j < mats.Length; j++)
            {
                if (!mats[j].HasProperty("_Color")) continue;

                Color baseColor = originalColors[i][j];
                mats[j].color = baseColor * brightnessBoost;

                // Enable emission highlight
                if (mats[j].HasProperty("_EmissionColor"))
                {
                    mats[j].EnableKeyword("_EMISSION");
                    mats[j].SetColor("_EmissionColor", baseColor * emissionIntensity);
                }
            }
        }
    }

    private void RestoreOriginalColors()
    {
        if (renderers == null || originalColors == null) return;

        for (int i = 0; i < renderers.Length; i++)
        {
            Material[] mats = renderers[i].materials;
            for (int j = 0; j < mats.Length; j++)
            {
                if (mats[j].HasProperty("_Color"))
                    mats[j].color = originalColors[i][j];
            }
        }
    }

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
            Debug.Log($"[SelectableItemController] SetMoveActive({enable}) on {name}; ItemDragger enabled");
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
        Debug.Log("[SelectableItemController] Applied transform for " + name);
        Deselect();
    }

    public void Remove()
    {
        Debug.Log("[SelectableItemController] Removed " + name);
        Destroy(gameObject);
    }
}
