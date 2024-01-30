using System;
using System.Collections.Generic;
using System.Linq;
using AudioSystem;
using Buildings;
using GameManager;
using Narrator;
using TMPro;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using UnityEngine.UI;
using World;
using Random = UnityEngine.Random;

namespace InGameUi
{
    public class GameplayHud : MonoBehaviour
    {
        [Header("System Refs")]
        [SerializeField] private Canvas _mainCanvas;
        [SerializeField] private NarratorManager _narratorManager;
        [SerializeField] private AudioManager _audioManager;
        [SerializeField] private MainGameManager _gameManager;
        [SerializeField] private WorldManager _worldManager;
        [SerializeField] private BuildingsManager _buildingsManager;
        [SerializeField] private SettingsPanel _settingsPanel;
        [SerializeField] private DecisionMakingPanel _decisionsPanel;
        [SerializeField] private InfoPanel _infoPanel;

        [Header("Elements Refs")] 
        [SerializeField] private Button[] _pointsButtons;
        [SerializeField] private GameObject StormSliderBackground;
        [SerializeField] private GameObject StormDaysPrefab;
        [SerializeField] private GameObject StormHandle;
        [SerializeField] private GameObject RankGo;

        [SerializeField] private Sprite NormalHandleImage;
        [SerializeField] private Sprite LightingHandleImage;
        [SerializeField] private Color NormalDay;
        [SerializeField] private Color StormDay;

        [SerializeField] private GameObject SkipDayGo;
        [SerializeField] private GameObject EndMissionGo;
        [SerializeField] private GameObject EndDayGo;
        [SerializeField] private GameObject VinetePanel;

        //right side of screen
        [SerializeField] private GameObject _settingsButtonGo;
        [SerializeField] private GameObject _addButtonGo;
        //left side of screen

        [Header("Quest")]
        [SerializeField] private GameObject QuestsCompletedGo;
        [SerializeField] private GameObject FirstMissionGo;
        [SerializeField] private GameObject SecondMissionGo;
        [SerializeField] private GameObject FirstMissionObjectiveGo;
        [SerializeField] private GameObject SecondMissionObjectiveGo;
        private List<GameObject> _createdDaysStorm;

        private Dictionary<RectTransform, List<GameObject>> _createdImages;
        [SerializeField] private TextMeshProUGUI _currentMissionText;
        [SerializeField] private TextMeshProUGUI _currentRankText;
        [SerializeField] private Button _stormButton;
        private Button _endDayButton;

        private TextMeshProUGUI _endDayButtonText;
        private Button _endMissionButton;

        private Button _firstMissionButton;
        private TextMeshProUGUI _firstMissionButtonText;
        private TextMeshProUGUI _firstQuestText;
        [SerializeField] private TextMeshProUGUI _paidSkipDayText;
        private Button _secondMissionButton;
        private TextMeshProUGUI _secondMissionButtonText;
        private TextMeshProUGUI _secondQuestText;
        private Button _settingsButton;
        private Button _addButton;

        private DuringDayState _state;

        //Audio clips
        [Header("Audio")]
        [SerializeField] private AudioClip _destinyShardsManipulation;
        [SerializeField] private AudioClip _resourcePointsManipulation;
        [SerializeField] private AudioClip _defensePointsManipulation;
        [SerializeField] private AudioClip _rankUpSound;
        [SerializeField] private AudioClip _onQuestCompletionSound;
        [SerializeField] private AudioClip _daySkippedSound;
        [SerializeField] private AudioClip _pointGot;

        // Quests
        private float _speedMultiplier = 200;
        private float _singleDayGoWidth;
        private Button _skipDayButton;

        [SerializeField] private TextMeshProUGUI _skipDayText;
        [SerializeField] private TextMeshProUGUI DefensePoints;
        [SerializeField] private Image DefensePointsImage;

        [SerializeField] private TextMeshProUGUI ResourcePoints;

        [SerializeField] private Image ResourcePointsImage;
        [SerializeField] private TextMeshProUGUI ShardsOfDestiny;
        [SerializeField] private Image ShardsOfDestinyImage;
        [SerializeField] private Slider StormSlider;

