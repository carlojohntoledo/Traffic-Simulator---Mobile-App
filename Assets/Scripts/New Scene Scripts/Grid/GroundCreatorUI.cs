using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GroundCreatorUI : MonoBehaviour
{
    [Header("UI References")]
    public TMP_InputField widthInput;
    public TMP_InputField heightInput;
    public Button confirmButton;

    [Header("Defaults")]
    public int defaultWidth = 100;
    public int defaultHeight = 200;
    public float cellSize = 2.5f;
    public Material groundMaterial;

    private void Start()
    {
        // Set defaults
        if (widthInput) widthInput.text = defaultWidth.ToString();
        if (heightInput) heightInput.text = defaultHeight.ToString();

        if (confirmButton)
            confirmButton.onClick.AddListener(OnConfirmBuild);
    }

    private void OnConfirmBuild()
    {
        int width = defaultWidth;
        int height = defaultHeight;

        int.TryParse(widthInput.text, out width);
        int.TryParse(heightInput.text, out height);

        CreateGround(width, height);
    }

    private void CreateGround(int width, int height)
    {
        // Remove any existing ground
        GameObject existing = GameObject.Find("Ground");
        if (existing) DestroyImmediate(existing);

        // Create new ground object
        GameObject ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
        ground.name = "Ground";
        ground.transform.position = Vector3.zero;

        // Scale plane: Unity's Plane is 10x10 by default
        ground.transform.localScale = new Vector3(width * cellSize / 10f, 1f, height * cellSize / 10f);

        ground.layer = LayerMask.NameToLayer("Ground");

        // Apply custom material if set
        if (groundMaterial)
            ground.GetComponent<Renderer>().material = groundMaterial;

        // Add GridManager
        GridManager grid = ground.AddComponent<GridManager>();
        grid.gridWidth = width;
        grid.gridHeight = height;
        grid.cellSize = cellSize;

        Debug.Log($"✅ Ground created with grid: {width}x{height}, cell size {cellSize}");

        // Optionally hide UI after creation
        gameObject.SetActive(false);
    }
}
