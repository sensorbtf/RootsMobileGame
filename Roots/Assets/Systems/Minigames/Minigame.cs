using System;
using System.Collections;
using AudioSystem;
using Buildings;
using Gods;
using TMPro;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Serialization;
using UnityEngine.UI;
using World;

namespace Minigames
{
    public abstract class Minigame : MonoBehaviour
    {
        [SerializeField] internal MinigameLocalizationSO _localization;

        [HideInInspector] public float _timer;
        [HideInInspector] public float _efficiency;
        [HideInInspector] public bool _isGameActive;
        private bool _isWatchtower;
        private bool _isAltar;
        private GodType _selectedGod;
        private BlessingLevel _selectedBlessing;
        
        internal AudioManager _audioManager;
        internal WorldManager _worldManager;
        internal BuildingsManager _buildingsManager;
        internal GodsManager _godsManager;

        [HideInInspector] public float _score;
        [HideInInspector] public PointsType _type;
        public Button _collectPointsButton;
        public TextMeshProUGUI _coutdownText; // Countdown
        [SerializeField] private TextMeshProUGUI _scoreText; // top panel title 
        [SerializeField] private TextMeshProUGUI _timeText; // bottom button time text

        public event Action OnMinigameEnded;
        public event Action<PointsType, int> OnMiniGamePointsCollected;

        public virtual void SetupGame(Building p_building)
        {
            _score = 0;
            _timeText.text = SelectRightBottomText(p_building.BuildingMainData.Type);
            _scoreText.text = SelectRightTopText(p_building.BuildingMainData.Type);

            var audioMgr = FindObjectOfType<AudioManager>();
            if (audioMgr != null)
                _audioManager = audioMgr;
            var worldMgr = FindObjectOfType<WorldManager>();
            if (worldMgr != null)
                _worldManager = worldMgr;
            var buildingsMgr = FindObjectOfType<BuildingsManager>();
            if (buildingsMgr != null)
                _buildingsManager = buildingsMgr;
            var godsMgr = FindObjectOfType<GodsManager>();
            if (godsMgr != null)
                _godsManager = godsMgr;
            
            _timer = p_building.BuildingMainData.Technology.DataPerTechnologyLevel[p_building.CurrentTechnologyLvl]
                .MinigameDuration;
            _efficiency = p_building.BuildingMainData.Technology.DataPerTechnologyLevel[p_building.CurrentTechnologyLvl]
                .Efficiency;
            _type = p_building.BuildingMainData.Technology.ProductionType;

            _collectPointsButton.onClick.AddListener(EndMinigame);
            _collectPointsButton.interactable = false;

            if (p_building.BuildingMainData.Type == BuildingType.GuardTower)
            {
                _isWatchtower = true;
                _isAltar = false;
            }
            else if (p_building.BuildingMainData.Type == BuildingType.Sacrificial_Altar)
            {
                _isWatchtower = false;
                _isAltar = true;
            }
            else
            {
                _isWatchtower = false;
                _isAltar = false;
            }

            StartCoroutine(StartCountdown());
        }

        public virtual void Update()
        {
            if (!_isGameActive)
                return;
            
            _timer -= Time.deltaTime;
            
            if (_timer <= 0)
            {
                _audioManager.CreateNewAudioSource(_localization.OnMinigameEnd);
                _timer = 0;
                _isGameActive = false;
                _collectPointsButton.interactable = true;
                
                if (_isWatchtower)
                {
                    _timeText.text = _localization.GuardTowerStorm.GetLocalizedString();
                }
                else if (_isAltar)
                {
                    _selectedGod = _buildingsManager.GetRandomGodInBuildings();
                    var maxAffordablePrice = 0;

                    foreach (var blessingEntry in _godsManager.BlessingPrices)
                    {
                        int blessingPrice = blessingEntry.Value;
                        BlessingLevel currentBlessingLevel = blessingEntry.Key;

                        if (_score >= blessingPrice && blessingPrice > maxAffordablePrice)
                        {
                            _selectedBlessing = currentBlessingLevel;
                            maxAffordablePrice = blessingPrice;
                        }
                    }
                    
                    var text = string.Format(_localization.SacrificialAltarReward.GetLocalizedString(), 
                        _godsManager.GetBlessingName(_selectedBlessing),
                        _godsManager.GetGodName(_selectedGod));
                    
                    _timeText.text = text;
                }
                else
                {
                    var points = $"{_score:F0}";

                    var halfPoints = Mathf.CeilToInt(_score / 2);
                    var pointsTwo = $"{halfPoints:F0}";
                    
                    switch (_type)
                    {
                        case PointsType.Resource:
                            _timeText.text = string.Format(_localization.ResourcePointsCollect.GetLocalizedString(), points);
                            break;
                        case PointsType.Defense:
                            _timeText.text = string.Format(_localization.DefensePointsCollect.GetLocalizedString(), points);
                            break;
                        case PointsType.StarDust:
                            _timeText.text = string.Format(_localization.StarDustPointsCollect.GetLocalizedString(), points);
                            break;
                        case PointsType.ResourcesAndDefense:
                            _timeText.text = string.Format(_localization.ResourcesAndDefenseCollect.GetLocalizedString(), points, pointsTwo);
                            break;
                        case PointsType.DefenseAndResources:
                            _timeText.text = string.Format(_localization.DefenseAndResourcesCollect.GetLocalizedString(), points, pointsTwo);
                            break;
                    }
                }

                return;
            }
            
            UpdateTimerText();
        }