        [Header("Localization")] 
        [SerializeField] private LocalizedString _finalizeBuildingProcess;
        [SerializeField] private LocalizedString _collectAvaiablePoints;
        [SerializeField] private LocalizedString _planNextDay;
        [SerializeField] private LocalizedString _settingWorkers;
        [SerializeField] private LocalizedString _dayIsPassing;
        [SerializeField] private LocalizedString _skippingFreeSkipText;
        [SerializeField] private LocalizedString _skippingStartDustText;
        [SerializeField] private LocalizedString _collectXYZDestinyShards;
        [SerializeField] private LocalizedString _completedQuest;
        [SerializeField] private LocalizedString _clickToRankUp;
        [SerializeField] private LocalizedString _completeMissionToRankUp;
        [SerializeField] private LocalizedString _endDay;

        private bool _wasMainButtonRefreshed = true;
        public static bool BlockHud;
        
        private void Awake()
        {
            _createdDaysStorm = new List<GameObject>();

            _createdImages = new Dictionary<RectTransform, List<GameObject>>
            {
                { ResourcePointsImage.rectTransform, new List<GameObject>() },
                { DefensePointsImage.rectTransform, new List<GameObject>() },
                { ShardsOfDestinyImage.rectTransform, new List<GameObject>() }
            };

            _skipDayButton = SkipDayGo.GetComponent<Button>();
            _endMissionButton = EndMissionGo.GetComponent<Button>();
            _endDayButton = EndDayGo.GetComponent<Button>();
            _settingsButton = _settingsButtonGo.GetComponent<Button>();
            _addButton = _addButtonGo.GetComponent<Button>();
            _addButton.onClick.AddListener(() => _decisionsPanel.AdvertisementAlert());

            _endDayButtonText = _endDayButton.GetComponentInChildren<TextMeshProUGUI>();
            _firstQuestText = FirstMissionGo.GetComponentInChildren<TextMeshProUGUI>();
            _secondQuestText = SecondMissionGo.GetComponentInChildren<TextMeshProUGUI>();

            _firstMissionButtonText = FirstMissionObjectiveGo.GetComponentInChildren<TextMeshProUGUI>();
            _secondMissionButtonText = SecondMissionObjectiveGo.GetComponentInChildren<TextMeshProUGUI>();
            _firstMissionButton = FirstMissionObjectiveGo.GetComponentInChildren<Button>();
            _secondMissionButton = SecondMissionObjectiveGo.GetComponentInChildren<Button>();

            FirstMissionObjectiveGo.SetActive(true);
            SecondMissionObjectiveGo.SetActive(true);
            QuestsCompletedGo.SetActive(false);

            SkipDayGo.SetActive(false);
            EndMissionGo.SetActive(false);
            BlockHud = false;

            _settingsButton.onClick.AddListener(delegate
            {
                _audioManager.PlayButtonSoundEffect(_settingsButton.interactable);
                _settingsPanel.OpenPanel();
            });

            ShardsOfDestiny.text = $"{_buildingsManager.CurrentDestinyShards}";
            DefensePoints.text = $"{_buildingsManager.CurrentDefensePoints}";
            ResourcePoints.text = $"{_buildingsManager.CurrentResourcePoints} / " +
                                  $"{_worldManager.RequiredResourcePoints}";

            foreach (var button in _pointsButtons)
            {
                button.onClick.RemoveAllListeners();
                button.onClick.AddListener(delegate { _infoPanel.ShowResourcesInfo(); });
            }
            
            _stormButton.onClick.RemoveAllListeners();
            _stormButton.onClick.AddListener(delegate { _infoPanel.ShowStormInfo(); });
            
            LocalizationSettings.SelectedLocaleChanged += OnLocaleChanged;
            
            _buildingsManager.OnResourcePointsChange += RefreshResourcePoints;
            _buildingsManager.OnDefensePointsChange += RefreshDefensePoints;
            _buildingsManager.OnDestinyShardsPointsChange += RefreshShardsPoints;
            _worldManager.OnResourcesRequirementsMeet += ActivateEndMissionButton;

            _worldManager.CurrentQuests[0].OnCompletion += HandleFirstQuestCompletion;
            _worldManager.CurrentQuests[1].OnCompletion += HandleSecondQuestCompletion;
            _worldManager.OnQuestsProgress += RefreshQuestsText;
            _worldManager.OnNewMissionStart += RefreshStormSlider;
            _worldManager.OnNewDayStarted += NewDayHandler;
            _worldManager.OnStormCheck += CheckNextDaysOnDemand;

            _gameManager.OnPlayerStateChange += MainButtonHandler;
            _gameManager.OnDaySkipPossibility += CheckDaySkipPossibility;
            _gameManager.OnAfterLoad += AfterLoadHandler;
        }
        
