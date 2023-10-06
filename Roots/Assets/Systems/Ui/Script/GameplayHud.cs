using System;
using System.Collections.Generic;
using Buildings;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;
using World;
using UnityEngine.UI;
using Random = UnityEngine.Random;

namespace InGameUi
{
    enum DuringDayState
    {
        OnCollecting,
        SettingWorkers,
        AfterWorkersSet,
        Working
    }

    public class GameplayHud : MonoBehaviour
    {
        public static bool BlockHud;
        [SerializeField] private Canvas _mainCanvas; 
        
        [SerializeField] private WorldManager _worldManager;
        [SerializeField] private BuildingManager _buildingManager;
        [SerializeField] private WorkersPanel _workersPanel;

        [SerializeField] private TextMeshProUGUI CurrentDay;
        [SerializeField] private TextMeshProUGUI DayToStorm;
        [SerializeField] private TextMeshProUGUI ResourcePoints;
        [SerializeField] private TextMeshProUGUI DefensePoints;
        [SerializeField] private TextMeshProUGUI ShardsOfDestiny;

        [SerializeField] private int _dayDurationInSeconds = 60;
        [SerializeField] private GameObject SkipDayGo;
        [SerializeField] private GameObject EndMissionGo;
        [SerializeField] private GameObject EndDayGo;
        [SerializeField] private GameObject VinetePanel;

        [SerializeField] private TextMeshProUGUI _skipDayText;
        [SerializeField] private TextMeshProUGUI _paidSkipDayText;
        [SerializeField] private TextMeshProUGUI _currentRankText;
        [SerializeField] private TextMeshProUGUI _currentMissionText;

        // Quests
        [SerializeField] private GameObject FirstMissionGo;
        [SerializeField] private GameObject SecondMissionGo;
        [SerializeField] private GameObject FirstMissionRewardGo;
        [SerializeField] private GameObject SecondMissionRewardGo;
        private TextMeshProUGUI _firstMissionText;
        private TextMeshProUGUI _secondMissionText;
            
        private Button _firstMissionButton;
        private Button _secondMissionButton;
        private TextMeshProUGUI _firstMissionButtonText;
        private TextMeshProUGUI _secondMissionButtonText;
        // Quests
        
        private DuringDayState CurrentPlayerState;
        private Button _skipDayButton;
        private Button _endMissionButton;
        private Button _endDayButton;
        
        private TextMeshProUGUI _endDayButtonText;

        private bool _wasMainButtonRefreshed = true;
        private bool _canUseSkipByTime = false;
        private float _timeLeftInSeconds; // 10 minutes * 60 seconds
        private DateTime _startTime;
        private Dictionary<RectTransform, List<GameObject>> _createdImages;
        
        private void Start()
        {
            _createdImages = new Dictionary<RectTransform, List<GameObject>>
            {
                { ResourcePoints.rectTransform, new List<GameObject>() },
                { DefensePoints.rectTransform, new List<GameObject>() },
                { ShardsOfDestiny.rectTransform, new List<GameObject>() }
            };

            _skipDayButton = SkipDayGo.GetComponent<Button>();
            _endMissionButton = EndMissionGo.GetComponent<Button>();
            _endDayButton = EndDayGo.GetComponent<Button>();

            _endDayButtonText = _endDayButton.GetComponentInChildren<TextMeshProUGUI>();
            _firstMissionText = FirstMissionGo.GetComponentInChildren<TextMeshProUGUI>();
            _secondMissionText = SecondMissionGo.GetComponentInChildren<TextMeshProUGUI>();
            
            _firstMissionButtonText = FirstMissionRewardGo.GetComponent<TextMeshProUGUI>();
            _secondMissionButtonText = SecondMissionRewardGo.GetComponent<TextMeshProUGUI>();
            _firstMissionButton = FirstMissionRewardGo.GetComponent<Button>();
            _secondMissionButton = SecondMissionRewardGo.GetComponent<Button>();
            FirstMissionRewardGo.SetActive(false);
            SecondMissionRewardGo.SetActive(false);

            SkipDayGo.SetActive(false);
            EndMissionGo.SetActive(false);
            BlockHud = false;

            _workersPanel.OnBackToMap += SetWorkers;
            _buildingManager.OnResourcePointsChange += RefreshResourcePoints;
            _buildingManager.OnDefensePointsChange += RefreshDefensePoints;
            _buildingManager.OnDestinyShardsPointsChange += RefreshShardsPoints;

            _worldManager.OnResourcesRequirementsMeet += ActivateEndMissionButton;
            
            ShardsOfDestiny.text = $"{_buildingManager.ShardsOfDestinyAmount.ToString()}";
            DefensePoints.text = $"{_buildingManager.CurrentDefensePoints.ToString()}";
            ResourcePoints.text = $"{_buildingManager.CurrentResourcePoints.ToString()} / " +
                                  $"{_worldManager.RequiredResourcePoints}";

            _firstMissionText.text = _worldManager.GetSpecificMissionText(0);
            _secondMissionText.text = _worldManager.GetSpecificMissionText(1);
            _worldManager.CurrentQuests.Quests[0].OnCompletion += HandleFirstQuestCompletion;
            _worldManager.CurrentQuests.Quests[1].OnCompletion += HandleSecondQuestCompletion;
            _currentRankText.text = _worldManager.CurrentRank.ToString();
            _currentMissionText.text = _worldManager.CurrentMission.ToString();
        }

