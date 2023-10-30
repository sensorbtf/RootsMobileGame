using System;
using System.Collections.Generic;
using Buildings;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

namespace World
{
    public class WorldManager : MonoBehaviour
    {
        [FormerlySerializedAs("_buildingManager")] [SerializeField]
        private BuildingsManager buildingsManager;

        [SerializeField] private WorkersManager _workersManager;
        [SerializeField] private Mission[] _missionData;
        [SerializeField] private CurrentQuests[] _questData;
        [SerializeField] private int _startingWorldResources;

        public Quest[] CurrentQuests => _questData[CurrentRank].CurrentLevelQuests;
        public int NeededMissionToRankUp => _questData[CurrentRank].NeededMissionToRankUp;
        public int RequiredResourcePoints => _missionData[CurrentMission].NeededResourcePoints;
        public int CurrentDay { get; private set; }

        public int CurrentRank { get; private set; }

        public int CurrentMission { get; private set; }

        public int FinalHiddenStormDay { get; private set; } = -1;

        public int StormPower { get; private set; }

        public Vector2Int StormDaysRange => _missionData[CurrentMission].DaysOfStormRange;

        public event Action OnNewDayStarted;
        public event Action OnResourcesRequirementsMeet;
        public event Action OnLeaveDecision;
        public event Action OnDefendingVillage;
        public event Action OnQuestsProgress;
        public event Action OnNewMissionStart;

        public event Action<int> OnStormCheck;
        public event Action<List<BuildingType>, bool> OnStormCame;

        public void CustomStart(bool p_willBeLoaded)
        {
            buildingsManager.StartOnWorld(p_willBeLoaded);
            if (!p_willBeLoaded) StartMission(true);

            buildingsManager.OnPointsGathered += HandleOverallResourcesQuests;
            buildingsManager.OnBuildingStateChanged += CheckBuildingsQuests;
            buildingsManager.OnBuildingTechnologyLvlUp += CheckTechnologyBuildingsQuests;
        }

        public void StartNewDay()
        {
            CurrentDay++;
            _workersManager.BaseWorkersAmounts = buildingsManager.GetFarmProductionAmount;

            if (CurrentDay == FinalHiddenStormDay)
            {
                EndMission(false, false);
            }
            else
            {
                HandleNewDayStarted();

                if (!buildingsManager.IsAnyBuildingNonGathered())
                    CheckResourcePoints();
            }

            // if not: new day has started tooltip: info about last one + panel for worker displacement
            // if yes: checlk if win
            // afterone "keep working" there should be turned on button "End Mission"
            // need gods
            //do other thins (days)
        }

        private void CheckResourcePoints()
        {
            if (buildingsManager.CurrentResourcePoints < RequiredResourcePoints)
                return;

            OnResourcesRequirementsMeet?.Invoke();
        }

        public void HandleNewDayStarted(bool p_invoke = true)
        {
            buildingsManager.RefreshBuildingsOnNewDay();

            if (p_invoke)
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
                if (StormPower > buildingsManager.CurrentDefensePoints) // loss
                    HandleEndMissionConsequences(false, false);
                else
                    OnDefendingVillage?.Invoke();
                //HandleEndMissionConsequences(p_lowerDamages, true);
            }
        }

        private void EndMissionHandler()
        {
            buildingsManager.EndMissionHandler();
            HandleResourceBasementTransition(true);
        }

        public void LeaveMission()
        {
            OnLeaveDecision?.Invoke();
        }

