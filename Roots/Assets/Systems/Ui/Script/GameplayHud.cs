using System;
using System.Collections.Generic;
using AudioSystem;
using Buildings;
using GameManager;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using World;
using Random = UnityEngine.Random;

namespace InGameUi
{
    public class GameplayHud : MonoBehaviour
    {
        public static bool BlockHud;
        [SerializeField] private Canvas _mainCanvas;

        [SerializeField] private AudioManager _audioManager;
        [SerializeField] private MainGameManager _gameManager;
        [SerializeField] private WorldManager _worldManager;
        [SerializeField] private BuildingsManager _buildingsManager;
        [SerializeField] private SettingsPanel _settingsPanel;
        [SerializeField] private GameObject StormSliderBackground;
        [SerializeField] private GameObject StormDaysPrefab;
        [SerializeField] private GameObject StormHandle;
        [SerializeField] private GameObject RankGo;

        [SerializeField] private Sprite StormImage;
        [SerializeField] private Sprite SunImage;

        [SerializeField] private GameObject SkipDayGo;
        [SerializeField] private GameObject EndMissionGo;
        [SerializeField] private GameObject EndDayGo;
        [SerializeField] private GameObject VinetePanel;

        //left side of screen
        [SerializeField] private GameObject _settingsButtonGo;
        //left side of screen

        // Quests
        [SerializeField] private GameObject QuestsCompletedGo;
        [SerializeField] private GameObject FirstMissionGo;
        [SerializeField] private GameObject SecondMissionGo;
        [SerializeField] private GameObject FirstMissionObjectiveGo;
        [SerializeField] private GameObject SecondMissionObjectiveGo;
        private List<GameObject> _createdDaysStorm;

        private Dictionary<RectTransform, List<GameObject>> _createdImages;
        [SerializeField] private TextMeshProUGUI _currentMissionText;
        [SerializeField] private TextMeshProUGUI _currentRankText;
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

        //Audio clips
        [SerializeField] private AudioClip _destinyShardsManipulation;
        [SerializeField] private AudioClip _resourcePointsManipulation;
        [SerializeField] private AudioClip _defensePointsManipulation;

        // Quests
        private float _singleDayGoWidth;
        private Button _skipDayButton;

        [SerializeField] private TextMeshProUGUI _skipDayText;
        private bool _wasMainButtonRefreshed = true;
        [SerializeField] private TextMeshProUGUI DefensePoints;
        [SerializeField] private Image DefensePointsImage;

        [SerializeField] private TextMeshProUGUI ResourcePoints;

        [SerializeField] private Image ResourcePointsImage;
        [SerializeField] private TextMeshProUGUI ShardsOfDestiny;
        [SerializeField] private Image ShardsOfDestinyImage;

        [SerializeField] private Slider StormSlider;

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

            _settingsButton.onClick.AddListener(delegate { _settingsPanel.OpenPanel(); });

            ShardsOfDestiny.text = $"{_buildingsManager.CurrentDestinyShards}";
            DefensePoints.text = $"{_buildingsManager.CurrentDefensePoints}";
            ResourcePoints.text = $"{_buildingsManager.CurrentResourcePoints} / " +
                                  $"{_worldManager.RequiredResourcePoints}";

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

        private void Update()
        {
            _skipDayText.text = _gameManager.TimePassed;

            MovePoints();
            VinetePanel.SetActive(BlockHud);
        }