        private void EndMinigame()
        {
            if (_isWatchtower)
            {
                _worldManager.RevealStorm(2);
            }
            else if (_isAltar)
            {
                if (_selectedBlessing != BlessingLevel.Noone)
                {
                    _godsManager.BuySpecificBlessing(_selectedGod, _selectedBlessing);
                }
            }
            else
            {
                OnMiniGamePointsCollected?.Invoke(_type, (int)_score);
            }

            OnMinigameEnded?.Invoke();
        }

        private void UpdateTimerText()
        {
            _timeText.text = Mathf.FloorToInt(_timer).ToString();
        }

        private IEnumerator StartCountdown()
        {
            _audioManager.CreateNewAudioSource(_localization.Countdown);

            var count = 3;

            while (count > 0)
            {
                _coutdownText.text = count + "...";
                yield return new WaitForSeconds(1);
                count--;
            }

            _coutdownText.text = "Start!";
            StartMinigame();
            _isGameActive = true;
            yield return new WaitForSeconds(0.5f);
            _coutdownText.enabled = false;
        }

        public virtual void AddScore()
        {
            _audioManager.CreateNewAudioSource(_localization.PointsAdded);

            _scoreText.text = $"{_localization.ScoreText.GetLocalizedString()} {_score:F0}";
        }

        public abstract void StartMinigame();

        private string SelectRightBottomText(BuildingType p_type)
        {
            switch (p_type)
            {
                case BuildingType.Farm:
                    return _localization.FarmInfo.GetLocalizedString();
                case BuildingType.GuardTower:
                    return _localization.GuardTowerInfo.GetLocalizedString();
                case BuildingType.Woodcutter:
                    return _localization.WoodcutterInfo.GetLocalizedString();
                case BuildingType.Alchemical_Hut:
                    return _localization.Alchemical_HutInfo.GetLocalizedString();
                case BuildingType.Mining_Shaft:
                    return _localization.Mining_ShaftInfo.GetLocalizedString();
                case BuildingType.Ritual_Circle:
                    return _localization.Ritual_CircleInfo.GetLocalizedString();
                case BuildingType.Peat_Excavation:
                    return _localization.Peat_ExcavationInfo.GetLocalizedString();
                case BuildingType.Charcoal_Pile:
                    return _localization.Charcoal_PileInfo.GetLocalizedString();
                case BuildingType.Herbs_Garden:
                    return _localization.Herbs_GardenInfo.GetLocalizedString();
                case BuildingType.Apiary:
                    return _localization.ApiaryInfo.GetLocalizedString();
                case BuildingType.Workshop:
                    return _localization.WorkshopInfo.GetLocalizedString();
                case BuildingType.Sacrificial_Altar:
                    return _localization.Sacrificial_AltarInfo.GetLocalizedString();
            }

            return "Enjoy Minigame";
        }

        private string SelectRightTopText(BuildingType p_type)
        {
            switch (p_type)
            {
                case BuildingType.Farm:
                    return _localization.FarmInfoBottom.GetLocalizedString();
                case BuildingType.GuardTower:
                    return _localization.GuardTowerInfoBottom.GetLocalizedString();
                case BuildingType.Woodcutter:
                    return _localization.WoodcutterInfoBottom.GetLocalizedString();
                case BuildingType.Alchemical_Hut:
                    return _localization.Alchemical_HutInfoBottom.GetLocalizedString();
                case BuildingType.Mining_Shaft:
                    return _localization.Mining_ShaftInfoBottom.GetLocalizedString();
                case BuildingType.Ritual_Circle:
                    return _localization.Ritual_CircleInfoBottom.GetLocalizedString();
                case BuildingType.Peat_Excavation:
                    return _localization.Peat_ExcavationInfoBottom.GetLocalizedString();
                case BuildingType.Charcoal_Pile:
                    return _localization.Charcoal_PileInfoBottom.GetLocalizedString();
                case BuildingType.Herbs_Garden:
                    return _localization.Herbs_GardenInfoBottom.GetLocalizedString();
                case BuildingType.Apiary:
                    return _localization.ApiaryInfoBottom.GetLocalizedString();
                case BuildingType.Workshop:
                    return _localization.WorkshopInfoBottom.GetLocalizedString();
                case BuildingType.Sacrificial_Altar:
                    return _localization.Sacrificial_AltarInfoBottom.GetLocalizedString();
            }

            return "Enjoy Minigame";
        }
    }
}