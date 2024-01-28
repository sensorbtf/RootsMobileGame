using System;
using System.Collections.Generic;
using Buildings;
using GeneralSystems;
using TMPro;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.UI;
using UnityEngine.Serialization;

namespace InGameUi
{
    public class GatheringDefensePanel : MonoBehaviour
    {
        [FormerlySerializedAs("_buildingManager")] [SerializeField]
        private BuildingsManager buildingsManager;

        [SerializeField] private WorkersManager _workersManager;
        [SerializeField] private BuildingPanel _buildingPanel;
        [SerializeField] private GameObject _gatheringDefenseEntryPrefab;
        [SerializeField] private GameObject _endWhileDisplacingWorkers;
        [SerializeField] private Transform contentTransform;
        [SerializeField] private TextMeshProUGUI _numberOfWorkers;
        [SerializeField] private TextMeshProUGUI _panelName;
        
        [SerializeField] private LocalizedString _gatheringPanelTitle;
        [SerializeField] private LocalizedString _defensePanelTitle;
        [SerializeField] private LocalizedString _workersText;
        [SerializeField] private LocalizedString _buildingUpgrading;
        [SerializeField] private LocalizedString _buildingDestroyed;
        [SerializeField] private LocalizedString _assignText;
        [SerializeField] private LocalizedString _unassignText;
        [SerializeField] private LocalizedString _resourceDefenseProduction;
        [SerializeField] private LocalizedString _resourceProduction;
        [SerializeField] private LocalizedString _defenseProduction;
        [SerializeField] private LocalizedString _defenseResourceProduction;
        
        [HideInInspector] public List<Building> BuildingsOnQueue;

        private Dictionary<Building, SingleBuildingRefs> _createdUiElements;
        private List<GameObject> _runtimeBuildingsUiToDestroy;

        private void Start()
        {
            _runtimeBuildingsUiToDestroy = new List<GameObject>();
            _createdUiElements = new Dictionary<Building, SingleBuildingRefs>();
            BuildingsOnQueue = new List<Building>();
            gameObject.SetActive(false);
        }

        public event Action OnBackToWorkersPanel;

        public void BackToWorkerTab()
        {
            ClosePanel();
            OnBackToWorkersPanel?.Invoke();
        }

        private void ClosePanel()
        {
            foreach (var createdUiElement in _runtimeBuildingsUiToDestroy) Destroy(createdUiElement);

            CameraController.IsUiOpen = false;

            _runtimeBuildingsUiToDestroy.Clear();
            _createdUiElements.Clear();

            GameplayHud.BlockHud = false;

            gameObject.SetActive(false);
        }

        public void ConfirmWorkersAssigment()
        {
            foreach (var building in BuildingsOnQueue) buildingsManager.AssignWorker(building, true);

            BuildingsOnQueue.Clear();
        }

        public void HandleView(bool p_gathering)
        {
            gameObject.SetActive(true);
            CameraController.IsUiOpen = true;
            GameplayHud.BlockHud = true;
            _endWhileDisplacingWorkers.SetActive(true);
            UpdateWorkersText(p_gathering);

            _panelName.text = p_gathering ? _gatheringPanelTitle.GetLocalizedString() : _defensePanelTitle.GetLocalizedString();
            
            foreach (var building in buildingsManager.CurrentBuildings)
            {
                var productionType = building.BuildingMainData.PerLevelData[building.CurrentLevel].ProductionType;
                if (ShouldSkip(p_gathering, productionType))
                    continue;

                var newGathering = Instantiate(_gatheringDefenseEntryPrefab, contentTransform);
                _runtimeBuildingsUiToDestroy.Add(newGathering);

                var script = newGathering.GetComponent<SingleBuildingRefs>();
                _createdUiElements.Add(building, script);
                script.BuildingName.GetComponent<TextMeshProUGUI>().text = building.BuildingMainData.BuildingName.GetLocalizedString();
                script.BuildingIcon.GetComponent<Image>().sprite = building.BuildingMainData.Icon;

                var productionPoints = buildingsManager.GetProductionOfBuilding(building.BuildingMainData.Type);
                
                if (p_gathering)
                {
                    if (productionType == PointsType.DefenseAndResources)
                    {
                        var modProd = productionPoints / 2;
                        var text = string.Format(_resourceDefenseProduction.GetLocalizedString(), modProd, productionPoints);
                        script.BuildingInfo.GetComponent<TextMeshProUGUI>().text = text;
                    }
                    else if (productionType == PointsType.Resource)
                    {
                        script.BuildingInfo.GetComponent<TextMeshProUGUI>().text = string.Format(_resourceProduction.GetLocalizedString(), productionPoints);
                    }
                    else if (productionType == PointsType.ResourcesAndDefense)
                    {
                        var modProd = productionPoints / 2;
                        var text = string.Format(_resourceDefenseProduction.GetLocalizedString(), productionPoints, modProd);
                        script.BuildingInfo.GetComponent<TextMeshProUGUI>().text = text;
                    }
                }
                else
                {
                    if (productionType == PointsType.ResourcesAndDefense)
                    {
                        var modProd = productionPoints / 2;
                        var text = string.Format(_defenseResourceProduction.GetLocalizedString(), modProd, productionPoints);
                        script.BuildingInfo.GetComponent<TextMeshProUGUI>().text = text;
                    }
                    else if (productionType == PointsType.Defense)
                    {
                        script.BuildingInfo.GetComponent<TextMeshProUGUI>().text = string.Format(_defenseProduction.GetLocalizedString(), productionPoints);
                    }
                    else if (productionType == PointsType.DefenseAndResources)
                    {
                        var modProd = productionPoints / 2;
                        var text = string.Format(_defenseResourceProduction.GetLocalizedString(), productionPoints, modProd);
                        script.BuildingInfo.GetComponent<TextMeshProUGUI>().text = text;
                    }
                }

                if (building.IsBeeingUpgradedOrBuilded || _buildingPanel.WillBuildingBeUpgraded(building))
                {
                    script.IsInConstruction.SetActive(true);
                    script.IsInConstruction.GetComponent<Image>().color = new Color(255,155,0);
                    script.IsInConstructionText.text = _buildingUpgrading.GetLocalizedString();
                    script.CreateOrUpgradeBuilding.interactable = false;
                }
                else if (building.IsDamaged)
                {
                    script.IsInConstruction.SetActive(true);
                    script.IsInConstruction.GetComponent<Image>().color = Color.red;
                    script.IsInConstructionText.text = _buildingDestroyed.GetLocalizedString();
                    script.CreateOrUpgradeBuilding.interactable = false;
                }
                else
                {
                    script.IsInConstruction.SetActive(false);

                    if (building.HaveWorker || BuildingsOnQueue.Contains(building))
                        AssignWorkerHandler(building, script, p_gathering);
                    else
                        UnAssignWorkerHandler(building, script, p_gathering);
                }
            }
        }

