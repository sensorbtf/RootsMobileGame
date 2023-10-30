using System;
using System.Collections.Generic;
using Buildings;
using GeneralSystems;
using TMPro;
using UnityEngine;
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

        [HideInInspector] public List<Building> BuildingsOnQueue;

        private Dictionary<Building, SingleBuildingRefs> _createdUiElements;
        [SerializeField] private TextMeshProUGUI _numberOfWorkers;

        [SerializeField] private TextMeshProUGUI _panelName;
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

            foreach (var building in buildingsManager.CurrentBuildings)
            {
                if (p_gathering)
                {
                    if (building.BuildingMainData.PerLevelData[building.CurrentLevel].ProductionType !=
                        PointsType.Resource &&
                        building.BuildingMainData.PerLevelData[building.CurrentLevel].ProductionType !=
                        PointsType.ResourcesAndDefense)
                        continue;
                }
                else
                {
                    if (building.BuildingMainData.PerLevelData[building.CurrentLevel].ProductionType !=
                        PointsType.Defense &&
                        building.BuildingMainData.PerLevelData[building.CurrentLevel].ProductionType !=
                        PointsType.ResourcesAndDefense)
                        continue;
                }

                var newGathering = Instantiate(_gatheringDefenseEntryPrefab, contentTransform);
                _runtimeBuildingsUiToDestroy.Add(newGathering);

                var script = newGathering.GetComponent<SingleBuildingRefs>();
                _createdUiElements.Add(building, script);
                script.BuildingName.GetComponent<TextMeshProUGUI>().text = building.BuildingMainData.Type.ToString();
                script.BuildingIcon.GetComponent<Image>().sprite = building.BuildingMainData.Icon;

                script.BuildingInfo.GetComponent<TextMeshProUGUI>().text = p_gathering
                    ? $"Production Points Per Day: {buildingsManager.GetProductionOfBuilding(building.BuildingMainData.Type)}"
                    : $"Defense Points Per Day: {buildingsManager.GetProductionOfBuilding(building.BuildingMainData.Type)}";

                if (building.IsBeeingUpgradedOrBuilded || _buildingPanel.WillBuildingBeUpgraded(building))
                {
                    script.IsInConstruction.SetActive(true);
                    script.IsInConstruction.GetComponent<Image>().color = Color.white;
                    script.CreateOrUpgradeBuilding.interactable = false;
                }
                else if (building.IsDamaged)
                {
                    script.IsInConstruction.SetActive(true);
                    script.IsInConstruction.GetComponent<Image>().color = Color.red;
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

        private void AssignWorkerHandler(Building p_building, SingleBuildingRefs p_script, bool p_gathering)
        {
            p_script.CreateOrUpgradeBuilding.image.color = Color.cyan;
            p_script.CreateOrUpgradeBuilding.interactable = true;
            p_script.LevelInfo.text = "Un assign";

            p_script.CreateOrUpgradeBuilding.onClick.RemoveAllListeners();
            p_script.CreateOrUpgradeBuilding.onClick.AddListener(() =>
                AssigningWorkerHandler(p_building, p_script, false, p_gathering));
        }

        private void UnAssignWorkerHandler(Building p_building, SingleBuildingRefs script, bool p_gathering)
        {
            script.CreateOrUpgradeBuilding.image.color = Color.green;
            script.CreateOrUpgradeBuilding.interactable = _workersManager.IsAnyWorkerFree();
            script.LevelInfo.text = "Assign";

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
                                 PointsType.ResourcesAndDefense;

                if (BuildingsOnQueue.Contains(building.Key))
                    AssignWorkerHandler(building.Key, building.Value, canProduce);
                else
                    UnAssignWorkerHandler(building.Key, building.Value, canProduce);
            }
        }

        private void UpdateWorkersText(bool p_gathering)
        {
            if (p_gathering)
                _numberOfWorkers.text =
                    $"Workers: {_workersManager.BaseWorkersAmounts.ToString()}/{_workersManager.OverallAssignedWorkers}";
            else
                _numberOfWorkers.text =
                    $"Workers: {_workersManager.BaseWorkersAmounts.ToString()}/{_workersManager.OverallAssignedWorkers}";
        }
    }
}