        private void Start()
        {
            RectTransform rectTransform = GetComponent<RectTransform>(); 
            Rect safeArea = Screen.safeArea;

            Vector2 anchorMin = safeArea.position;
            Vector2 anchorMax = safeArea.position + safeArea.size;
            anchorMin.x /= Screen.width;
            anchorMin.y /= Screen.height;
            anchorMax.x /= Screen.width;
            anchorMax.y /= Screen.height;

            rectTransform.anchorMin = anchorMin;
            rectTransform.anchorMax = anchorMax;
        }

        private void OnDestroy()
        {
            LocalizationSettings.SelectedLocaleChanged -= OnLocaleChanged;
            _buildingsManager.OnResourcePointsChange -= RefreshResourcePoints;
            _buildingsManager.OnDefensePointsChange -= RefreshDefensePoints;
            _buildingsManager.OnDestinyShardsPointsChange -= RefreshShardsPoints;
            _worldManager.OnResourcesRequirementsMeet -= ActivateEndMissionButton;
            _worldManager.CurrentQuests[0].OnCompletion -= HandleFirstQuestCompletion;
            _worldManager.CurrentQuests[1].OnCompletion -= HandleSecondQuestCompletion;
            _worldManager.OnQuestsProgress -= RefreshQuestsText;
            _worldManager.OnNewMissionStart -= RefreshStormSlider;
            _worldManager.OnNewDayStarted -= NewDayHandler;
            _worldManager.OnStormCheck -= CheckNextDaysOnDemand;
            _gameManager.OnPlayerStateChange -= MainButtonHandler;
            _gameManager.OnDaySkipPossibility -= CheckDaySkipPossibility;
            _gameManager.OnAfterLoad -= AfterLoadHandler;
            _worldManager.CurrentQuests[0].OnCompletion -= HandleFirstQuestCompletion;
            _worldManager.CurrentQuests[1].OnCompletion -= HandleSecondQuestCompletion;
            
            _addButton.onClick.RemoveAllListeners();
        }

        private void Update()
        {
            VinetePanel.SetActive(BlockHud);
            _skipDayText.text = _gameManager.TimePassed;

            if (_createdImages.Values.Any(x=> x.Count > 0))
            {
                MovePoints();
            }
            else
            {
                _speedMultiplier = 200;
            }

            if (_state != DuringDayState.WorkDayFinished)
            {
                _endDayButton.interactable = false;
            }
            else
            {
                if (_narratorManager.CurrentTutorialStep == TutorialStep.OnFarmPanelWithTechnology_Q12)
                {
                    _endDayButton.interactable = !_buildingsManager.GetSpecificBuilding(BuildingType.Farm).CanUpgradeTechnology();
                }
                else if (_narratorManager.CurrentTutorialStep == TutorialStep.AfterRankUp_Q16)
                {
                    var gt = _buildingsManager.GetSpecificBuilding(BuildingType.GuardTower);
                    
                    if (gt != null && (gt.CanUpgradeTechnology() || gt.CanPlayMinigame()))
                    {
                        _endDayButton.interactable = false;
                    }
                    else
                    {
                        _endDayButton.interactable = true;
                    }
                }
                else
                {
                    _endDayButton.interactable = !_narratorManager.ShouldBlockBuildingPanelButton();
                }
            }
            
            if (_narratorManager.CurrentTutorialStep is TutorialStep.OnFinishedFarm_Q9
                or TutorialStep.OnFarmPanelOpen_Q10 or TutorialStep.OnFourthWorkingPanelOpen_Q11
                or TutorialStep.OnFarmPanelWithTechnology_Q12
                or TutorialStep.OnTechnologyInFarmLvlUp_Q13 or TutorialStep.OnFarmMinigameEnded_Q14)
            {
                RankGo.GetComponent<Button>().interactable = false;
            }

            if (_narratorManager.CurrentTutorialStep is TutorialStep.OnFarmPanelClosed_Q15)
            {
                RankGo.GetComponent<Button>().interactable = true;
            }
            
            _skipDayButton.interactable = !_narratorManager.ShouldBlockSkipButton();
        }