        private void CheckNextDaysOnDemand(int p_daysFromCurrent)
        {
            var currentDay = Convert.ToInt32(StormSlider.value);
            var nextDay = 1 + currentDay;
            var nextDaysToCheck = currentDay + p_daysFromCurrent;

            Debug.Log(
                $"Checking days. Current Day {currentDay}. Next Day {nextDaysToCheck}. Checking: {nextDaysToCheck}");

            for (var i = nextDay; i < nextDaysToCheck; i++)
            {
                if (_createdDaysStorm[i])
                {
                    if (i >= _worldManager.FinalHiddenStormDay)
                    {
                        _createdDaysStorm[i].GetComponentInChildren<Image>().sprite = StormImage;
                        StormHandle.GetComponent<Image>().color = Color.red;
                        StormSlider.fillRect.GetComponent<Image>().color = new Color(255, 0, 0, 0.5f);
                    }
                    else
                    {
                        _createdDaysStorm[i].GetComponentInChildren<Image>().sprite = SunImage;
                        StormHandle.GetComponent<Image>().color = Color.white;
                        StormSlider.fillRect.GetComponent<Image>().color = new Color(0, 0, 0, 0.5f);
                    }
                }
            }
        }

        private void AfterLoadHandler()
        {
            RefreshStormSlider();

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
            StormSlider.value = _worldManager.CurrentDay;
            var roundedInt = Convert.ToInt32(StormSlider.value);

            if (roundedInt >= _worldManager.StormDaysRange.x)
            {
                StormHandle.GetComponent<Image>().color = Color.red;
                StormSlider.fillRect.GetComponent<Image>().color = new Color(255, 0, 0, 0.5f);
            }
            else
            {
                StormHandle.GetComponent<Image>().color = Color.white;
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
                if (i >= _worldManager.StormDaysRange.x)
                    gO.GetComponentInChildren<Image>().sprite = StormImage;

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
                TryToCreatePoints(p_points, PointsType.ShardsOfDestiny);
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

            var dividedPoints = p_points / 3;

            if (dividedPoints == 0)
                dividedPoints = 1;

            for (var i = 0; i < dividedPoints; i++)
            {
                var imageObject = new GameObject("Points" + i);
                imageObject.transform.SetParent(_mainCanvas.transform);
                Image image = imageObject.AddComponent<Image>();

                var rectTransform = imageObject.GetComponent<RectTransform>();
                rectTransform.sizeDelta = new Vector2(50, 50);
                Vector3 lastClickPosition = TransparentPanelClickHandler.LastClickPosition;

                var jitter = 50f;
                lastClickPosition.x += Random.Range(-jitter, jitter);
                lastClickPosition.y += Random.Range(-jitter, jitter);

                // Set the jittered position to the RectTransform
                rectTransform.position = lastClickPosition;

                switch (p_pointsType)
                {
                    case PointsType.Resource:
                        image.sprite = _buildingsManager.ResourcesPointsIcon;
                        _createdImages[ResourcePointsImage.rectTransform].Add(imageObject);
                        _audioManager.PlaySpecificSoundEffect(_resourcePointsManipulation);
                        break;
                    case PointsType.Defense:
                        image.sprite = _buildingsManager.DefensePointsIcon;
                        _createdImages[DefensePointsImage.rectTransform].Add(imageObject);
                        _audioManager.PlaySpecificSoundEffect(_defensePointsManipulation);
                        break;
                    case PointsType.ShardsOfDestiny:
                        image.sprite = _buildingsManager.ShardsOfDestinyIcon;
                        _createdImages[ShardsOfDestinyImage.rectTransform].Add(imageObject);
                        _audioManager.PlaySpecificSoundEffect(_destinyShardsManipulation);
                        break;
                }
            }
        }

        private void MovePoints()
        {
            foreach (var specificImages in _createdImages)
                for (var i = specificImages.Value.Count - 1; i >= 0; i--)
                {
                    var imageObject = specificImages.Value[i];
                    var rectTransform = imageObject.GetComponent<RectTransform>();
                    rectTransform.position = Vector2.MoveTowards(rectTransform.position,
                        specificImages.Key.transform.position, 1000 * Time.deltaTime);

                    if (Vector2.Distance(rectTransform.position, specificImages.Key.transform.position) < 0.1f)
                    {
                        specificImages.Value.RemoveAt(i);
                        Destroy(imageObject);
                    }
                }
        }

        private void MainButtonHandler(DuringDayState p_newState)
        {
            switch (p_newState)
            {
                case DuringDayState.FinishingBuilding:
                    _endDayButtonText.text = "Finalize building process";
                    break;
                case DuringDayState.CollectingResources:
                    _endDayButtonText.text = "Collect available points";
                    break;
                case DuringDayState.WorkDayFinished:
                    _endDayButton.interactable = true;

                    if (_wasMainButtonRefreshed)
                    {
                        _endDayButtonText.text = "Plan next day";
                        _endDayButton.onClick.AddListener(OpenWorkersDisplacementPanel);
                        _wasMainButtonRefreshed = false;
                    }

                    break;

                case DuringDayState.SettingWorkers:
                    _endDayButtonText.text = "Setting workers";
                    break;

                case DuringDayState.DayPassing:
                    _endDayButtonText.text = "Day is passing...";
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
                _skipDayText.text = "Skip";
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
                        _paidSkipDayText.text = $"Skip by: Free Skips ({_gameManager.FreeSkipsLeft})";
                    else if (skipPossibility == WayToSkip.PaidSkip)
                        _paidSkipDayText.text =
                            $"Skip for: {_gameManager.DestinyShardsSkipPrice} Destiny Shards";

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
            _gameManager.SetPlayerState(DuringDayState.SettingWorkers);
            _endDayButton.onClick.RemoveListener(OpenWorkersDisplacementPanel);
            _wasMainButtonRefreshed = true;
        }

        private void OnWorkDaySkipped(WayToSkip p_skipSource)
        {
            _skipDayButton.onClick.RemoveAllListeners();
            SkipDayGo.SetActive(false);
            _skipDayButton.interactable = false;
            _wasMainButtonRefreshed = true;

            _gameManager.SetPlayerState(DuringDayState.FinishingBuilding);
            _gameManager.SkipDay(p_skipSource);
        }

        #region Quests

        private void HandleFirstQuestCompletion(Quest p_completedQuest)
        {
            _firstMissionButton.interactable = true;
            _firstMissionButton.onClick.RemoveAllListeners();
            _firstMissionButton.onClick.AddListener(() => GatherPointsFromQuest(0, p_completedQuest));
            _firstMissionButtonText.text =
                $"Collect {p_completedQuest.SpecificQuest.ShardsOfDestinyReward} Destiny Shards";
        }

        private void HandleSecondQuestCompletion(Quest p_completedQuest)
        {
            _secondMissionButton.interactable = true;
            _secondMissionButton.onClick.RemoveAllListeners();
            _secondMissionButton.onClick.AddListener(() => GatherPointsFromQuest(1, p_completedQuest));
            _secondMissionButtonText.text =
                $"Collect {p_completedQuest.SpecificQuest.ShardsOfDestinyReward} Destiny Shards";
        }

        private void GatherPointsFromQuest(int p_questIndex, Quest p_quest)
        {
            p_quest.IsRedeemed = true;

            if (p_questIndex == 0)
            {
                _firstMissionButton.interactable = false;
                _firstMissionButtonText.text = "Completed";
            }
            else
            {
                _secondMissionButton.interactable = false;
                _secondMissionButtonText.text = "Completed";
            }

            _buildingsManager.HandlePointsManipulation(PointsType.ShardsOfDestiny,
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
                _currentRankText.text = "Click To Rank Up";
                button.onClick.RemoveAllListeners();
                button.onClick.AddListener(HandleQuestsCompletion);
                button.interactable = true;

                QuestsCompletedGo.SetActive(false);
            }
            else
            {
                QuestsCompletedGo.SetActive(true);
                QuestsCompletedGo.GetComponentInChildren<TextMeshProUGUI>().text =
                    $"Complete mission {_worldManager.NeededMissionToRankUp} to rank up";
            }
        }

        private void HandleQuestsCompletion()
        {
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
            _currentRankText.text = _worldManager.CurrentRank.ToString();
        }

        #endregion
    }
}