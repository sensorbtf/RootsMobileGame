using System;
using System.Collections.Generic;
using AudioSystem;
using Buildings;
using Gods;
using UnityEngine;
using UnityEngine.Localization;
using Random = UnityEngine.Random;

namespace World
{
    public class WorldManager : MonoBehaviour
    {
        [Header("System Refs")]
        [SerializeField] private LightManager _lightManager;
        [SerializeField] private BuildingsManager _buildingsManager;
        [SerializeField] private GodsManager _godsManager;
        [SerializeField] private AudioManager _audioManager;
        [SerializeField] private WorkersManager _workersManager;
        
        [SerializeField] private MissionsSO _missions;
        [SerializeField] private CurrentQuests[] _questData;
        [SerializeField] public Quest[] SafeFailQuests;
        [SerializeField] private int _startingWorldResources;

        public Quest[] CurrentQuests => CurrentRank < _questData.Length ? 
            _questData[CurrentRank].CurrentLevelQuests : SafeFailQuests;
        
        public int NeededMissionToRankUp => CurrentRank < _questData.Length ? 
            _questData[CurrentRank].NeededMissionToRankUp : 1;
        public int RequiredResourcePoints => _missions.AllMissions[CurrentMission].NeededResourcePoints;
        public int CurrentDay { get; private set; }

        public int CurrentRank { get; private set; }

        public int CurrentMission { get; private set; }

        public int FinalHiddenStormDay { get; private set; } = -1;

        public int StormPower { get; private set; }

        public Vector2Int StormDaysRange => _missions.AllMissions[CurrentMission].DaysOfStormRange;

        public event Action OnNewDayStarted;
        public event Action OnResourcesRequirementsMeet;
        public event Action OnLeaveDecision;
        public event Action OnDefendingVillage;
        public event Action OnQuestsProgress;
        public event Action OnNewMissionStart;
        public event Action<int> OnStormCheck;
        public event Action<List<BuildingData>, bool> OnStormCame;

        public void CustomStart(bool p_willBeLoaded)
        {
            _buildingsManager.StartOnWorld(p_willBeLoaded);
            _godsManager.CustomStart(p_willBeLoaded);
            _audioManager.CustomStart();
            
            if (!p_willBeLoaded) 
                StartMission(true);
            else
            {
                _workersManager.BaseWorkersAmounts = _buildingsManager.GetFarmProductionAmount != 0 ? _buildingsManager.GetFarmProductionAmount : 1;
            }
            _buildingsManager.OnPointsGathered += HandleOverallResourcesQuests;
            _buildingsManager.OnBuildingStateChanged += CheckBuildingsQuests;
            _buildingsManager.OnBuildingTechnologyLvlUp += CheckTechnologyBuildingsQuests;
        }

        public void StartNewDay()
        {
            CurrentDay++;
            _workersManager.BaseWorkersAmounts = _buildingsManager.GetFarmProductionAmount != 0 ? _buildingsManager.GetFarmProductionAmount : 1;

            if (CurrentDay == FinalHiddenStormDay)
            {
                EndMission(false, false);
            }
            else
            {
                HandleNewDayStarted(CurrentDay != 1);

                if (!_buildingsManager.IsAnyBuildingNonGathered())
                    CheckResourcePoints();
            }
        }

        private void CheckResourcePoints()
        {
            if (!AreResourcesEnough())
                return;

            OnResourcesRequirementsMeet?.Invoke();
        }

        public bool AreResourcesEnough()
        {
            return _buildingsManager.CurrentResourcePoints >= RequiredResourcePoints;
        }

        public void HandleNewDayStarted(bool p_invoke)
        {
            _buildingsManager.RefreshBuildingsOnNewDay();
            _godsManager.ResetBlessingOnNewDayStart();
            
            if (p_invoke)
            {
                OnNewDayStarted?.Invoke();
            }
        }

        public void EndMission(bool p_byLeft, bool p_lowerDamages)
        {
            if (p_byLeft)
            {
                HandleEndMissionConsequences(p_lowerDamages, true);
            }
            else
            {
                _lightManager.MakeStormEffect();
                
                if (StormPower > _buildingsManager.CurrentDefensePoints) // loss
                    HandleEndMissionConsequences(false, false);
                else
                    OnDefendingVillage?.Invoke();
                //HandleEndMissionConsequences(p_lowerDamages, true);
            }
        }

