using System;
using System.Collections.Generic;
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
        [SerializeField] private int _freeDaysSkipAmount = 5;

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
        public int FreeSkipsLeft => _freeDaysSkipAmount;
        public int DestinyShardsSkipPrice = 10;

        public event Action OnNewDayStarted;
        public event Action OnResourcesRequirementsMeet;
        public event Action OnLeaveDecision;
        public event Action<List<BuildingType>> OnStormWon;

        private void Start()
        {
            _buildingManager.StartOnWorld();
            StartMission();
        }

        public void SkipDay(WayToSkip p_skipSource)
        {
            switch (p_skipSource)
            {
                case WayToSkip.FreeSkip:
                    _freeDaysSkipAmount--;
                    break;
                case WayToSkip.PaidSkip:
                    _buildingManager.ShardsOfDestinyAmount -= DestinyShardsSkipPrice;
                    break;
            }

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
                if (_buildingManager.CurrentResourcePoints >= NeededResourcePoints)
                {
                    OnResourcesRequirementsMeet?.Invoke();

                    return;
                }

                HandleNewDayStarted();
            }

            // if not: new day has started tooltip: info about last one + panel for worker displacement
            // if yes: checlk if win

            //do other thins (days)
        }

        public void HandleNewDayStarted()
        {
            _buildingManager.RefreshBuildingsOnNewDay();

            if (_buildingManager.IsAnyBuildingNonGathered())
            {
            }

            OnNewDayStarted?.Invoke();
        }

        public void EndMission()
        {
            if (StormPower > _buildingManager.CurrentDefensePoints) // loss
            {
                HandleStormWon(false);
            }
            else // win
            {
                // prepare for fight panel -> placing workers -> resultats
            }
        }

        public void LeaveMission()
        {
            OnLeaveDecision?.Invoke();
        }

        public void HandleStormWon(bool p_lowerDamages)
        {
            List<BuildingType> damagedBuildings = new List<BuildingType>();

            foreach (var building in _buildingManager.CurrentBuildings)
            {
                if (building.IsProtected)
                    continue;
                    
                int random = Random.Range(0, p_lowerDamages ? 3 : 5);

                if (random == 1) // need better evaluation
                {
                    building.IsDamaged = true;
                    damagedBuildings.Add(building.BuildingMainData.Type);
                }
            }
            
            OnStormWon?.Invoke(damagedBuildings);
        }

        public void StartMission()
        {
            if (_currentMission == 0)
            {
                _buildingManager.CurrentResourcePoints += _startingWorldResources;
            }

            _currentMission++;
            _currentDay = 0;
            
            _stormPower = Random.Range(_missionData[_currentMission].StormPowerRange.x,
                _missionData[_currentMission].StormPowerRange.y);
            _finalHiddenStormDay = Random.Range(_missionData[_currentMission].DaysOfStormRange.x,
                _missionData[_currentMission].DaysOfStormRange.y);
            
            Debug.Log("_finalHiddenStormDay" + _finalHiddenStormDay + " in " + _currentMission + " mission");
            
            //get resources from basement

            _workersManager.BaseWorkersAmounts = _buildingManager.GetFarmProductionAmount;

            StartNewDay();
        }

        public bool CanSkipDay(out WayToSkip p_reason)
        {
            if (_freeDaysSkipAmount > 0)
            {
                p_reason = WayToSkip.FreeSkip;
                return true;
            }

            if (_buildingManager.ShardsOfDestinyAmount >= DestinyShardsSkipPrice)
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

    public enum WayToSkip
    {
        CantSkip = 0,
        FreeSkip = 1,
        PaidSkip = 2,
        AddSkip = 3,
        NormalTimeSkip = 4,
    }
}