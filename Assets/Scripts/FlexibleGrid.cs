using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(GridLayoutGroup))]
public class FlexibleGrid : MonoBehaviour
{
    public int columns = 3;         // number of columns
    public float fixedHeight = 150; // item height
    public Vector2 spacing = new Vector2(10, 10);

    private GridLayoutGroup grid;

    void Awake()
    {
        grid = GetComponent<GridLayoutGroup>();
        grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        grid.constraintCount = columns;
        grid.spacing = spacing;
    }

    void Update()
    {
        AdjustCellSize();
    }

    void AdjustCellSize()
    {
        float width = ((RectTransform)transform).rect.width;
        float totalSpacing = spacing.x * (columns - 1);
        float cellWidth = (width - totalSpacing) / columns;

        grid.cellSize = new Vector2(cellWidth, fixedHeight);
    }
}
