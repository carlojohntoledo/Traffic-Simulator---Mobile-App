using UnityEngine;

public class ItemBuilder : MonoBehaviour
{
    public ItemData itemToBuild;

    public void Build()
    {
        if (itemToBuild == null || itemToBuild.itemPrefab == null)
        {
            Debug.LogWarning("Missing item or prefab!");
            return;
        }

        // Spawn prefab
        GameObject go = Instantiate(itemToBuild.itemPrefab, Vector3.zero, Quaternion.identity);

        // Ensure BuildItem exists
        BuildItem buildItem = go.GetComponent<BuildItem>();
        if (buildItem == null)
            buildItem = go.AddComponent<BuildItem>();

        // Initialize it
        buildItem.Initialize(itemToBuild);
    }
}
