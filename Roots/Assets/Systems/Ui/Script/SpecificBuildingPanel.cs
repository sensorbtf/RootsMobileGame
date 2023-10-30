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
        [SerializeField] private BuildingsManager buildingsManager;
        [SerializeField] private GameObject _buildingIconPrefab;
        [SerializeField] private GameObject _goBackGo;
        [SerializeField] private GameObject _lvlUpGo;
        [SerializeField] private GameObject _getIntoWorkGo;
        [SerializeField] private GameObject _scrollBarGo;
        [SerializeField] private Transform _contentTransform;

        private bool _areRequirementsMet;
        private Building _building;
        private BuildingData _buildingData;

        [SerializeField] private TextMeshProUGUI _buildingName;
        private bool _canDevelopTechnology;
        [SerializeField] private TextMeshProUGUI _description;
        private Button _goBackButton;
        [SerializeField] private Slider _levelUpProgression;
        private Button _lvlUpButton;

        private TextMeshProUGUI _lvlUpButtonText;

        private List<GameObject> _runtimeBuildingsUiToDestroy;
        [SerializeField] private TextMeshProUGUI _sliderValue;
        private Button _startMiniGameButton;
        private TechnologyDataPerLevel[] _technology;

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

        public event Action<Building> OnOpenMiniGameOfType;

        private void ClosePanel()
        {
            foreach (var createdUiElement in _runtimeBuildingsUiToDestroy) Destroy(createdUiElement);

            CameraController.IsUiOpen = false;
            GameplayHud.BlockHud = false;

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
            _startMiniGameButton.interactable = p_building.CanPlayMinigame();

            _goBackButton.interactable = true;
            _goBackButton.onClick.AddListener(ClosePanel);

            _building = p_building;
            _buildingData = p_building.BuildingMainData;

            _buildingIconPrefab.SetActive(true);
            _goBackGo.SetActive(true);
            _lvlUpGo.SetActive(true);
            _getIntoWorkGo.SetActive(true);
            _scrollBarGo.SetActive(true);

            _buildingName.text = $"{_buildingData.Type} ({_building.CurrentLevel})";

            if (_buildingData.Type == BuildingType.Cottage)
            {
                HandleCottageView();
                return;
            }

            _technology = _buildingData.Technology.DataPerTechnologyLevel;
            HandleView();
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
        }

        private void HandleView()
        {
            _description.text = _buildingData.Description;
            _levelUpProgression.minValue = 0;
            _levelUpProgression.maxValue = _technology[_building.CurrentTechnologyLvl].WorksDayToAchieve;
            _levelUpProgression.value = _building.CurrentTechnologyDayOnQueue;
            _sliderValue.text = $"{_levelUpProgression.value}/{_levelUpProgression.maxValue}";
            _getIntoWorkGo.GetComponentInChildren<TextMeshProUGUI>().text = "Start Work \n" +
                                                                            $"Efficiency: {_technology[_building.CurrentTechnologyLvl].Efficiency}\n Duration: {_technology[_building.CurrentTechnologyLvl].MinigameDuration} seconds.";

            var nextLevel = _building.CurrentTechnologyLvl;
            nextLevel++;
            var techInfo =
                $"Efficiency: {_technology[_building.CurrentTechnologyLvl].Efficiency} >> {_technology[nextLevel].Efficiency} \n Duration: {_technology[_building.CurrentTechnologyLvl].MinigameDuration} >> {_technology[nextLevel].MinigameDuration} seconds.";

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

                    uiReferences.BuildingIcon.image.sprite = currentBuildingData.Icon;
                    uiReferences.NewGo.SetActive(true);
                    uiReferences.InfoGo.SetActive(true);
                    uiReferences.NewInfo.text = "Not built";
                    uiReferences.Informations.text = $"Need lvl: {building.Level}";
                    uiReferences.Informations.color = Color.red;
                    _areRequirementsMet = false;
                }
                else
                {
                    uiReferences.BuildingIcon.image.sprite = currentBuilding.BuildingMainData.Icon;
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

            if (_areRequirementsMet)
            {
                _lvlUpButtonText.text = "Assign workers here to develop technologies";
                _lvlUpGo.GetComponentInChildren<TextMeshProUGUI>().text =
                    $"Technology Level:  {_building.CurrentTechnologyLvl}";
            }
            else if (!_areRequirementsMet)
            {
                _lvlUpButtonText.text = "Meet Requirements to Develop Technologies";
                _lvlUpGo.GetComponentInChildren<TextMeshProUGUI>().text =
                    $"Technology Level:  {_building.CurrentTechnologyLvl}";
            }

            if (_canDevelopTechnology)
            {
                _lvlUpGo.GetComponentInChildren<TextMeshProUGUI>().text =
                    $"{_building.CurrentTechnologyLvl} >> {nextLevel}";
                _lvlUpButtonText.text = "Develop technology to get better in mini game: \n" + techInfo;
                _lvlUpButton.interactable = true;
                _lvlUpButton.onClick.AddListener(() => UpgradeTechnology(_building));
            }
        }

        private void UpgradeTechnology(Building p_building)
        {
            p_building.UpgradeTechnologyLevel();

            ActivateOnClick(p_building);
        }

        private void StartMiniGame(Building p_building)
        {
            gameObject.SetActive(false);
            p_building.PlayedMinigame = true;
            OnOpenMiniGameOfType?.Invoke(p_building);
        }
    }
}