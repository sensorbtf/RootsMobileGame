using System;
using Buildings;
using TMPro;
using UnityEngine;
using World;
using UnityEngine.UI;

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

        [SerializeField] private WorldManager _worldManager;
        [SerializeField] private BuildingManager _buildingManager;
        [SerializeField] private WorkersPanel _workersPanel;

        [SerializeField] private TextMeshProUGUI CurrentDay;
        [SerializeField] private TextMeshProUGUI DayToStorm;
        [SerializeField] private TextMeshProUGUI ResourcePoints;
        [SerializeField] private TextMeshProUGUI DefensePoints;
        [SerializeField] private TextMeshProUGUI ShardsOfDestiny;

        [SerializeField] private GameObject SkipDayGo;
        [SerializeField] private GameObject EndMissionGo;
        [SerializeField] private GameObject EndDayGo;

        private DuringDayState CurrentPlayerState;
        private Button _skipDayButton;
        private Button _endMissionButton;
        private Button _endDayButton;
        private TextMeshProUGUI _endDayButtonText;
        
        bool _wasMainButtonRefreshed = true;

        private void Start()
        {
            _skipDayButton = SkipDayGo.GetComponent<Button>();
            _endMissionButton = EndMissionGo.GetComponent<Button>();
            _endDayButton = EndDayGo.GetComponent<Button>();

            _endDayButtonText = _endDayButton.GetComponentInChildren<TextMeshProUGUI>();

            SkipDayGo.SetActive(false);
            _workersPanel.OnBackToMap += SetWorkers;
        }

        private void Update() // Better way to do it?
        {
            CurrentDay.text = $"Current day: {_worldManager.CurrentDay.ToString()}";
            DayToStorm.text = $"Storm in: {_worldManager.StormDaysRange.ToString()}";
            ResourcePoints.text = $"Resource Points: {_buildingManager.CurrentResourcePoints.ToString()} / " +
                                  $"{_worldManager.NeededResourcePoints}";
            DefensePoints.text = $"Defense Points: {_buildingManager.CurrentDefensePoints.ToString()}";
            ShardsOfDestiny.text = $"Shards Of Destiny: {_buildingManager.ShardsOfDestinyAmount.ToString()}";


            if (_worldManager.CanLeaveMission())
            {
                EndMissionGo.SetActive(true);
                _endMissionButton.interactable = true;
            }
            else
            {
                EndMissionGo.SetActive(false);
                _endMissionButton.interactable = false;
            }

            MainButtonHandler(); // change that for events on click

            if (BlockHud)
            {
                _skipDayButton.interactable = false;
                _endMissionButton.interactable = false;
                _endDayButton.interactable = false;
            }
        }
        
        private void MainButtonHandler()
        {
            switch (CurrentPlayerState)
            {
                case DuringDayState.OnCollecting:

                    if (!_worldManager.CanSetWorkers())
                    {
                        _endDayButtonText.text = "Collect Points";
                        return;
                    }

                    if (_wasMainButtonRefreshed)
                    {
                        _endDayButton.interactable = true;

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
                            _endDayButton.onClick.AddListener(OnDayStarted);
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

                    if (_worldManager.CanSkipDay())
                    {
                        _skipDayButton.interactable = true;

                        if (_wasMainButtonRefreshed)
                        {
                            _skipDayButton.onClick.AddListener(OnDaySkipped);
                            _wasMainButtonRefreshed = false;
                        }
                    }

                    break;
            }
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

        private void OnDayStarted()
        {
            CurrentPlayerState = DuringDayState.Working;
            SkipDayGo.SetActive(true);
            _endDayButton.onClick.RemoveListener(OnDayStarted);
            _wasMainButtonRefreshed = true;
        }

        private void OnDaySkipped()
        {
            CurrentPlayerState = DuringDayState.OnCollecting;
            _skipDayButton.onClick.RemoveListener(OnDaySkipped);
            SkipDayGo.SetActive(false);
            _skipDayButton.interactable = false;
            _wasMainButtonRefreshed = true;

            _worldManager.SkipDay();
        }
    }
}