using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ConfirmBuildPanel : MonoBehaviour
{
    public static ConfirmBuildPanel Instance { get; private set; }

    [Header("Main Panel References")]
    public GameObject panel;
    public TMP_Text itemTypeText;
    public TMP_Text itemNameText;
    public Image itemImage;

    [Header("Description")]
    public GameObject descriptionPanel;
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
    private BuildItem editingItem;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    void Start()
    {
        panel.SetActive(false);
        descriptionPanel.SetActive(false);

        cancelButton.onClick.AddListener(() =>
        {
            panel.SetActive(false);
            editingItem = null;
        });

        confirmButton.onClick.AddListener(OnConfirmBuild);

        if (descriptionToggleButton != null)
            descriptionToggleButton.onClick.AddListener(ToggleDescription);
    }

    private void ToggleDescription()
    {
        descVisible = !descVisible;
        descriptionPanel.SetActive(descVisible);
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

    public void Show(ItemData data)
    {
        editingItem = null;
        currentData = data;
        panel.SetActive(true);

        confirmButton.onClick.RemoveAllListeners();
        confirmButton.onClick.AddListener(OnConfirmBuild);

        itemTypeText.text = data.type.ToString();
        itemNameText.text = data.itemName;
        itemImage.sprite = data.previewImage;
        descriptionText.text = data.description;

        HideAllEditPanels();

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

    private void OnConfirmBuild()
    {
        if (currentData == null) return;

        if (currentData.type == ItemType.Roads)
        {
            float inputLength;
            if (!float.TryParse(lengthInput.text, out inputLength))
                inputLength = currentData.roadLength;

            BuildRoad(currentData, inputLength);
        }
        else
        {
            BuildGenericItem(currentData);
        }

        panel.SetActive(false);
    }

    private void BuildRoad(ItemData data, float inputLength)
    {
        GameObject roadParent = new GameObject(data.itemName + "_RoadGroup");

        Camera cam = Camera.main;
        Vector3 spawnPos = cam.transform.position + cam.transform.forward * 10f;

        if (Physics.Raycast(spawnPos + Vector3.up * 50f, Vector3.down, out RaycastHit hit, 200f, LayerMask.GetMask("Ground")))
            spawnPos = hit.point;

        roadParent.transform.position = spawnPos;

        int segmentCount = Mathf.Max(1, Mathf.RoundToInt(inputLength));

        BuildItem parentBuild = roadParent.AddComponent<BuildItem>();
        parentBuild.Initialize(data);
        parentBuild.length = segmentCount;

        for (int i = 0; i < segmentCount; i++)
        {
            Vector3 offset = Vector3.forward * i * 5f;
            GameObject seg = Instantiate(data.itemPrefab, spawnPos + offset, Quaternion.identity, roadParent.transform);

            if (seg.GetComponent<Collider>() == null)
                seg.AddComponent<BoxCollider>();

            SelectableItem si = seg.AddComponent<SelectableItem>();
            si.parentBuild = parentBuild; // now public

            parentBuild.roadSegments.Add(seg);
        }

        Debug.Log($"Road built: {roadParent.name} with {parentBuild.roadSegments.Count} segments");
    }

    private void BuildGenericItem(ItemData data)
    {
        GameObject instance = Instantiate(data.itemPrefab, Vector3.zero, Quaternion.identity);

        if (instance.GetComponent<Collider>() == null)
            instance.AddComponent<BoxCollider>();

        BuildItem bi = instance.AddComponent<BuildItem>();
        bi.Initialize(data);
    }

    public void EditItem(BuildItem buildItem)
    {
        if (buildItem == null) return;

        editingItem = buildItem;
        currentData = buildItem.data;
        panel.SetActive(true);

        HideAllEditPanels();

        switch (currentData.type)
        {
            case ItemType.Roads:
                roadPanel.SetActive(true);
                lengthInput.text = buildItem.length.ToString();
                break;

            case ItemType.Vehicles:
                vehiclePanel.SetActive(true);
                vehicleSpeedInput.text = buildItem.vehicleSpeed.ToString();
                break;

            case ItemType.Pedestrians:
                pedestrianPanel.SetActive(true);
                pedestrianSpeedInput.text = buildItem.pedestrianSpeed.ToString();
                break;

            case ItemType.Rules:
                if (currentData.trafficRuleType == TrafficRuleType.TrafficLight)
                {
                    ruleTrafficLightStopPanel.SetActive(true);
                    stopTimeInput.text = buildItem.stopTime.ToString();

                    ruleTrafficLightSlowdownPanel.SetActive(true);
                    slowdownTimeInput.text = buildItem.slowdownTime.ToString();

                    ruleTrafficLightGoPanel.SetActive(true);
                    goTimeInput.text = buildItem.goTime.ToString();

                    ruleTrafficLightHazardPanel.SetActive(true);
                    hazardToggle.isOn = buildItem.hazardMode;

                    ruleTrafficLightFlashingPanel.SetActive(true);
                    flashingToggle.isOn = buildItem.flashingMode;
                }
                else if (currentData.trafficRuleType == TrafficRuleType.TrafficSign)
                {
                    ruleTrafficSignPanel.SetActive(true);
                    signPriorityInput.text = buildItem.signPriority.ToString();
                }
                break;

            case ItemType.Spawner:
                spawnerMaxPanel.SetActive(true);
                maxSpawnInput.text = buildItem.maxSpawnCount.ToString();
                spawnerIntervalPanel.SetActive(true);
                spawnIntervalInput.text = buildItem.spawnInterval.ToString();
                break;
        }

        confirmButton.onClick.RemoveAllListeners();
        confirmButton.onClick.AddListener(() =>
        {
            ApplyEdits(buildItem);
            panel.SetActive(false);
            editingItem = null;
        });
    }

    private void ApplyEdits(BuildItem buildItem)
    {
        if (buildItem == null) return;

        switch (buildItem.data.type)
        {
            case ItemType.Roads:
                float newLengthFloat;
                if (!float.TryParse(lengthInput.text, out newLengthFloat))
                    newLengthFloat = buildItem.length;

                int newLength = Mathf.Max(1, Mathf.RoundToInt(newLengthFloat));
                int currentSegments = buildItem.roadSegments.Count;

                if (newLength > currentSegments)
                {
                    for (int i = currentSegments; i < newLength; i++)
                    {
                        Vector3 offset = Vector3.forward * i * 5f;
                        GameObject seg = Instantiate(buildItem.data.itemPrefab,
                            buildItem.roadSegments[0].transform.position + offset,
                            Quaternion.identity,
                            buildItem.transform);

                        if (seg.GetComponent<Collider>() == null)
                            seg.AddComponent<BoxCollider>();

                        SelectableItem si = seg.AddComponent<SelectableItem>();
                        si.parentBuild = buildItem;

                        buildItem.roadSegments.Add(seg);
                    }
                }
                else if (newLength < currentSegments)
                {
                    for (int i = currentSegments - 1; i >= newLength; i--)
                    {
                        GameObject seg = buildItem.roadSegments[i];
                        buildItem.roadSegments.RemoveAt(i);
                        Destroy(seg);
                    }
                }

                buildItem.length = newLength;
                break;

            case ItemType.Vehicles:
                float.TryParse(vehicleSpeedInput.text, out buildItem.vehicleSpeed);
                break;

            case ItemType.Pedestrians:
                float.TryParse(pedestrianSpeedInput.text, out buildItem.pedestrianSpeed);
                break;

            case ItemType.Rules:
                if (buildItem.data.trafficRuleType == TrafficRuleType.TrafficLight)
                {
                    float.TryParse(stopTimeInput.text, out buildItem.stopTime);
                    float.TryParse(slowdownTimeInput.text, out buildItem.slowdownTime);
                    float.TryParse(goTimeInput.text, out buildItem.goTime);
                    buildItem.hazardMode = hazardToggle.isOn;
                    buildItem.flashingMode = flashingToggle.isOn;
                }
                else if (buildItem.data.trafficRuleType == TrafficRuleType.TrafficSign)
                {
                    int.TryParse(signPriorityInput.text, out buildItem.signPriority);
                }
                break;

            case ItemType.Spawner:
                int.TryParse(maxSpawnInput.text, out buildItem.maxSpawnCount);
                float.TryParse(spawnIntervalInput.text, out buildItem.spawnInterval);
                break;
        }
    }
}
