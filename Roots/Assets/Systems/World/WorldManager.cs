using System;
using Buildings;
using UnityEngine;
using Random = UnityEngine.Random;

namespace World
{
    public class WorldManager : MonoBehaviour
    {
        [SerializeField] private BuildingManager _buildingManager;
        [SerializeField] private WorkersManager _workersManager;
        [SerializeField] private Mission[] _missionData;
        [SerializeField] private int _startingWorldResources = 0;

        private int _currentDay = 0;
        private int _currentMission = 0;
        private int _finalHiddenStormDay = -1;
        private int _stormPower;
        private Vector2Int _stormDaysRange;

        private bool _hasLeftMission = false;

        public int NeededResourcePoints => _missionData[_currentMission].NeededResourcePoints;
        public int CurrentDay => _currentDay;
        public int FinalHiddenStormDay => _finalHiddenStormDay;
        public int StormPower => _stormPower;
        public Vector2Int StormDaysRange => _missionData[_currentMission].DaysOfStormRange;

        public event Action OnNewDayStarted;

        private void Start()
        {
            _buildingManager.StartOnWorld();
            StartMission();
            _workersManager.WorkersAmount = _buildingManager.GetFarmProductionAmount;
        }

        public void SkipDay()
        {
            StartNewDay();
        }

        private void StartNewDay()
        {
            _currentDay++;

            if (_currentDay == _finalHiddenStormDay)
            {
                EndMission();
            }
            else
            {
                _buildingManager.RefreshBuildingsOnNewDay();
                
                if (_buildingManager.IsAnyBuildingNonGathered())
                {
                    
                }
                
                
                OnNewDayStarted?.Invoke();
            }

            // if not: new day has started tooltip: info about last one + panel for worker displacement
            // if yes: checlk if win
           
            //do other thins (days)

        }

        private void EndMission()
        {
            if (StormPower > _buildingManager.CurrentDefensePoints) // loss
            {
                //resultats
            }
            else // win
            {
                // prepare for fight panel -> placing workers -> resultats
            }
            
            _currentMission++;
        }

        private void StartMission()
        {
            if (_currentMission == 0)
            {
                _buildingManager.CurrentResourcePoints += _startingWorldResources;
            }

            _stormPower = Random.Range(_missionData[_currentMission].StormPowerRange.x,
                _missionData[_currentMission].StormPowerRange.y);
            _finalHiddenStormDay = Random.Range(_missionData[_currentMission].DaysOfStormRange.x,
                _missionData[_currentMission].DaysOfStormRange.y);
            //get resources from basement

            StartNewDay();
        }

        public bool CanSkipDay()
        {
            // payment of shards/free skips
            return true;
        }

        public bool CanLeaveMission()
        {
            if (_currentDay > _stormDaysRange.x && _currentDay < _stormDaysRange.y &&
                _buildingManager.CurrentResourcePoints > NeededResourcePoints)
            {
                return true;
            }

            return false;
        }

        public bool CanSetWorkers()
        {
            return !_buildingManager.IsAnyBuildingNonGathered();
        }

        public bool CanStartDay()
        {
            // need manager of minigames. If minigame ended => true
            return true;
        }
    }

    [Serializable]
    public class Mission
    {
        public int NeededResourcePoints;
        public Vector2Int DaysOfStormRange;
        public Vector2Int StormPowerRange;
    }
}