        private void CheckNextDaysOnDemand(int p_daysFromCurrent)
        {
            var currentDay = Convert.ToInt32(StormSlider.value);
            var nextDaysToCheck =
                currentDay + p_daysFromCurrent; // Include p_daysFromCurrent days starting from currentDay

            for (var i = currentDay; i <= nextDaysToCheck; i++) // <= to include nextDaysToCheck in the loop
            {
                if (_createdDaysStorm.Count <= i - 1)
                    continue;

                if (_createdDaysStorm[i - 1])
                {
                    if (i >= _worldManager.FinalHiddenStormDay)
                    {
                        _createdDaysStorm[i - 1].GetComponentInChildren<Image>().color = StormDay;
                        StormHandle.GetComponent<Image>().sprite = LightingHandleImage;
                        StormSlider.fillRect.GetComponent<Image>().color = new Color(255, 0, 0, 0.5f);
                    }
                    else
                    {
                        _createdDaysStorm[i - 1].GetComponentInChildren<Image>().color = NormalDay;
                        StormHandle.GetComponent<Image>().sprite = NormalHandleImage;
                        StormSlider.fillRect.GetComponent<Image>().color = new Color(0, 0, 0, 0.5f);
                    }
                }
            }
        }


        private void AfterLoadHandler()
        {
            RefreshStormSlider();
            _state = _gameManager.CurrentPlayerState;

            var rectGo = StormHandle.GetComponent<RectTransform>();
            var rect = rectGo.anchoredPosition;
            rect.x = _singleDayGoWidth;
            rectGo.anchoredPosition = rect;

            RefreshQuestsText();

            _worldManager.CurrentQuests[0].OnCompletion += HandleFirstQuestCompletion;
            _worldManager.CurrentQuests[1].OnCompletion += HandleSecondQuestCompletion;

            ShardsOfDestiny.text = $"{_buildingsManager.CurrentDestinyShards.ToString()}";
            DefensePoints.text = $"{_buildingsManager.CurrentDefensePoints.ToString()}";
            if (_buildingsManager.CurrentResourcePoints >= _worldManager.RequiredResourcePoints)
                ResourcePoints.text = $"{_buildingsManager.CurrentResourcePoints} / " +
                                      $"<color=green>{_worldManager.RequiredResourcePoints}</color>";
            else
                ResourcePoints.text = $"{_buildingsManager.CurrentResourcePoints} / " +
                                      $"<color=red>{_worldManager.RequiredResourcePoints}</color>";

            CheckQuestsCompletion();
        }

        private void NewDayHandler()
        {
            if (_narratorManager.CurrentTutorialStep == TutorialStep.OnThirdWorkingPanelOpen_Q7)
            {
                var farm = _buildingsManager.CurrentBuildings.Find(x => x.BuildingMainData.Type == BuildingType.Farm);
                if (farm != null && farm.CanEndBuildingSequence && farm.CurrentLevel == 0)
                {
                    _narratorManager.TryToActivateNarrator(TutorialStep.OnFinishingFarmAvaiable_Q8);
                }
            }

            _state = DuringDayState.WorkDayFinished;
            StormSlider.value = _worldManager.CurrentDay;
            var roundedInt = Convert.ToInt32(StormSlider.value);

            if (roundedInt >= _worldManager.StormDaysRange.x)
            {
                StormHandle.GetComponent<Image>().sprite = LightingHandleImage;
                StormSlider.fillRect.GetComponent<Image>().color = new Color(255, 0, 0, 0.5f);
            }
            else
            {
                StormHandle.GetComponent<Image>().sprite = NormalHandleImage;
            }

            if (roundedInt != 1)
                return;

            var rectGo = StormHandle.GetComponent<RectTransform>();
            var rect = rectGo.anchoredPosition;
            rect.x = _singleDayGoWidth;
            rectGo.anchoredPosition = rect;
        }

