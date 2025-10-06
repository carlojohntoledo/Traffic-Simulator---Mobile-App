using UnityEngine;
using UnityEditor;

public class AutoCenterPivotTool : EditorWindow
{
    [MenuItem("Tools/Pivot/Center Selected Pivot")]
    public static void CenterSelectedPivot()
    {
        if (Selection.gameObjects.Length == 0)
        {
            Debug.LogWarning("No GameObjects selected.");
            return;
        }

        foreach (GameObject go in Selection.gameObjects)
        {
            CenterPivot(go);
        }

        Debug.Log($"✅ Centered pivot for {Selection.gameObjects.Length} object(s).");
    }

    private static void CenterPivot(GameObject go)
    {
        Renderer[] renderers = go.GetComponentsInChildren<Renderer>();
        if (renderers.Length == 0)
        {
            Debug.LogWarning($"No renderers found in {go.name}, skipping.");
            return;
        }

        // Calculate combined bounds
        Bounds combinedBounds = renderers[0].bounds;
        foreach (Renderer r in renderers)
            combinedBounds.Encapsulate(r.bounds);

        Vector3 center = combinedBounds.center;

        // Create wrapper
        GameObject pivotRoot = new GameObject("PivotRoot");
        Undo.RegisterCreatedObjectUndo(pivotRoot, "Create PivotRoot");

        pivotRoot.transform.SetParent(go.transform, false);
        pivotRoot.transform.localPosition = go.transform.InverseTransformPoint(center);
        pivotRoot.transform.localRotation = Quaternion.identity;
        pivotRoot.transform.localScale = Vector3.one;

        // Move all visual children into PivotRoot
        Transform[] children = new Transform[go.transform.childCount];
        for (int i = 0; i < children.Length; i++)
            children[i] = go.transform.GetChild(i);

        foreach (Transform child in children)
        {
            if (child != pivotRoot.transform)
                Undo.SetTransformParent(child, pivotRoot.transform, "Move children to PivotRoot");
        }

        // Reset GameObject pivot to origin of wrapper
        Undo.RecordObject(go.transform, "Adjust Pivot");
        go.transform.position = center;

        // Move PivotRoot back to align visually
        pivotRoot.transform.SetParent(go.transform, false);
        pivotRoot.transform.localPosition = Vector3.zero;

        // Apply prefab modification if inside prefab stage
        PrefabUtility.RecordPrefabInstancePropertyModifications(go);

        Selection.activeGameObject = go;
    }
}
