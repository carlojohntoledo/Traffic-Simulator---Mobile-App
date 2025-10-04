using UnityEngine;

[RequireComponent(typeof(Collider))]
public class SelectableItemController : MonoBehaviour
{
    [Header("Visuals")]
    public Material transparentMaterial;

    private Material[] originalMaterials;
    private Renderer[] renderers;
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

        renderers = GetComponentsInChildren<Renderer>();
        CacheOriginalMaterials();

        lastAppliedPosition = transform.position;
        lastAppliedRotation = transform.rotation;
    }

    private void CacheOriginalMaterials()
    {
        if (renderers == null) return;
        int count = 0;
        foreach (var r in renderers) count += r.sharedMaterials.Length;

        originalMaterials = new Material[count];
        int index = 0;
        foreach (var r in renderers)
            foreach (var mat in r.sharedMaterials)
                originalMaterials[index++] = mat;
    }

    // Called by the UI or when first spawned
    public void Select()
    {
        isSelected = true;
        SetTransparentMaterial(true);
    }

    public void Deselect()
    {
        isSelected = false;
        SetTransparentMaterial(false);
        itemDragger.EnableDragging(false);
        isMoveMode = false;
    }

    private void SetTransparentMaterial(bool enable)
    {
        if (renderers == null || transparentMaterial == null) return;

        foreach (var rend in renderers)
        {
            Material[] mats = new Material[rend.sharedMaterials.Length];
            for (int i = 0; i < mats.Length; i++)
                mats[i] = enable ? transparentMaterial : originalMaterials[Mathf.Clamp(i, 0, originalMaterials.Length - 1)];
            rend.sharedMaterials = mats;
        }
    }

    // --- Public Controls (called by UI) ---
    public void ToggleMove()
    {
        isMoveMode = !isMoveMode;
        itemDragger.EnableDragging(isMoveMode);
    }

    public void RotateLeft() => transform.Rotate(Vector3.up, -90f, Space.World);
    public void RotateRight() => transform.Rotate(Vector3.up, 90f, Space.World);

    public void Revert()
    {
        transform.position = lastAppliedPosition;
        transform.rotation = lastAppliedRotation;
    }

    public void Apply()
    {
        lastAppliedPosition = transform.position;
        lastAppliedRotation = transform.rotation;
        Deselect();
    }

    public void Remove()
    {
        Destroy(gameObject);
    }
}
