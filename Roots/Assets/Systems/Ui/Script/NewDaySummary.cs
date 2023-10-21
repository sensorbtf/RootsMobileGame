using System;
using System.Collections.Generic;
using System.Linq;
using Buildings;
using GeneralSystems;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using World;

namespace InGameUi
{
    public class NewDaySummary : MonoBehaviour
    {
        [SerializeField] private BuildingManager _buildingManager;
        [SerializeField] private WorldManager worldManager;

        [SerializeField] private GameObject _buildingInfoPrefab;
        [SerializeField] private GameObject _goBackGo;
        [SerializeField] private Transform _contentTransform;

        private Button _goBackButton;
        private List<GameObject> _runtimeBuildingsUiToDestroy;
        private Building _building;
        
        private TechnologyDataPerLevel[] _technology;

        public TechnologyDataPerLevel TechnologyData => _technology[_building.CurrentTechnologyLvl];
        public event Action<Building> OpenMiniGameOfType;

        private void Start()
        {
            worldManager.OnNewDayStarted += ActivateOnClick; // activate on day skip/end
            _runtimeBuildingsUiToDestroy = new List<GameObject>();

            _goBackButton = _goBackGo.GetComponent<Button>();

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

            _building = null;
            _runtimeBuildingsUiToDestroy.Clear();
            gameObject.SetActive(false);
        }

        public void ActivateOnClick()
        {
            ClosePanel();
            gameObject.SetActive(true);
            CameraController.IsUiOpen = true;
            GameplayHud.BlockHud = true;

            _goBackGo.SetActive(true);
            _goBackButton.interactable = true;
            _goBackButton.onClick.AddListener(ClosePanel);

            HandleView();
        }

        private void HandleView()  // can upgrade minigame??
        {
            foreach (var building in _buildingManager.UnlockedBuildings)
            {
                CreateUiElement(building.BuildingMainData.PerLevelData[0].Icon, $"New building unlocked: {building.BuildingMainData.Type}");
            }

            foreach (var building in _buildingManager.CompletlyNewBuildings)
            {
                CreateUiElement(building.BuildingMainData.PerLevelData[building.CurrentLevel].Icon, "Just built");
            }

            foreach (var building in _buildingManager.UpgradedBuildings)
            {
                CreateUiElement(building.BuildingMainData.PerLevelData[building.CurrentLevel].Icon, "Building upgraded");
            }

            foreach (var building in _buildingManager.BuildingWithEnabledMinigame)
            {
                CreateUiElement(building.BuildingMainData.PerLevelData[building.CurrentLevel].Icon, "Minigame unlocked");
            }

            foreach (var building in _buildingManager.BuildingsToGatherFrom)
            {
                CreateUiElement(building.BuildingMainData.PerLevelData[building.CurrentLevel].Icon, "Points can be gathered");
            }

            foreach (var building in _buildingManager.BuildingsWithTechnologyUnlocked)
            {
                CreateUiElement(building.BuildingMainData.PerLevelData[building.CurrentLevel].Icon, "Technology can be upgraded");
            }

            _buildingManager.BuildingsToGatherFrom.Clear();
            _buildingManager.UpgradedBuildings.Clear();
            _buildingManager.BuildingWithEnabledMinigame.Clear();
            _buildingManager.CompletlyNewBuildings.Clear();
            _buildingManager.UnlockedBuildings.Clear();
            _buildingManager.BuildingsWithTechnologyUnlocked.Clear();
        }

        private void CreateUiElement(Sprite p_icon, string p_text)
        {
            GameObject newIcon = Instantiate(_buildingInfoPrefab, _contentTransform);
            _runtimeBuildingsUiToDestroy.Add(newIcon);

            newIcon.GetComponentInChildren<Image>().sprite = p_icon;
            newIcon.GetComponentInChildren<TextMeshProUGUI>().text = p_text;
        }
    }
}