using System;
using System.Collections.Generic;
using System.Linq;
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

        [SerializeField] private BuildingPanel _buildingPanel;
        [SerializeField] private GatheringDefensePanel _gatheringDefensePanel;

        [SerializeField] private TextMeshProUGUI _tabName;
        [SerializeField] private TextMeshProUGUI _numberOfWorkers;
        [SerializeField] private GameObject _iconPrefab;
        [SerializeField] private GameObject _barPrefab;
        [SerializeField] private GameObject _finishWorkersAssigningButton;
        [SerializeField] private Transform contentTransform;

        private List<GameObject> _runtimeBuildingsUiToDestroy;
        private Button _button;
        private TextMeshProUGUI _buttonText;
        public event Action OnBackToMap;

        private void Start()
        {
            _buildingPanel.OnBackToWorkersPanel += ActivatePanel;
            _gatheringDefensePanel.OnBackToWorkersPanel += ActivatePanel;
            _buttonText = _finishWorkersAssigningButton.GetComponentInChildren<TextMeshProUGUI>();
            _button = _finishWorkersAssigningButton.GetComponent<Button>();
            _runtimeBuildingsUiToDestroy = new List<GameObject>();
            gameObject.SetActive(false);
        }

        public void ActivatePanel()
        {
            gameObject.SetActive(true);
            GameplayHud.BlockHud = true;
            CameraController.IsUiOpen = true;

            _tabName.text = "Worker Displacement";

            UpdateWorkersText();
            UpdateButtonText();

            for (int i = 0; i < 3; i++)
            {
                var newBar = Instantiate(_barPrefab, contentTransform);
                _runtimeBuildingsUiToDestroy.Add(newBar);
                var scriptOfBar = newBar.GetComponent<WorkersDisplacementBarRefs>();
                int points = 0;
                GameObject newEntry = null;

                switch (i)
                {
                    case 0:
                        scriptOfBar.BarText.text = "Buildings";

                        foreach (var data in _buildingPanel.BuildingsToShow)
                        {
                            newEntry = Instantiate(_iconPrefab, scriptOfBar.ScrollContext);
                            ButtonIconPrefabRefs references = newEntry.GetComponent<ButtonIconPrefabRefs>();

                            references.NewGo.SetActive(true);
                            references.InfoGo.SetActive(true);
                            references.NewInfo.text = "In Progress";

                            if (data.Value)
                            {
                                references.NewInfo.text = "Will be build";
                            }

                            var building = _buildingManager.CurrentBuildings.Find(x => x.BuildingMainData == data.Key);

                            if (building != null)
                            {
                                references.BuildingIcon.image.sprite = data.Key.Icon;
                                var daysToComplete = 
                                    building.BuildingMainData.PerLevelData[building.CurrentLevel].Requirements
                                        .DaysToComplete - building.CurrentDayOnQueue;
                                
                                references.Informations.text = $"End in: {daysToComplete} day(s)";

                                if (data.Value) 
                                    continue;
                                
                                references.NewInfo.text = "Paused";
                                var willBeCancelled =
                                    _buildingPanel.WillBuildingBeCancelled(building, out bool wasOnList);

                                if (building.HaveWorker && willBeCancelled)
                                {
                                    references.NewInfo.text = "Will Be Paused";
                                }
                                else if (building.IsDamaged && !building.HaveWorker && !willBeCancelled && wasOnList)
                                {
                                    references.NewInfo.text = "Will be repaired";
                                }
                                else if (building.IsDamaged && !building.HaveWorker && willBeCancelled && wasOnList)
                                {
                                    references.NewInfo.text = "Will be Paused";
                                }
                                else if (!building.HaveWorker && !willBeCancelled && wasOnList)
                                {
                                    references.NewInfo.text = "Will be resumed";
                                }
                                else if (building.HaveWorker)
                                {   
                                    references.NewInfo.text = "In Progress";
                                }
                            }
                            else
                            {
                                references.BuildingIcon.image.sprite = data.Key.Icon;
                            }
                        }

                        scriptOfBar.BarButton.onClick.AddListener(OnBuildOrUpgradeButtonClicked);
                        break;
                    case 1:
                        foreach (var building in _gatheringDefensePanel.BuildingsOnQueue)
                        {
                            if (building.BuildingMainData.PerLevelData[building.CurrentLevel].ProductionType != PointsType.ResourcesAndDefense &&
                                building.BuildingMainData.PerLevelData[building.CurrentLevel].ProductionType != PointsType.Resource)
                                continue;

                            points += building.BuildingMainData.PerLevelData[building.CurrentLevel].ProductionAmountPerDay;

                            newEntry = Instantiate(_iconPrefab, scriptOfBar.ScrollContext);
                            newEntry.GetComponent<Image>().sprite =
                                building.BuildingMainData.Icon;
                            
                            ButtonIconPrefabRefs references = newEntry.GetComponent<ButtonIconPrefabRefs>();

                            references.NewGo.SetActive(false);
                            references.InfoGo.SetActive(true);
                            
                            references.Informations.text = "Worker Assigned";
                        }

                        scriptOfBar.BarText.text = $"Resource Points: {points}";
                        scriptOfBar.BarButton.onClick.AddListener(() => OnGatheringOrDefenseButtonClicked(true));
                        break;
                    case 2:

                        foreach (var building in _gatheringDefensePanel.BuildingsOnQueue)
                        {
                            if (building.BuildingMainData.PerLevelData[building.CurrentLevel].ProductionType != PointsType.ResourcesAndDefense &&
                                building.BuildingMainData.PerLevelData[building.CurrentLevel].ProductionType != PointsType.Defense)
                                continue;

                            points += building.BuildingMainData.PerLevelData[building.CurrentLevel].ProductionAmountPerDay;

                            newEntry = Instantiate(_iconPrefab, scriptOfBar.ScrollContext);
                            newEntry.GetComponent<Image>().sprite = building.BuildingMainData.Icon;
                            
                            ButtonIconPrefabRefs references = newEntry.GetComponent<ButtonIconPrefabRefs>();

                            references.NewGo.SetActive(false);
                            references.InfoGo.SetActive(true);
                            
                            references.Informations.text = "Worker Assigned";
                        }

                        scriptOfBar.BarText.text = $"Defense Points: {points}";

                        scriptOfBar.BarButton.onClick.AddListener(() => OnGatheringOrDefenseButtonClicked(false));
                        break;
                }
            }
        }
        

        private void ClosePanel()
        {
            foreach (var createdUiElement in _runtimeBuildingsUiToDestroy)
            {
                Destroy(createdUiElement);
            }

            _runtimeBuildingsUiToDestroy.Clear();
            CameraController.IsUiOpen = false;
            GameplayHud.BlockHud = false;
            gameObject.SetActive(false);
        }

        private void AssignWorkersForNewDay()
        {
            _buildingPanel.ConfirmWorkersAssigment();
            _gatheringDefensePanel.ConfirmWorkersAssigment();
            _workersManager.ResetAssignedWorkers();

            foreach (var building in _buildingPanel.BuildingsToShow.ToList())
            {
                _buildingPanel.BuildingsToShow[building.Key] = false;
            }

            ClosePanel();
            OnBackToMap?.Invoke();
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

        private void UpdateWorkersText()
        {
            _buildingPanel.RefreshWorkersAmount();
            _numberOfWorkers.text = $"Workers: {_workersManager.BaseWorkersAmounts.ToString()}/{_workersManager.OverallAssignedWorkers}";
        }
        
        private void UpdateButtonText()
        {
            _finishWorkersAssigningButton.SetActive(true);

            if (_workersManager.BaseWorkersAmounts - _workersManager.OverallAssignedWorkers == 0)
            {
                _button.onClick.RemoveAllListeners();
                _button.onClick.AddListener(AssignWorkersForNewDay);
                _button.interactable = true;
                _buttonText.text = "Start the day";
            }
            else
            {
                _buttonText.text = "Set workers to work before starting the day";
                _button.interactable = false;
            }
        }
    }
}