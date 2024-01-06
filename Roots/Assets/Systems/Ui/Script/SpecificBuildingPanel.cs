using System;
using System.Collections.Generic;
using System.Linq;
using AudioSystem;
using Buildings;
using GameManager;
using GeneralSystems;
using Narrator;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace InGameUi
{
    public class SpecificBuildingPanel : MonoBehaviour
    {
        [SerializeField] private AudioManager _audioManager;
        [SerializeField] private NarratorManager _narratorManager;
        [SerializeField] private BuildingsManager buildingsManager;
        [SerializeField] private MainGameManager _gameManager;
        
        [SerializeField] private TextMeshProUGUI _buildingName;
        [SerializeField] private TextMeshProUGUI _description;
        [Header("Lvl Up Slider")]
        [SerializeField] private Slider _levelUpProgression;
        [SerializeField] private TextMeshProUGUI _sliderValue;
        [SerializeField] private GameObject _sliderGos;

        [Header("Efficiency Slider")]
        [SerializeField] private Slider _efficiencySlider;
        [SerializeField] private TextMeshProUGUI _efficiencySliderValue;
        [Header("Duration Slider")]
        [SerializeField] private Slider _durationSlider;
        [SerializeField] private TextMeshProUGUI _durationSliderValue;

        [SerializeField] private GameObject _buildingIconPrefab;
        [SerializeField] private GameObject _goBackGo;
        [SerializeField] private GameObject _lvlUpGo;
        [SerializeField] private GameObject _getIntoWorkGo;
        [SerializeField] private GameObject _scrollBarGo;
        [SerializeField] private Transform _contentTransform;

        private bool _areRequirementsMet;
        private Building _building;
        private BuildingData _buildingData;

        private bool _canDevelopTechnology;
        private Button _goBackButton;
        private Button _lvlUpButton;

        private TextMeshProUGUI _lvlUpButtonText;

        private List<GameObject> _runtimeBuildingsUiToDestroy;
        private Button _startMiniGameButton;
        private TechnologyDataPerLevel[] _technology;

        public event Action<Building> OnOpenMiniGameOfType;
        
        private void Start()
        {
            buildingsManager.OnBuildingClicked += ActivateOnClick;

            _runtimeBuildingsUiToDestroy = new List<GameObject>();

            _goBackButton = _goBackGo.GetComponent<Button>();
            _lvlUpButton = _lvlUpGo.GetComponent<Button>();
            _lvlUpButtonText = _lvlUpGo.GetComponentInChildren<TextMeshProUGUI>();
            _startMiniGameButton = _getIntoWorkGo.GetComponent<Button>();

            gameObject.SetActive(false);
        }

        private void OnDestroy()
        {
            buildingsManager.OnBuildingClicked -= ActivateOnClick;
        }

        private void ClosePanel()
        {
            foreach (var createdUiElement in _runtimeBuildingsUiToDestroy) Destroy(createdUiElement);

            CameraController.IsUiOpen = false;
            GameplayHud.BlockHud = false;

            if (_building != null)
            {
                if (_building.BuildingMainData.Type == BuildingType.Farm)
                {
                    _narratorManager.TryToActivateNarrator(TutorialStep.OnFarmPanelClosed_Q15);
                }
            }

            _building = null;
            _runtimeBuildingsUiToDestroy.Clear();
            gameObject.SetActive(false);
        }

        public void ActivateOnClick(Building p_building)
        {
            ClosePanel();
            gameObject.SetActive(true);
            CameraController.IsUiOpen = true;
            GameplayHud.BlockHud = true;

            _lvlUpButton.onClick.RemoveAllListeners();
            _lvlUpButton.interactable = false;

            _startMiniGameButton.onClick.RemoveAllListeners();
            _startMiniGameButton.onClick.AddListener(() => StartMiniGame(p_building));
            _startMiniGameButton.interactable = _gameManager.CanPlayMinigame && p_building.CanPlayMinigame();

            _goBackButton.interactable = true;
            _goBackButton.onClick.AddListener(ClosePanel);

            _building = p_building;
            _buildingData = p_building.BuildingMainData;

            _buildingIconPrefab.SetActive(true);
            _goBackGo.SetActive(true);
            _lvlUpGo.SetActive(true);
            _getIntoWorkGo.SetActive(true);
            _scrollBarGo.SetActive(true);
            _sliderGos.SetActive(true);
            GetComponent<RectTransform>().sizeDelta = new Vector2(900, 1200);
            _buildingName.text = $"{_buildingData.Type} ({_building.CurrentLevel} lvl)";

            if (_buildingData.Type == BuildingType.Cottage)
            {
                HandleCottageView();
                return;
            }

            _technology = _buildingData.Technology.DataPerTechnologyLevel;
            HandleView();

            if (_narratorManager.CurrentTutorialStep == TutorialStep.OnFinishedFarm_Q9)
            {
                if (p_building.BuildingMainData.Type == BuildingType.Farm)
                {
                    _narratorManager.TryToActivateNarrator(TutorialStep.OnFarmPanelOpen_Q10);
                }
            }
            else if (_narratorManager.CurrentTutorialStep == TutorialStep.OnFourthWorkingPanelOpen_Q11)
            {
                if (p_building.BuildingMainData.Type == BuildingType.Farm && p_building.CurrentTechnologyDayOnQueue == 
                    p_building.BuildingMainData.Technology.DataPerTechnologyLevel[p_building.CurrentTechnologyLvl].WorksDayToAchieve)
                {
                    _narratorManager.TryToActivateNarrator(TutorialStep.OnFarmPanelWithTechnology_Q12);
                }
            }
        }

        private void HandleCottageView()
        {
            var amountOfStorageInBasement = _buildingData.PerLevelData[_building.CurrentLevel].ProductionAmountPerDay;
            _levelUpProgression.minValue = 0;
            _levelUpProgression.maxValue = amountOfStorageInBasement;

            _description.text = _buildingData.Description;

            if (amountOfStorageInBasement >= buildingsManager.CurrentResourcePoints)
                _levelUpProgression.value = _levelUpProgression.maxValue;
            else
                _levelUpProgression.value = buildingsManager.CurrentResourcePoints;

            _sliderValue.text =
                $"Resources stored in basement: {_levelUpProgression.value}/{_levelUpProgression.maxValue}";
            _getIntoWorkGo.SetActive(false);
            _lvlUpGo.SetActive(false);
            _getIntoWorkGo.SetActive(false);
            _scrollBarGo.SetActive(false);
            _sliderGos.SetActive(false);
            GetComponent<RectTransform>().sizeDelta = new Vector2(900, 700);
        }

        private void HandleView()
        {
            _description.text = _buildingData.Description;
            _levelUpProgression.minValue = 0;
            _levelUpProgression.maxValue = _technology[_building.CurrentTechnologyLvl].WorksDayToAchieve;
            _levelUpProgression.value = _building.CurrentTechnologyDayOnQueue;
            _sliderValue.text = $"Working days: {_levelUpProgression.value}/{_levelUpProgression.maxValue}";
            
            _efficiencySlider.minValue = 0;
            _efficiencySlider.maxValue = _buildingData.Technology.DataPerTechnologyLevel.Last().Efficiency;
            _efficiencySlider.value = _technology[_building.CurrentTechnologyLvl].Efficiency;
            _efficiencySliderValue.text = $"{_efficiencySlider.value}/{_efficiencySlider.maxValue}";
            
            _durationSlider.minValue = 0;
            _durationSlider.maxValue = _buildingData.Technology.DataPerTechnologyLevel.Last().Efficiency;
            _durationSlider.value = _technology[_building.CurrentTechnologyLvl].Efficiency;
            _durationSliderValue.text = $"{_efficiencySlider.value}/{_efficiencySlider.maxValue}";
            
            var nextLevel = _building.CurrentTechnologyLvl;
            nextLevel++;

            if (Math.Abs(_levelUpProgression.value - _levelUpProgression.maxValue) < 0.5)
                _canDevelopTechnology = true;
            else
                _canDevelopTechnology = false;

            foreach (var building in _technology[_building.CurrentTechnologyLvl].OtherBuildingsRequirements)
            {
                var currentBuilding =
                    buildingsManager.CurrentBuildings.Find(x => x.BuildingMainData.Type == building.Building);

                var newIcon = Instantiate(_buildingIconPrefab, _contentTransform);
                _runtimeBuildingsUiToDestroy.Add(newIcon);
                var uiReferences = newIcon.GetComponent<ButtonIconPrefabRefs>();

                if (_building.CurrentLevel == _technology[_building.CurrentTechnologyLvl].RequiredBuildingLevel)
                    _areRequirementsMet = true;

                if (currentBuilding == null)
                {
                    var currentBuildingData = buildingsManager.AllBuildingsDatabase.allBuildings.First(x =>
                        x.Type == building.Building);

                    uiReferences.BuildingIcon.sprite = currentBuildingData.Icon;
                    uiReferences.NewGo.SetActive(true);
                    uiReferences.InfoGo.SetActive(true);
                    uiReferences.NewInfo.text = "Not built";
                    uiReferences.Informations.text = $"Need lvl: {building.Level}";
                    uiReferences.Informations.color = Color.red;
                    _areRequirementsMet = false;
                }
                else
                {
                    uiReferences.BuildingIcon.sprite = currentBuilding.BuildingMainData.Icon;
                    uiReferences.NewGo.SetActive(false);
                    uiReferences.InfoGo.SetActive(true);
                    uiReferences.Informations.text = $"Need lvl: {building.Level}";

                    if (currentBuilding.CurrentLevel >= building.Level)
                    {
                        uiReferences.Informations.color = Color.green;
                    }
                    else
                    {
                        uiReferences.Informations.color = Color.red;

                        _areRequirementsMet = false;
                    }
                }
            }

            if (_building.CurrentTechnologyLvl + 1 > _building.BuildingMainData.Technology.DataPerTechnologyLevel.Length)
            {
                _lvlUpButtonText.text = "Max Level";
                _lvlUpButton.interactable = false;

                return;
            }
            
            if (_canDevelopTechnology && _areRequirementsMet)
            {
                _lvlUpGo.GetComponentInChildren<TextMeshProUGUI>().text =
                    $"{_building.CurrentTechnologyLvl} >> {nextLevel}";
                _lvlUpButton.interactable = true;
                _lvlUpButton.onClick.AddListener(() => UpgradeTechnology(_building));
                return;
            }

            if (!_canDevelopTechnology && _areRequirementsMet)
            {
                _lvlUpButtonText.text = "Assign more workers";
                _lvlUpButton.interactable = false;
                return;
            }
            
            if (_canDevelopTechnology && !_areRequirementsMet)
            {
                _lvlUpButtonText.text = "Meet Requirements to Develop Technologies";
                _lvlUpButton.interactable = false;
                return;
            }
        }

        private void UpgradeTechnology(Building p_building)
        {
            p_building.UpgradeTechnologyLevel();

            ActivateOnClick(p_building);
        }

        private void StartMiniGame(Building p_building)
        {
            _gameManager.OnMinigameActivity(true);
            gameObject.SetActive(false);
            p_building.PlayedMinigame = true;
            OnOpenMiniGameOfType?.Invoke(p_building);
        }
    }
}