        private void RefreshStormSlider()
        {
            foreach (var daysOnUi in _createdDaysStorm)
                Destroy(daysOnUi);

            _createdDaysStorm.Clear();
            StormSlider.maxValue = _worldManager.StormDaysRange.y;

            for (var i = 0; i < _worldManager.StormDaysRange.y; i++)
            {
                var gO = Instantiate(StormDaysPrefab, StormSliderBackground.transform);
                var currentDay = i + 1;

                gO.GetComponentInChildren<TextMeshProUGUI>().text = currentDay.ToString();
                gO.GetComponentInChildren<Image>().color = i >= _worldManager.StormDaysRange.x ? StormDay : NormalDay;

                _createdDaysStorm.Add(gO);
            }

            _singleDayGoWidth = -StormSliderBackground.GetComponent<RectTransform>().rect.width /
                                _worldManager.StormDaysRange.y / 2;

            NewDayHandler();
        }

        private void RefreshShardsPoints(int p_points, bool p_makeIcons)
        {
            ShardsOfDestiny.text = $"{_buildingsManager.CurrentDestinyShards.ToString()}";

            if (p_makeIcons)
                TryToCreatePoints(p_points, PointsType.StarDust);
        }

        private void RefreshDefensePoints(int p_points, bool p_makeIcons)
        {
            DefensePoints.text = $"{_buildingsManager.CurrentDefensePoints.ToString()}";

            if (p_makeIcons)
                TryToCreatePoints(p_points, PointsType.Defense);
        }

        private void RefreshResourcePoints(int p_points, bool p_makeIcons)
        {
            if (_buildingsManager.CurrentResourcePoints >= _worldManager.RequiredResourcePoints)
                ResourcePoints.text = $"{_buildingsManager.CurrentResourcePoints} / " +
                                      $"<color=green>{_worldManager.RequiredResourcePoints}</color>";
            else
                ResourcePoints.text = $"{_buildingsManager.CurrentResourcePoints} / " +
                                      $"<color=red>{_worldManager.RequiredResourcePoints}</color>";

            if (p_makeIcons)
                TryToCreatePoints(p_points, PointsType.Resource);
        }
        

        private void TryToCreatePoints(int p_points, PointsType p_pointsType)
        {
            if (p_points <= 0)
                return;

            var dividedPoints = p_points / 2;

            if (dividedPoints == 0)
                dividedPoints = 1;

            var resolution = Mathf.Min(Screen.width, Screen.height);
            
            float jitterPercentage = 0.1f; 
            float jitter = resolution * jitterPercentage;
            float pointSizePercentage = resolution > 1080 ? 0.05f : 0.1f; 
            float pointSize =  resolution * pointSizePercentage;
            Vector2 pointSizeVector = new Vector2(pointSize, pointSize);

            for (var i = 0; i < dividedPoints; i++)
            {
                var imageObject = new GameObject("Points" + i);
                imageObject.transform.SetParent(_mainCanvas.transform, false);
                Image image = imageObject.AddComponent<Image>();

                var rectTransform = imageObject.GetComponent<RectTransform>();
                rectTransform.sizeDelta = pointSizeVector;

                Vector3 lastClickPosition = TransparentPanelClickHandler.LastClickPosition;

                lastClickPosition.x += Random.Range(-jitter, jitter);
                lastClickPosition.y += Random.Range(-jitter, jitter);

                rectTransform.position = lastClickPosition;

                switch (p_pointsType)
                {
                    case PointsType.Resource:
                        image.sprite = _buildingsManager.ResourcesPointsIcon;
                        _createdImages[ResourcePointsImage.rectTransform].Add(imageObject);
                        break;
                    case PointsType.Defense:
                        image.sprite = _buildingsManager.DefensePointsIcon;
                        _createdImages[DefensePointsImage.rectTransform].Add(imageObject);
                        break;
                    case PointsType.StarDust:
                        image.sprite = _buildingsManager.ShardsOfDestinyIcon;
                        _createdImages[ShardsOfDestinyImage.rectTransform].Add(imageObject);
                        break;
                }
            }
            
            switch (p_pointsType)
            {
                case PointsType.Resource:
                    _audioManager.CreateNewAudioSource(_resourcePointsManipulation);
                    break;
                case PointsType.Defense:
                    _audioManager.CreateNewAudioSource(_defensePointsManipulation);
                    break;
                case PointsType.StarDust:
                    _audioManager.CreateNewAudioSource(_destinyShardsManipulation);
                    break;
            }
        }
        
