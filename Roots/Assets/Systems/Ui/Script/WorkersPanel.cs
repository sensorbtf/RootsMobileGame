using System;
using System.Collections.Generic;
using Buildings;
using GeneralSystems;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using World;

// should have been "keep same displacement on new day"

namespace InGameUi
{
    public class WorkersPanel : MonoBehaviour
    {
        [SerializeField] private BuildingManager _buildingManager;
        [SerializeField] private WorkersManager _workersManager;
        [SerializeField] private WorldManager _worldManager;
        [SerializeField] private BuildingPanel _buildingPanel;
        [SerializeField] private GatheringDefensePanel _gatheringDefensePanel;

        [SerializeField] private TextMeshProUGUI _tabName;
        [SerializeField] private TextMeshProUGUI _numberOfWorkers;
        [SerializeField] private GameObject _iconPrefab;
        [SerializeField] private GameObject _barPrefab;
        [SerializeField] private GameObject _finishWorkersAssigningButton;
        [SerializeField] private Transform contentTransform;

        private List<GameObject> _runtimeBuildingsUiToDestroy;
        
        private void Start()
        {
            _worldManager.OnNewDayStarted += ActivatePanel;
            _buildingPanel.OnBackToWorkersPanel += ActivatePanel;
            _gatheringDefensePanel.OnBackToWorkersPanel += ActivatePanel;
            _workersManager.OnWorkersUpdated += UpdateWorkersText;
            _runtimeBuildingsUiToDestroy = new List<GameObject>();
            gameObject.SetActive(false);
        }

        private void UpdateWorkersText(int p_workers)
        {
            _numberOfWorkers.text = $"Workers: {p_workers.ToString()}";
        }

        private void OnDisable()
        {
            //_worldManager.OnNewDayStarted -= ActivatePanel;
            //_buildingPanel.OnBackToWorkersPanel -= ActivatePanel;
        }

        private void ActivatePanel()
        {
            gameObject.SetActive(true);
            GameplayHud.BlockHud = true;
            CameraController.IsUiOpen = true;
            _tabName.text = "Worker Displacement";

            if (_workersManager.WorkersAmount <= 0)
            {
                _finishWorkersAssigningButton.SetActive(true);
                _finishWorkersAssigningButton.GetComponent<Button>().onClick.AddListener(ClosePanel);
            }
            else
            {
                _finishWorkersAssigningButton.SetActive(false);
            }
            
            for (int i = 0; i < 3; i++)
            {
                var newBar = Instantiate(_barPrefab, contentTransform);
                _runtimeBuildingsUiToDestroy.Add(newBar);
                var scriptOfBar = newBar.GetComponent<WorkersDisplacementBarUi>();
                // scriptOfBar.BarSprite add when needed

                switch (i)
                {
                    case 0:
                        scriptOfBar.BarText.text = "Buildings";

                        foreach (var building in _buildingManager.CurrentBuildings)
                        {
                            if (!building.IsBeeingUpgradedOrBuilded)
                                continue;
                            
                            Instantiate(_iconPrefab, scriptOfBar.ScrollContext);
                            _iconPrefab.GetComponent<Image>().sprite = building.BuildingMainData.PerLevelData[building.CurrentLevel].Icon;
                        }

                        scriptOfBar.BarButton.onClick.AddListener(OnBuildOrUpgradeButtonClicked);
                        break;
                    case 1:
                        scriptOfBar.BarText.text = "Resources";
                        scriptOfBar.BarButton.onClick.AddListener(() => OnGatheringOrDefenseButtonClicked(true));
                        break;
                    case 2:
                        scriptOfBar.BarText.text = "Defense Points";
                        scriptOfBar.BarButton.onClick.AddListener(() => OnGatheringOrDefenseButtonClicked(false));
                        break;
                }
            }
            
            // View all buildings in cottage as it is like centrum dowodzenia for fast building
        }

        private void ClosePanel()
        {
            foreach (var createdUiElement in _runtimeBuildingsUiToDestroy)
            {
                Destroy(createdUiElement);
            }

            CameraController.IsUiOpen = false;

            _runtimeBuildingsUiToDestroy.Clear();
            GameplayHud.BlockHud = false;
            gameObject.SetActive(false);
        }

        private void OnBuildOrUpgradeButtonClicked()
        {
            ClosePanel();
            _buildingPanel.HandleView(true);
        }
        
        private void OnGatheringOrDefenseButtonClicked(bool p_gathering)
        {
            ClosePanel();
            _gatheringDefensePanel.HandleView(p_gathering);
        }
    }
}