using System;
using Buildings;
using Gods;
using Saving;
using UnityEngine;
using World;

namespace GameManager
{
    // odciążyć saving poprzez zbieranie tutaj wszyystkich danych i managerów
    // przenieść tutaj free skipsy z word managera
    // przenieś aktualny stan gracza tickowanie czasu z gameplay huda (i wszystko co się da)
    // przenieśc logikę z sejvingu
    // przenieść ogólną logikę z word managera

    public class MainGameManager : MonoBehaviour
    {
        [SerializeField] private GodsManager _godsManager;
        [SerializeField] private WorldManager _worldManager;
        [SerializeField] private BuildingsManager _buildingsManager;
        [SerializeField] private SavingManager _savingManager;
        [SerializeField] private int _destinyShardsSkipPrice;
        [SerializeField] private int _oneDayTimerDurationInSeconds = 60;
        [SerializeField] private int _maxFreeDaysSkipAmount = 3;

        private DateTime _startTime;
        private float _timeLeftInSeconds;

        public int FreeSkipsLeft { get; private set; }
        public int HoursOfAbstence { get; private set; }
        public int FreeSkipsGotten { get; private set; }

        public DuringDayState CurrentPlayerState { get; private set; }

        public int DestinyShardsSkipPrice => _destinyShardsSkipPrice;
        public string TimePassed { get; private set; }

        public bool CanUseSkipByTime { get; private set; }

        public event Action OnPlayerCameBack;
        public event Action OnAfterLoad;
        public event Action<DuringDayState> OnPlayerStateChange;
        public event Action OnDaySkipPossibility;

        private void Start()
        {
            // 0 for no sync, 1 for panel refresh rate, 2 for 1/2 panel rate
            //QualitySettings.vSyncCount = 2;
            Application.targetFrameRate = 30;
            _startTime = DateTime.UtcNow;

            var willBeLoaded = _savingManager.IsSaveAvaiable();

            _worldManager.CustomStart(willBeLoaded);
            _savingManager.OnLoad += LoadSavedData;

            if (willBeLoaded)
                _savingManager.LoadMainGame();
            else
                SetPlayerState(DuringDayState.FinishingBuilding);
        }

        private void Update() // need server?
        {
            var elapsedSeconds = (DateTime.UtcNow - _startTime).TotalSeconds;
            _timeLeftInSeconds = _oneDayTimerDurationInSeconds - (float)elapsedSeconds;

            var minutes = Mathf.FloorToInt(_timeLeftInSeconds / 60);
            var seconds = Mathf.FloorToInt(_timeLeftInSeconds % 60);

            if (_timeLeftInSeconds > 0) 
                TimePassed = $"{minutes}:{seconds:00}";

            CheckPlayerState();
        }

        private void LoadSavedData(MainGameManagerSavedData p_data)
        {
            CurrentPlayerState = (DuringDayState)p_data.CurrentPlayerState;
            FreeSkipsLeft = p_data.FreeDaysSkipAmount;

            var savedTime = p_data.TimeOfWorkersSet;
            var currentTime = DateTime.UtcNow;

            TimeSpan elapsed = currentTime - savedTime;
            var timePassedInSeconds = elapsed.TotalSeconds;

            HoursOfAbstence = 0;
            FreeSkipsGotten = 0;

            while (timePassedInSeconds >= _oneDayTimerDurationInSeconds)
            {
                HoursOfAbstence++;
                timePassedInSeconds -= _oneDayTimerDurationInSeconds;

                if (FreeSkipsLeft < _maxFreeDaysSkipAmount)
                {
                    FreeSkipsGotten++;
                    FreeSkipsLeft++;
                }
            }

            if (FreeSkipsGotten == 0)
            {
                var secondsToSubtract = elapsed.TotalSeconds % _oneDayTimerDurationInSeconds;
                _startTime = DateTime.UtcNow;
                var addSeconds = _startTime.AddSeconds(-secondsToSubtract);
                _startTime = addSeconds;
            }
            
            OnPlayerCameBack?.Invoke();
            OnPlayerStateChange?.Invoke(CurrentPlayerState);
            HandleAfterLoad();
        }

        private void HandleAfterLoad()
        {
            // _buildingsManager.UnlockedBuildings.Clear();
            // _worldManager.CustomStart(false);
            // _godsManager
            
            OnAfterLoad?.Invoke();
        }

        public void SetPlayerState(DuringDayState p_newState)
        {
            CurrentPlayerState = p_newState;

            if (CurrentPlayerState == DuringDayState.DayPassing)
            {
                _startTime = DateTime.UtcNow;

                _savingManager.SaveMainGame(new MainGameManagerSavedData
                {
                    TimeOfWorkersSet = _startTime,
                    CurrentPlayerState = (int)CurrentPlayerState,
                    FreeDaysSkipAmount = FreeSkipsLeft,
                    TimeLeftInSeconds = _timeLeftInSeconds
                });
            }

            OnPlayerStateChange?.Invoke(CurrentPlayerState);
        }

        private void CheckPlayerState()
        {
            switch (CurrentPlayerState)
            {
                // evening - day ended now collecting/building up
                case DuringDayState.FinishingBuilding:
                    if (!_buildingsManager.IsAnyBuildingNonBuilt()) SetPlayerState(DuringDayState.CollectingResources);
                    break;
                case DuringDayState.CollectingResources:
                    if (!_buildingsManager.IsAnyBuildingNonGathered()) SetPlayerState(DuringDayState.WorkDayFinished);
                    break;
                case DuringDayState.WorkDayFinished:
                    // night - timer, effect
                    break;
                // planning of day - new day started, cock is shouting or storm is coming
                case DuringDayState.SettingWorkers:
                    // workers setting - mumbling tawernsound in the background?
                    break;
                case DuringDayState.DayPassing:
                    if (_timeLeftInSeconds <= 0)
                    {
                        CanUseSkipByTime = true;
                        OnDaySkipPossibility?.Invoke();
                    }
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        #region Saving

        public void ResetSave()
        {
            _savingManager.ResetSave();
        }

        #endregion

        #region Day Skipping

        public void SkipDay(WayToSkip p_skipSource)
        {
            switch (p_skipSource)
            {
                case WayToSkip.FreeSkip:
                    FreeSkipsLeft--;
                    break;
                case WayToSkip.PaidSkip:
                    _buildingsManager.HandlePointsManipulation(PointsType.ShardsOfDestiny, DestinyShardsSkipPrice,
                        false);
                    break;
            }

            _worldManager.StartNewDay();
        }

        public bool CanSkipDay(out WayToSkip p_reason)
        {
            if (FreeSkipsLeft > 0)
            {
                p_reason = WayToSkip.FreeSkip;
                return true;
            }

            if (_buildingsManager.CurrentDestinyShards >= DestinyShardsSkipPrice)
            {
                p_reason = WayToSkip.PaidSkip;
                return true;
            }

            p_reason = WayToSkip.CantSkip;
            return false;
        }

        #endregion
    }

    public enum DuringDayState
    {
        FinishingBuilding = 0,
        CollectingResources = 1,
        WorkDayFinished = 2,
        SettingWorkers = 3,
        DayPassing = 4
    }
}