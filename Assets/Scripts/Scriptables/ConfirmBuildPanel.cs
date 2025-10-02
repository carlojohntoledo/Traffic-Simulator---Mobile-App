using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ConfirmBuildPanel : MonoBehaviour
{
    [Header("Main Panel References")]
    public GameObject panel;
    public TMP_Text itemTypeText;
    public TMP_Text itemNameText;
    public Image itemImage;

    [Header("Description")]
    public GameObject descriptionPanel;   // hidden by default
    public TMP_Text descriptionText;
    public Button descriptionToggleButton;
    private bool descVisible = false;

    [Header("Grouped Edit Panels")]
    public GameObject roadPanel;
    public TMP_InputField lengthInput;

    public GameObject vehiclePanel;
    public TMP_InputField vehicleSpeedInput;

    public GameObject pedestrianPanel;
    public TMP_InputField pedestrianSpeedInput;

    [Header("Traffic Light Panels")]
    public GameObject ruleTrafficLightStopPanel;
    public TMP_InputField stopTimeInput;

    public GameObject ruleTrafficLightSlowdownPanel;
    public TMP_InputField slowdownTimeInput;

    public GameObject ruleTrafficLightGoPanel;
    public TMP_InputField goTimeInput;

    public GameObject ruleTrafficLightHazardPanel;
    public Toggle hazardToggle;

    public GameObject ruleTrafficLightFlashingPanel;
    public Toggle flashingToggle;

    [Header("Traffic Sign Panel")]
    public GameObject ruleTrafficSignPanel;
    public TMP_InputField signPriorityInput;

    [Header("Spawner Panel")]
    public GameObject spawnerMaxPanel;
    public TMP_InputField maxSpawnInput;
    public GameObject spawnerIntervalPanel;
    public TMP_InputField spawnIntervalInput;

    [Header("Buttons")]
    public Button confirmButton;
    public Button cancelButton;

    private ItemData currentData;

    void Start()
    {
        panel.SetActive(false);
        descriptionPanel.SetActive(false);

        cancelButton.onClick.AddListener(() =>
        {
            panel.SetActive(false);
        });

        confirmButton.onClick.AddListener(OnConfirmBuild);

        if (descriptionToggleButton != null)
            descriptionToggleButton.onClick.AddListener(ToggleDescription);
    }

    /// <summary>
    /// Show confirm build panel for selected item
    /// </summary>
    public void Show(ItemData data)
    {
        currentData = data;
        panel.SetActive(true);

        // Static info
        itemTypeText.text = data.type.ToString();
        itemNameText.text = data.itemName;
        itemImage.sprite = data.previewImage;
        descriptionText.text = data.description;

        // Reset UI
        HideAllEditPanels();

        // Show only relevant inputs
        switch (data.type)
        {
            case ItemType.Roads:
                roadPanel.SetActive(true);
                lengthInput.text = data.roadLength.ToString();
                break;

            case ItemType.Vehicles:
                vehiclePanel.SetActive(true);
                vehicleSpeedInput.text = data.vehicleDefaultSpeed.ToString();
                break;

            case ItemType.Pedestrians:
                pedestrianPanel.SetActive(true);
                pedestrianSpeedInput.text = data.pedestrianDefaultSpeed.ToString();
                break;

            case ItemType.Rules:
                if (data.trafficRuleType == TrafficRuleType.TrafficLight)
                {
                    ruleTrafficLightStopPanel.SetActive(true);
                    stopTimeInput.text = data.stopTime.ToString();

                    ruleTrafficLightSlowdownPanel.SetActive(true);
                    slowdownTimeInput.text = data.slowdownTime.ToString();

                    ruleTrafficLightGoPanel.SetActive(true);
                    goTimeInput.text = data.goTime.ToString();

                    ruleTrafficLightHazardPanel.SetActive(true);
                    hazardToggle.isOn = data.hazardMode;

                    ruleTrafficLightFlashingPanel.SetActive(true);
                    flashingToggle.isOn = data.flashingMode;
                }
                else if (data.trafficRuleType == TrafficRuleType.TrafficSign)
                {
                    ruleTrafficSignPanel.SetActive(true);
                    signPriorityInput.text = data.signPriority.ToString();
                }
                break;

            case ItemType.Spawner:
                spawnerMaxPanel.SetActive(true);
                maxSpawnInput.text = data.maxSpawnCount.ToString();
                spawnerIntervalPanel.SetActive(true);
                spawnIntervalInput.text = data.spawnInterval.ToString();
                break;
        }
    }

    private void HideAllEditPanels()
    {
        roadPanel.SetActive(false);
        vehiclePanel.SetActive(false);
        pedestrianPanel.SetActive(false);

        ruleTrafficLightStopPanel.SetActive(false);
        ruleTrafficLightSlowdownPanel.SetActive(false);
        ruleTrafficLightGoPanel.SetActive(false);
        ruleTrafficLightHazardPanel.SetActive(false);
        ruleTrafficLightFlashingPanel.SetActive(false);

        ruleTrafficSignPanel.SetActive(false);

        spawnerMaxPanel.SetActive(false);
        spawnerIntervalPanel.SetActive(false);
    }

    private void ToggleDescription()
    {
        descVisible = !descVisible;
        descriptionPanel.SetActive(descVisible);
    }

    private void OnConfirmBuild()
    {
        if (currentData == null) return;

        // Spawn prefab now (center world position for now, can change to UI anchor if needed)
        GameObject instance = Instantiate(currentData.itemPrefab, Vector3.zero, Quaternion.identity);
        var buildItem = instance.AddComponent<BuildItem>();
        buildItem.Initialize(currentData);

        // Apply edited values
        switch (currentData.type)
        {
            case ItemType.Roads:
                float.TryParse(lengthInput.text, out buildItem.length);
                break;

            case ItemType.Vehicles:
                float.TryParse(vehicleSpeedInput.text, out buildItem.vehicleSpeed);
                break;

            case ItemType.Pedestrians:
                float.TryParse(pedestrianSpeedInput.text, out buildItem.pedestrianSpeed);
                break;

            case ItemType.Rules:
                if (currentData.trafficRuleType == TrafficRuleType.TrafficLight)
                {
                    float.TryParse(stopTimeInput.text, out buildItem.stopTime);
                    float.TryParse(slowdownTimeInput.text, out buildItem.slowdownTime);
                    float.TryParse(goTimeInput.text, out buildItem.goTime);
                    buildItem.hazardMode = hazardToggle.isOn;
                    buildItem.flashingMode = flashingToggle.isOn;
                }
                else if (currentData.trafficRuleType == TrafficRuleType.TrafficSign)
                {
                    int.TryParse(signPriorityInput.text, out buildItem.signPriority);
                }
                break;

            case ItemType.Spawner:
                int.TryParse(maxSpawnInput.text, out buildItem.maxSpawnCount);
                float.TryParse(spawnIntervalInput.text, out buildItem.spawnInterval);
                break;
        }

        // Close panel
        panel.SetActive(false);
    }
}
