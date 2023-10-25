using System;
using System.Collections.Generic;
using System.Linq;
using Buildings;
using GeneralSystems;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;
using World;

namespace InGameUi
{
    public class BuildingDefendPanel : MonoBehaviour
    {
        [FormerlySerializedAs("_buildingManager")] [SerializeField] private BuildingsManager buildingsManager;
        [SerializeField] private WorkersManager _workersManager;
        [SerializeField] private WorldManager _worldManager;

        [SerializeField] private TextMeshProUGUI _panelTitle;
        [SerializeField] private TextMeshProUGUI _numberOfWorkers;
        [SerializeField] private GameObject _buildingEntryPrefab;
        [SerializeField] private GameObject _endAssigningGo;
        [SerializeField] private Transform contentTransform;

        private Button _endAssigmentButton;

        private List<GameObject> _runtimeBuildingsUiToDestroy;
        private List<Building> _buildingsOnInPanelQueue;

        private void Start()
        {
            _runtimeBuildingsUiToDestroy = new List<GameObject>();
            _buildingsOnInPanelQueue = new List<Building>();

            _endAssigmentButton = _endAssigningGo.GetComponent<Button>();
            _endAssigmentButton.onClick.AddListener(ClosePanel);
            
            _worldManager.OnDefendingVillage += OpenPanel;
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

            _runtimeBuildingsUiToDestroy.Clear();

            _worldManager.HandleEndMissionConsequences(true, true);
            gameObject.SetActive(false);
        }

        private void OpenPanel()
        {
            gameObject.SetActive(true);
            CameraController.IsUiOpen = true;
            GameplayHud.BlockHud = true;

            UpdatePanelState();
            HandleBuildingsCreation();
        }

        private void HandleBuildingsCreation()
        {
            foreach (var building in buildingsManager.CurrentBuildings)
            {
                var newBuilding = Instantiate(_buildingEntryPrefab, contentTransform);
                _runtimeBuildingsUiToDestroy.Add(newBuilding);

                BuildingWorkerRefs script = newBuilding.GetComponent<BuildingWorkerRefs>();
                script.BuildingImage.sprite = building.BuildingMainData.Icon;
                script.BuildingName.text = building.BuildingMainData.Type.ToString();
                script.DefendToggle.interactable = true;

                script.DefendToggle.onValueChanged.AddListener(delegate
                {
                    HandleToggleValueChanged(building, script.DefendToggle);
                });
            }
        }

        private void UpdatePanelState()
        {
            _numberOfWorkers.text = $"Workers: {_workersManager.BaseWorkersAmounts.ToString()}/{_workersManager.OverallAssignedWorkers}";
            
            _endAssigningGo.SetActive(!_workersManager.IsAnyWorkerFree());
            RefreshToggles();
        }
        
        private void RefreshToggles()
        {
            if (_workersManager.IsAnyWorkerFree())
            {
                foreach (var building in _runtimeBuildingsUiToDestroy)
                {
                    building.GetComponent<BuildingWorkerRefs>().DefendToggle.interactable = true;
                }  
            }
            else
            {
                foreach (var building in _runtimeBuildingsUiToDestroy)
                {
                    BuildingWorkerRefs script = building.GetComponent<BuildingWorkerRefs>();
                
                    if (!script.DefendToggle.isOn)
                    {
                        script.DefendToggle.interactable = false;
                    }
                }  
            }
        }

        private void HandleToggleValueChanged(Building p_building, Toggle p_scriptDefendToggle)
        {
            if (p_scriptDefendToggle.isOn)
            {
                _workersManager.WorkersDefending++;
                p_building.IsProtected = true;
            }
            else
            {
                _workersManager.WorkersDefending--;
                p_building.IsProtected = false;
            }
            
            UpdatePanelState();
        }
    }
}