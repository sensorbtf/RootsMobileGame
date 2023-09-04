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
            // check if storm is not happening
            // if not: new day has started tooltip: info about last one + panel for worker displacement
            // if yes: checlk if win
            _buildingManager.CurrentResourcePoints += _buildingManager.GatherProductionPointsFromBuildings();
            _buildingManager.CurrentDefensePoints += _buildingManager.GatherDefensePointsFromBuildings();
            _buildingManager.RefreshQueue();
            //do other thins (days)
            
            OnNewDayStarted?.Invoke();
        }
        
        private void EndMission()
        {
            _currentMission++;
            //get resources from basement
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
    }

    [Serializable]
    public class Mission
    {
        public int NeededResourcePoints;
        public Vector2Int DaysOfStormRange;
        public Vector2Int StormPowerRange;
    }
}