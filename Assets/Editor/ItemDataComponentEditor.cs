using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(ItemDataComponent))]
public class ItemDataComponentEditor : Editor
{
    private SerializedProperty itemName;
    private SerializedProperty itemModelType;
    private SerializedProperty itemImagePreview;
    private SerializedProperty itemModelPrefab;
    private SerializedProperty itemDescription;

    // Road
    private SerializedProperty roadType;
    private SerializedProperty roadLength;

    // Vehicle
    private SerializedProperty carType;
    private SerializedProperty vehicleSpeed;
    private SerializedProperty vehicleMaxSpeed;
    private SerializedProperty vehicleMinSpeed;

    // Pedestrian
    private SerializedProperty pedestrianType;
    private SerializedProperty pedestrianSpeed;

    // Spawner
    private SerializedProperty spawnType;
    private SerializedProperty maxSpawn;
    private SerializedProperty spawnInterval;
    private SerializedProperty spawnModelPrefabs;
    private SerializedProperty spawnMode;

    // Signage
    private SerializedProperty rulePriority;

    // Traffic Light
    private SerializedProperty startLight;
    private SerializedProperty goTime;
    private SerializedProperty slowTime;
    private SerializedProperty stopTime;
    private SerializedProperty hazardMode;

    private void OnEnable()
    {
        // Item Details
        itemName = serializedObject.FindProperty("itemName");
        itemModelType = serializedObject.FindProperty("itemModelType");
        itemImagePreview = serializedObject.FindProperty("itemImagePreview");
        itemModelPrefab = serializedObject.FindProperty("itemModelPrefab");
        itemDescription = serializedObject.FindProperty("itemDescription");

        // Road
        roadType = serializedObject.FindProperty("roadType");
        roadLength = serializedObject.FindProperty("roadLength");

        // Vehicle
        carType = serializedObject.FindProperty("carType");
        vehicleSpeed = serializedObject.FindProperty("vehicleSpeed");
        vehicleMaxSpeed = serializedObject.FindProperty("vehicleMaxSpeed");
        vehicleMinSpeed = serializedObject.FindProperty("vehicleMinSpeed");

        // Pedestrian
        pedestrianType = serializedObject.FindProperty("pedestrianType");
        pedestrianSpeed = serializedObject.FindProperty("pedestrianSpeed");

        // Spawner
        spawnType = serializedObject.FindProperty("spawnType");
        maxSpawn = serializedObject.FindProperty("maxSpawn");
        spawnInterval = serializedObject.FindProperty("spawnInterval");
        spawnModelPrefabs = serializedObject.FindProperty("spawnModelPrefabs");
        spawnMode = serializedObject.FindProperty("spawnMode");

        // Signage
        rulePriority = serializedObject.FindProperty("rulePriority");

        // Traffic Light
        startLight = serializedObject.FindProperty("startLight");
        goTime = serializedObject.FindProperty("goTime");
        slowTime = serializedObject.FindProperty("slowTime");
        stopTime = serializedObject.FindProperty("stopTime");
        hazardMode = serializedObject.FindProperty("hazardMode");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUILayout.LabelField("Item Details", EditorStyles.boldLabel);
        using (new EditorGUI.DisabledScope(Application.isPlaying))
        {
            EditorGUILayout.PropertyField(itemName);
            EditorGUILayout.PropertyField(itemModelType);
            EditorGUILayout.PropertyField(itemImagePreview);
            EditorGUILayout.PropertyField(itemModelPrefab);
            EditorGUILayout.PropertyField(itemDescription);
        }

        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("Item Components", EditorStyles.boldLabel);
        EditorGUILayout.Space(5);

        ItemDataComponent data = (ItemDataComponent)target;

        switch (data.itemModelType)
        {
            case ItemModelType.Road:
                EditorGUILayout.PropertyField(roadType, new GUIContent("Road Type"));
                EditorGUILayout.PropertyField(roadLength, new GUIContent("Road Length"));
                break;

            case ItemModelType.Vehicle:
                EditorGUILayout.PropertyField(carType);
                EditorGUILayout.PropertyField(vehicleSpeed);
                EditorGUILayout.PropertyField(vehicleMaxSpeed);
                EditorGUILayout.PropertyField(vehicleMinSpeed);
                break;

            case ItemModelType.Pedestrian:
                EditorGUILayout.PropertyField(pedestrianType);
                EditorGUILayout.PropertyField(pedestrianSpeed);
                break;

            case ItemModelType.Spawner:
                EditorGUILayout.PropertyField(spawnType);
                EditorGUILayout.PropertyField(maxSpawn);
                EditorGUILayout.PropertyField(spawnInterval);
                EditorGUILayout.PropertyField(spawnModelPrefabs, true);
                EditorGUILayout.PropertyField(spawnMode);
                break;

            case ItemModelType.Signage:
                EditorGUILayout.IntSlider(rulePriority, 0, 1, new GUIContent("Rule Priority"));
                break;

            case ItemModelType.TrafficLight:
                EditorGUILayout.PropertyField(startLight);
                EditorGUILayout.PropertyField(goTime);
                EditorGUILayout.PropertyField(slowTime);
                EditorGUILayout.PropertyField(stopTime);
                EditorGUILayout.PropertyField(hazardMode);
                break;
        }

        serializedObject.ApplyModifiedProperties();

        // Optional live preview
        if (data.itemImagePreview)
        {
            EditorGUILayout.Space();
            GUILayout.Label("Item Preview", EditorStyles.boldLabel);
            GUILayout.Label(AssetPreview.GetAssetPreview(data.itemImagePreview.texture), GUILayout.Height(100));
        }
    }
}
