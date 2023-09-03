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
        private List<GameObject> _runtimeBuildingsUiToDestroy;

        public event Action OnBackToWorkersPanel;

        private void Start()
        {
            _buildingManager.OnBuildingClicked += ActivateOnClick;
            _workersManager.OnWorkersUpdated += UpdateWorkersText;
            _runtimeBuildingsUiToDestroy = new List<GameObject>();
            gameObject.SetActive(false);
        }

        private void OnDisable()
        {
            //_buildingManager.OnBuildingClicked -= ActivateOnClick;
            //_buildingManager.OnWorkersUpdated -= UpdateWorkersText;
        }

        private void ActivateOnClick(BuildingData p_specificBuilding, int p_level)
        {
            _currentBuildingData = p_specificBuilding;
            _currentBuildingLevel = p_level;
            _panelName.text = p_specificBuilding.Type.ToString();

            //_buildingManager.c
            // View all buildings in cottage as it is like centrum dowodzenia for fast building
        }

        public void BackToWorkerTab()
        {
            ClosePanel();
            OnBackToWorkersPanel?.Invoke();
        }

        private void UpdateWorkersText(int p_workers)
        {
            _numberOfWorkers.text = $"Workers: {p_workers.ToString()}";
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

        public void HandleView(bool p_gathering)
        {
            gameObject.SetActive(true);
            CameraController.IsUiOpen = true;
            GameplayHud.BlockHud = true;
            _endWhileDisplacingWorkers.SetActive(true);
            
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
                    $"Production Points Per Day: {building.BuildingMainData.PerLevelData[building.CurrentLevel].ProductionPerDay}" 
                    : $"Defense Points Per Day: {building.BuildingMainData.PerLevelData[building.CurrentLevel].DefencePointsPerDay}";

                if (_buildingManager.CanAssignWorker(building))
                {
                    script.SelectWorker.image.color = Color.green;
                    script.SelectWorker.interactable = _workersManager.WorkersAmount > 0;
                    script.SelectWorker.onClick.AddListener(() => TryToAssignWorkerFromUi(building, newGathering, true));
                }
                else
                {
                    script.SelectWorker.image.color = Color.yellow;
                    script.SelectWorker.interactable = true;
                    script.SelectWorker.onClick.AddListener(() => TryToAssignWorkerFromUi(building, newGathering, false));
                }
            }
        }

        private void TryToAssignWorkerFromUi(Building p_building, GameObject p_newGathering, bool p_assign)
        {
            OnButtonClicked(p_building, p_newGathering, p_assign);
            SingleGatheringUi script = p_newGathering.GetComponent<SingleGatheringUi>();
            script.SelectWorker.onClick.RemoveAllListeners();   

            if (p_assign)
            {
                script.SelectWorker.image.color = Color.yellow;                    
                script.SelectWorker.interactable = true;
                script.SelectWorker.onClick.AddListener(() => TryToAssignWorkerFromUi(p_building, p_newGathering, false));   
            }
            else
            {
                script.SelectWorker.image.color = Color.green;
                script.SelectWorker.interactable = _workersManager.WorkersAmount > 0;
                script.SelectWorker.onClick.AddListener(() => TryToAssignWorkerFromUi(p_building, p_newGathering, true));   
            }
        }
        
        private void OnButtonClicked(Building p_building, GameObject p_panelUi, bool p_assign)
        {
            _buildingManager.AssignWorker(p_building, p_assign);

            p_panelUi.GetComponent<Image>().color = !p_assign ? Color.gray : Color.magenta;
        }
    }
}