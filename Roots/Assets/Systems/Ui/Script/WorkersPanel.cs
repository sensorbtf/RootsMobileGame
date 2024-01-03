using System;
using System.Collections.Generic;
using System.Linq;
using AudioSystem;
using Buildings;
using GameManager;
using GeneralSystems;
using Gods;
using Narrator;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace InGameUi
{
    public class WorkersPanel : MonoBehaviour
    {
        [SerializeField] private BuildingsManager buildingsManager;
        [SerializeField] private WorkersManager _workersManager;
        [SerializeField] private MainGameManager _gameManager;
        [SerializeField] private GodsManager _godsManager;
        [SerializeField] private NarratorManager _narratorManager;
        [SerializeField] private AudioManager _audioManager;

        [SerializeField] private BuildingPanel _buildingPanel;
        [SerializeField] private GatheringDefensePanel _gatheringDefensePanel;
        [SerializeField] private GodsPanel _godsPanel;
        [SerializeField] private GameObject _iconPrefab;
        [SerializeField] private GameObject _barPrefab;
        [SerializeField] private Transform contentTransform;

        [SerializeField] private GameObject _finishAssigning;
        [SerializeField] private Button _activateButton;
        [SerializeField] private TextMeshProUGUI _activateButtonText;
        [SerializeField] private Button _godsButton;
        [SerializeField] private TextMeshProUGUI _numberOfWorkers;

        private List<GameObject> _runtimeBuildingsUiToDestroy;
        
        [SerializeField] private TextMeshProUGUI _tabName;

        private void Start()
        {
            _buildingPanel.OnBackToWorkersPanel += ActivatePanel;
            _gatheringDefensePanel.OnBackToWorkersPanel += ActivatePanel;
            _godsPanel.OnBackToWorkersPanel += ActivatePanel;
            _godsPanel.OnGodsPanelOpened += ClosePanel;
            _gameManager.OnPlayerStateChange += ActivatePanel;
            _godsButton.onClick.AddListener(delegate
            {
                _audioManager.PlayButtonSoundEffect(true);
                _godsPanel.ActivatePanel();
            });

            _runtimeBuildingsUiToDestroy = new List<GameObject>();

            gameObject.SetActive(false);
        }

        private void OnDestroy()
        {
            _buildingPanel.OnBackToWorkersPanel -= ActivatePanel;
            _gatheringDefensePanel.OnBackToWorkersPanel -= ActivatePanel;
            _godsPanel.OnBackToWorkersPanel -= ActivatePanel;
            _godsPanel.OnGodsPanelOpened -= ClosePanel;
            _gameManager.OnPlayerStateChange -= ActivatePanel;
        }

        private void ActivatePanel(DuringDayState p_currentState)
        {
            if (p_currentState == DuringDayState.SettingWorkers)
                ActivatePanel();
        }
        
        private void ActivatePanel()
        {
            TryToOpenNarrator();
            
            gameObject.SetActive(true);
            GameplayHud.BlockHud = true;
            CameraController.IsUiOpen = true;
            
            _tabName.text = "Worker Displacement";
            
            UpdateWorkersText();
            UpdateButtonText();

            for (var i = 0; i < 3; i++)
            {
                var newBar = Instantiate(_barPrefab, contentTransform);
                _runtimeBuildingsUiToDestroy.Add(newBar);
                var scriptOfBar = newBar.GetComponent<WorkersDisplacementBarRefs>();
                var points = 0;
                GameObject newEntry = null;

                switch (i)
                {
                    case 0:
                        scriptOfBar.BarText.text = "Buildings";

                        foreach (var data in _buildingPanel.BuildingsToShow)
                        {
                            newEntry = Instantiate(_iconPrefab, scriptOfBar.ScrollContext);
                            var references = newEntry.GetComponent<ButtonIconPrefabRefs>();

                            references.NewGo.SetActive(true);
                            references.InfoGo.SetActive(true);
                            references.NewInfo.text = "In Progress";

                            if (data.Value) references.NewInfo.text = "Will be build";

                            var building = buildingsManager.CurrentBuildings.Find(x => x.BuildingMainData == data.Key);

                            if (building != null)
                            {
                                references.BuildingIcon.sprite = data.Key.Icon;
                                var daysToComplete =
                                    building.BuildingMainData.PerLevelData[building.CurrentLevel].Requirements
                                        .DaysToComplete - building.CurrentDayOnQueue;

                                if (building.IsDamaged && !building.HaveWorker)
                                    references.Informations.text = "End in: 1 day";
                                else
                                    references.Informations.text = $"End in: {daysToComplete} day(s)";
                                
                                if (data.Value)
                                    continue;

                                references.NewInfo.text = "Paused";
                                var willBeCancelled =
                                    _buildingPanel.WillBuildingBeCancelled(building, out var wasOnList);

                                if (building.HaveWorker && willBeCancelled)
                                    references.NewInfo.text = "Will Be Paused";
                                else if (building.IsDamaged && !building.HaveWorker && !willBeCancelled && wasOnList)
                                    references.NewInfo.text = "Will be repaired";
                                else if (building.IsDamaged && !building.HaveWorker && willBeCancelled && wasOnList)
                                    references.NewInfo.text = "Will be Paused";
                                else if (!building.HaveWorker && !willBeCancelled && wasOnList)
                                    references.NewInfo.text = "Will be resumed";
                                else if (building.HaveWorker) references.NewInfo.text = "In Progress";
                            }
                            else
                            {
                                references.BuildingIcon.sprite = data.Key.Icon;
                            }
                        }

                        scriptOfBar.BarButton.onClick.AddListener(OnBuildOrUpgradeButtonClicked);
                        if (_narratorManager.CurrentTutorialStep == TutorialStep.AfterRankUp_Q16)
                        {
                            var gt = buildingsManager.GetSpecificBuilding(BuildingType.GuardTower);

                            if (gt == null || gt.CurrentLevel != 1)
                            {
                                scriptOfBar.BarButton.interactable = true;
                            }
                            else
                            {
                                scriptOfBar.BarButton.interactable = false;
                            }
                        }
                        else
                            scriptOfBar.BarButton.interactable = !_narratorManager.ShouldBlockBuildingTab();
                        break;
                    case 1:
                        foreach (var building in _gatheringDefensePanel.BuildingsOnQueue)
                        {
                            if (building.BuildingMainData.PerLevelData[building.CurrentLevel].ProductionType !=
                                PointsType.ResourcesAndDefense &&
                                building.BuildingMainData.PerLevelData[building.CurrentLevel].ProductionType !=
                                PointsType.Resource)
                                continue;

                            points += buildingsManager.GetProductionOfBuilding(building.BuildingMainData.Type);
                            
                            newEntry = Instantiate(_iconPrefab, scriptOfBar.ScrollContext);

                            var references = newEntry.GetComponent<ButtonIconPrefabRefs>();
                            
                            references.BuildingIcon.sprite = building.BuildingMainData.Icon;
                            references.NewGo.SetActive(false);
                            references.InfoGo.SetActive(true);
                            references.GlowEffect.color = GetColor(building.BuildingMainData.GodType);
                            references.Informations.text = "Worker Assigned";
                        }

                        scriptOfBar.BarText.text = $"Resource Points: +{points}";
                        scriptOfBar.BarButton.onClick.AddListener(() => OnGatheringOrDefenseButtonClicked(true));
                        
                        scriptOfBar.BarButton.interactable = !_narratorManager.ShouldBlockResource();
                        break;
                    case 2:

                        foreach (var building in _gatheringDefensePanel.BuildingsOnQueue)
                        {
                            if (building.BuildingMainData.PerLevelData[building.CurrentLevel].ProductionType !=
                                PointsType.ResourcesAndDefense &&
                                building.BuildingMainData.PerLevelData[building.CurrentLevel].ProductionType !=
                                PointsType.Defense)
                                continue;

                            points += buildingsManager.GetProductionOfBuilding(building.BuildingMainData.Type);

                            newEntry = Instantiate(_iconPrefab, scriptOfBar.ScrollContext);

                            var references = newEntry.GetComponent<ButtonIconPrefabRefs>();

                            references.BuildingIcon.sprite = building.BuildingMainData.Icon;
                            references.NewGo.SetActive(false);
                            references.InfoGo.SetActive(true);
                            references.GlowEffect.color = GetColor(building.BuildingMainData.GodType);
                            references.Informations.text = "Worker Assigned";
                        }

                        scriptOfBar.BarText.text = $"Defense Points: +{points}";

                        scriptOfBar.BarButton.onClick.AddListener(() => OnGatheringOrDefenseButtonClicked(false));
                        
                        if (_narratorManager.CurrentTutorialStep == TutorialStep.AfterRankUp_Q16)
                        {
                            var gt = buildingsManager.GetSpecificBuilding(BuildingType.GuardTower);

                            if (gt != null && gt.CurrentLevel == 1)
                            {
                                scriptOfBar.BarButton.interactable = true;
                            }
                            else
                            {
                                scriptOfBar.BarButton.interactable = false;
                            }
                        }
                        else
                        {
                            scriptOfBar.BarButton.interactable = !_narratorManager.ShouldBlockDefense();
                        }
                        
                        break;
                }
            }
        }

        private Color GetColor(GodType p_godType)
        {
            return _godsManager.GetCurrentBlessingLevel(p_godType) switch
            {
                BlessingLevel.Noone => _godsManager.NoEffect,
                BlessingLevel.Small => _godsManager.SmallEffect,
                BlessingLevel.Medium => _godsManager.MediumEffect,
                BlessingLevel.Big => _godsManager.BigEffect,
                _ => throw new ArgumentOutOfRangeException()
            };
        }
        
        private void ClosePanel()
        {
            foreach (var createdUiElement in _runtimeBuildingsUiToDestroy) 
                Destroy(createdUiElement);

            _runtimeBuildingsUiToDestroy.Clear();
            CameraController.IsUiOpen = false;
            GameplayHud.BlockHud = false;
            gameObject.SetActive(false);
        }

        private void AssignWorkersForNewDay()
        {
            _audioManager.PlayButtonSoundEffect(true);
            
            _buildingPanel.ConfirmWorkersAssigment();
            _gatheringDefensePanel.ConfirmWorkersAssigment();
            _workersManager.ResetAssignedWorkers();

            foreach (var building in _buildingPanel.BuildingsToShow.ToList())
                _buildingPanel.BuildingsToShow[building.Key] = false;
            
            _gameManager.SetPlayerState(DuringDayState.DayPassing);
            
            _narratorManager.TryToActivateNarrator(TutorialStep.OnFirstDayStarted_Q4);
                
            ClosePanel();
        }

        private void OnBuildOrUpgradeButtonClicked()
        {
            _audioManager.PlayButtonSoundEffect(true);
            ClosePanel();
            _buildingPanel.HandleView(true);
        }

        private void OnGatheringOrDefenseButtonClicked(bool p_gathering)
        {
            _audioManager.PlayButtonSoundEffect(true);
            ClosePanel();
            _gatheringDefensePanel.HandleView(p_gathering);
        }

        private void UpdateWorkersText()
        {
            _buildingPanel.RefreshWorkersAmount();
            _numberOfWorkers.text =
                $"Workers: {_workersManager.BaseWorkersAmounts}/{_workersManager.OverallAssignedWorkers}";
        }

        private void UpdateButtonText()
        {
            _finishAssigning.SetActive(true);

            if (_workersManager.BaseWorkersAmounts - _workersManager.OverallAssignedWorkers == 0 ||
                buildingsManager.CurrentBuildings.Count - 1 < _workersManager.BaseWorkersAmounts)
            {
                _activateButton.onClick.RemoveAllListeners();
                _activateButton.onClick.AddListener(AssignWorkersForNewDay);
                _activateButton.interactable = true;
                _activateButtonText.text = "Start the day";
            }
            else
            {
                _activateButtonText.text = "Set workers to work";
                _activateButton.interactable = false;
            }
        }
        
        private void TryToOpenNarrator()
        {
            if (_narratorManager.CurrentTutorialStep == TutorialStep.OnMissionRestart_Q20)
            {
                _narratorManager.TryToActivateNarrator(TutorialStep.OnWorkersPanelOpenAfterRestart_Q21);
                _godsButton.gameObject.SetActive(true);
            }
            else
            {
                _godsButton.gameObject.SetActive(false);
            }
            
            _narratorManager.TryToActivateNarrator(TutorialStep.OnFourthWorkingPanelOpen_Q11);
            _narratorManager.TryToActivateNarrator(TutorialStep.OnThirdWorkingPanelOpen_Q7);
            _narratorManager.TryToActivateNarrator(TutorialStep.OnSecondWorkingPanelOpen_Q3);
            _narratorManager.TryToActivateNarrator(TutorialStep.OnFirstWorkingPanelOpen_Q2);
            _narratorManager.TryToActivateNarrator(TutorialStep.OnGameStarted_Q1);
        }
    }
}