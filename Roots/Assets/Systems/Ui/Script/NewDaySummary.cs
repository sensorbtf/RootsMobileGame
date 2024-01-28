using System;
using System.Collections.Generic;
using AudioSystem;
using Buildings;
using GeneralSystems;
using TMPro;
using UnityEngine;
using UnityEngine.Localization;
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
        [SerializeField] private TextMeshProUGUI _panelTitle;

        [SerializeField] private LocalizedString _rankUpSummary;
        [SerializeField] private LocalizedString _buildingsUnlocked;
        [SerializeField] private LocalizedString _dayPassedStormSoon;
        [SerializeField] private LocalizedString _dayPassed;
        [SerializeField] private LocalizedString _justBuilt;
        [SerializeField] private LocalizedString _justUpgraded;
        [SerializeField] private LocalizedString _justRepaired;
        [SerializeField] private LocalizedString _minigameUnlocked;
        [SerializeField] private LocalizedString _pointsToGather;
        [SerializeField] private LocalizedString _technologyUpgrade;
        
        private Button _goBackButton;
        private bool _isCottage;
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

            _panelTitle.text = _rankUpSummary.GetLocalizedString();

            foreach (var building in buildingsManager.UnlockedBuildings)
                CreateUiElement(building.Icon, $"{_buildingsUnlocked.GetLocalizedString()} {building.BuildingName.GetLocalizedString()}");

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
                text = string.Format(_dayPassedStormSoon.GetLocalizedString(), worldManager.CurrentDay);
            }
            else
            {
                text = string.Format(_dayPassed.GetLocalizedString(), worldManager.CurrentDay);
            }
            
            _panelTitle.text = text;

            foreach (var building in buildingsManager.UnlockedBuildings)
                CreateUiElement(building.Icon, $"{_buildingsUnlocked.GetLocalizedString()} {building.BuildingName.GetLocalizedString()}");

            foreach (var building in buildingsManager.CompletlyNewBuildings)
                CreateUiElement(building.BuildingMainData.Icon, _justBuilt.GetLocalizedString());

            foreach (var building in buildingsManager.UpgradedBuildings)
                CreateUiElement(building.BuildingMainData.Icon, _justUpgraded.GetLocalizedString());

            foreach (var building in buildingsManager.RepairedBuildings)
                CreateUiElement(building.BuildingMainData.Icon, _justRepaired.GetLocalizedString());

            foreach (var building in buildingsManager.BuildingWithEnabledMinigame)
                CreateUiElement(building.BuildingMainData.Icon, _minigameUnlocked.GetLocalizedString());

            foreach (var building in buildingsManager.BuildingsToGatherFrom)
                CreateUiElement(building.BuildingMainData.Icon, _pointsToGather.GetLocalizedString());

            foreach (var building in buildingsManager.BuildingsWithTechnologyUpgrade)
                CreateUiElement(building.BuildingMainData.Icon, _technologyUpgrade.GetLocalizedString());

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