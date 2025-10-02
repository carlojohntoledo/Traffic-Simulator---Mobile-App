using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(ItemData))]
public class ItemDataEditor : Editor
{
    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        // General Info
        EditorGUILayout.PropertyField(serializedObject.FindProperty("type"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("itemName"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("description"));

        // Preview Image
        var previewProp = serializedObject.FindProperty("previewImage");
        EditorGUILayout.PropertyField(previewProp);

        if (previewProp.objectReferenceValue != null)
        {
            Texture2D tex = AssetPreview.GetAssetPreview(previewProp.objectReferenceValue);
            if (tex != null)
                GUILayout.Label(tex, GUILayout.Height(80), GUILayout.Width(80));
        }

        // Prefab Reference
        EditorGUILayout.PropertyField(serializedObject.FindProperty("itemPrefab"));

        // Switch based on ItemType
        ItemType type = (ItemType)serializedObject.FindProperty("type").enumValueIndex;

        switch (type)
        {
            case ItemType.Roads:
                EditorGUILayout.PropertyField(serializedObject.FindProperty("roadLength"));
                break;

            case ItemType.Spawner:
                EditorGUILayout.PropertyField(serializedObject.FindProperty("spawnerType"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("maxSpawnCount"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("spawnInterval"));
                break;

            case ItemType.Pedestrians:
                EditorGUILayout.PropertyField(serializedObject.FindProperty("pedestrianDefaultSpeed"));
                break;

            case ItemType.Rules:
                EditorGUILayout.PropertyField(serializedObject.FindProperty("trafficRuleType"));
                TrafficRuleType ruleType = (TrafficRuleType)serializedObject.FindProperty("trafficRuleType").enumValueIndex;

                if (ruleType == TrafficRuleType.TrafficSign)
                {
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("signPriority"));
                }
                else if (ruleType == TrafficRuleType.TrafficLight)
                {
                    EditorGUILayout.LabelField("Traffic Light Settings", EditorStyles.boldLabel);
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("stopTime"));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("slowdownTime"));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("goTime"));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("hazardMode"));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("flashingMode"));
                }
                break;

            case ItemType.Vehicles:
                EditorGUILayout.PropertyField(serializedObject.FindProperty("vehicleType"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("vehicleDefaultSpeed"));
                break;
        }

        serializedObject.ApplyModifiedProperties();
    }
}