        public void HandleEndMissionConsequences(bool p_lowerDamages, bool p_haveWon)
        {
            var damagedBuildings = new List<BuildingType>();

            foreach (var building in buildingsManager.CurrentBuildings)
            {
                if (building.IsProtected)
                    continue;

                var random = Random.Range(0, p_lowerDamages ? 3 : 5);

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
            if (CurrentMission == 0)
                buildingsManager.HandlePointsManipulation(PointsType.Resource, _startingWorldResources, true);

            if (p_progressInMissions)
            {
                CurrentMission++;
                OnQuestsProgress?.Invoke();
            }

            CurrentDay = 1;

            StormPower = Random.Range(_missionData[CurrentMission].StormPowerRange.x,
                _missionData[CurrentMission].StormPowerRange.y);
            FinalHiddenStormDay = Random.Range(_missionData[CurrentMission].DaysOfStormRange.x,
                _missionData[CurrentMission].DaysOfStormRange.y);

            Debug.Log("Storm power" + StormPower + "_finalHiddenStormDay" + FinalHiddenStormDay
                      + " in " + CurrentMission + " mission");

            HandleResourceBasementTransition(false); //get resources from basement
            _workersManager.BaseWorkersAmounts = buildingsManager.GetFarmProductionAmount;

            OnNewMissionStart?.Invoke();
        }

        private void HandleResourceBasementTransition(bool p_putInto)
        {
            if (p_putInto)
            {
                buildingsManager.HandlePointsManipulation(PointsType.Resource, RequiredResourcePoints, false);
                buildingsManager.ResourcesInBasement = buildingsManager.CurrentResourcePoints;
            }
            else
            {
                buildingsManager.HandlePointsManipulation(PointsType.Resource, buildingsManager.ResourcesInBasement,
                    true);
                buildingsManager.ResourcesInBasement = 0;
            }
        }

        public bool CheckIfDayIsFinished()
        {
            // need manager of minigames. If minigame ended => true
            return true;
        }

        public string GetSpecificQuestText(int p_index)
        {
            string textToReturn = null;

            switch (CurrentQuests[p_index].SpecificQuest.QuestKind)
            {
                case QuestType.DoMinigame:
                    textToReturn = $"Help workers in {CurrentQuests[p_index].SpecificQuest.TargetName}";
                    break;
                case QuestType.RepairBuilding:
                    textToReturn = $"Repair {CurrentQuests[p_index].SpecificQuest.TargetName}";
                    break;
                case QuestType.AchieveBuildingLvl:
                    textToReturn =
                        $"Get {CurrentQuests[p_index].SpecificQuest.TargetName} to {CurrentQuests[p_index].SpecificQuest.TargetAmount} lvl";
                    break;
                case QuestType.AchieveTechnologyLvl:
                    textToReturn =
                        $"Develop technology in {CurrentQuests[p_index].SpecificQuest.TargetName} to {CurrentQuests[p_index].SpecificQuest.TargetAmount} lvl";
                    break;
                case QuestType.MinigameResourcePoints:
                    textToReturn =
                        $"Get {CurrentQuests[p_index].SpecificQuest.TargetAmount} resource points from minigame";
                    break;
                case QuestType.MinigameDefensePoints:
                    textToReturn =
                        $"Get {CurrentQuests[p_index].SpecificQuest.TargetAmount} defense points from minigame";
                    break;
                case QuestType.ResourcePoints:
                    textToReturn = $"Get {CurrentQuests[p_index].SpecificQuest.TargetAmount} resource points";
                    break;
                case QuestType.DefensePoints:
                    textToReturn = $"Get {CurrentQuests[p_index].SpecificQuest.TargetAmount} defense points";
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
                return "Completed";

            switch (CurrentQuests[p_index].SpecificQuest.QuestKind)
            {
                case QuestType.AchieveBuildingLvl:
                    building = buildingsManager.GetSpecificBuilding(CurrentQuests[p_index].SpecificQuest.TargetName);

                    if (building != null)
                        level = building.CurrentLevel;

                    textToReturn = $"Current level: {level}";
                    break;
                case QuestType.AchieveTechnologyLvl:
                    building = buildingsManager.GetSpecificBuilding(CurrentQuests[p_index].SpecificQuest.TargetName);

                    if (building != null)
                        level = building.CurrentTechnologyLvl;
                    textToReturn = $"Current level: {level}";
                    break;
                case QuestType.MinigameResourcePoints:
                case QuestType.MinigameDefensePoints:
                case QuestType.DefensePoints:
                case QuestType.ResourcePoints:
                    textToReturn =
                        $"{CurrentQuests[p_index].AchievedTargetAmount}/{CurrentQuests[p_index].SpecificQuest.TargetAmount}";
                    break;
                case QuestType.RepairBuilding:
                    textToReturn = "Repair Building 0/1";
                    break;
                case QuestType.DoMinigame:
                    textToReturn = $"Do Minigame in {CurrentQuests[p_index].SpecificQuest.TargetName}";
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
                    if (p_pointsType is PointsType.Resource or PointsType.ResourcesAndDefense)
                        quest.AchievedTargetAmount += p_pointsNumber;

                if (quest.SpecificQuest.QuestKind is QuestType.MinigameDefensePoints or QuestType.DefensePoints)
                    if (p_pointsType is PointsType.Defense or PointsType.ResourcesAndDefense)
                        quest.AchievedTargetAmount += p_pointsNumber;

                if (quest.SpecificQuest.QuestKind == QuestType.DoMinigame &&
                    quest.SpecificQuest.TargetName == p_building)
                    quest.IsCompleted = true;
            }

            OnQuestsProgress?.Invoke();
        }

        private void HandleOverallResourcesQuests(PointsType p_pointsType, int p_pointsNumber)
            // get points only from gathering or minigame
        {
            foreach (var quest in CurrentQuests)
            {
                if (quest.SpecificQuest.QuestKind == QuestType.ResourcePoints)
                    if (p_pointsType is PointsType.Resource or PointsType.ResourcesAndDefense)
                        quest.AchievedTargetAmount += p_pointsNumber;

                if (quest.SpecificQuest.QuestKind == QuestType.DefensePoints)
                    if (p_pointsType is PointsType.Defense or PointsType.ResourcesAndDefense)
                        quest.AchievedTargetAmount += p_pointsNumber;
            }

            OnQuestsProgress?.Invoke();
        }

        public void HandleRankUp()
        {
            CurrentRank++;
            buildingsManager.HandleUpgradeOfBuilding(BuildingType.Cottage, true);
        }

        public void CheckNewQuests()
        {
            foreach (var building in buildingsManager.CurrentBuildings)
            {
                CheckBuildingsQuests(building);
                CheckTechnologyBuildingsQuests(building);
            }
        }

        public void RevealStorm(int p_daysToSee)
        {
            OnStormCheck?.Invoke(p_daysToSee);
        }

        public WorldManagerSavedData GetSavedData()
        {
            return new WorldManagerSavedData
            {
                CurrentDay = CurrentDay,
                CurrentMission = CurrentMission,
                CurrentRank = CurrentRank,
                FinalHiddenStormDay = FinalHiddenStormDay,
                StormPower = StormPower
            };
        }

        public void LoadSavedData(WorldManagerSavedData p_data)
        {
            CurrentDay = p_data.CurrentDay;
            CurrentMission = p_data.CurrentMission;
            CurrentRank = p_data.CurrentRank;
            FinalHiddenStormDay = p_data.FinalHiddenStormDay;
            StormPower = p_data.StormPower;

            OnNewMissionStart?.Invoke();
            OnQuestsProgress?.Invoke();
        }
    }

    [Serializable]
    public struct WorldManagerSavedData
    {
        public int CurrentDay;
        public int CurrentMission;
        public int CurrentRank;
        public int FinalHiddenStormDay;
        public int StormPower;
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