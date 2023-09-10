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

        [SerializeField] private TextMeshProUGUI _panelName;
        [SerializeField] private TextMeshProUGUI _numberOfWorkers;
        [SerializeField] private GameObject _gatheringDefenseEntryPrefab;
        [SerializeField] private GameObject _endWhileDisplacingWorkers;
        [SerializeField] private Transform contentTransform;

        private BuildingData _currentBuildingData;
        private int _currentBuildingLevel;
        [HideInInspector] public List<Building> BuildingsOnQueue;
        private List<GameObject> _runtimeBuildingsUiToDestroy;

        public event Action OnBackToWorkersPanel;

        private void Start()
        {
            _buildingManager.OnBuildingClicked += ActivateOnClick;
            _runtimeBuildingsUiToDestroy = new List<GameObject>();
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

                SingleGatheringUi script = newGathering.GetComponent<SingleGatheringUi>();
                script.BuildingName.GetComponent<TextMeshProUGUI>().text = building.BuildingMainData.Type.ToString();
                script.BuildingIcon.GetComponent<Image>().sprite =
                    building.BuildingMainData.PerLevelData[building.CurrentLevel].Icon;
                
                script.BuildingInfo.GetComponent<TextMeshProUGUI>().text = p_gathering ? 
                    $"Production Points Per Day: {_buildingManager.GetProductionDataOfBuilding(building)}" 
                    : $"Defense Points Per Day: {_buildingManager.GetDefenseRisingDataOfBuilding(building)}";

                if (building.HaveWorker)
                {
                    script.SelectWorker.image.color = Color.cyan;
                    script.SelectWorker.interactable = true;
                    script.SelectWorker.onClick.AddListener(() => AssigningWorkerHandler(building, script, false, p_gathering));
                }
                else
                {
                    script.SelectWorker.image.color = Color.green;
                    script.SelectWorker.interactable = true;//AssignedWorkers < 0;
                    script.SelectWorker.onClick.AddListener(() => AssigningWorkerHandler(building, script, true, p_gathering));
                }

                if (building.HaveWorker)
                {
                    script.GetComponent<Image>().color = Color.magenta;
                }
                else
                {
                    script.GetComponent<Image>().color = Color.gray;
                }
            }
        }

        private void AssigningWorkerHandler(Building p_building, SingleGatheringUi p_newGatheringPanel, bool p_assign, bool p_gathering)
        {
            p_newGatheringPanel.SelectWorker.onClick.RemoveAllListeners();   

            if (p_assign)
            {
                p_newGatheringPanel.SelectWorker.image.color = Color.cyan;                    
                p_newGatheringPanel.SelectWorker.interactable = true;
                p_newGatheringPanel.SelectWorker.onClick.AddListener(() => AssigningWorkerHandler(p_building, p_newGatheringPanel, false, p_gathering));   
            }
            else
            {
                p_newGatheringPanel.SelectWorker.image.color = Color.green;
                p_newGatheringPanel.SelectWorker.interactable = true;//AssignedWorkers > 0;
                p_newGatheringPanel.SelectWorker.onClick.AddListener(() => AssigningWorkerHandler(p_building, p_newGatheringPanel, true, p_gathering));   
            }
            
            OnButtonClicked(p_building, p_assign, p_gathering);
        }
        
        private void OnButtonClicked(Building p_building, bool p_assign, bool p_gathering)
        {
            if (p_assign)
            {
                if (p_gathering)
                {
                    _workersManager.WorkersInResources--;
                }
                else
                {
                    _workersManager.WorkersInDefences--;
                }

                BuildingsOnQueue.Add(p_building);
            }
            else
            {
                if (p_gathering)
                {
                    _workersManager.WorkersInResources++;
                }
                else
                {
                    _workersManager.WorkersInDefences++;
                }

                BuildingsOnQueue.Remove(p_building);
            }

            UpdateWorkersText(p_gathering);
        }
        

        private void UpdateWorkersText(bool p_gathering)
        {
            if (p_gathering)
            {
                if (_workersManager.WorkersInResources >= 0)
                {
                    _numberOfWorkers.text = $"Workers: {_workersManager.BaseWorkersAmounts.ToString()} (+{_workersManager.WorkersInResources})";
                }
            }
            else
            {
                if (_workersManager.WorkersInDefences >= 0)
                {
                    _numberOfWorkers.text = $"Workers: {_workersManager.BaseWorkersAmounts.ToString()} (+{_workersManager.WorkersInDefences})";
                }
            }
        }
    }
}