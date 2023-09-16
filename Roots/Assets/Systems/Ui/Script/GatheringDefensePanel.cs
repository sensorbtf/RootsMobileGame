using System;
using System.Collections.Generic;
using Buildings;
using GeneralSystems;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace InGameUi
{
    public class GatheringDefensePanel : MonoBehaviour
    {
        [SerializeField] private BuildingManager _buildingManager;
        [SerializeField] private WorkersManager _workersManager;
        [SerializeField] private BuildingPanel _buildingPanel;

        [SerializeField] private TextMeshProUGUI _panelName;
        [SerializeField] private TextMeshProUGUI _numberOfWorkers;
        [SerializeField] private GameObject _gatheringDefenseEntryPrefab;
        [SerializeField] private GameObject _endWhileDisplacingWorkers;
        [SerializeField] private Transform contentTransform;

        private BuildingData _currentBuildingData;
        private int _currentBuildingLevel;
        [HideInInspector] public List<Building> BuildingsOnQueue;  // ITS NOT WORKING. TRY ADDING -> BACK -> FORWARD TO PANEL. NEED TO SAVE LIKE 
        
        private Dictionary<Building, SingleBuildingRefs> _createdUiElements;
        private List<GameObject> _runtimeBuildingsUiToDestroy;

        public event Action OnBackToWorkersPanel;

        private void Start()
        {
            _buildingManager.OnBuildingClicked += ActivateOnClick;
            _runtimeBuildingsUiToDestroy = new List<GameObject>();
            _createdUiElements = new Dictionary<Building, SingleBuildingRefs>();
            BuildingsOnQueue = new List<Building>();
            gameObject.SetActive(false);
        }

        private void ActivateOnClick(BuildingData p_specificBuilding, int p_level)
        {
            _currentBuildingData = p_specificBuilding;
            _currentBuildingLevel = p_level;
            _panelName.text = p_specificBuilding.Type.ToString();

            // View all buildings in cottage as it is like centrum dowodzenia for fast building
        }

        public void BackToWorkerTab()
        {
            ClosePanel();
            OnBackToWorkersPanel?.Invoke();
        }

        private void ClosePanel()
        {
            foreach (var createdUiElement in _runtimeBuildingsUiToDestroy)
            {
                Destroy(createdUiElement);
            }

            CameraController.IsUiOpen = false;

            _currentBuildingData = null;
            _runtimeBuildingsUiToDestroy.Clear();
            _createdUiElements.Clear();

            GameplayHud.BlockHud = false;

            gameObject.SetActive(false);
        }

        public void ConfirmWorkersAssigment()
        {
            foreach (var building in BuildingsOnQueue)
            {
                _buildingManager.AssignWorker(building, true);
            }

            BuildingsOnQueue.Clear();
        }

        public void HandleView(bool p_gathering)
        {
            gameObject.SetActive(true);
            CameraController.IsUiOpen = true;
            GameplayHud.BlockHud = true;
            _endWhileDisplacingWorkers.SetActive(true);
            UpdateWorkersText(p_gathering);

            // Now create UI elements by tier
            foreach (var building in _buildingManager.CurrentBuildings)
            {
                if (p_gathering)
                {
                    if (!building.BuildingMainData.PerLevelData[building.CurrentLevel].CanProduce)
                    {
                        continue;
                    }
                }
                else
                {
                    if (!building.BuildingMainData.PerLevelData[building.CurrentLevel].CanRiseDefenses)
                    {
                        continue;
                    }
                }

                var newGathering = Instantiate(_gatheringDefenseEntryPrefab, contentTransform);
                _runtimeBuildingsUiToDestroy.Add(newGathering);

                SingleBuildingRefs script = newGathering.GetComponent<SingleBuildingRefs>();
                _createdUiElements.Add(building, script);
                script.BuildingName.GetComponent<TextMeshProUGUI>().text = building.BuildingMainData.Type.ToString();
                script.BuildingIcon.GetComponent<Image>().sprite = building.BuildingMainData.PerLevelData[building.CurrentLevel].Icon;

                script.BuildingInfo.GetComponent<TextMeshProUGUI>().text = p_gathering
                    ? $"Production Points Per Day: {_buildingManager.GetProductionDataOfBuilding(building)}"
                    : $"Defense Points Per Day: {_buildingManager.GetDefenseRisingDataOfBuilding(building)}";

                if (building.IsBeeingUpgradedOrBuilded || _buildingPanel.WillBuildingBeUpgraded(building))
                {
                    script.InConstruction.SetActive(true);
                    script.CreateOrUpgradeBuilding.interactable = false;
                }
                else
                {
                    script.InConstruction.SetActive(false);
                    
                    if (building.HaveWorker || BuildingsOnQueue.Contains(building))
                    {
                        AssignWorkerHandler(building, script, p_gathering);
                    }
                    else
                    {
                        UnAssignWorkerHandler(building, script, p_gathering);
                    }
                }
            }
        }

        private void AssignWorkerHandler(Building p_building, SingleBuildingRefs script, bool p_gathering)
        {
            script.CreateOrUpgradeBuilding.image.color = Color.cyan;
            script.CreateOrUpgradeBuilding.interactable = true;
            script.LevelInfo.text = "Un assign";

            script.CreateOrUpgradeBuilding.onClick.RemoveAllListeners();
            script.CreateOrUpgradeBuilding.onClick.AddListener(() =>
                AssigningWorkerHandler(p_building, script, false, p_gathering));
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
            {
                AssignWorkerHandler(p_building, script, p_gathering);
            }
            else
            {
                UnAssignWorkerHandler(p_building, script, p_gathering);
            }

            OnButtonClicked(p_building, p_assign, p_gathering);
        }

        private void OnButtonClicked(Building p_building, bool p_assign, bool p_gathering)
        {
            if (p_assign)
            {
                if (p_gathering)
                {
                    _workersManager.WorkersInResources++;
                }
                else
                {
                    _workersManager.WorkersInDefences++;
                }

                BuildingsOnQueue.Add(p_building);
            }
            else
            {
                if (p_gathering)
                {
                    _workersManager.WorkersInResources--;
                }
                else
                {
                    _workersManager.WorkersInDefences--;
                }

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
                
                if (BuildingsOnQueue.Contains(building.Key))
                {
                    AssignWorkerHandler(building.Key, building.Value, building.Key.BuildingMainData.PerLevelData[building.Key.CurrentLevel].CanProduce);
                }
                else
                {
                    UnAssignWorkerHandler(building.Key, building.Value, building.Key.BuildingMainData.PerLevelData[building.Key.CurrentLevel].CanProduce);
                }
            }
        }

        private void UpdateWorkersText(bool p_gathering)
        {
            if (p_gathering)
            {
                _numberOfWorkers.text =
                    $"Workers: {_workersManager.BaseWorkersAmounts.ToString()}/{_workersManager.OverallAssignedWorkers}";
            }
            else
            {
                _numberOfWorkers.text =
                    $"Workers: {_workersManager.BaseWorkersAmounts.ToString()}/{_workersManager.OverallAssignedWorkers}";
            }
        }
    }
}