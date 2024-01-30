using System;
using System.Collections.Generic;
using AudioSystem;
using Buildings;
using GeneralSystems;
using TMPro;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace InGameUi
{
    public class BuildingPanel : MonoBehaviour
    {
        [SerializeField] private AudioManager _audioManager;
        [SerializeField] private BuildingsManager buildingsManager;
        [SerializeField] private WorkersManager _workersManager;
        [SerializeField] private GameObject buildingEntryPrefab;
        [SerializeField] private GameObject tierPanelPrefab;
        [SerializeField] private GameObject _endBuildingButton;
        [SerializeField] private Transform contentTransform;

        [SerializeField] private TextMeshProUGUI _panelTitle;
        [SerializeField] private TextMeshProUGUI _numberOfWorkers;
        
        [Header("Audio")]
        [SerializeField] private AudioClip _onAssignEffect;
        [SerializeField] private AudioClip _onUnAssignEffect;
        
        [Header("Localziation")]
        [SerializeField] private LocalizedString _buildOrUpgrade;
        [SerializeField] private LocalizedString _maxLevel;
        [SerializeField] private LocalizedString _inProgress;
        [SerializeField] private LocalizedString _cancel;
        [SerializeField] private LocalizedString _cancelled;
        [SerializeField] private LocalizedString _repair;
        [SerializeField] private LocalizedString _stopRepair;
        [SerializeField] private LocalizedString _daysToComplete;
        [SerializeField] private LocalizedString _resourcePoints;
        [SerializeField] private LocalizedString _build;
        [SerializeField] private LocalizedString _restartWork;
        [SerializeField] private LocalizedString _workers;
        
        private List<BuildingData> _buildingsOnInPanelQueue;
        private Dictionary<Building, bool> _builtOrDamagedBuildings;
        private Dictionary<BuildingData, SingleBuildingRefs> _createdUiElements;
        private Dictionary<Building, bool> _influencedBuildings;

        private List<GameObject> _runtimeBuildingsUiToDestroy;

        public Dictionary<BuildingData, bool> BuildingsToShow;
        
        public event Action OnBackToWorkersPanel;

        private void Start()
        {
            buildingsManager.OnBuildingStateChanged += HandleBuildEnded;
            buildingsManager.OnBuildingDestroyed += HandleBuildEnded;
            _runtimeBuildingsUiToDestroy = new List<GameObject>();
            _buildingsOnInPanelQueue = new List<BuildingData>();
            _builtOrDamagedBuildings = new Dictionary<Building, bool>();
            _influencedBuildings = new Dictionary<Building, bool>();

            BuildingsToShow = new Dictionary<BuildingData, bool>();
            _createdUiElements = new Dictionary<BuildingData, SingleBuildingRefs>();
            gameObject.SetActive(false);
            UpdateWorkersText();
        }

        private void OnDestroy()
        {
            buildingsManager.OnBuildingStateChanged -= HandleBuildEnded;
            buildingsManager.OnBuildingDestroyed -= HandleBuildEnded;
        }

        private void ClosePanel()
        {
            _audioManager.PlayButtonSoundEffect(true);
            
            foreach (var createdUiElement in _runtimeBuildingsUiToDestroy) 
                Destroy(createdUiElement);

            foreach (var buildingData in _buildingsOnInPanelQueue) BuildingsToShow.TryAdd(buildingData, true);

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

            _panelTitle.text = _buildOrUpgrade.GetLocalizedString();

            RefreshWorkersAmount();
            UpdateWorkersText();

            _endBuildingButton.SetActive(p_fromWorkerPanel);

            foreach (var building in buildingsManager.CurrentBuildings)
            {
                if (building.BuildingMainData.Type == BuildingType.Cottage && !building.IsDamaged)
                    continue;

                if (!building.IsBeeingUpgradedOrBuilded)
                {
                    if (_builtOrDamagedBuildings.ContainsKey(building) && !building.IsDamaged)
                    {
                        _builtOrDamagedBuildings.Remove(building);
                        continue;
                    }

                    if (building.IsDamaged) _builtOrDamagedBuildings.TryAdd(building, building.HaveWorker);
                }
                else
                {
                    _builtOrDamagedBuildings.TryAdd(building, building.HaveWorker);
                }

                if (building.BuildingMainData.Type == BuildingType.Cottage)
                    break;
            }

            CreateBuildings();
        }

        private void CreateBuildings()
        {
            var buildingsByTier = new Dictionary<int, List<BuildingData>>();
            var tier = 1;
            var currentCottageLevel = buildingsManager.CurrentBuildings
                .Find(x => x.BuildingMainData.Type == BuildingType.Cottage).CurrentLevel;

            foreach (var building in buildingsManager.AllBuildingsDatabase.allBuildings)
            {
                if (building.Type == BuildingType.Cottage)
                    if (!buildingsManager.CurrentBuildings.Find(x => x.BuildingMainData.Type == BuildingType.Cottage)
                            .IsDamaged)
                        continue;

                if (building.BaseCottageLevelNeeded > currentCottageLevel)
                    continue;

                if (building.BaseCottageLevelNeeded is >= 10 and < 20)
                    tier = 3;
                else if (building.BaseCottageLevelNeeded >= 20)
                    tier = 3;

                if (!buildingsByTier.ContainsKey(tier))
                    buildingsByTier[tier] = new List<BuildingData>();

                buildingsByTier[tier].Add(building);

                if (building.Type == BuildingType.Cottage)
                    break;
            }

            HandleBuildingsCreation(buildingsByTier);
        }

        private void HandleBuildingsCreation(Dictionary<int, List<BuildingData>> p_buildingsByTier)
        {
            foreach (var tier in p_buildingsByTier.Keys)
            {
                var newTierPanel = Instantiate(tierPanelPrefab, contentTransform);
                newTierPanel.GetComponentInChildren<TextMeshProUGUI>().text = "Tier: " + tier;
                _runtimeBuildingsUiToDestroy.Add(newTierPanel);

                foreach (var buildingData in p_buildingsByTier[tier])
                {
                    var newBuildingUi = Instantiate(buildingEntryPrefab, contentTransform);
                    _runtimeBuildingsUiToDestroy.Add(newBuildingUi);

                    var script = newBuildingUi.GetComponent<SingleBuildingRefs>();
                    _createdUiElements.Add(buildingData, script);

                    script.BuildingName.GetComponent<TextMeshProUGUI>().text = buildingData.BuildingName.GetLocalizedString();
                    script.BuildingInfo.GetComponent<TextMeshProUGUI>().text = "";

                    var builtBuilding = buildingsManager.GetSpecificBuilding(buildingData.Type);

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

                            HandleBuildingToRepairCreation(script, builtBuilding, false);
                            continue;
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
                            HandleCurrentBuildingCreation(script, builtBuilding);
                        else if (_buildingsOnInPanelQueue.Contains(builtBuilding.BuildingMainData))
                            HandleInProgressBuildingCreation(script, buildingData, builtBuilding.CurrentLevel);
                    }
                    else // is completly not builded - even in building stage
                    {
                        CreateOutcomeIcon(script, buildingData, 0);

                        if (_buildingsOnInPanelQueue.Contains(buildingData))
                            HandleInProgressBuildingCreation(script, buildingData, 0);
                        else
                            HandleCompletelyNewBuildingCreation(script, buildingData);
                    }
                }
            }
        }

        private void CreateOutcomeIcon(SingleBuildingRefs p_refsScript, BuildingData p_buildingData, int p_currentLevel)
        {
            if (p_buildingData.PerLevelData[p_currentLevel].ProductionType == PointsType.ResourcesAndDefense)
                p_refsScript.TypeOfOutcome.sprite = buildingsManager.ResourcesAndDefensePointsIcon;
            else if (p_buildingData.PerLevelData[p_currentLevel].ProductionType == PointsType.DefenseAndResources)
                p_refsScript.TypeOfOutcome.sprite = buildingsManager.DefenseAndResourcesPointsIcon;
            else if (p_buildingData.PerLevelData[p_currentLevel].ProductionType == PointsType.Defense)
                p_refsScript.TypeOfOutcome.sprite = buildingsManager.DefensePointsIcon;
            else if (p_buildingData.PerLevelData[p_currentLevel].ProductionType == PointsType.Resource)
                p_refsScript.TypeOfOutcome.sprite = buildingsManager.ResourcesPointsIcon;
            else
                p_refsScript.TypeOfOutcome.color = new Color(0, 0, 0, 0);

            p_refsScript.BuildingIcon.GetComponent<Image>().sprite = p_buildingData.Icon;
        }

        private void HandleCurrentBuildingCreation(SingleBuildingRefs p_refsScript, Building p_building)
        {
            var nextLevel = p_building.CurrentLevel;
            nextLevel++;

            string info;
            
            if (nextLevel < p_building.BuildingMainData.PerLevelData.Length)
            {
                info = $"{p_building.CurrentLevel} >> {nextLevel}";
            }
            else
            {
                info = _maxLevel.GetLocalizedString();
            }

            p_refsScript.LevelInfo.GetComponent<TextMeshProUGUI>().text = info;

            if (buildingsManager.CanUpgradeBuilding(p_building))
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
            p_refsScript.BuildingInfo.GetComponent<TextMeshProUGUI>().text = _inProgress.GetLocalizedString();
            p_refsScript.LevelInfo.GetComponent<TextMeshProUGUI>().text = _cancel.GetLocalizedString();

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
                p_refsScript.LevelInfo.GetComponent<TextMeshProUGUI>().text = _repair.GetLocalizedString();
                p_refsScript.BuildingInfo.GetComponent<TextMeshProUGUI>().text = $"{_daysToComplete.GetLocalizedString()} 1";
                p_refsScript.CreateOrUpgradeBuilding.image.color = Color.green;
                p_refsScript.CreateOrUpgradeBuilding.interactable = _workersManager.IsAnyWorkerFree();
            }
            else
            {
                p_refsScript.LevelInfo.GetComponent<TextMeshProUGUI>().text = _stopRepair.GetLocalizedString();
                p_refsScript.BuildingInfo.GetComponent<TextMeshProUGUI>().text = _inProgress.GetLocalizedString();
                p_refsScript.CreateOrUpgradeBuilding.image.color = Color.yellow;
                p_refsScript.CreateOrUpgradeBuilding.interactable = true;
            }

            p_refsScript.CreateOrUpgradeBuilding.onClick.RemoveAllListeners();
            p_refsScript.CreateOrUpgradeBuilding.onClick.AddListener(() =>
                AssigningWorkerHandler(p_building.BuildingMainData, p_building.CurrentLevel, p_refsScript, p_assign,
                    true));
        }

        private void HandleCompletelyNewBuildingCreation(SingleBuildingRefs p_refsScript, BuildingData p_buildingData)
        {
            p_refsScript.LevelInfo.GetComponent<TextMeshProUGUI>().text = _build.GetLocalizedString();
            UpdateRequirementsText(p_refsScript, p_buildingData.PerLevelData[0].Requirements);

            if (buildingsManager.CanBuildBuilding(p_buildingData))
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

            p_refsScript.BuildingInfo.GetComponent<TextMeshProUGUI>().text = _inProgress.GetLocalizedString();
            p_refsScript.LevelInfo.GetComponent<TextMeshProUGUI>().text = _cancel.GetLocalizedString();

            p_refsScript.CreateOrUpgradeBuilding.onClick.RemoveAllListeners();
            p_refsScript.CreateOrUpgradeBuilding.onClick.AddListener(() => AssigningWorkerHandler(p_buildingData,
                p_buildingLevel, p_refsScript, false, p_isBuilt));
        }

        private void HandleCancelledBuildingWork(SingleBuildingRefs p_refsScript, BuildingData p_buildingData,
            int p_buildingLevel, bool p_isBuilt = false)
        {
            p_refsScript.CreateOrUpgradeBuilding.image.color = Color.blue;
            p_refsScript.CreateOrUpgradeBuilding.interactable = _workersManager.IsAnyWorkerFree();

            p_refsScript.BuildingInfo.GetComponent<TextMeshProUGUI>().text = _cancelled.GetLocalizedString();
            p_refsScript.LevelInfo.GetComponent<TextMeshProUGUI>().text = _restartWork.GetLocalizedString();

            p_refsScript.CreateOrUpgradeBuilding.onClick.RemoveAllListeners();
            p_refsScript.CreateOrUpgradeBuilding.onClick.AddListener(() => AssigningWorkerHandler(p_buildingData,
                p_buildingLevel, p_refsScript, true, p_isBuilt));
        }

        private void AssigningWorkerHandler(BuildingData p_buildingData, int p_buildingLevel,
            SingleBuildingRefs p_refsScript, bool p_assign, bool p_isBuilt = false)
        {
            if (p_assign)
            {
                _audioManager.CreateNewAudioSource(_onAssignEffect);
                HandleStartedBuildingWork(p_refsScript, p_buildingData, p_buildingLevel, p_isBuilt);
            }
            else
            {
                _audioManager.CreateNewAudioSource(_onUnAssignEffect); 
                p_refsScript.CreateOrUpgradeBuilding.image.color = Color.green;
                p_refsScript.CreateOrUpgradeBuilding.interactable = _workersManager.IsAnyWorkerFree();

                var building = buildingsManager.GetSpecificBuilding(p_buildingData.Type);

                if (building == null)
                {
                    UpdateRequirementsText(p_refsScript, p_buildingData.PerLevelData[0].Requirements);
                    p_refsScript.LevelInfo.GetComponent<TextMeshProUGUI>().text = _build.GetLocalizedString();

                    if (BuildingsToShow.ContainsKey(p_buildingData)) BuildingsToShow.Remove(p_buildingData);
                }
                else
                {
                    var nextLevel = building.CurrentLevel;
                    nextLevel++;

                    p_refsScript.LevelInfo.GetComponent<TextMeshProUGUI>().text = $"{building.CurrentLevel} >> {nextLevel}";
                    UpdateRequirementsText(p_refsScript, p_buildingData.PerLevelData[building.CurrentLevel].Requirements);

                    if (BuildingsToShow.ContainsKey(p_buildingData))
                        if (!building.IsBeeingUpgradedOrBuilded)
                            BuildingsToShow.Remove(p_buildingData);
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
                var building = buildingsManager.GetSpecificBuilding(p_buildingData.Type);

                _influencedBuildings.TryAdd(building, _builtOrDamagedBuildings[building]);
                _builtOrDamagedBuildings[building] = !_builtOrDamagedBuildings[building];
            }
            else
            {
                if (p_assign)
                {
                    _workersManager.WorkersInBuilding++;
                    _buildingsOnInPanelQueue.Add(p_buildingData);
                    buildingsManager.HandlePointsManipulation(PointsType.Resource,
                        p_buildingData.PerLevelData[p_buildingLevel].Requirements.ResourcePoints, false);
                }
                else
                {
                    _workersManager.WorkersInBuilding--;
                    _buildingsOnInPanelQueue.Remove(p_buildingData);
                    buildingsManager.HandlePointsManipulation(PointsType.Resource,
                        p_buildingData.PerLevelData[p_buildingLevel].Requirements.ResourcePoints, true);
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
                var building = buildingsManager.GetSpecificBuilding(element.Key.Type);

                if (building == null)
                {
                    if (_buildingsOnInPanelQueue.Contains(element.Key))
                    {
                        element.Value.BuildingInfo.GetComponent<TextMeshProUGUI>().text = _inProgress.GetLocalizedString();
                        element.Value.LevelInfo.GetComponent<TextMeshProUGUI>().text = _cancel.GetLocalizedString();

                        element.Value.CreateOrUpgradeBuilding.interactable = true;
                        element.Value.CreateOrUpgradeBuilding.image.color = Color.yellow;
                    }
                    else
                    {
                        if (buildingsManager.CanBuildBuilding(element.Key))
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
                        element.Key.Icon;

                    if (building.IsDamaged)
                    {
                        if (_builtOrDamagedBuildings[building])
                            HandleBuildingToRepairCreation(element.Value, building, false);
                        else
                            HandleBuildingToRepairCreation(element.Value, building, true);

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
                        if (buildingsManager.CanUpgradeBuilding(building))
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
            foreach (var buildingData in _buildingsOnInPanelQueue) buildingsManager.PutBuildingOnQueue(buildingData);

            foreach (var building in _builtOrDamagedBuildings)
            {
                if (!_influencedBuildings.ContainsKey(building.Key))
                    continue;

                if (_influencedBuildings[building.Key] != _builtOrDamagedBuildings[building.Key])
                    buildingsManager.HandleBuildingsModifications(building.Key);
            }

            RefreshWorkersAmount();

            _buildingsOnInPanelQueue.Clear();
            _influencedBuildings.Clear();
        }

        private void UpdateWorkersText()
        {
            _numberOfWorkers.text =
                $"{_workers.GetLocalizedString()}{_workersManager.BaseWorkersAmounts}/{_workersManager.OverallAssignedWorkers}";
        }

        private void UpdateRequirementsText(SingleBuildingRefs p_script, Requirements p_requirements)
        {
            p_script.BuildingInfo.GetComponent<TextMeshProUGUI>().text =
                $"{_resourcePoints.GetLocalizedString()}: {p_requirements.ResourcePoints}\n" +
                $"{_daysToComplete.GetLocalizedString()} {p_requirements.DaysToComplete}\n";
        }

        public void RefreshWorkersAmount()
        {
            _workersManager.WorkersInBuilding = 0;
            var addedBuildings = new List<BuildingData>();

            foreach (var building in buildingsManager.CurrentBuildings)
            {
                if (!building.IsBeeingUpgradedOrBuilded && !building.IsDamaged)
                    continue;

                if (_builtOrDamagedBuildings.TryGetValue(building, out var damagedBuilding))
                {
                    if (!damagedBuilding) 
                        continue;
                    
                    _workersManager.WorkersInBuilding++;
                    BuildingsToShow.TryAdd(building.BuildingMainData, false);
                    addedBuildings.Add(building.BuildingMainData);
                }
                else
                {
                    if (!building.HaveWorker) 
                        continue;
                    
                    _workersManager.WorkersInBuilding++;
                    addedBuildings.Add(building.BuildingMainData);
                }
            }

            foreach (var building in _buildingsOnInPanelQueue)
                if (!addedBuildings.Contains(building))
                    _workersManager.WorkersInBuilding++;
        }

        public bool WillBuildingBeCancelled(Building p_building, out bool p_wasOnList)
        {
            if (_builtOrDamagedBuildings.TryGetValue(p_building, out var building))
            {
                p_wasOnList = true;
                return !building;
            }

            p_wasOnList = false;
            return false;
        }

        public bool WillBuildingBeUpgraded(Building p_building)
        {
            if (_buildingsOnInPanelQueue.Contains(p_building.BuildingMainData))
                return true;

            return false;
        }

        private void HandleBuildEnded(Building p_building)
        {
            BuildingsToShow.Remove(p_building.BuildingMainData);
        }
    }
}