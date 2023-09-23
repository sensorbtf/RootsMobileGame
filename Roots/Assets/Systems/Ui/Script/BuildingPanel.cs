using System;
using System.Collections.Generic;
using System.Linq;
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
        private Dictionary<BuildingData, SingleBuildingRefs> _createdUiElements;
        private Dictionary<Building, bool> _builtOrDamagedBuildings;
        private Dictionary<Building, bool> _influencedBuildings;

        public Dictionary<BuildingData, bool> BuildingsToShow;
        public event Action OnBackToWorkersPanel;

        private void Start()
        {
            _buildingManager.OnBuildingBuilt += HandleBuildEnded;
            _buildingManager.OnBuildingDestroyed += HandleBuildEnded;
            _runtimeBuildingsUiToDestroy = new List<GameObject>();
            _buildingsOnInPanelQueue = new List<BuildingData>();
            _builtOrDamagedBuildings = new Dictionary<Building, bool>();
            _influencedBuildings = new Dictionary<Building, bool>();

            BuildingsToShow = new Dictionary<BuildingData, bool>();
            _createdUiElements = new Dictionary<BuildingData, SingleBuildingRefs>();
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

            foreach (var building in _buildingManager.CurrentBuildings)
            {
                if (!building.IsBeeingUpgradedOrBuilded)
                {
                    if (_builtOrDamagedBuildings.ContainsKey(building) && !building.IsDamaged)
                    {
                        _builtOrDamagedBuildings.Remove(building);
                        continue;
                    }

                    if (building.IsDamaged)
                    {
                        _builtOrDamagedBuildings.TryAdd(building, building.HaveWorker);
                    }
                }
                else
                {
                    _builtOrDamagedBuildings.TryAdd(building, building.HaveWorker);
                }
            }

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
                    _runtimeBuildingsUiToDestroy.Add(newBuildingUi);

                    SingleBuildingRefs script = newBuildingUi.GetComponent<SingleBuildingRefs>();
                    _createdUiElements.Add(buildingData, script);

                    script.BuildingName.GetComponent<TextMeshProUGUI>().text = buildingData.Type.ToString();
                    script.BuildingInfo.GetComponent<TextMeshProUGUI>().text = "";

                    var builtBuilding = _buildingManager.GetSpecificBuilding(buildingData);

                    if (builtBuilding != null) // is builded or building is in progress
                    {
                        CreateOutcomeIcon(script, buildingData, builtBuilding.CurrentLevel);
                        
                        if (builtBuilding.IsDamaged)
                        {
                            script.BuildingIcon.color = Color.red;

                            if (!_builtOrDamagedBuildings[builtBuilding])
                            {
                                HandleBuildingToRepairCreation(script, builtBuilding, true);
                                continue;
                            }
                            else
                            {
                                HandleBuildingToRepairCreation(script, builtBuilding, false);
                                continue;
                            }
                        }

                        if (builtBuilding.IsBeeingUpgradedOrBuilded)
                        {
                            if (_builtOrDamagedBuildings[builtBuilding])
                            {
                                HandleInProgressBuildingCreation(script, buildingData, 0, true);
                                continue;
                            }

                            HandleCancelledBuildingWork(script, buildingData, 0, true);
                            continue;
                        }

                        if (!_buildingsOnInPanelQueue.Contains(builtBuilding.BuildingMainData)) // open way to upgrade
                        {
                            HandleCurrentBuildingCreation(script, builtBuilding);
                        }
                        else if (_buildingsOnInPanelQueue.Contains(builtBuilding.BuildingMainData))
                        {
                            HandleInProgressBuildingCreation(script, buildingData, builtBuilding.CurrentLevel);
                        }
                    }
                    else // is completly not builded - even in building stage
                    {
                        CreateOutcomeIcon(script, buildingData, 0);
                            
                        if (_buildingsOnInPanelQueue.Contains(buildingData))
                        {
                            HandleInProgressBuildingCreation(script, buildingData, 0);
                        }
                        else
                        {
                            HandleCompletelyNewBuildingCreation(script, buildingData);
                        }
                    }
                }
            }
        }

        private void CreateOutcomeIcon(SingleBuildingRefs p_refsScript, BuildingData p_buildingData, int p_currentLevel)
        {
            if (p_buildingData.PerLevelData[p_currentLevel].ProductionType == PointsType.ResourcesAndDefense)
            {
                p_refsScript.TypeOfOutcome.sprite = _buildingManager.DefenseAndResourcesPointsIcon;
            }
            else if (p_buildingData.PerLevelData[p_currentLevel].ProductionType == PointsType.Defense)
            {
                p_refsScript.TypeOfOutcome.sprite = _buildingManager.DefensePointsIcon;
            }
            else if (p_buildingData.PerLevelData[p_currentLevel].ProductionType == PointsType.Resource)
            {
                p_refsScript.TypeOfOutcome.sprite = _buildingManager.ResourcesPointsIcon;
            }
            else
            {
                p_refsScript.TypeOfOutcome.color = new Color(0,0,0,0);
            }
                        
            p_refsScript.BuildingIcon.GetComponent<Image>().sprite = p_buildingData.PerLevelData[p_currentLevel].Icon;
        }

        private void HandleCurrentBuildingCreation(SingleBuildingRefs p_refsScript, Building p_building)
        {
            var nextLevel = p_building.CurrentLevel;
            nextLevel++;

            p_refsScript.LevelInfo.GetComponent<TextMeshProUGUI>().text =
                $"{p_building.CurrentLevel} >> {nextLevel}";

            if (_buildingManager.CanUpgradeBuilding(p_building))
            {
                p_refsScript.CreateOrUpgradeBuilding.interactable = true;
                p_refsScript.CreateOrUpgradeBuilding.image.color = Color.green;
            }
            else
            {
                p_refsScript.CreateOrUpgradeBuilding.interactable = false;
                p_refsScript.CreateOrUpgradeBuilding.image.color = Color.red;
            }

            UpdateRequirementsText(p_refsScript,
                p_building.BuildingMainData.PerLevelData[p_building.CurrentLevel].Requirements);

            p_refsScript.CreateOrUpgradeBuilding.onClick.RemoveAllListeners();
            p_refsScript.CreateOrUpgradeBuilding.onClick.AddListener(() =>
                AssigningWorkerHandler(p_building.BuildingMainData, p_building.CurrentLevel,
                    p_refsScript, true)); // unabling usual working for defense/resources + kicking out worker
        }

        private void HandleInProgressBuildingCreation(SingleBuildingRefs p_refsScript, BuildingData p_buildingData,
            int p_level, bool p_wasBuilt = false) // add red cross to icon. Clicking -> canceling building
        {
            p_refsScript.BuildingInfo.GetComponent<TextMeshProUGUI>().text = "In Progress";
            p_refsScript.LevelInfo.GetComponent<TextMeshProUGUI>().text = "Cancel";

            p_refsScript.CreateOrUpgradeBuilding.interactable = true;
            p_refsScript.CreateOrUpgradeBuilding.image.color = Color.yellow;

            p_refsScript.CreateOrUpgradeBuilding.onClick.RemoveAllListeners();
            p_refsScript.CreateOrUpgradeBuilding.onClick.AddListener(() =>
                AssigningWorkerHandler(p_buildingData, p_level, p_refsScript, false, p_wasBuilt));
        }

        private void HandleBuildingToRepairCreation(SingleBuildingRefs p_refsScript, Building p_building, bool p_assign)
        {
            if (p_assign)
            {
                p_refsScript.LevelInfo.GetComponent<TextMeshProUGUI>().text = "Repair";
                p_refsScript.BuildingInfo.GetComponent<TextMeshProUGUI>().text = $"Days To Complete: 1";
                p_refsScript.CreateOrUpgradeBuilding.image.color = Color.green;
                p_refsScript.CreateOrUpgradeBuilding.interactable = _workersManager.IsAnyWorkerFree();
            }
            else
            {
                p_refsScript.LevelInfo.GetComponent<TextMeshProUGUI>().text = "Stop Repairing";
                p_refsScript.BuildingInfo.GetComponent<TextMeshProUGUI>().text = $"In Progress";
                p_refsScript.CreateOrUpgradeBuilding.image.color = Color.yellow;
                p_refsScript.CreateOrUpgradeBuilding.interactable = true;
            }

            p_refsScript.CreateOrUpgradeBuilding.onClick.RemoveAllListeners();
            p_refsScript.CreateOrUpgradeBuilding.onClick.AddListener(() =>
                AssigningWorkerHandler(p_building.BuildingMainData, p_building.CurrentLevel, p_refsScript, p_assign, true));
        }

        private void HandleCompletelyNewBuildingCreation(SingleBuildingRefs p_refsScript, BuildingData p_buildingData)
        {
            p_refsScript.LevelInfo.GetComponent<TextMeshProUGUI>().text = "Build";
            UpdateRequirementsText(p_refsScript, p_buildingData.PerLevelData[0].Requirements);

            if (_buildingManager.CanBuildBuilding(p_buildingData))
            {
                p_refsScript.CreateOrUpgradeBuilding.image.color = Color.green;
                p_refsScript.CreateOrUpgradeBuilding.interactable = true;
            }
            else
            {
                p_refsScript.CreateOrUpgradeBuilding.image.color = Color.red;
                p_refsScript.CreateOrUpgradeBuilding.interactable = false;
            }

            p_refsScript.CreateOrUpgradeBuilding.onClick.RemoveAllListeners();
            p_refsScript.CreateOrUpgradeBuilding.onClick.AddListener(() =>
                AssigningWorkerHandler(p_buildingData, 0, p_refsScript, true));
        }

        private void HandleStartedBuildingWork(SingleBuildingRefs p_refsScript, BuildingData p_buildingData,
            int p_buildingLevel, bool p_isBuilt = false)
        {
            p_refsScript.CreateOrUpgradeBuilding.image.color = Color.cyan; // in progress
            p_refsScript.CreateOrUpgradeBuilding.interactable = true;

            p_refsScript.BuildingInfo.GetComponent<TextMeshProUGUI>().text = "In Progress";
            p_refsScript.LevelInfo.GetComponent<TextMeshProUGUI>().text = "Cancel";

            p_refsScript.CreateOrUpgradeBuilding.onClick.RemoveAllListeners();
            p_refsScript.CreateOrUpgradeBuilding.onClick.AddListener(() => AssigningWorkerHandler(p_buildingData,
                p_buildingLevel, p_refsScript, false, p_isBuilt));
        }

        private void HandleCancelledBuildingWork(SingleBuildingRefs p_refsScript, BuildingData p_buildingData,
            int p_buildingLevel, bool p_isBuilt = false)
        {
            p_refsScript.CreateOrUpgradeBuilding.image.color = Color.blue;
            p_refsScript.CreateOrUpgradeBuilding.interactable = _workersManager.IsAnyWorkerFree();

            p_refsScript.BuildingInfo.GetComponent<TextMeshProUGUI>().text = "Cancelled";
            p_refsScript.LevelInfo.GetComponent<TextMeshProUGUI>().text = "Back To Work";

            p_refsScript.CreateOrUpgradeBuilding.onClick.RemoveAllListeners();
            p_refsScript.CreateOrUpgradeBuilding.onClick.AddListener(() => AssigningWorkerHandler(p_buildingData,
                p_buildingLevel, p_refsScript, true, p_isBuilt));
        }

        private void AssigningWorkerHandler(BuildingData p_buildingData, int p_buildingLevel,
            SingleBuildingRefs p_refsScript, bool p_assign, bool p_isBuilt = false)
        {
            if (p_assign)
            {
                HandleStartedBuildingWork(p_refsScript, p_buildingData, p_buildingLevel, p_isBuilt);
            }
            else
            {
                p_refsScript.CreateOrUpgradeBuilding.image.color = Color.green;
                p_refsScript.CreateOrUpgradeBuilding.interactable = _workersManager.IsAnyWorkerFree();

                var building = _buildingManager.GetSpecificBuilding(p_buildingData);

                if (building == null)
                {
                    UpdateRequirementsText(p_refsScript, p_buildingData.PerLevelData[0].Requirements);
                    p_refsScript.LevelInfo.GetComponent<TextMeshProUGUI>().text = "Build";

                    if (BuildingsToShow.ContainsKey(p_buildingData))
                    {
                        BuildingsToShow.Remove(p_buildingData);
                    }
                }
                else
                {
                    var nextLevel = building.CurrentLevel;
                    nextLevel++;

                    p_refsScript.LevelInfo.GetComponent<TextMeshProUGUI>().text =
                        $"{building.CurrentLevel} >> {nextLevel}";
                    UpdateRequirementsText(p_refsScript,
                        p_buildingData.PerLevelData[building.CurrentLevel].Requirements);

                    if (BuildingsToShow.ContainsKey(p_buildingData))
                    {
                        if (!building.IsBeeingUpgradedOrBuilded)
                        {
                            BuildingsToShow.Remove(p_buildingData);
                        }
                    }
                }

                p_refsScript.CreateOrUpgradeBuilding.onClick.RemoveAllListeners();
                p_refsScript.CreateOrUpgradeBuilding.onClick.AddListener(() => AssigningWorkerHandler(p_buildingData,
                    p_buildingLevel, p_refsScript, true, p_isBuilt));
            }

            OnButtonClicked(p_buildingData, p_buildingLevel, p_assign, p_isBuilt);
        }

        private void OnButtonClicked(BuildingData p_buildingData, int p_buildingLevel, bool p_assign, bool p_wasBuilt)
        {
            if (p_wasBuilt)
            {
                var building = _buildingManager.GetSpecificBuilding(p_buildingData);

                _influencedBuildings.TryAdd(building, _builtOrDamagedBuildings[building]);
                _builtOrDamagedBuildings[building] = !_builtOrDamagedBuildings[building];
            }
            else
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
            }

            RefreshWorkersAmount();
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
                    element.Value.BuildingIcon.GetComponent<Image>().sprite =
                        element.Key.PerLevelData[building.CurrentLevel].Icon;

                    if (building.IsDamaged)
                    {
                        if (_builtOrDamagedBuildings[building])
                        {
                            HandleBuildingToRepairCreation(element.Value, building, false);
                        }
                        else
                        {
                            HandleBuildingToRepairCreation(element.Value, building, true);
                        }

                        continue;
                    }

                    if (building.IsBeeingUpgradedOrBuilded)
                    {
                        if (_builtOrDamagedBuildings[building])
                        {
                            HandleInProgressBuildingCreation(element.Value, building.BuildingMainData,
                                building.CurrentLevel, true);
                            continue;
                        }

                        HandleCancelledBuildingWork(element.Value, building.BuildingMainData, building.CurrentLevel,
                            true);
                        continue;
                    }

                    if (!_buildingsOnInPanelQueue.Contains(building.BuildingMainData)) // open way to upgrade
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

            foreach (var building in _builtOrDamagedBuildings)
            {
                if (!_influencedBuildings.ContainsKey(building.Key))
                    continue;

                if (_influencedBuildings[building.Key] != _builtOrDamagedBuildings[building.Key])
                {
                    _buildingManager.HandleBuildingsModifications(building.Key);
                }
            }

            RefreshWorkersAmount();

            _buildingsOnInPanelQueue.Clear();
            _influencedBuildings.Clear();
        }

        private void UpdateWorkersText()
        {
            _numberOfWorkers.text =
                $"Workers: {_workersManager.BaseWorkersAmounts.ToString()}/{_workersManager.OverallAssignedWorkers}";
        }

        private void UpdateRequirementsText(SingleBuildingRefs p_script, Requirements p_requirements)
        {
            p_script.BuildingInfo.GetComponent<TextMeshProUGUI>().text =
                $"Resource Points: {p_requirements.ResourcePoints}\n" +
                $"Days To Complete: {p_requirements.DaysToComplete}\n";
        }

        public void RefreshWorkersAmount()
        {
            _workersManager.WorkersInBuilding = 0;
            List<BuildingData> addedBuildings = new List<BuildingData>();

            foreach (var building in _buildingManager.CurrentBuildings)
            {
                if (!building.IsBeeingUpgradedOrBuilded && !building.IsDamaged)
                    continue;

                if (_builtOrDamagedBuildings.ContainsKey(building))
                {
                    if (_builtOrDamagedBuildings[building])
                    {
                        _workersManager.WorkersInBuilding++;
                        BuildingsToShow.TryAdd(building.BuildingMainData, false);
                        addedBuildings.Add(building.BuildingMainData);
                    }
                }
                else
                {
                    if (building.HaveWorker)
                    {
                        _workersManager.WorkersInBuilding++;
                        addedBuildings.Add(building.BuildingMainData);
                    }
                }
            }

            foreach (var building in _buildingsOnInPanelQueue)
            {
                if (!addedBuildings.Contains(building))
                {
                    _workersManager.WorkersInBuilding++;
                }
            }
        }

        public bool WillBuildingBeCancelled(Building p_building, out bool p_wasOnList)
        {
            if (_builtOrDamagedBuildings.ContainsKey(p_building))
            {
                p_wasOnList = true;
                return !_builtOrDamagedBuildings[p_building];
            }

            p_wasOnList = false;
            return false;
        }

        public bool WillBuildingBeUpgraded(Building p_building)
        {
            if (_buildingsOnInPanelQueue.Contains(p_building.BuildingMainData))
            {
                return true;
            }

            return false;
        }

        private void HandleBuildEnded(Building p_building)
        {
            BuildingsToShow.Remove(p_building.BuildingMainData);
        }
    }
}