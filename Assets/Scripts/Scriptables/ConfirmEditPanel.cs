using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ConfirmEditPanel : MonoBehaviour
{
    [Header("General UI References")]
    public GameObject panelRoot;
    public Image previewImage;
    public TMP_Text itemNameText;
    public TMP_Text typeText;
    public TMP_Text descriptionText;

    [Header("Description Toggle")]
    public Button descriptionToggleButton;
    public GameObject descriptionPanel;

    [Header("Road Fields")]
    public GameObject roadFields;
    public TMP_InputField roadLengthInput;

    [Header("Shared Spawn Fields")]
    public GameObject spawnIntervalPanel;
    public TMP_InputField spawnIntervalInput;

    public GameObject maxSpawnPanel;
    public TMP_InputField maxSpawnCountInput;

    [Header("Car Spawner Fields")]
    public GameObject vehicleSpeedPanel;
    public TMP_InputField vehicleSpeedInput;

    [Header("Pedestrian Spawner Fields")]
    public GameObject pedestrianSpeedPanel;
    public TMP_InputField pedestrianSpeedInput;

    [Header("Rule Fields")]
    public GameObject ruleFields;

    // Sign rule
    public GameObject signPriorityPanel;
    public TMP_InputField signPriorityInput;

    // Traffic light rule
    public GameObject stopTimePanel;
    public TMP_InputField stopTimeInput;
    public GameObject slowdownTimePanel;
    public TMP_InputField slowdownTimeInput;
    public GameObject goTimePanel;
    public TMP_InputField goTimeInput;

    // Common to Traffic light
    public GameObject hazardPanel;
    public Toggle hazardToggle;
    public GameObject flashingPanel;
    public Toggle flashingToggle;

    [Header("Buttons")]
    public Button cancelButton;
    public Button confirmButton;

    private ItemData currentData;
    private ItemData originalCopy;
    private GameObject currentInstance; // 👈 the currently selected prefab instance

    private void Awake()
    {
        cancelButton.onClick.AddListener(CancelEdit);
        confirmButton.onClick.AddListener(ConfirmEdit);
        descriptionToggleButton.onClick.AddListener(ToggleDescriptionPanel);

        HideAllPanels();
        descriptionPanel.SetActive(false);
        panelRoot.SetActive(false);
    }

    // ============================================================
    // OPEN PANEL
    // ============================================================

    public void Open(ItemData data, GameObject instance)
    {
        if (data == null)
        {
            Debug.LogWarning("[ConfirmEditPanel] Tried to open with null data!");
            return;
        }

        currentData = data;
        currentInstance = instance;

        // Backup for cancel
        originalCopy = ScriptableObject.CreateInstance<ItemData>();
        JsonUtility.FromJsonOverwrite(JsonUtility.ToJson(data), originalCopy);

        panelRoot.SetActive(true);
        PopulateGeneralInfo();
        ShowRelevantPanels();
        PopulateEditableValues();
    }

    private void PopulateGeneralInfo()
    {
        itemNameText.text = currentData.itemName;
        typeText.text = currentData.type.ToString();
        descriptionText.text = currentData.description;
        previewImage.sprite = currentData.previewImage;
    }

    private void HideAllPanels()
    {
        roadFields.SetActive(false);

        spawnIntervalPanel.SetActive(false);
        maxSpawnPanel.SetActive(false);
        vehicleSpeedPanel.SetActive(false);
        pedestrianSpeedPanel.SetActive(false);

        ruleFields.SetActive(false);
        signPriorityPanel.SetActive(false);
        stopTimePanel.SetActive(false);
        slowdownTimePanel.SetActive(false);
        goTimePanel.SetActive(false);
        hazardPanel.SetActive(false);
        flashingPanel.SetActive(false);
    }

    private void ShowRelevantPanels()
    {
        HideAllPanels();

        switch (currentData.type)
        {
            case ItemType.Roads:
                roadFields.SetActive(true);
                break;

            case ItemType.Spawner:
                spawnIntervalPanel.SetActive(true);
                maxSpawnPanel.SetActive(true);

                if (currentData.spawnerType == SpawnerType.Car)
                    vehicleSpeedPanel.SetActive(true);
                else if (currentData.spawnerType == SpawnerType.Pedestrian)
                    pedestrianSpeedPanel.SetActive(true);
                break;

            case ItemType.Rules:
                ruleFields.SetActive(true);
                if (currentData.trafficRuleType == TrafficRuleType.TrafficLight)
                {
                    stopTimePanel.SetActive(true);
                    slowdownTimePanel.SetActive(true);
                    goTimePanel.SetActive(true);
                    hazardPanel.SetActive(true);
                    flashingPanel.SetActive(true);
                }
                else if (currentData.trafficRuleType == TrafficRuleType.TrafficSign)
                {
                    signPriorityPanel.SetActive(true);
                }
                break;
        }
    }

    private void PopulateEditableValues()
    {
        switch (currentData.type)
        {
            case ItemType.Roads:
                roadLengthInput.text = currentData.roadLength.ToString("F2");
                break;

            case ItemType.Spawner:
                spawnIntervalInput.text = currentData.spawnInterval.ToString("F2");
                maxSpawnCountInput.text = currentData.maxSpawnCount.ToString();

                if (currentData.spawnerType == SpawnerType.Car)
                    vehicleSpeedInput.text = currentData.vehicleDefaultSpeed.ToString("F2");
                else if (currentData.spawnerType == SpawnerType.Pedestrian)
                    pedestrianSpeedInput.text = currentData.pedestrianDefaultSpeed.ToString("F2");
                break;

            case ItemType.Rules:
                if (currentData.trafficRuleType == TrafficRuleType.TrafficLight)
                {
                    stopTimeInput.text = currentData.stopTime.ToString("F2");
                    slowdownTimeInput.text = currentData.slowdownTime.ToString("F2");
                    goTimeInput.text = currentData.goTime.ToString("F2");
                    hazardToggle.isOn = currentData.hazardMode;
                    flashingToggle.isOn = currentData.flashingMode;
                }
                else if (currentData.trafficRuleType == TrafficRuleType.TrafficSign)
                {
                    signPriorityInput.text = currentData.signPriority.ToString();
                }
                break;
        }
    }

    // ============================================================
    // CANCEL / CONFIRM
    // ============================================================

    private void CancelEdit()
    {
        if (currentData != null && originalCopy != null)
        {
            JsonUtility.FromJsonOverwrite(JsonUtility.ToJson(originalCopy), currentData);
            Debug.Log($"[ConfirmEditPanel] Cancelled edits for {currentData.itemName}");
        }

        ClosePanel();
    }

    private void ConfirmEdit()
    {
        if (currentData == null) return;

        float Clamp(float v, float min, float max) => Mathf.Clamp(v, min, max);

        switch (currentData.type)
        {
            case ItemType.Roads:
                if (float.TryParse(roadLengthInput.text, out float rl))
                    currentData.roadLength = Clamp(rl, 1f, 50f);
                break;

            case ItemType.Spawner:
                if (float.TryParse(spawnIntervalInput.text, out float si))
                    currentData.spawnInterval = Clamp(si, 0.1f, 50f);
                if (int.TryParse(maxSpawnCountInput.text, out int maxS))
                    currentData.maxSpawnCount = Mathf.Clamp(maxS, 1, 100);

                if (currentData.spawnerType == SpawnerType.Car)
                {
                    if (float.TryParse(vehicleSpeedInput.text, out float vs))
                        currentData.vehicleDefaultSpeed = Clamp(vs, 1f, 100f);
                }
                else if (currentData.spawnerType == SpawnerType.Pedestrian)
                {
                    if (float.TryParse(pedestrianSpeedInput.text, out float ps))
                        currentData.pedestrianDefaultSpeed = Clamp(ps, 0.1f, 10f);
                }
                break;

            case ItemType.Rules:
                if (currentData.trafficRuleType == TrafficRuleType.TrafficLight)
                {
                    if (float.TryParse(stopTimeInput.text, out float st))
                        currentData.stopTime = Clamp(st, 1f, 50f);
                    if (float.TryParse(slowdownTimeInput.text, out float sl))
                        currentData.slowdownTime = Clamp(sl, 1f, 50f);
                    if (float.TryParse(goTimeInput.text, out float gt))
                        currentData.goTime = Clamp(gt, 1f, 50f);

                    currentData.hazardMode = hazardToggle.isOn;
                    currentData.flashingMode = flashingToggle.isOn;
                }
                else if (currentData.trafficRuleType == TrafficRuleType.TrafficSign)
                {
                    if (int.TryParse(signPriorityInput.text, out int sp))
                        currentData.signPriority = Mathf.Clamp(sp, 1, 50);
                }
                break;
        }

        Debug.Log($"[ConfirmEditPanel] Confirmed edits for {currentData.itemName}");

        // ✅ Apply updated data instantly to the selected object
        if (currentInstance != null)
        {
            // For Road
            EditRoadItem editRoad = currentInstance.GetComponent<EditRoadItem>();
            if (editRoad != null)
            {
                editRoad.ApplyEditChanges(currentData);
                ClosePanel();
                return;
            }

            // ✅ For Spawner
            EditSpawnerItem editSpawner = currentInstance.GetComponent<EditSpawnerItem>();
            if (editSpawner != null)
            {
                editSpawner.ApplyToSpawner();   // Apply runtime values
                editSpawner.SaveBackToData();   // Save to ItemData
                ClosePanel();
                Debug.Log($"[ConfirmEditPanel] Applied Spawner edits → Interval={currentData.spawnInterval}, Max={currentData.maxSpawnCount}");
                return;
            }
        }

        ClosePanel();
    }

    private void ClosePanel() => panelRoot.SetActive(false);

    private void ToggleDescriptionPanel()
    {
        if (descriptionPanel != null)
            descriptionPanel.SetActive(!descriptionPanel.activeSelf);
    }
}