        private void MovePoints()
        {
            _speedMultiplier += Time.deltaTime * 50f;
            float referenceDimension = Screen.width + Screen.height;
            float speedScaleFactor = referenceDimension / 1080f; 
            float distanceThreshold = referenceDimension * 0.001f;

            foreach (var specificImages in _createdImages)
            {
                for (var i = specificImages.Value.Count - 1; i >= 0; i--)
                {
                    var imageObject = specificImages.Value[i];
                    var rectTransform = imageObject.GetComponent<RectTransform>();
                    var speed = _speedMultiplier * speedScaleFactor * Time.deltaTime;

                    rectTransform.position = Vector2.MoveTowards(rectTransform.position,
                        specificImages.Key.transform.position, speed);

                    if (Vector2.Distance(rectTransform.position, specificImages.Key.transform.position) < distanceThreshold)
                    {
                        specificImages.Value.RemoveAt(i);
                        _audioManager.CreateNewAudioSource(_pointGot);
                        Destroy(imageObject);
                    }
                }
            }
        }
        
        private void MainButtonHandler(DuringDayState p_newState)
        {
            _state = p_newState;
            switch (p_newState)
            {
                case DuringDayState.FinishingBuilding:
                    _endDayButton.interactable = false;
                    _endDayButtonText.text = _finalizeBuildingProcess.GetLocalizedString();
                    _wasMainButtonRefreshed = true;
                    break;
                case DuringDayState.CollectingResources:
                    _endDayButton.interactable = false;
                    _endDayButtonText.text = _collectAvaiablePoints.GetLocalizedString();
                    _wasMainButtonRefreshed = true;
                    break;
                case DuringDayState.WorkDayFinished:
                    _endDayButton.interactable = true;

                    if (_wasMainButtonRefreshed)
                    {
                        _endDayButton.onClick.AddListener(OpenWorkersDisplacementPanel);
                        _endDayButtonText.text = _planNextDay.GetLocalizedString();
                        _wasMainButtonRefreshed = false;
                    }

                    break;

                case DuringDayState.SettingWorkers:
                    _endDayButton.interactable = false;
                    _endDayButtonText.text =  _settingWorkers.GetLocalizedString();
                    break;

                case DuringDayState.DayPassing:
                    _endDayButtonText.text = _dayIsPassing.GetLocalizedString();
                    _endDayButton.interactable = false;
                    SkipDayGo.SetActive(true);
                    _wasMainButtonRefreshed = true;
                    CheckDaySkipPossibility();
                    break;
            }
        }

        private void CheckDaySkipPossibility()
        {
            if (_gameManager.CurrentPlayerState != DuringDayState.DayPassing)
                return;

            if (_gameManager.CanUseSkipByTime)
            {
                _skipDayButton.interactable = true;

                _skipDayButton.onClick.RemoveAllListeners();
                _skipDayText.text = _endDay.GetLocalizedString();
                _paidSkipDayText.text = "";
                _skipDayButton.onClick.AddListener(() => OnWorkDaySkipped(WayToSkip.NormalTimeSkip));
                _wasMainButtonRefreshed = false;
            }

            if (!_gameManager.CanUseSkipByTime && _gameManager.CanSkipDay(out var skipPossibility))
            {
                _skipDayButton.interactable = true;

                if (_wasMainButtonRefreshed)
                {
                    if (skipPossibility == WayToSkip.FreeSkip)
                        _paidSkipDayText.text = string.Format(_skippingFreeSkipText.GetLocalizedString(), _gameManager.FreeSkipsLeft);
                    else if (skipPossibility == WayToSkip.PaidSkip)
                        _paidSkipDayText.text = string.Format(_skippingStartDustText.GetLocalizedString(), _gameManager.DestinyShardsSkipPrice);

                    _skipDayButton.onClick.RemoveAllListeners();
                    _skipDayButton.onClick.AddListener(() => OnWorkDaySkipped(skipPossibility));
                    _wasMainButtonRefreshed = false;
                }
            }
        }

