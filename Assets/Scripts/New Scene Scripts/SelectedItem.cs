using UnityEngine;

[RequireComponent(typeof(Collider))]
public class SelectedItem : MonoBehaviour
{
    [HideInInspector] public ItemDataComponent data;

    public bool IsPreviewMode { get; private set; } = true;
    public bool IsMoving { get; private set; } = false;
    public bool IsRotated { get; private set; } = false;
    public bool IsSelected { get; private set; } = false;

    private DragItem dragItem;
    private ItemPreviewUI previewUI;
    private GridManager gridManager;
    private Renderer[] renderers;
    private Color[] originalColors;

    public void Initialize(ItemDataComponent itemData, ItemPreviewUI ui, GridManager grid = null)
    {
        data = itemData;
        previewUI = ui;
        gridManager = grid;

        dragItem = GetComponent<DragItem>() ?? gameObject.AddComponent<DragItem>();
        if (gridManager != null)
            dragItem.gridSize = gridManager.cellSize;

        CacheRenderers();
        SetGhostVisual(true);
        ToggleMoveMode(true);

        IsPreviewMode = true;
        IsSelected = true;
    }

    private void CacheRenderers()
    {
        renderers = GetComponentsInChildren<Renderer>(true);
        originalColors = new Color[renderers.Length];
        for (int i = 0; i < renderers.Length; i++)
        {
            if (renderers[i].material.HasProperty("_Color"))
                originalColors[i] = renderers[i].material.color;
        }
    }

    private void SetGhostVisual(bool ghost)
    {
        foreach (var r in renderers)
        {
            if (r == null) continue;
            foreach (var mat in r.materials)
            {
                if (!mat.HasProperty("_Color")) continue;
                Color c = mat.color;
                c.a = ghost ? 0.45f : 1f;
                mat.color = c;
            }
        }
    }

    public void ToggleMoveMode(bool enable)
    {
        IsMoving = enable;
        if (dragItem != null) dragItem.ToggleMoveMode(enable);

        // ✅ disable camera drag when moving model
        InputManager.Instance.SetModelDragging(enable);
    }

    public void ToggleRotate()
    {
        IsRotated = !IsRotated;
        transform.rotation = IsRotated ? Quaternion.Euler(0f, -90f, 0f) : Quaternion.identity;
    }

    public void ConfirmPlacement()
    {
        if (!IsPreviewMode) return;

        if (gridManager != null)
        {
            var coord = gridManager.GetGridCoordinate(transform.position);
            transform.position = gridManager.GetWorldPosition(coord);
            gridManager.SetTileOccupied(coord, true);
        }

        ToggleMoveMode(false);
        IsPreviewMode = false;
        IsSelected = false;
        SetGhostVisual(false);
        name = data != null ? $"{data.itemName}_Placed" : name;
    }

    public void CancelPlacement()
    {
        Destroy(gameObject);
        previewUI?.Hide();
    }

    public void EnterEditMode()
    {
        if (IsPreviewMode) return;
        ToggleMoveMode(true);
        IsSelected = true;
        previewUI?.Show(this);
    }

    public void ApplyEdit()
    {
        ToggleMoveMode(false);
        IsSelected = false;
        previewUI?.Hide();
    }

    public void Remove()
    {
        if (!IsPreviewMode && gridManager != null)
        {
            var coord = gridManager.GetGridCoordinate(transform.position);
            gridManager.SetTileOccupied(coord, false);
        }

        Destroy(gameObject);
        previewUI?.Hide();
    }
}