        private void HandleFirstQuestCompletion(Quest p_completedQuest)
        {
            // activate UX 
            // activate button for getting reward
            // after click - ux for full task completion
            if (_worldManager.CurrentQuests.Quests[1].IsCompleted)
            {
                // check if 2 missions are completed _currentRank < _currentMission
                // yes - let player lvl up
                // no - info about need to copmlete X mission 
            }
        }

        private void HandleSecondQuestCompletion(Quest p_completedQuest)
        {

            if (_worldManager.CurrentQuests.Quests[0].IsCompleted)
            {
                
            }
        }

        private void RefreshShardsPoints(int p_points, bool p_makeIcons)
        {
            ShardsOfDestiny.text = $"{_buildingManager.ShardsOfDestinyAmount.ToString()}";
            
            if (p_makeIcons)
            {
                TryToCreatePoints(p_points, PointsType.ShardsOfDestiny);
            }
        }

        private void RefreshDefensePoints(int p_points, bool p_makeIcons)
        {
            DefensePoints.text = $"{_buildingManager.CurrentDefensePoints.ToString()}";
            
            if (p_makeIcons)
            {
                TryToCreatePoints(p_points, PointsType.Defense);
            }
        }
        
        private void RefreshResourcePoints(int p_points, bool p_makeIcons)
        {
            ResourcePoints.text = $"{_buildingManager.CurrentResourcePoints.ToString()} / " +
                                  $"{_worldManager.RequiredResourcePoints}";
            if (p_makeIcons)
            {
                TryToCreatePoints(p_points, PointsType.Resource);
            }
        }

        private void TryToCreatePoints(int p_points, PointsType p_pointsType)
        {
            if (p_points <= 0)
                return;

            int dividedPoints = p_points / 4;
            
            for (int i = 0; i < dividedPoints; i++)
            {
                GameObject imageObject = new GameObject("ResourcePoint" + i);
                imageObject.transform.SetParent(_mainCanvas.transform);
                Image image = imageObject.AddComponent<Image>();

                RectTransform rectTransform = imageObject.GetComponent<RectTransform>();
                rectTransform.sizeDelta = new Vector2(50, 50);
                Vector3 lastClickPosition = TransparentPanelClickHandler.LastClickPosition;
                
                float jitter = 50f; 
                lastClickPosition.x += Random.Range(-jitter, jitter);
                lastClickPosition.y += Random.Range(-jitter, jitter);

                // Set the jittered position to the RectTransform
                rectTransform.position = lastClickPosition;

                switch (p_pointsType)
                {
                    case PointsType.Resource:
                        image.sprite = _buildingManager.ResourcesPointsIcon;
                        _createdImages[ResourcePoints.rectTransform].Add(imageObject);
                        break;
                    case PointsType.Defense:
                        image.sprite = _buildingManager.DefensePointsIcon;
                        _createdImages[DefensePoints.rectTransform].Add(imageObject);
                        break;
                }
            }
        }

        private void MovePoints()
        {
            foreach (var specificImages in _createdImages)
            {
                for (int i = specificImages.Value.Count - 1; i >= 0; i--)
                {
                    GameObject imageObject = specificImages.Value[i];
                    RectTransform rectTransform = imageObject.GetComponent<RectTransform>();
                    rectTransform.position = Vector2.MoveTowards(rectTransform.position, 
                        specificImages.Key.transform.position, 1000 * Time.deltaTime);

                    if (Vector2.Distance(rectTransform.position, specificImages.Key.transform.position) < 0.1f)
                    {
                        specificImages.Value.RemoveAt(i);
                        Destroy(imageObject);
                    }
                }
            }
        }

