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

        public int RequiredResourcePoints => _missionData[_currentMission].NeededResourcePoints;
        public int CurrentDay => _currentDay;
        public int FinalHiddenStormDay => _finalHiddenStormDay;
        public int StormPower => _stormPower;
        public Vector2Int StormDaysRange => _missionData[_currentMission].DaysOfStormRange;
        public int FreeSkipsLeft => _freeDaysSkipAmount;
        public int DestinyShardsSkipPrice = 10;

        public event Action OnNewDayStarted;
        public event Action OnResourcesRequirementsMeet;
        public event Action OnLeaveDecision;
        public event Action OnDefendingVillage;
        public event Action<List<BuildingType>, bool> OnStormCame;

        private void Start()
        {
            _buildingManager.StartOnWorld();
            StartMission(true);

            _buildingManager.OnResourcePointsGather += CheckResourcePoints;
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
                EndMission(false, false);
            }
            else
            {
                if (!_buildingManager.IsAnyBuildingNonGathered())
                {
                    CheckResourcePoints();
                }
                
                HandleNewDayStarted();
            }

            // if not: new day has started tooltip: info about last one + panel for worker displacement
            // if yes: checlk if win

            //do other thins (days)
        }

        private void CheckResourcePoints()
        {
            if (_buildingManager.CurrentResourcePoints < RequiredResourcePoints) 
                return;
            
            OnResourcesRequirementsMeet?.Invoke();
        }

        public void HandleNewDayStarted()
        {
            _buildingManager.RefreshBuildingsOnNewDay();

            OnNewDayStarted?.Invoke();
        }

        public void EndMission(bool p_byLeft, bool p_lowerDamages)
        {
            if (p_byLeft)
            {
                HandleEndMissionConsequences(p_lowerDamages, true);
            }
            else
            {
                if (StormPower > _buildingManager.CurrentDefensePoints) // loss
                {
                    HandleEndMissionConsequences(false, false);
                }
                else
                {
                    OnDefendingVillage?.Invoke();
                    return;
                    //HandleEndMissionConsequences(p_lowerDamages, true);
                }
            }
        }

        private void EndMissionHandler()
        {
            _buildingManager.EndMissionHandler();
            HandleResourceBasementTransition(true);
        }

        public void LeaveMission()
        {
            OnLeaveDecision?.Invoke();
        }

        public void HandleEndMissionConsequences(bool p_lowerDamages, bool p_haveWon)
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

            EndMissionHandler();
            OnStormCame?.Invoke(damagedBuildings, p_haveWon);
        }

        public void StartMission(bool p_progressInMissions) // if !progress == lower points in ranking
        {
            if (_currentMission == 0)
            {
                _buildingManager.CurrentResourcePoints += _startingWorldResources;
            }

            if (p_progressInMissions)
            {
                _currentMission++;
            }

            _currentDay = 0;

            _stormPower = Random.Range(_missionData[_currentMission].StormPowerRange.x,
                _missionData[_currentMission].StormPowerRange.y);
            _finalHiddenStormDay = Random.Range(_missionData[_currentMission].DaysOfStormRange.x,
                _missionData[_currentMission].DaysOfStormRange.y);

            Debug.Log("Storm power" + _stormPower+ "_finalHiddenStormDay" + _finalHiddenStormDay 
                      + " in " + _currentMission + " mission");

            _workersManager.BaseWorkersAmounts = _buildingManager.GetFarmProductionAmount;
            HandleResourceBasementTransition(false); //get resources from basement
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

        private void HandleResourceBasementTransition(bool p_putInto)
        {
            if (p_putInto)
            {
                _buildingManager.CurrentResourcePoints -= RequiredResourcePoints;
                _buildingManager.ResourcesInBasement = _buildingManager.CurrentResourcePoints;
            }
            else
            {
                _buildingManager.CurrentResourcePoints += _buildingManager.ResourcesInBasement;
                _buildingManager.ResourcesInBasement = 0;
            }
        }

        public bool CanLeaveMission()
        {
            if (_currentDay > _stormDaysRange.x && _currentDay < _stormDaysRange.y &&
                _buildingManager.CurrentResourcePoints > RequiredResourcePoints)
            {
                return true;
            }

            return false;
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