using System;
using System.Collections.Generic;
using System.Linq;
using Buildings;
using GeneralSystems;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace InGameUi
{
    public class SpecificBuildingPanel : MonoBehaviour
    {
        [SerializeField] private BuildingManager _buildingManager;
        [SerializeField] private WorkersManager _workersManager;

        [SerializeField] private TextMeshProUGUI _buildingName;
        [SerializeField] private TextMeshProUGUI _description;
        [SerializeField] private Image _buildingIcon;
        [SerializeField] private GameObject _buildingIconPrefab;
        [SerializeField] private GameObject _goBackGo;
        [SerializeField] private GameObject _lvlUpGo;
        [SerializeField] private GameObject _getIntoWorkGo;
        [SerializeField] private Slider _levelUpProgression;
        [SerializeField] private TextMeshProUGUI _sliderValue;
        [SerializeField] private Transform _contentTransform;

        private Button _goBackButton;
        private Button _lvlUpButton;
        private Button _startMiniGameButton;
        
        private List<GameObject> _runtimeBuildingsUiToDestroy;
        private Building _currentBuilding;
        
        private bool _areRequirementsMet;
        private bool _canDevelopTechnology;
        private TechnologyDataPerLevel[] _technology;

        public TechnologyDataPerLevel TechnologyData => _technology[_currentBuilding.CurrentTechnologyLvl];
        public event Action<Building> OpenMiniGameOfType;

        private void Start()
        {
            _buildingManager.OnBuildingClicked += ActivateOnClick;

            _runtimeBuildingsUiToDestroy = new List<GameObject>();

            _goBackButton = _goBackGo.GetComponent<Button>();
            _lvlUpButton = _lvlUpGo.GetComponent<Button>();
            _startMiniGameButton = _getIntoWorkGo.GetComponent<Button>();

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

            _currentBuilding = null;
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
            
            _currentBuilding = p_building;
            _technology = p_building.BuildingMainData.Technology.DataPerTechnologyLevel;
            HandleView();
        }

        private void HandleView()
        {
            _buildingName.text = $"{_currentBuilding.BuildingMainData.Type.ToString()} ({_currentBuilding.CurrentLevel})";
            _buildingIcon.sprite = _currentBuilding.BuildingMainData.PerLevelData[_currentBuilding.CurrentLevel].Icon;
            _levelUpProgression.minValue = 0;
            _levelUpProgression.maxValue = _technology[_currentBuilding.CurrentTechnologyLvl].WorksDayToAchieve;
            _levelUpProgression.value = _currentBuilding.CurrentTechnologyDayOnQueue;
            _sliderValue.text = $"{_levelUpProgression.value}/{_levelUpProgression.maxValue}";
            _getIntoWorkGo.GetComponentInChildren<TextMeshProUGUI>().text = "Start Work \n" + $"Efficiency: {_technology[_currentBuilding.CurrentTechnologyLvl].Efficiency}\n Duration: {_technology[_currentBuilding.CurrentTechnologyLvl].MinigameDuration} seconds.";

            var nextLevel = _currentBuilding.CurrentTechnologyLvl;
            nextLevel++;
            string techInfo = $"Efficiency: {_technology[_currentBuilding.CurrentTechnologyLvl].Efficiency} >> {_technology[nextLevel].Efficiency} \n Duration: {_technology[_currentBuilding.CurrentTechnologyLvl].MinigameDuration} >> {_technology[nextLevel].MinigameDuration} seconds.";

            if (Math.Abs(_levelUpProgression.value - _levelUpProgression.maxValue) < 0.5)
            {
                _canDevelopTechnology = true;
            }
            else
            {
                _canDevelopTechnology = false;
            }

            foreach (var building in _technology[_currentBuilding.CurrentTechnologyLvl].OtherBuildingsRequirements)
            {
                var currentBuilding =
                    _buildingManager.CurrentBuildings.Find(x => x.BuildingMainData.Type == building.Building);

                GameObject newIcon = Instantiate(_buildingIconPrefab, _contentTransform);
                _runtimeBuildingsUiToDestroy.Add(newIcon);
                ButtonIconPrefabRefs uiReferences = newIcon.GetComponent<ButtonIconPrefabRefs>();

                if (_currentBuilding.CurrentLevel == _technology[_currentBuilding.CurrentTechnologyLvl].RequiredBuildingLevel)
                {
                    _areRequirementsMet = true;
                }

                if (currentBuilding == null)
                {
                    var currentBuildingData = _buildingManager.AllBuildingsDatabase.allBuildings.First(x =>
                        x.Type == building.Building);

                    uiReferences.BuildingIcon.image.sprite = currentBuildingData.PerLevelData[0].Icon;
                    uiReferences.NewGo.SetActive(true);
                    uiReferences.InfoGo.SetActive(true);
                    uiReferences.NewInfo.text = "Not built";
                    uiReferences.Informations.text = $"Need lvl: {building.Level}";
                    uiReferences.Informations.color = Color.red;
                    _areRequirementsMet = false;
                }
                else
                {
                    uiReferences.BuildingIcon.image.sprite = currentBuilding.BuildingMainData
                        .PerLevelData[currentBuilding.CurrentLevel].Icon;
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
                _description.text = $"Assign workers here to develop technologies";
                _lvlUpGo.GetComponentInChildren<TextMeshProUGUI>().text = $"Technology Level:  {_currentBuilding.CurrentTechnologyLvl}";
            }
            else if (!_areRequirementsMet)
            {
                _description.text = $"Meet Requirements to Develop Technologies";
                _lvlUpGo.GetComponentInChildren<TextMeshProUGUI>().text = $"Technology Level:  {_currentBuilding.CurrentTechnologyLvl}";
            }

            if (_canDevelopTechnology)
            {
                _lvlUpGo.GetComponentInChildren<TextMeshProUGUI>().text = $"{_currentBuilding.CurrentTechnologyLvl} >> {nextLevel}";
                _description.text = "Develop technology to get better in mini game: \n" + techInfo;
                _lvlUpButton.interactable = true;
                _lvlUpButton.onClick.AddListener(() => UpgradeTechnology(_currentBuilding));
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
            OpenMiniGameOfType?.Invoke(p_building);
        }
    }
}