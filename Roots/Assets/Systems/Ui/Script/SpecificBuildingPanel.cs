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
    public class SpecificBuildingPanel : MonoBehaviour
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
            //_buildingManager.OnBuildingClicked += ActivateOnClick;
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

        private void ActivateOnClick(BuildingData p_specificBuilding, int p_level)
        {
            _currentBuildingData = p_specificBuilding;
            _currentBuildingLevel = p_level;
        
            HandleView();
        }

        public void HandleView(bool p_fromWorkerPanel = false)
        {
            gameObject.SetActive(true);
            CameraController.IsUiOpen = true;
            GameplayHud.BlockHud = true;

            _buildingName.text = "Start Building";

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

            CreateTechnologies();
        }

        private void CreateTechnologies()
        {
            
        }
    }
}