        private void EndMissionHandler()
        {
            _buildingsManager.EndMissionHandler();
            _workersManager.ResetAssignedWorkers();
            HandleResourceBasementTransition(true);
        }

        public void LeaveMission()
        {
            OnLeaveDecision?.Invoke();
        }

        public void HandleEndMissionConsequences(bool p_lowerDamages, bool p_haveWon)
        {
            var damagedBuildings = new List<BuildingData>();

            foreach (var building in _buildingsManager.CurrentBuildings)
            {
                if (building.IsProtected)
                    continue;

                var random = Random.Range(0, p_lowerDamages ? 3 : 5);

                if (random == 1) // need better evaluation
                {
                    building.IsDamaged = true;
                    damagedBuildings.Add(building.BuildingMainData);
                }
            }

            EndMissionHandler();
            StartMission(p_haveWon);
            OnStormCame?.Invoke(damagedBuildings, p_haveWon);
        }

        public void StartMission(bool p_progressInMissions)
        {
            if (CurrentMission == 0)
                _buildingsManager.HandlePointsManipulation(PointsType.Resource, _startingWorldResources, true);

            if (p_progressInMissions)
            {
                if (CurrentMission + 1 < _missions.AllMissions.Length)
                {
                    CurrentMission++;
                }

                OnQuestsProgress?.Invoke();
            }

            CurrentDay = 0;
            StartNewDay();

            StormPower = Random.Range(_missions.AllMissions[CurrentMission].StormPowerRange.x,
                _missions.AllMissions[CurrentMission].StormPowerRange.y);
            FinalHiddenStormDay = Random.Range(_missions.AllMissions[CurrentMission].DaysOfStormRange.x,
                _missions.AllMissions[CurrentMission].DaysOfStormRange.y);

            Debug.Log("Storm power" + StormPower + "_finalHiddenStormDay" + FinalHiddenStormDay
                      + " in " + CurrentMission + " mission");

            if (CurrentMission != 1)
                HandleResourceBasementTransition(false); //get resources from basement
            _workersManager.BaseWorkersAmounts = _buildingsManager.GetFarmProductionAmount;

            OnNewMissionStart?.Invoke();
        }

        private void HandleResourceBasementTransition(bool p_putInto)
        {
            if (p_putInto)
            {
                _buildingsManager.HandlePointsManipulation(PointsType.Resource, RequiredResourcePoints, false);
                _buildingsManager.ResourcesInBasement = _buildingsManager.CurrentResourcePoints;
            }
            else
            {
                _buildingsManager.SetZeroResourcePoints();
                _buildingsManager.HandlePointsManipulation(PointsType.Resource, _buildingsManager.ResourcesInBasement,
                    true);
                _buildingsManager.ResourcesInBasement = 0;
            }
        }

        #region Quests

        [SerializeField] private LocalizedString _helpWorkersIn;
        [SerializeField] private LocalizedString _repair;
        [SerializeField] private LocalizedString _getXYZToXYZlvl;
        [SerializeField] private LocalizedString _getTechLvlInXYZToXYZLvl;
        [SerializeField] private LocalizedString _getResourcesMinigame;
        [SerializeField] private LocalizedString _getDefenceMinigame;
        [SerializeField] private LocalizedString _getResource;
        [SerializeField] private LocalizedString _getDefence;
        [SerializeField] private LocalizedString _completed;
        [SerializeField] private LocalizedString _currentLevel;
        [SerializeField] private LocalizedString _repairBuilding;
        [SerializeField] private LocalizedString _doMinigameInXYZ;