        private void ActivateEndMissionButton()
        {
            EndMissionGo.SetActive(true);
            _endMissionButton.interactable = true;
        }

        private void OpenWorkersDisplacementPanel()
        {
            _audioManager.PlayButtonSoundEffect(_endDayButton.interactable);

            _gameManager.SetPlayerState(DuringDayState.SettingWorkers);
            _endDayButton.onClick.RemoveListener(OpenWorkersDisplacementPanel);
            _wasMainButtonRefreshed = true;
        }

        private void OnWorkDaySkipped(WayToSkip p_skipSource)
        {
            _audioManager.CreateNewAudioSource(_daySkippedSound);
            
            _skipDayButton.onClick.RemoveAllListeners();
            SkipDayGo.SetActive(false);
            _skipDayButton.interactable = false;
            _wasMainButtonRefreshed = true;

            _gameManager.SetPlayerState(DuringDayState.FinishingBuilding);
            _gameManager.EndTheDay(p_skipSource);

            _narratorManager.TryToActivateNarrator(TutorialStep.OnDaySkipped_Q5);
        }

        #region Quests

        private void HandleFirstQuestCompletion(Quest p_completedQuest)
        {
            _firstMissionButton.interactable = true;
            _firstMissionButton.onClick.RemoveAllListeners();
            _firstMissionButton.onClick.AddListener(() => GatherPointsFromQuest(0, p_completedQuest));
            _firstMissionButtonText.text = string.Format(_collectXYZDestinyShards.GetLocalizedString(),
                p_completedQuest.SpecificQuest.ShardsOfDestinyReward); 
        }

        private void HandleSecondQuestCompletion(Quest p_completedQuest)
        {
            _secondMissionButton.interactable = true;
            _secondMissionButton.onClick.RemoveAllListeners();
            _secondMissionButton.onClick.AddListener(() => GatherPointsFromQuest(1, p_completedQuest));
            _secondMissionButtonText.text = string.Format(_collectXYZDestinyShards.GetLocalizedString(),
                p_completedQuest.SpecificQuest.ShardsOfDestinyReward);
        }

        private void GatherPointsFromQuest(int p_questIndex, Quest p_quest)
        {
            _audioManager.CreateNewAudioSource(_onQuestCompletionSound);

            p_quest.IsRedeemed = true;

            if (p_questIndex == 0)
            {
                _firstMissionButton.interactable = false;
                _firstMissionButtonText.text = _completedQuest.GetLocalizedString();
            }
            else
            {
                _secondMissionButton.interactable = false;
                _secondMissionButtonText.text = _completedQuest.GetLocalizedString();
            }

            _buildingsManager.HandlePointsManipulation(PointsType.StarDust,
                p_quest.SpecificQuest.ShardsOfDestinyReward, true, true);

            CheckQuestsCompletion();
        }

        private void CheckQuestsCompletion()
        {
            if (!_worldManager.CurrentQuests[0].IsRedeemed ||
                !_worldManager.CurrentQuests[1].IsRedeemed)
                return;

            if (_worldManager.CurrentMission >= _worldManager.NeededMissionToRankUp)
            {
                var button = RankGo.GetComponent<Button>();
                _currentRankText.text = _clickToRankUp.GetLocalizedString();
                button.onClick.RemoveAllListeners();
                button.onClick.AddListener(HandleQuestsCompletion);
                button.interactable = true;

                QuestsCompletedGo.SetActive(false);
            }
            else
            {
                QuestsCompletedGo.SetActive(true);
                QuestsCompletedGo.GetComponentInChildren<TextMeshProUGUI>().text =
                    string.Format(_completeMissionToRankUp.GetLocalizedString(), _worldManager.NeededMissionToRankUp);
            }
        }