        private void Update() // Better way to do it?
        {
            CurrentDay.text = $"Current day: {_worldManager.CurrentDay.ToString()}";
            DayToStorm.text = $"Storm in: {_worldManager.StormDaysRange.ToString()}";

            double elapsedSeconds = (DateTime.UtcNow - _startTime).TotalSeconds;
            _timeLeftInSeconds = _dayDurationInSeconds - (float)elapsedSeconds;

            int minutes = Mathf.FloorToInt(_timeLeftInSeconds / 60);
            int seconds = Mathf.FloorToInt(_timeLeftInSeconds % 60);

            if (_timeLeftInSeconds > 0)
            {
                _skipDayText.text = $"{minutes}:{seconds:00}";
            }
            else
            {
                _canUseSkipByTime = true;
            }

            MovePoints();
            MainButtonHandler(); // change that for events on click

            VinetePanel.SetActive(BlockHud);
        }
        
        private void MainButtonHandler()
        {
            switch (CurrentPlayerState)
            {
                case DuringDayState.OnCollecting:

                    if (_buildingManager.IsAnyBuildingNonGathered())
                    {
                        _endDayButtonText.text = "Collect Points";
                        return;
                    }

                    _endDayButton.interactable = true;

                    if (_wasMainButtonRefreshed)
                    {
                        _endDayButtonText.text = "Set Workers";
                        _endDayButton.onClick.AddListener(OpenWorkersDisplacementPanel);
                        _wasMainButtonRefreshed = false;
                    }

                    break;

                case DuringDayState.SettingWorkers:
                    _endDayButtonText.text = "Setting Workers...";
                    break;

                case DuringDayState.AfterWorkersSet:
                    _endDayButtonText.text = "Start the day";

                    if (_worldManager.CanStartDay())
                    {
                        _endDayButton.interactable = true;
                        if (_wasMainButtonRefreshed)
                        {
                            _endDayButton.onClick.AddListener(OnWorkDayStarted);
                            _wasMainButtonRefreshed = false;
                        }
                    }
                    else
                    {
                        _endDayButton.interactable = false;
                    }

                    break;

                case DuringDayState.Working:
                    _endDayButtonText.text = "Working...";
                    _endDayButton.interactable = false;

                    if (_canUseSkipByTime)
                    {
                        _skipDayButton.interactable = true;

                        _skipDayButton.onClick.RemoveAllListeners();
                        _skipDayText.text = "Skip";
                        _paidSkipDayText.text = "";
                        _skipDayButton.onClick.AddListener(() => OnWorkDaySkipped(WayToSkip.NormalTimeSkip));
                        _wasMainButtonRefreshed = false;
                    }

                    if (!_canUseSkipByTime && _worldManager.CanSkipDay(out var skipPossibility))
                    {
                        _skipDayButton.interactable = true;

                        if (_wasMainButtonRefreshed)
                        {
                            if (skipPossibility == WayToSkip.FreeSkip)
                            {
                                _paidSkipDayText.text = $"Skip by: Free Skips ({_worldManager.FreeSkipsLeft})";
                            }
                            else if (skipPossibility == WayToSkip.PaidSkip)
                            {
                                _paidSkipDayText.text = $"Skip for: {_worldManager.DestinyShardsSkipPrice} Destiny Shards";
                            }
                            _skipDayButton.onClick.AddListener(() => OnWorkDaySkipped(skipPossibility));
                            _wasMainButtonRefreshed = false;
                        }
                    }

                    break;
            }
        }
        
        private void ActivateEndMissionButton()
        {
            EndMissionGo.SetActive(true);
            _endMissionButton.interactable = true;
        }

        private void OpenWorkersDisplacementPanel()
        {
            CurrentPlayerState = DuringDayState.SettingWorkers;
            _endDayButton.onClick.RemoveListener(OpenWorkersDisplacementPanel);
            _wasMainButtonRefreshed = true;

            _workersPanel.ActivatePanel();
        }

        private void SetWorkers()
        {
            CurrentPlayerState = DuringDayState.AfterWorkersSet;
            _wasMainButtonRefreshed = true;
        }

        private void OnWorkDayStarted()
        {
            CurrentPlayerState = DuringDayState.Working;
            SkipDayGo.SetActive(true);
            _endDayButton.onClick.RemoveListener(OnWorkDayStarted);
            _startTime = DateTime.UtcNow;

            _wasMainButtonRefreshed = true;
            _canUseSkipByTime = false;
        }

        private void OnWorkDaySkipped(WayToSkip p_skipSource)
        {
            CurrentPlayerState = DuringDayState.OnCollecting;
            _skipDayButton.onClick.RemoveAllListeners();
            SkipDayGo.SetActive(false);
            _skipDayButton.interactable = false;
            _wasMainButtonRefreshed = true;

            _worldManager.SkipDay(p_skipSource);
        }
    }
}