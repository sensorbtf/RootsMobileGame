using System;
using System.Collections;
using System.Linq;
using System.Threading.Tasks;
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
        [SerializeField] private GameObject _loadingPanel;
        [SerializeField] private GodsManager _godsManager;
        [SerializeField] private WorldManager _worldManager;
        [SerializeField] private BuildingsManager _buildingsManager;
        [SerializeField] private SavingManager _savingManager;
        [SerializeField] private int _destinyShardsSkipPrice;
        [SerializeField] private int _oneDayTimerDurationInSeconds = 60;
        [SerializeField] private int _maxFreeDaysSkipAmount = 3;
        [SerializeField] private DayOfWeekReward[] _everyDayReward;

        private DateTime _sessionStartTime;
        private TimeSpan _totalPlayTime;
        private DateTime _startDayTime;
        private float _timeLeftInSeconds;
        private bool _shouldUpdate;
        
        private DateTime _giftTakenTime;
        private bool _shouldMakeGiftViable;
        private int _loginDay;

        public int FreeSkipsLeft { get; private set; }
        public int HoursOfAbstence { get; private set; }
        public int FreeSkipsGotten { get; private set; }
        public int MaxFreeSkipsAmount => _maxFreeDaysSkipAmount;

        public DuringDayState CurrentPlayerState { get; private set; }

        public int DestinyShardsSkipPrice => _destinyShardsSkipPrice;
        public string TimePassed { get; private set; }

        public bool CanUseSkipByTime { get; private set; }
        public int GetDailyReward => _everyDayReward[_loginDay].DestinyShardsAmount;

        public event Action<bool> OnPlayerCameBack;
        public event Action OnAfterLoad;
        public event Action<DuringDayState> OnPlayerStateChange;
        public event Action OnDaySkipPossibility;

        private void Awake()
        {
            _loadingPanel.SetActive(true);
            _savingManager.OnAuthenticationEnded += () => StartCoroutine(CustomStart());
        }

        private IEnumerator CustomStart()
        {
            _shouldUpdate = false;

            Application.targetFrameRate = 30;
            _startDayTime = DateTime.UtcNow;
            _sessionStartTime = DateTime.UtcNow;

            bool isSaveAvailable = false;
            yield return StartCoroutine(_savingManager.CheckSave(result => isSaveAvailable = result));
            _worldManager.CustomStart(isSaveAvailable);

            if (isSaveAvailable)
            {
                _savingManager.OnLoad += LoadSavedData;
                yield return StartCoroutine(ChooseAndLoadSave()); 
                Debug.Log("Save loaded");
            }
            else
            {
                _totalPlayTime = TimeSpan.Zero;
                _loginDay = 0;
                SetPlayerState(DuringDayState.FinishingBuilding);

                _shouldUpdate = true;

                Debug.Log("NOT LOADED");
            }
            
            _loadingPanel.SetActive(false);
            _savingManager.OnAuthenticationEnded -= () => StartCoroutine(CustomStart());
        }
        
        private IEnumerator ChooseAndLoadSave()
        {
            yield return StartCoroutine(_savingManager.ChooseProperSave());
        }

        private void Update() // need server?
        {
            if (!_shouldUpdate)
                return;

            var elapsedSeconds = (DateTime.UtcNow - _startDayTime).TotalSeconds;
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
            _loginDay = p_data.LoginDay;
            FreeSkipsLeft = p_data.FreeDaysSkipAmount;
            _totalPlayTime = TimeSpan.Parse(p_data.TotalTimePlayed);
            var savedTime = p_data.TimeOfWorkersSet;
            var currentTime = DateTime.UtcNow;

            TimeSpan passedTime = currentTime - savedTime;
            var timePassedInSeconds = passedTime.TotalSeconds;

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

            if (_maxFreeDaysSkipAmount != FreeSkipsLeft)
            {
                var secondsToSubtract = passedTime.TotalSeconds % _oneDayTimerDurationInSeconds;
                _startDayTime = DateTime.UtcNow;
                var addSeconds = _startDayTime.AddSeconds(-secondsToSubtract);
                _startDayTime = addSeconds;
            }
            
            _giftTakenTime = p_data.TimeOfGiftTaken;
            var timeDifference = currentTime - _giftTakenTime;

            if (timeDifference.TotalHours >= 24)
            {
                if (timeDifference.TotalHours >= 48) // TODO: info about reset + info about next login reward?
                {
                    _loginDay = 0;
                }
                
                _shouldMakeGiftViable = _everyDayReward[_loginDay].DestinyShardsAmount > 0;
            }

            _savingManager.OnLoad -= LoadSavedData;
            OnPlayerCameBack?.Invoke(_shouldMakeGiftViable);
            OnPlayerStateChange?.Invoke(CurrentPlayerState);
            
            HandleAfterLoad();
        }

        private void HandleAfterLoad()
        {
            _loadingPanel.SetActive(false);
            _shouldUpdate = true;
            
            OnAfterLoad?.Invoke();
        }

        public void SetPlayerState(DuringDayState p_newState)
        {
            CurrentPlayerState = p_newState;

            if (CurrentPlayerState == DuringDayState.DayPassing)
            {
                InitiateSaving();
            }

            OnPlayerStateChange?.Invoke(CurrentPlayerState);
        }

        public void HandleLoginReward()
        {
            _buildingsManager.HandlePointsManipulation(PointsType.ShardsOfDestiny, GetDailyReward, true, true);

            _giftTakenTime = DateTime.UtcNow;
            
            _loginDay++;
            if (_loginDay == 7)
                _loginDay = 0;
        }

        private void CheckPlayerState()
        {
            switch (CurrentPlayerState)
            {
                // evening - day ended now collecting/building up
                case DuringDayState.FinishingBuilding:
                    if (!_buildingsManager.IsAnyBuildingNonBuilt())
                        SetPlayerState(DuringDayState.CollectingResources);
                    break;
                case DuringDayState.CollectingResources:
                    if (!_buildingsManager.IsAnyBuildingNonGathered())
                        SetPlayerState(DuringDayState.WorkDayFinished);
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

        private void InitiateSaving()
        {
            _startDayTime = DateTime.UtcNow;
            UpdateTotalPlayTime();
                
            _savingManager.SaveMainGame(new MainGameManagerSavedData
            {
                LoginDay = _loginDay,
                FreeDaysSkipAmount = FreeSkipsLeft,
                CurrentPlayerState = (int)CurrentPlayerState,
                TotalTimePlayed = _totalPlayTime.ToString(),
                TimeOfWorkersSet = _startDayTime,
                TimeOfGiftTaken = _giftTakenTime,
            });
        }

        private void OnApplicationPause(bool p_pauseStatus)
        {
            if (p_pauseStatus)
            {
                // Game is paused or going into background
                UpdateTotalPlayTime();
            }
            else
            {
                // Game is resumed
                _sessionStartTime = DateTime.UtcNow;
            }
        }

        private void UpdateTotalPlayTime()
        {
            _totalPlayTime += DateTime.UtcNow - _sessionStartTime;
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
    
    [Serializable]
    public struct DayOfWeekReward
    {
        public int DestinyShardsAmount;
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