        private bool ShouldSkip(bool p_gathering, PointsType p_pointsType)
        {
            if (p_gathering)
            {
                if (p_pointsType != PointsType.Resource && p_pointsType != PointsType.DefenseAndResources && p_pointsType != PointsType.ResourcesAndDefense)
                    return true;
            }
            else
            {
                if (p_pointsType != PointsType.Defense && p_pointsType != PointsType.DefenseAndResources && p_pointsType != PointsType.ResourcesAndDefense)
                    return true;
            }

            return false;
        }

        private void AssignWorkerHandler(Building p_building, SingleBuildingRefs p_script, bool p_gathering)
        {
            p_script.CreateOrUpgradeBuilding.image.color = Color.cyan;
            p_script.CreateOrUpgradeBuilding.interactable = true;
            p_script.LevelInfo.text = _unassignText.GetLocalizedString();

            p_script.CreateOrUpgradeBuilding.onClick.RemoveAllListeners();
            p_script.CreateOrUpgradeBuilding.onClick.AddListener(() =>
                AssigningWorkerHandler(p_building, p_script, false, p_gathering));
        }

        private void UnAssignWorkerHandler(Building p_building, SingleBuildingRefs script, bool p_gathering)
        {
            script.CreateOrUpgradeBuilding.image.color = Color.green;
            script.CreateOrUpgradeBuilding.interactable = _workersManager.IsAnyWorkerFree();
            script.LevelInfo.text = _assignText.GetLocalizedString();

            script.CreateOrUpgradeBuilding.onClick.RemoveAllListeners();
            script.CreateOrUpgradeBuilding.onClick.AddListener(() =>
                AssigningWorkerHandler(p_building, script, true, p_gathering));
        }

        private void AssigningWorkerHandler(Building p_building, SingleBuildingRefs script, bool p_assign,
            bool p_gathering)
        {
            script.CreateOrUpgradeBuilding.onClick.RemoveAllListeners();

            if (p_assign)
                AssignWorkerHandler(p_building, script, p_gathering);
            else
                UnAssignWorkerHandler(p_building, script, p_gathering);

            OnButtonClicked(p_building, p_assign, p_gathering);
        }

        private void OnButtonClicked(Building p_building, bool p_assign, bool p_gathering)
        {
            if (p_assign)
            {
                if (p_gathering)
                    _workersManager.WorkersInResources++;
                else
                    _workersManager.WorkersInDefences++;

                BuildingsOnQueue.Add(p_building);
            }
            else
            {
                if (p_gathering)
                    _workersManager.WorkersInResources--;
                else
                    _workersManager.WorkersInDefences--;

                BuildingsOnQueue.Remove(p_building);
            }

            UpdateWorkersText(p_gathering);
            UpdateOtherButtonsUi();
        }

        private void UpdateOtherButtonsUi()
        {
            foreach (var building in _createdUiElements)
            {
                if (building.Key.IsBeeingUpgradedOrBuilded || _buildingPanel.WillBuildingBeUpgraded(building.Key))
                    continue;

                var canProduce = building.Key.BuildingMainData.PerLevelData[building.Key.CurrentLevel].ProductionType ==
                                 PointsType.Resource ||
                                 building.Key.BuildingMainData.PerLevelData[building.Key.CurrentLevel].ProductionType ==
                                 PointsType.ResourcesAndDefense ||
                                 building.Key.BuildingMainData.PerLevelData[building.Key.CurrentLevel].ProductionType ==
                                 PointsType.DefenseAndResources;

                if (BuildingsOnQueue.Contains(building.Key))
                    AssignWorkerHandler(building.Key, building.Value, canProduce);
                else
                    UnAssignWorkerHandler(building.Key, building.Value, canProduce);
            }
        }

        private void UpdateWorkersText(bool p_gathering)
        {
                _numberOfWorkers.text = $"{_workersText.GetLocalizedString()} {_workersManager.BaseWorkersAmounts.ToString()}/{_workersManager.OverallAssignedWorkers}";
        }
    }
}