        public string GetSpecificQuestText(int p_index)
        {
            string textToReturn = null;

            switch (CurrentQuests[p_index].SpecificQuest.QuestKind)
            {
                case QuestType.DoMinigame:
                    textToReturn = string.Format(_helpWorkersIn.GetLocalizedString(), 
                        _buildingsManager.GetLocalizedName(CurrentQuests[p_index].SpecificQuest.TargetName), 
                        CurrentQuests[p_index].SpecificQuest.TargetAmount);
                    break;
                case QuestType.RepairBuilding:
                    textToReturn = string.Format(_repair.GetLocalizedString(), _buildingsManager.GetLocalizedName(CurrentQuests[p_index].SpecificQuest.TargetName));
                    break;
                case QuestType.AchieveBuildingLvl:
                    textToReturn = string.Format(_getXYZToXYZlvl.GetLocalizedString(), 
                        _buildingsManager.GetLocalizedName(CurrentQuests[p_index].SpecificQuest.TargetName), 
                        CurrentQuests[p_index].SpecificQuest.TargetAmount);
                    break;
                case QuestType.AchieveTechnologyLvl:
                    textToReturn = string.Format(_getTechLvlInXYZToXYZLvl.GetLocalizedString(),
                        CurrentQuests[p_index].SpecificQuest.TargetAmount,
                        _buildingsManager.GetLocalizedName(CurrentQuests[p_index].SpecificQuest.TargetName));
                    break;
                case QuestType.MinigameResourcePoints:
                    textToReturn = string.Format(_getResourcesMinigame.GetLocalizedString(),CurrentQuests[p_index].SpecificQuest.TargetAmount);
                    break;
                case QuestType.MinigameDefensePoints:
                    textToReturn = string.Format(_getDefenceMinigame.GetLocalizedString(),CurrentQuests[p_index].SpecificQuest.TargetAmount);
                    break;
                case QuestType.ResourcePoints:
                    textToReturn = string.Format(_getResource.GetLocalizedString(),CurrentQuests[p_index].SpecificQuest.TargetAmount);
                    break;
                case QuestType.DefensePoints:
                    textToReturn = string.Format(_getDefence.GetLocalizedString(),CurrentQuests[p_index].SpecificQuest.TargetAmount);
                    break;
            }

            return textToReturn;
        }

        public string GetSpecificQuestObjectiveText(int p_index)
        {
            string textToReturn = null;
            var level = 0;
            Building building;

            if (CurrentQuests[p_index].IsCompleted)
                return _completed.GetLocalizedString();

            switch (CurrentQuests[p_index].SpecificQuest.QuestKind)
            {
                case QuestType.AchieveBuildingLvl:
                    building = _buildingsManager.GetSpecificBuilding(CurrentQuests[p_index].SpecificQuest.TargetName);

                    if (building != null)
                        level = building.CurrentLevel;

                    textToReturn = string.Format(_currentLevel.GetLocalizedString(), level);
                    break;
                case QuestType.AchieveTechnologyLvl:
                    building = _buildingsManager.GetSpecificBuilding(CurrentQuests[p_index].SpecificQuest.TargetName);

                    if (building != null)
                        level = building.CurrentTechnologyLvl;
                    textToReturn = string.Format(_currentLevel.GetLocalizedString(), level);
                    break;
                case QuestType.MinigameResourcePoints:
                case QuestType.MinigameDefensePoints:
                case QuestType.DefensePoints:
                case QuestType.ResourcePoints:
                    textToReturn = $"{CurrentQuests[p_index].AchievedTargetAmount}/{CurrentQuests[p_index].SpecificQuest.TargetAmount}";
                    break;
                case QuestType.RepairBuilding:
                    textToReturn = _repairBuilding.GetLocalizedString();
                    break;
                case QuestType.DoMinigame:
                    textToReturn = string.Format(_doMinigameInXYZ.GetLocalizedString(), CurrentQuests[p_index].AchievedTargetAmount, CurrentQuests[p_index].SpecificQuest.TargetAmount);
                    break;
            }

            return textToReturn;
        }

        private void CheckBuildingsQuests(Building p_building)
        {
            foreach (var quest in CurrentQuests)
            {
                if (p_building.BuildingMainData.Type != quest.SpecificQuest.TargetName)
                    continue;

                if (quest.SpecificQuest.QuestKind == QuestType.AchieveBuildingLvl)
                {
                    if (quest.SpecificQuest.TargetAmount >= p_building.CurrentLevel)
                        quest.IsCompleted = true;
                }
                else if (quest.SpecificQuest.QuestKind == QuestType.RepairBuilding)
                {
                    quest.IsCompleted = true;
                }
            }

            OnQuestsProgress?.Invoke();
        }

