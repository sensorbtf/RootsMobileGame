using System;
using Buildings;
using UnityEngine;
using World;
using Saving;

namespace GameManager
{
    // odciążyć saving poprzez zbieranie tutaj wszyystkich danych i managerów
    // przenieść tutaj free skipsy z word managera
    // przenieś aktualny stan gracza tickowanie czasu z gameplay huda (i wszystko co się da)
    // przenieśc logikę z sejvingu
    // przenieść ogólną logikę z word managera
    
    public class MainGameManager : MonoBehaviour
    {
        [SerializeField] private WorldManager _worldManager;
        [SerializeField] private BuildingsManager _buildingsManager;
        [SerializeField] private SavingManager _savingManager;
        [SerializeField] private int _destinyShardsSkipPrice;
        [SerializeField] private int _oneDayTimerDurationInSeconds = 60;
        [SerializeField] private int _maxFreeDaysSkipAmount = 3;

        private int _freeDaysSkipAmount;
        private DuringDayState _currentPlayerState;
        
        private bool _canUseSkipByTime = false;
        private float _timeLeftInSeconds; 
        private DateTime _startTime;
        private string _timePassed;
        
        public int FreeSkipsLeft => _freeDaysSkipAmount;
        public DuringDayState CurrentPlayerState => _currentPlayerState;
        public int DestinyShardsSkipPrice => _destinyShardsSkipPrice;
        public string TimePassed => _timePassed;
        public bool CanUseSkipByTime => _canUseSkipByTime;

        public event Action<DuringDayState> OnPlayerStateChange;
        public event Action OnDaySkipPossibility;
        
        private void Start()
        {
            // 0 for no sync, 1 for panel refresh rate, 2 for 1/2 panel rate
            //QualitySettings.vSyncCount = 2;
            Application.targetFrameRate = 30;

            var willBeLoaded = _savingManager.IsSaveAvaiable();
            
            _worldManager.CustomStart(willBeLoaded);
            _savingManager.OnLoad += LoadSavedData;
            
            if (willBeLoaded)
            {
                _savingManager.LoadMainGame();
            }
            else
            {
                SetPlayerState(DuringDayState.FinishingBuilding);
            }
        }

        private void LoadSavedData(MainGameManagerSavedData p_data)
        {
            SetPlayerState((DuringDayState)p_data.CurrentPlayerState);
            _freeDaysSkipAmount = p_data.FreeDaysSkipAmount;
            _timeLeftInSeconds = p_data.TimeLeftInSeconds;

            var savedTime = p_data.TimeOfWorkersSet;
            var currentTime = DateTime.UtcNow;

            // Calculate the elapsed time in seconds
            var elapsed = currentTime - savedTime;
            var elapsedSeconds = elapsed.TotalSeconds;

            for (int i = 0; i < _maxFreeDaysSkipAmount; i++)
            {
                elapsedSeconds -= _oneDayTimerDurationInSeconds;

                if (!(elapsedSeconds > 0)) 
                    continue;
                
                _freeDaysSkipAmount++;
                _canUseSkipByTime = true;
                OnDaySkipPossibility?.Invoke();
            }
        }
        private void Update() // need server?
        {
            double elapsedSeconds = (DateTime.UtcNow - _startTime).TotalSeconds;
            _timeLeftInSeconds = _oneDayTimerDurationInSeconds - (float)elapsedSeconds;

            int minutes = Mathf.FloorToInt(_timeLeftInSeconds / 60);
            int seconds = Mathf.FloorToInt(_timeLeftInSeconds % 60);

            if (_timeLeftInSeconds > 0)
            {
                _timePassed = $"{minutes}:{seconds:00}";
            }

            CheckPlayerState();
        }

        public void SetPlayerState(DuringDayState p_newState)
        {
            _currentPlayerState = p_newState;

            if (_currentPlayerState == DuringDayState.DayPassing)
            {
                _savingManager.SaveMainGame(new MainGameManagerSavedData()
                {
                    TimeOfWorkersSet = _startTime,
                    CurrentPlayerState = (int)_currentPlayerState, 
                    FreeDaysSkipAmount = _freeDaysSkipAmount,
                    TimeLeftInSeconds = _timeLeftInSeconds
                });
                
                _startTime = DateTime.UtcNow;
                _canUseSkipByTime = false;
            }
            
            OnPlayerStateChange?.Invoke(_currentPlayerState);
        }

        private void CheckPlayerState()
        {
            switch (_currentPlayerState)
            {
                // evening - day ended now collecting/building up
                case DuringDayState.FinishingBuilding:
                    if (!_buildingsManager.IsAnyBuildingNonBuilt())
                    {
                        SetPlayerState(DuringDayState.CollectingResources);
                    }
                    break;
                case DuringDayState.CollectingResources: 
                    if (!_buildingsManager.IsAnyBuildingNonGathered())
                    {
                        SetPlayerState(DuringDayState.WorkDayFinished);
                    }
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
                        _canUseSkipByTime = true;
                        OnDaySkipPossibility?.Invoke();
                    }
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        #region Day Skipping
        public void SkipDay(WayToSkip p_skipSource)
        {
            switch (p_skipSource)
            {
                case WayToSkip.FreeSkip:
                    _freeDaysSkipAmount--;
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
            if (_freeDaysSkipAmount > 0)
            {
                p_reason = WayToSkip.FreeSkip;
                return true;
            }

            if (_buildingsManager.ShardsOfDestinyAmount >= DestinyShardsSkipPrice)
            {
                p_reason = WayToSkip.PaidSkip;
                return true;
            }
            else
            {
                p_reason = WayToSkip.CantSkip;
                return false;
            }
        }
        
        #endregion

        #region Saving
        public void ResetSave()
        {
            _savingManager.ResetSave();
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