        private void HandleQuestsCompletion()
        {
            _audioManager.CreateNewAudioSource(_rankUpSound);

            _narratorManager.TryToActivateNarrator(TutorialStep.AfterRankUp_Q16);

            _worldManager.CurrentQuests[0].OnCompletion -= HandleFirstQuestCompletion;
            _worldManager.CurrentQuests[1].OnCompletion -= HandleSecondQuestCompletion;

            _worldManager.HandleRankUp();

            _worldManager.CurrentQuests[0].OnCompletion += HandleFirstQuestCompletion;
            _worldManager.CurrentQuests[1].OnCompletion += HandleSecondQuestCompletion;

            RankGo.GetComponent<Button>().interactable = false;
            _worldManager.CheckNewQuests();
            RefreshQuestsText();
        }

        private void RefreshQuestsText()
        {
            _firstQuestText.text = _worldManager.GetSpecificQuestText(0);
            _secondQuestText.text = _worldManager.GetSpecificQuestText(1);
            _firstMissionButtonText.text = _worldManager.GetSpecificQuestObjectiveText(0);
            _secondMissionButtonText.text = _worldManager.GetSpecificQuestObjectiveText(1);

            _currentMissionText.text = _worldManager.CurrentMission.ToString();

            if (_worldManager.CurrentQuests[0].IsRedeemed && _worldManager.CurrentQuests[1].IsRedeemed &&
                _worldManager.CurrentMission >= _worldManager.NeededMissionToRankUp)
            {
                _currentRankText.text = _clickToRankUp.GetLocalizedString();
            }
            else
            {
                _currentRankText.text = _worldManager.CurrentRank.ToString();
            }
        }

        #endregion

        private void OnLocaleChanged(Locale p_locale)
        {
            RefreshQuestsText();
            
            if (_gameManager.CanUseSkipByTime)
            {
                _skipDayText.text = _endDay.GetLocalizedString();
            }
            
            if (!_gameManager.CanUseSkipByTime && _gameManager.CanSkipDay(out var skipPossibility))
            {
                if (_wasMainButtonRefreshed)
                {
                    if (skipPossibility == WayToSkip.FreeSkip)
                        _paidSkipDayText.text = string.Format(_skippingFreeSkipText.GetLocalizedString(), _gameManager.FreeSkipsLeft);
                    else if (skipPossibility == WayToSkip.PaidSkip)
                        _paidSkipDayText.text = string.Format(_skippingStartDustText.GetLocalizedString(), _gameManager.DestinyShardsSkipPrice);
                }
            }

            if (_worldManager.CurrentMission >= _worldManager.NeededMissionToRankUp)
            {
                _currentRankText.text = _clickToRankUp.GetLocalizedString();
            }
            else
            {
                QuestsCompletedGo.GetComponentInChildren<TextMeshProUGUI>().text =
                    string.Format(_completeMissionToRankUp.GetLocalizedString(), _worldManager.NeededMissionToRankUp);
            }
            
            _currentRankText.text = _clickToRankUp.GetLocalizedString();
            
            switch (_state)
            {
                case DuringDayState.FinishingBuilding:
                    _endDayButtonText.text = _finalizeBuildingProcess.GetLocalizedString();
                    break;
                case DuringDayState.CollectingResources:
                    _endDayButtonText.text = _collectAvaiablePoints.GetLocalizedString();
                    break;
                case DuringDayState.WorkDayFinished:
                        _endDayButtonText.text = _planNextDay.GetLocalizedString();
                    break;

                case DuringDayState.SettingWorkers:
                    _endDayButtonText.text =  _settingWorkers.GetLocalizedString();
                    break;

                case DuringDayState.DayPassing:
                    _endDayButtonText.text = _dayIsPassing.GetLocalizedString();
                    break;
            }
        }
    }
}