using System;
using System.Collections.Generic;
using Buildings;
using GeneralSystems;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace InGameUi
{
    public class BuildingPanel : MonoBehaviour
    {
        [SerializeField] private BuildingManager _buildingManager;
        [SerializeField] private WorkersManager _workersManager;

        [SerializeField] private TextMeshProUGUI _buildingName;
        [SerializeField] private TextMeshProUGUI _numberOfWorkers;
        [SerializeField] private GameObject buildingEntryPrefab;
        [SerializeField] private GameObject tierPanelPrefab;
        [SerializeField] private GameObject _endBuildingButton;
        [SerializeField] private Transform contentTransform;

        private BuildingData _currentBuildingData;
        private int _currentBuildingLevel;
        private List<GameObject> _runtimeBuildingsUiToDestroy;
        private List<BuildingData> _buildingsOnInPanelQueue;
        private Dictionary<BuildingData, SingleBuildingUi> _createdUiElements;

        [HideInInspector] public Dictionary<BuildingData, bool> BuildingsToShow;
        public event Action OnBackToWorkersPanel;

        private void Start()
        {
            //_buildingManager.OnBuildingClicked += ActivateOnClick;
            _runtimeBuildingsUiToDestroy = new List<GameObject>();
            _buildingsOnInPanelQueue = new List<BuildingData>();
            BuildingsToShow = new Dictionary<BuildingData, bool>();
            _createdUiElements = new Dictionary<BuildingData, SingleBuildingUi>();
            gameObject.SetActive(false);
        }

        private void ClosePanel()
        {
            foreach (var createdUiElement in _runtimeBuildingsUiToDestroy)
            {
                Destroy(createdUiElement);
            }

            CameraController.IsUiOpen = false;
            GameplayHud.BlockHud = false;
            
            foreach (var buildingData in _buildingsOnInPanelQueue)
            {
                BuildingsToShow.TryAdd(buildingData, true);
            }

            _runtimeBuildingsUiToDestroy.Clear();
            _createdUiElements.Clear();
            gameObject.SetActive(false);
        }

        // private void ActivateOnClick(BuildingData p_specificBuilding, int p_level)
        // {
        //     _currentBuildingData = p_specificBuilding;
        //     _currentBuildingLevel = p_level;
        //
        //     HandleView();
        // }

        public void BackToWorkerTab()
        {
            ClosePanel();
            OnBackToWorkersPanel?.Invoke();
        }

        public void HandleView(bool p_fromWorkerPanel = false)
        {
            gameObject.SetActive(true);
            CameraController.IsUiOpen = true;
            GameplayHud.BlockHud = true;

            _buildingName.text = "Start Building";
            
            RefreshWorkersAmount();
            UpdateWorkersText();
            
            _endBuildingButton.SetActive(p_fromWorkerPanel);

            CreateBuildings();
        }

        private void CreateBuildings()
        {
            Dictionary<int, List<BuildingData>> buildingsByTier = new Dictionary<int, List<BuildingData>>();

            foreach (BuildingData building in _buildingManager.AllBuildingsDatabase.allBuildings)
            {
                if (!buildingsByTier.ContainsKey(building.BaseCottageLevelNeeded))
                {
                    buildingsByTier[building.BaseCottageLevelNeeded] = new List<BuildingData>();
                }

                buildingsByTier[building.BaseCottageLevelNeeded].Add(building);
            }

            HandleBuildingsCreation(buildingsByTier);
        }

        private void HandleBuildingsCreation(Dictionary<int, List<BuildingData>> p_buildingsByTier)
        {
            foreach (int tier in p_buildingsByTier.Keys)
            {
                var newTierPanel = Instantiate(tierPanelPrefab, contentTransform);
                newTierPanel.GetComponentInChildren<TextMeshProUGUI>().text = "Tier: " + tier;
                _runtimeBuildingsUiToDestroy.Add(newTierPanel);

                foreach (BuildingData buildingData in p_buildingsByTier[tier])
                {
                    GameObject newBuildingUi = Instantiate(buildingEntryPrefab, contentTransform);
                    SingleBuildingUi script = newBuildingUi.GetComponent<SingleBuildingUi>();
                    _createdUiElements.Add(buildingData, script);
                    
                    script.BuildingName.GetComponent<TextMeshProUGUI>().text = buildingData.Type.ToString();
                    script.BuildingInfo.GetComponent<TextMeshProUGUI>().text = "";

                    var builtBuilding = _buildingManager.GetSpecificBuilding(buildingData);

                    if (builtBuilding != null) // is builded or building is in progress
                    {
                        script.BuildingIcon.GetComponent<Image>().sprite = buildingData.PerLevelData[builtBuilding.CurrentLevel].Icon;

                        if (!builtBuilding.IsBeeingUpgradedOrBuilded &&
                            !_buildingsOnInPanelQueue.Contains(builtBuilding.BuildingMainData)) // open way to upgrade
                        {
                            HandleCurrentBuildingCreation(script, builtBuilding);
                        }
                        else if (_buildingsOnInPanelQueue.Contains(builtBuilding.BuildingMainData))
                        {
                            HandleBuildingInProgressBuildingCreation(script, buildingData, builtBuilding.CurrentLevel);
                        }
                    }
                    else // is completly not builded - even in building stage
                    {
                        script.BuildingIcon.GetComponent<Image>().sprite = buildingData.PerLevelData[0].Icon;
                        
                        if (_buildingsOnInPanelQueue.Contains(buildingData))
                        {
                            HandleBuildingInProgressBuildingCreation(script, buildingData, 0);
                        }
                        else
                        {
                            HandleCompletelyNewBuildingCreation(script, buildingData);
                        }
                    }

                    _runtimeBuildingsUiToDestroy.Add(newBuildingUi);
                }
            }
        }

        private void HandleCurrentBuildingCreation(SingleBuildingUi p_uiScript, Building p_building)
        {
            var nextLevel = p_building.CurrentLevel;
            nextLevel++;

            p_uiScript.LevelInfo.GetComponent<TextMeshProUGUI>().text =
                $"{p_building.CurrentLevel} >> {nextLevel}";

            if (_buildingManager.CanUpgradeBuilding(p_building))
            {
                p_uiScript.CreateOrUpgradeBuilding.interactable = true;
                p_uiScript.CreateOrUpgradeBuilding.image.color = Color.green;

                p_uiScript.CreateOrUpgradeBuilding.onClick.AddListener(() =>
                    AssigningWorkerHandler(p_building.BuildingMainData, p_building.CurrentLevel,
                        p_uiScript, true)); // unabling usual working for defense/resources + kicking out worker
            }
            else
            {
                p_uiScript.CreateOrUpgradeBuilding.interactable = false;
                p_uiScript.CreateOrUpgradeBuilding.image.color = Color.red;
            }

            UpdateRequirementsText(p_uiScript, p_building.BuildingMainData.PerLevelData[p_building.CurrentLevel].Requirements);
        }

        private void HandleBuildingInProgressBuildingCreation(SingleBuildingUi p_uiScript, BuildingData p_buildingData, int p_level)
        {
            p_uiScript.BuildingInfo.GetComponent<TextMeshProUGUI>().text = "In Progress";
            p_uiScript.LevelInfo.GetComponent<TextMeshProUGUI>().text = "Cancel";

            p_uiScript.CreateOrUpgradeBuilding.interactable = true;
            p_uiScript.CreateOrUpgradeBuilding.image.color = Color.yellow;  
                            
            p_uiScript.CreateOrUpgradeBuilding.onClick.AddListener(() =>
                AssigningWorkerHandler(p_buildingData, p_level, p_uiScript, false));
        }

        private void HandleCompletelyNewBuildingCreation(SingleBuildingUi p_uiScript, BuildingData p_buildingData)
        {
            p_uiScript.LevelInfo.GetComponent<TextMeshProUGUI>().text = "Build";
            UpdateRequirementsText(p_uiScript, p_buildingData.PerLevelData[0].Requirements);

            if (_buildingManager.CanBuildBuilding(p_buildingData))
            {
                p_uiScript.CreateOrUpgradeBuilding.image.color = Color.green;
                p_uiScript.CreateOrUpgradeBuilding.interactable = true;
                p_uiScript.CreateOrUpgradeBuilding.onClick.AddListener(() =>
                    AssigningWorkerHandler(p_buildingData, 0, p_uiScript, true));
            }
            else
            {
                p_uiScript.CreateOrUpgradeBuilding.image.color = Color.red;
                p_uiScript.CreateOrUpgradeBuilding.interactable = false;
            }  
        }

        private void AssigningWorkerHandler(BuildingData p_buildingData, int p_buildingLevel,
            SingleBuildingUi p_uiScript, bool p_assign)  // REFRESHING AFTER CLICKING
        {
            p_uiScript.CreateOrUpgradeBuilding.onClick.RemoveAllListeners();

            if (p_assign)
            {
                p_uiScript.CreateOrUpgradeBuilding.image.color = Color.cyan; // in progress
                p_uiScript.CreateOrUpgradeBuilding.interactable = true;

                p_uiScript.BuildingInfo.GetComponent<TextMeshProUGUI>().text = "Started Working";
                p_uiScript.LevelInfo.GetComponent<TextMeshProUGUI>().text = "Cancel";

                p_uiScript.CreateOrUpgradeBuilding.onClick.AddListener(() => AssigningWorkerHandler(p_buildingData,
                    p_buildingLevel, p_uiScript, false));
            }
            else
            {
                p_uiScript.CreateOrUpgradeBuilding.image.color = Color.green;
                p_uiScript.CreateOrUpgradeBuilding.interactable = _workersManager.BaseWorkersAmounts > 0;
                var building = _buildingManager.GetSpecificBuilding(p_buildingData);

                if (building == null)
                {
                    UpdateRequirementsText(p_uiScript, p_buildingData.PerLevelData[0].Requirements);
                    p_uiScript.LevelInfo.GetComponent<TextMeshProUGUI>().text = "Build";
                }
                else
                {
                    var nextLevel = building.CurrentLevel;
                    nextLevel++;

                    p_uiScript.LevelInfo.GetComponent<TextMeshProUGUI>().text = $"{building.CurrentLevel} >> {nextLevel}";
                    UpdateRequirementsText(p_uiScript, p_buildingData.PerLevelData[building.CurrentLevel].Requirements);
                }
                
                if (BuildingsToShow.ContainsKey(p_buildingData))
                {
                    BuildingsToShow.Remove(p_buildingData);
                }

                p_uiScript.CreateOrUpgradeBuilding.onClick.AddListener(() => AssigningWorkerHandler(p_buildingData,
                    p_buildingLevel, p_uiScript, true));
            }

            OnButtonClicked(p_buildingData, p_buildingLevel, p_assign);
        }

        private void OnButtonClicked(BuildingData p_buildingData, int p_buildingLevel, bool p_assign)
        {
            if (p_assign)
            {
                _workersManager.WorkersInBuilding++;
                _buildingsOnInPanelQueue.Add(p_buildingData);
                _buildingManager.RemoveResourcePoints(p_buildingData, p_buildingLevel);
            }
            else
            {
                _workersManager.WorkersInBuilding--;
                _buildingsOnInPanelQueue.Remove(p_buildingData);
                _buildingManager.AddResourcePoints(p_buildingData, p_buildingLevel);
            }

            UpdateWorkersText();
            UpdateOtherButtonsUi();
        }

        private void UpdateOtherButtonsUi()
        {
            foreach (var element in _createdUiElements)
            {
                var building = _buildingManager.GetSpecificBuilding(element.Key);
                
                if (building == null)
                {
                    if (_buildingsOnInPanelQueue.Contains(element.Key))
                    {
                        element.Value.BuildingInfo.GetComponent<TextMeshProUGUI>().text = "In Progress";
                        element.Value.LevelInfo.GetComponent<TextMeshProUGUI>().text = "Cancel";

                        element.Value.CreateOrUpgradeBuilding.interactable = true;
                        element.Value.CreateOrUpgradeBuilding.image.color = Color.yellow;  
                    }
                    else
                    {
                        if (_buildingManager.CanBuildBuilding(element.Key))
                        {
                            element.Value.CreateOrUpgradeBuilding.image.color = Color.green;
                            element.Value.CreateOrUpgradeBuilding.interactable = true;
                        }
                        else
                        {
                            element.Value.CreateOrUpgradeBuilding.image.color = Color.red;
                            element.Value.CreateOrUpgradeBuilding.interactable = false;
                        }  
                    } 
                }
                else
                {
                    element.Value.BuildingIcon.GetComponent<Image>().sprite = element.Key.PerLevelData[building.CurrentLevel].Icon;

                    if (!building.IsBeeingUpgradedOrBuilded &&
                        !_buildingsOnInPanelQueue.Contains(building.BuildingMainData)) // open way to upgrade
                    {
                        if (_buildingManager.CanUpgradeBuilding(building))
                        {
                            element.Value.CreateOrUpgradeBuilding.interactable = true;
                            element.Value.CreateOrUpgradeBuilding.image.color = Color.green;
                        }
                        else
                        {
                            element.Value.CreateOrUpgradeBuilding.interactable = false;
                            element.Value.CreateOrUpgradeBuilding.image.color = Color.red;
                        }
                    }
                    else if (_buildingsOnInPanelQueue.Contains(building.BuildingMainData))
                    {
                        element.Value.CreateOrUpgradeBuilding.interactable = true;
                        element.Value.CreateOrUpgradeBuilding.image.color = Color.yellow;  
                    }
                }
            }
        }

        public void ConfirmWorkersAssigment()
        {
            foreach (var buildingData in _buildingsOnInPanelQueue)
            {
                _buildingManager.PutBuildingOnQueue(buildingData);
            }

            _buildingsOnInPanelQueue.Clear();
        }

        private void UpdateWorkersText()
        {
            _numberOfWorkers.text =
                $"Workers: {_workersManager.BaseWorkersAmounts.ToString()}/{_workersManager.WorkersInBuilding}";
        }

        private void UpdateRequirementsText(SingleBuildingUi p_script, Requirements p_requirements)
        {
            p_script.BuildingInfo.GetComponent<TextMeshProUGUI>().text =
                $"Resource Points: {p_requirements.ResourcePoints}\n" +
                $"Days To Complete: {p_requirements.DaysToComplete}\n";
        }

        public void RefreshWorkersAmount()
        {
            _workersManager.WorkersInBuilding = 0;
            
            foreach (var building in _buildingManager.CurrentBuildings)
            {
                if (building.HaveWorker && building.IsBeeingUpgradedOrBuilded)
                {
                    _workersManager.WorkersInBuilding++;
                }
            }

            _workersManager.WorkersInBuilding += _buildingsOnInPanelQueue.Count;
        }
    }
}