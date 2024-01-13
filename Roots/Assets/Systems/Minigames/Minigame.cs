using System;
using System.Collections;
using AudioSystem;
using Buildings;
using TMPro;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.UI;

namespace Minigames
{
    public abstract class Minigame : MonoBehaviour
    {
        [SerializeField] internal AudioManager _audioManager;
        [SerializeField] internal MinigameLocalizationSO _localization;

        [HideInInspector] public float _timer;
        [HideInInspector] public float _efficiency;
        [HideInInspector] public bool _isGameActive;
        [HideInInspector] public bool _givesResources;
        [HideInInspector] public float _score;
        [HideInInspector] public PointsType _type;
        public Button _collectPointsButton;
        public TextMeshProUGUI _coutdownText; // Countdown
        public TextMeshProUGUI _scoreText; // top panel title 
        public TextMeshProUGUI _timeText; // bottom button time text

        public event Action OnMinigameEnded;
        public event Action<PointsType, int> OnMiniGamePointsCollected;

        public event Action<int> OnStormReveal;

        public virtual void Update()
        {
            if (!_isGameActive)
                return;

            UpdateTimerText();

            if (_timer <= 0)
            {
                _timer = 0;
                _isGameActive = false;
                _collectPointsButton.interactable = true;

                var points = $"{_score:F0}";

                var halfPoints = Mathf.CeilToInt(_score / 2);
                var pointsTwo = $"{halfPoints:F0}";

                switch (_type)
                {
                    case PointsType.Resource:
                        _timeText.text = _localization.ResourcePointsCollect.GetLocalizedString().Replace("0", points);
                        break;
                    case PointsType.Defense:
                        _timeText.text = _localization.DefensePointsCollect.GetLocalizedString().Replace("0", points);
                        break;
                    case PointsType.StarDust:
                        _timeText.text = _localization.StarDustPointsCollect.GetLocalizedString().Replace("0", points);
                        break;
                    case PointsType.ResourcesAndDefense:
                        _timeText.text = _localization.ResourcesAndDefenseCollect.GetLocalizedString()
                            .Replace("0", points).Replace("1", pointsTwo);
                        break;
                    case PointsType.DefenseAndResources:
                        _timeText.text = _localization.DefenseAndResourcesCollect.GetLocalizedString()
                            .Replace("0", points).Replace("1", pointsTwo);
                        break;
                }
            }
        }

        public virtual void SetupGame(Building p_building)
        {
            _score = 0;
            _timeText.text = SelectRightText(p_building.BuildingMainData.Type);

            AudioManager myObject = FindObjectOfType<AudioManager>();

            if (myObject != null)
            {
                _audioManager = myObject;
            }

            _timer = p_building.BuildingMainData.Technology.DataPerTechnologyLevel[p_building.CurrentTechnologyLvl]
                .MinigameDuration;
            _efficiency = p_building.BuildingMainData.Technology.DataPerTechnologyLevel[p_building.CurrentTechnologyLvl]
                .Efficiency;
            _type = p_building.BuildingMainData.Technology.ProductionType;

            _collectPointsButton.onClick.AddListener(EndMinigame);
            _collectPointsButton.interactable = false;

            if (p_building.BuildingMainData.Type == BuildingType.GuardTower)
                _givesResources = false;
            else
                _givesResources = true;

            StartCoroutine(StartCountdown());
        }

        private void EndMinigame()
        {
            if (_givesResources)
                OnMiniGamePointsCollected?.Invoke(_type, (int)_score);
            else
                OnStormReveal?.Invoke(2);

            OnMinigameEnded?.Invoke();
        }

        private void UpdateTimerText()
        {
            _timer -= Time.deltaTime;
            _timeText.text = Mathf.FloorToInt(_timer).ToString();
        }

        private IEnumerator StartCountdown()
        {
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
            _scoreText.text = $"{_localization.ScoreText.GetLocalizedString()} {_score:F0}";
        }

        public abstract void StartMinigame();

        private string SelectRightText(BuildingType p_type)
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
    }
}