        private void CheckTechnologyBuildingsQuests(Building p_building)
        {
            foreach (var quest in CurrentQuests)
            {
                if (quest.SpecificQuest.QuestKind != QuestType.AchieveTechnologyLvl ||
                    quest.SpecificQuest.TargetName != p_building.BuildingMainData.Type)
                    continue;

                if (p_building.CurrentTechnologyLvl >= quest.SpecificQuest.TargetAmount)
                    quest.IsCompleted = true;
            }

            OnQuestsProgress?.Invoke();
        }

        public void HandleMinigamesQuests(PointsType p_pointsType, int p_pointsNumber, BuildingType p_building)
        {
            foreach (var quest in CurrentQuests)
            {
                if (quest.SpecificQuest.QuestKind is QuestType.MinigameResourcePoints or QuestType.ResourcePoints)
                    if (p_pointsType is PointsType.Resource or PointsType.ResourcesAndDefense or PointsType.DefenseAndResources)
                        quest.AchievedTargetAmount += p_pointsNumber;

                if (quest.SpecificQuest.QuestKind is QuestType.MinigameDefensePoints or QuestType.DefensePoints)
                    if (p_pointsType is PointsType.Defense or PointsType.ResourcesAndDefense or PointsType.DefenseAndResources)
                        quest.AchievedTargetAmount += p_pointsNumber;

                if (quest.SpecificQuest.QuestKind == QuestType.DoMinigame && quest.SpecificQuest.TargetName == p_building)
                    quest.AchievedTargetAmount++;
            }

            OnQuestsProgress?.Invoke();
        }

        private void HandleOverallResourcesQuests(PointsType p_pointsType, int p_pointsNumber)
        {
            foreach (var quest in CurrentQuests)
            {
                if (quest.SpecificQuest.QuestKind == QuestType.ResourcePoints)
                    if (p_pointsType is PointsType.Resource or PointsType.ResourcesAndDefense or PointsType.DefenseAndResources)
                        quest.AchievedTargetAmount += p_pointsNumber;

                if (quest.SpecificQuest.QuestKind == QuestType.DefensePoints)
                    if (p_pointsType is PointsType.Defense or PointsType.ResourcesAndDefense or PointsType.DefenseAndResources)
                        quest.AchievedTargetAmount += p_pointsNumber;
            }

            OnQuestsProgress?.Invoke();
            CheckResourcePoints();
        }

        public void HandleRankUp()
        {
            CurrentRank++;
            _buildingsManager.HandleUpgradeOfBuilding(BuildingType.Cottage, true);
        }

        public void CheckNewQuests()
        {
            foreach (var building in _buildingsManager.CurrentBuildings)
            {
                CheckBuildingsQuests(building);
                CheckTechnologyBuildingsQuests(building);
            }
        }

        #endregion

        public void RevealStorm(int p_daysToSee)
        {
            OnStormCheck?.Invoke(p_daysToSee);
        }
        
        public bool WillStormBeInTwoDays()
        {
            var day = CurrentDay;
            if (day + 2 < FinalHiddenStormDay)
            {
                return false;
            }
            
            return true;
        }

        #region Saving
        public WorldManagerSavedData GetSavedData()
        {
            return new WorldManagerSavedData
            {
                CurrentDay = CurrentDay,
                CurrentMission = CurrentMission,
                CurrentRank = CurrentRank,
                FinalHiddenStormDay = FinalHiddenStormDay,
                StormPower = StormPower,
                QuestOne = CurrentQuests[0].GetSavedData(),
                QuestTwo = CurrentQuests[1].GetSavedData(),
            };
        }

        public void LoadSavedData(WorldManagerSavedData p_data)
        {
            CurrentDay = p_data.CurrentDay;
            CurrentMission = p_data.CurrentMission;
            CurrentRank = p_data.CurrentRank;
            FinalHiddenStormDay = p_data.FinalHiddenStormDay;
            StormPower = p_data.StormPower;

            CurrentQuests[0].LoadSavedData(p_data.QuestOne);
            CurrentQuests[1].LoadSavedData(p_data.QuestTwo);
        }
        #endregion
    }

    [Serializable]
    public struct WorldManagerSavedData
    {
        public int CurrentDay;
        public int CurrentMission;
        public int CurrentRank;
        public int FinalHiddenStormDay;
        public int StormPower;
        public SavedQuestData QuestOne;
        public SavedQuestData QuestTwo;
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
        NormalTimeSkip = 4
    }
}