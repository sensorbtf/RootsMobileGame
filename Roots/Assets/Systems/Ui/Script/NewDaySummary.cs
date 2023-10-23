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
        [SerializeField] private TextMeshProUGUI _panelTitle;

        [SerializeField] private GameObject _buildingInfoPrefab;
        [SerializeField] private GameObject _goBackGo;
        [SerializeField] private Transform _contentTransform;

        private Button _goBackButton;
        private List<GameObject> _runtimeBuildingsUiToDestroy;
        private Building _building;
        private bool _isCottage;
        
        private TechnologyDataPerLevel[] _technology;

        private void Start()
        {
            worldManager.OnNewDayStarted += ActivateOnNewDay;
            _buildingManager.OnBuildingStateChanged += AfterRankUp;

            _runtimeBuildingsUiToDestroy = new List<GameObject>();

            _goBackButton = _goBackGo.GetComponent<Button>();

            gameObject.SetActive(false);
        }

        private void AfterRankUp(Building p_cottage)
        {
            if (p_cottage.BuildingMainData.Type != BuildingType.Cottage)
                return;
            
            _isCottage = true;
            ActivateOnNewDay(_isCottage);
            _isCottage = false;
            
            _panelTitle.text = "Rank up summary";
            
            foreach (var building in _buildingManager.UnlockedBuildings)
            {
                CreateUiElement(building.Icon, $"New building unlocked: {building.Type}");
            }

            _buildingManager.UnlockedBuildings.Clear();
        }


        private void ActivateOnNewDay()
        {
            _isCottage = false;
            ActivateOnNewDay(_isCottage);
        }

        private void ActivateOnNewDay(bool p_isCottage)
        {
            ClosePanel();
            gameObject.SetActive(true);
            CameraController.IsUiOpen = true;
            GameplayHud.BlockHud = true;

            _goBackGo.SetActive(true);
            _goBackButton.interactable = true;
            _goBackButton.onClick.AddListener(ClosePanel);

            if (!p_isCottage)
            {
                HandleViewOfSummary();
            }
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

        private void HandleViewOfSummary()
        {
            _panelTitle.text = "End of the day summary";
            
            foreach (var building in _buildingManager.UnlockedBuildings)
            {
                CreateUiElement(building.Icon, $"New building unlocked: {building.Type}");
            }

            foreach (var building in _buildingManager.CompletlyNewBuildings)
            {
                CreateUiElement(building.BuildingMainData.Icon, "Just built");
            }

            foreach (var building in _buildingManager.UpgradedBuildings)
            {
                CreateUiElement(building.BuildingMainData.Icon, "Building upgraded");
            }

            foreach (var building in _buildingManager.RepairedBuildings)
            {
                CreateUiElement(building.BuildingMainData.Icon, "Building repaired");
            }

            foreach (var building in _buildingManager.BuildingWithEnabledMinigame)
            {
                CreateUiElement(building.BuildingMainData.Icon, "Minigame unlocked");
            }

            foreach (var building in _buildingManager.BuildingsToGatherFrom)
            {
                CreateUiElement(building.BuildingMainData.Icon, "Points can be gathered");
            }

            foreach (var building in _buildingManager.BuildingsWithTechnologyUpgrade)
            {
                CreateUiElement(building.BuildingMainData.Icon, "Technology can be upgraded");
            }

            _buildingManager.BuildingsToGatherFrom.Clear();
            _buildingManager.UpgradedBuildings.Clear();
            _buildingManager.BuildingWithEnabledMinigame.Clear();
            _buildingManager.CompletlyNewBuildings.Clear();
            _buildingManager.UnlockedBuildings.Clear();
            _buildingManager.BuildingsWithTechnologyUpgrade.Clear();
            _buildingManager.RepairedBuildings.Clear();
        }

        private void CreateUiElement(Sprite p_icon, string p_text)
        {
            GameObject newIcon = Instantiate(_buildingInfoPrefab, _contentTransform);
            _runtimeBuildingsUiToDestroy.Add(newIcon);
            var references = newIcon.GetComponent<DaySummaryUiElement>();

            references.BuildingSprite.sprite = p_icon;
            references.BuildingDesc.text = p_text;
        }
    }
}