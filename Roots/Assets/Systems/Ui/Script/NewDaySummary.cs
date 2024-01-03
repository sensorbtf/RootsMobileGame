using System;
using System.Collections.Generic;
using AudioSystem;
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
        [SerializeField] private AudioManager _audioManager;
        [SerializeField] private BuildingsManager buildingsManager;
        [SerializeField] private WorldManager worldManager;

        [SerializeField] private GameObject _buildingInfoPrefab;
        [SerializeField] private GameObject _goBackGo;
        [SerializeField] private Transform _contentTransform;

        private Button _goBackButton;
        private bool _isCottage;
        [SerializeField] private TextMeshProUGUI _panelTitle;
        private List<GameObject> _runtimeBuildingsUiToDestroy;

        private TechnologyDataPerLevel[] _technology;

        private void Start()
        {
            worldManager.OnNewDayStarted += ActivateOnNewDay;
            buildingsManager.OnCottageLevelUp += AfterRankUp;

            _runtimeBuildingsUiToDestroy = new List<GameObject>();

            _goBackButton = _goBackGo.GetComponent<Button>();

            gameObject.SetActive(false);
        }

        private void OnDestroy()
        {
            worldManager.OnNewDayStarted -= ActivateOnNewDay;
            buildingsManager.OnCottageLevelUp -= AfterRankUp;
        }

        private void AfterRankUp()
        {
            _isCottage = true;
            ActivateOnNewDay(_isCottage);
            _isCottage = false;

            _panelTitle.text = "Rank up!";

            foreach (var building in buildingsManager.UnlockedBuildings)
                CreateUiElement(building.Icon, $"New building unlocked: {building.Type}");

            buildingsManager.UnlockedBuildings.Clear();
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
            
            _goBackButton.onClick.AddListener(delegate
            {
                _audioManager.PlayButtonSoundEffect(true);
                ClosePanel();
            });

            if (!p_isCottage) 
                HandleViewOfSummary();
        }

        private void ClosePanel()
        {
            foreach (var createdUiElement in _runtimeBuildingsUiToDestroy) 
                Destroy(createdUiElement);

            CameraController.IsUiOpen = false;
            GameplayHud.BlockHud = false;

            _runtimeBuildingsUiToDestroy.Clear();
            gameObject.SetActive(false);
        }

        private void HandleViewOfSummary()
        {
            string text = "";
            
            if (worldManager.StormDaysRange.x < worldManager.CurrentDay)
            {
                text = $"Day {worldManager.CurrentDay} of current mission has passed. Storm might hit us anytime";
            }
            else
            {
                text = $"Day {worldManager.CurrentDay} has passed.";
            }
            
            _panelTitle.text = text;

            foreach (var building in buildingsManager.UnlockedBuildings)
                CreateUiElement(building.Icon, $"New building unlocked: {building.Type}");

            foreach (var building in buildingsManager.CompletlyNewBuildings)
                CreateUiElement(building.BuildingMainData.Icon, "Just built");

            foreach (var building in buildingsManager.UpgradedBuildings)
                CreateUiElement(building.BuildingMainData.Icon, "Building upgraded");

            foreach (var building in buildingsManager.RepairedBuildings)
                CreateUiElement(building.BuildingMainData.Icon, "Building repaired");

            foreach (var building in buildingsManager.BuildingWithEnabledMinigame)
                CreateUiElement(building.BuildingMainData.Icon, "Minigame unlocked");

            foreach (var building in buildingsManager.BuildingsToGatherFrom)
                CreateUiElement(building.BuildingMainData.Icon, "Points can be gathered");

            foreach (var building in buildingsManager.BuildingsWithTechnologyUpgrade)
                CreateUiElement(building.BuildingMainData.Icon, "Technology can be upgraded");

            buildingsManager.BuildingsToGatherFrom.Clear();
            buildingsManager.UpgradedBuildings.Clear();
            buildingsManager.BuildingWithEnabledMinigame.Clear();
            buildingsManager.CompletlyNewBuildings.Clear();
            buildingsManager.UnlockedBuildings.Clear();
            buildingsManager.BuildingsWithTechnologyUpgrade.Clear();
            buildingsManager.RepairedBuildings.Clear();
        }

        private void CreateUiElement(Sprite p_icon, string p_text)
        {
            var newIcon = Instantiate(_buildingInfoPrefab, _contentTransform);
            _runtimeBuildingsUiToDestroy.Add(newIcon);
            var references = newIcon.GetComponent<DaySummaryUiElement>();

            references.BuildingSprite.sprite = p_icon;
            references.BuildingDesc.text = p_text;
        }
    }
}