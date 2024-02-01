using System;
using System.Collections.Generic;
using System.Linq;
using AudioSystem;
using Gods;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Buildings
{
    public class BuildingsManager : MonoBehaviour
    {
        [Header("System References")] [SerializeField]
        private AudioManager _audioManager;

        [SerializeField] private GodsManager _godsManager;
        [SerializeField] private WorkersManager _workersManager;
        [SerializeField] private BuildingTransforms[] _placesForBuildings;
        [SerializeField] private BuildingDatabase _buildingsDatabase;
        [SerializeField] private int _startingDestinyPoints = 3000;

        [Header("Icons")] public Sprite DefenseAndResourcesPointsIcon;
        public Sprite ResourcesAndDefensePointsIcon;
        public Sprite ResourcesPointsIcon;
        public Sprite DefensePointsIcon;
        public Sprite ShardsOfDestinyIcon;
        public Sprite FinishBuildingIcon;

        [Header("Audio Clips")] [SerializeField]
        private AudioClip _buildingFinishedEffect;

        [SerializeField] private AudioClip CottageSoundEffect;
        [SerializeField] private AudioClip FarmSoundEffect;
        [SerializeField] private AudioClip GuardTowerSoundEffect;
        [SerializeField] private AudioClip WoodcutterSoundEffect;
        [SerializeField] private AudioClip Alchemical_HutSoundEffect;
        [SerializeField] private AudioClip Mining_ShaftSoundEffect;
        [SerializeField] private AudioClip Ritual_CircleSoundEffect;
        [SerializeField] private AudioClip Peat_ExcavationSoundEffect;
        [SerializeField] private AudioClip Charcoal_PileSoundEffect;
        [SerializeField] private AudioClip Herbs_GardenSoundEffect;
        [SerializeField] private AudioClip ApiarySoundEffect;
        [SerializeField] private AudioClip Woodworking_StationSoundEffect;
        [SerializeField] private AudioClip Sacrificial_AltarSoundEffect;

        [HideInInspector] public List<BuildingData> UnlockedBuildings;
        [HideInInspector] public List<Building> CompletlyNewBuildings;
        [HideInInspector] public List<Building> UpgradedBuildings;
        [HideInInspector] public List<Building> RepairedBuildings;
        [HideInInspector] public List<Building> BuildingWithEnabledMinigame;
        [HideInInspector] public List<Building> BuildingsToGatherFrom;
        [HideInInspector] public List<Building> BuildingsWithTechnologyUpgrade;

        #region Properties

        private CurrentMissionBonus _bonus;
        private int _resourcesStoredInBasement;
        private bool _tutorialStarted = false;
        public List<Building> CurrentBuildings { get; private set; }

        public BuildingDatabase AllBuildingsDatabase => _buildingsDatabase;

        public int GetFarmProductionAmount
        {
            get
            {
                var building = CurrentBuildings.Find(x => x.BuildingMainData.Type == BuildingType.Farm);
                if (building == null || building.CurrentLevel == 0)
                    return 1;

                return building.CurrentLevel;
            }
        }

        private int GetBaseCottageBasementDeep
        {
            get
            {
                var building = GetSpecificBuilding(BuildingType.Cottage);
                return GetSpecificBuilding(BuildingType.Cottage).BuildingMainData.PerLevelData[building.CurrentLevel]
                    .ProductionAmountPerDay;
            }
        }

        public int ResourcesInBasement
        {
            set => _resourcesStoredInBasement =
                GetBaseCottageBasementDeep <= value ? value : GetBaseCottageBasementDeep;
            get => _resourcesStoredInBasement;
        }

        public CurrentMissionBonus Bonus => _bonus;

        public int CurrentResourcePoints { get; private set; }

        public int CurrentDefensePoints { get; private set; }

        public int CurrentDestinyShards { get; private set; }

        public event Action<Building> OnBuildingClicked;
        public event Action<Building> OnBuildingStateChanged;
        public event Action<Building> OnBuildingRepaired;
        public event Action OnCottageLevelUp;
        public event Action<Building> OnBuildingTechnologyLvlUp;
        public event Action<Building> OnBuildingDestroyed;
        public event Action<PointsType, int> OnPointsGathered;

        public event Action<int, bool> OnResourcePointsChange;
        public event Action<int, bool> OnDefensePointsChange;
        public event Action<int, bool> OnDestinyShardsPointsChange;

        public event Action OnTutorialStart;

        #endregion

        public void StartOnWorld(bool p_willBeLoaded)
        {
            UnlockedBuildings = new List<BuildingData>();
            CompletlyNewBuildings = new List<Building>();
            UpgradedBuildings = new List<Building>();
            BuildingWithEnabledMinigame = new List<Building>();
            BuildingsToGatherFrom = new List<Building>();
            BuildingsWithTechnologyUpgrade = new List<Building>();
            RepairedBuildings = new List<Building>();
            CurrentBuildings = new List<Building>();

            if (p_willBeLoaded)
            {
                _tutorialStarted = true;
                return;
            }

            foreach (var buildingToBuild in _buildingsDatabase.allBuildings)
                if (buildingToBuild.Type is BuildingType.Cottage)
                    HandleBuiltOfBuilding(buildingToBuild, true);

            HandlePointsManipulation(PointsType.StarDust, _startingDestinyPoints, true);
            CurrentBuildings.Find(x => x.BuildingMainData.Type == BuildingType.Cottage).IsDamaged = true;
        }

        private void Update()
        {
            if (CurrentBuildings == null)
                return;

            foreach (var building in CurrentBuildings)
            {
                building.TryToHighlight();
            }

            if (_tutorialStarted)
                return;

            if (ShouldStartTutorial())
            {
                OnTutorialStart?.Invoke();
                _tutorialStarted = true;
            }
        }

        public Building GetSpecificBuilding(BuildingType p_data)
        {
            foreach (var building in CurrentBuildings)
                if (building.BuildingMainData.Type == p_data)
                    return building;

            return null;
        }

        public void PutBuildingOnQueue(BuildingData p_buildingData)
        {
            var building = GetSpecificBuilding(p_buildingData.Type);

            if (building == null)
                HandleBuiltOfBuilding(p_buildingData, false);
            else
                HandleUpgradeOfBuilding(p_buildingData.Type, false);

            AssignWorker(GetSpecificBuilding(p_buildingData.Type), true);
        }

        public void HandleBuildingsModifications(Building p_building)
        {
            AssignWorker(p_building, !p_building.HaveWorker);
        }

        public void RefreshBuildingsOnNewDay()
        {
            foreach (var building in CurrentBuildings)
            {
                if (building.IsBeeingUpgradedOrBuilded || !building.HaveWorker || building.IsDamaged)
                    continue;

                building.CurrentTechnologyDayOnQueue++;

                if (building.CurrentTechnologyDayOnQueue == building.BuildingMainData.Technology
                        .DataPerTechnologyLevel[building.CurrentTechnologyLvl].WorksDayToAchieve)
                    BuildingsWithTechnologyUpgrade.Add(building);

                switch (building.BuildingMainData.PerLevelData[building.CurrentLevel].ProductionType)
                {
                    case PointsType.Nothing:
                        break;
                    case PointsType.Resource:
                        building.SetCollectionIcon(ResourcesPointsIcon);
                        BuildingsToGatherFrom.Add(building);
                        break;
                    case PointsType.Defense:
                        building.SetCollectionIcon(DefensePointsIcon);
                        BuildingsToGatherFrom.Add(building);
                        break;
                    case PointsType.ResourcesAndDefense:
                        building.SetCollectionIcon(ResourcesAndDefensePointsIcon);
                        BuildingsToGatherFrom.Add(building);
                        break;
                    case PointsType.DefenseAndResources:
                        building.SetCollectionIcon(DefenseAndResourcesPointsIcon);
                        BuildingsToGatherFrom.Add(building);
                        break;
                    case PointsType.StarDust:
                        break;
                }

                // add some sort of bonus here to avoid saving up data in Buildingscript
            }

            foreach (var building in CurrentBuildings)
            {
                building.PlayedMinigame = false;

                if (!building.HaveWorker)
                    continue;

                if (building.HandleNewDay())
                {
                    if (building.CurrentLevel == 0)
                        CompletlyNewBuildings.Add(building);
                    else if (building.CurrentLevel > 1)
                        UpgradedBuildings.Add(building);
                    else if (building.IsDamaged)
                        RepairedBuildings.Add(building);
                }
            }
        }

        public bool CanBuildBuilding(BuildingData p_building)
        {
            if (!_workersManager.IsAnyWorkerFree())
                return false;

            foreach (var building in CurrentBuildings)
            {
                if (building.BuildingMainData.Type != p_building.Type)
                    continue;

                if (building.IsBeeingUpgradedOrBuilded)
                    return false;
            }

            if (CurrentResourcePoints < p_building.PerLevelData[0].Requirements.ResourcePoints) return false;

            return true;
        }

        public bool CanUpgradeBuilding(Building p_building)
        {
            if (!_workersManager.IsAnyWorkerFree())
                return false;

            if (p_building.CurrentLevel + 1 >= p_building.BuildingMainData.PerLevelData.Length)
                return false;

            if (CurrentResourcePoints < p_building.BuildingMainData.PerLevelData
                    [p_building.CurrentLevel].Requirements.ResourcePoints)
                return false;

            return true;
        }

        public void HandleBuiltOfBuilding(BuildingData p_buildingData, bool p_instant)
        {
            if (CurrentBuildings.Any(x => x.BuildingMainData.Type == p_buildingData.Type))
                return;

            foreach (var building in _placesForBuildings)
            {
                if (building.BuildingData != p_buildingData)
                    continue;

                GameObject newBuildingGo = null;
                Building newBuilding = null;

                if (p_instant)
                {
                    newBuildingGo = Instantiate(p_buildingData.MainPrefab,
                        building.SiteForBuilding.position, Quaternion.identity);
                    newBuilding = newBuildingGo.GetComponent<Building>();
                    newBuilding.IsBeeingUpgradedOrBuilded = false;

                    newBuilding.FinishBuildingSequence();
                }
                else
                {
                    newBuildingGo = Instantiate(p_buildingData.MainPrefab,
                        building.SiteForBuilding.position, Quaternion.identity);
                    newBuilding = newBuildingGo.GetComponent<Building>();
                    newBuilding.SetFirstStage();
                    newBuilding.CurrentLevel = 0;

                    newBuilding.InitiateBuildingSequence();
                }

                CurrentBuildings.Add(newBuilding);
                newBuilding.OnPointsGathered += GatherPoints;
                newBuilding.OnWorkDone += PublishBuildingBuiltEvent;
                newBuilding.OnRepaired += PublishBuildingRepaired;
                newBuilding.OnTechnologyUpgrade += PublishBuildingTechnologyEvent;
                newBuilding.OnBuildingClicked += HandleBuildingClicked;
                newBuilding.OnBuildingDamaged += HandleBuildingDamaged;
            }
        }

        private void OnDestroy()
        {
            foreach (var newBuilding in CurrentBuildings)
            {
                newBuilding.OnPointsGathered -= GatherPoints;
                newBuilding.OnWorkDone -= PublishBuildingBuiltEvent;
                newBuilding.OnRepaired -= PublishBuildingRepaired;
                newBuilding.OnTechnologyUpgrade -= PublishBuildingTechnologyEvent;
                newBuilding.OnBuildingClicked -= HandleBuildingClicked;
                newBuilding.OnBuildingDamaged -= HandleBuildingDamaged;
            }
        }

        public void HandleUpgradeOfBuilding(BuildingType p_buildingType, bool p_instant)
        {
            for (var i = 0; i < CurrentBuildings.Count; i++)
            {
                if (CurrentBuildings[i].BuildingMainData.Type != p_buildingType)
                    continue;

                if (p_instant)
                {
                    CurrentBuildings[i].HandleLevelUp();
                }
                else
                {
                    CurrentBuildings[i].SetUpgradeStage();
                    CurrentBuildings[i].InitiateUpgradeSequence();
                }
            }
        }

        private void PublishBuildingTechnologyEvent(Building p_building)
        {
            OnBuildingTechnologyLvlUp?.Invoke(p_building);
        }

        private void PublishBuildingRepaired(Building p_building)
        {
            _audioManager.CreateNewAudioSource(_buildingFinishedEffect);

            AssignWorker(p_building, false);
            OnBuildingStateChanged?.Invoke(p_building);
            OnBuildingRepaired?.Invoke(p_building);
        }

        private void PublishBuildingBuiltEvent(Building p_building, bool p_unassignWorkers)
        {
            _audioManager.CreateNewAudioSource(_buildingFinishedEffect);

            if (p_building.BuildingMainData.LevelToEnableMinigame == p_building.CurrentLevel)
                BuildingWithEnabledMinigame.Add(p_building);

            if (p_building.BuildingMainData.Type == BuildingType.Cottage)
            {
                foreach (var building in AllBuildingsDatabase.allBuildings)
                    if (building.BaseCottageLevelNeeded == p_building.CurrentLevel)
                        UnlockedBuildings.Add(building);

                OnCottageLevelUp?.Invoke();
            }

            AssignWorker(p_building, p_unassignWorkers);
            OnBuildingStateChanged?.Invoke(p_building);
        }

        private void HandleBuildingDamaged(Building p_building)
        {
            OnBuildingDestroyed?.Invoke(p_building);
        }

        private void GatherPoints(BuildingType p_type, PointsType p_pointsType, int p_amount)
        {
            var amount = GetProductionOfBuilding(p_type);

            HandlePointsManipulation(p_pointsType, amount, true, true);
            OnPointsGathered?.Invoke(p_pointsType, amount);
        }

        private void HandleBuildingClicked(Building p_building)
        {
            _audioManager.CreateNewAudioSource(GetRightSoundEffect(p_building.BuildingMainData.Type));

            OnBuildingClicked?.Invoke(p_building);
        }

        public void AssignWorker(Building p_building, bool p_assign)
        {
            if ((p_assign && p_building.HaveWorker) || (!p_assign && !p_building.HaveWorker))
                return;

            p_building.HaveWorker = p_assign;

            if (p_assign)
            {
                Debug.Log("Worker added to: " + p_building);
                _workersManager.WorkersInBuilding++;
            }
            else
            {
                Debug.Log("Worker Removed from: " + p_building);
                _workersManager.WorkersInBuilding--;
            }
        }

        public bool IsAnyBuildingNonGathered()
        {
            foreach (var building in CurrentBuildings)
                if (building.HaveSomethingToCollect)
                    return true;

            return false;
        }

        public bool IsAnyBuildingNonBuilt()
        {
            var anyBuildingNeedsBuilding = false;

            foreach (var building in CurrentBuildings)
            {
                if (!building.CanEndBuildingSequence)
                    continue;

                if (building.IsDamaged)
                    building.SetBuildingIcon(FinishBuildingIcon);
                else
                    building.SetBuildingIcon(FinishBuildingIcon);

                anyBuildingNeedsBuilding = true;
            }

            return anyBuildingNeedsBuilding;
        }

        public void EndMissionHandler()
        {
            foreach (var building in CurrentBuildings)
            {
                building.HaveWorker = false;
                building.IsProtected = false;
            }

            CurrentResourcePoints = 0;
            CurrentDefensePoints = 0;
        }

        public int GetProductionOfBuilding(BuildingType p_building)
        {
            var specificBuilding = CurrentBuildings.Find(x => x.BuildingMainData.Type == p_building);
            float production = specificBuilding.BuildingMainData.PerLevelData[specificBuilding.CurrentLevel]
                .ProductionAmountPerDay;

            if (_godsManager.IsAnyBlessingActivated(specificBuilding.BuildingMainData.GodType))
            {
                production += production * _godsManager.GetBlessingValue(specificBuilding.BuildingMainData.GodType);
            }

            if (_bonus != null && _bonus.Building == p_building)
            {
                production += production * _bonus.BonusInPercents;
            }

            return Mathf.RoundToInt(production);
        }

        public BuildingData GetGodsBuilding(GodType p_godName)
        {
            foreach (var building in _buildingsDatabase.allBuildings)
                if (building.GodType == p_godName)
                    return building;

            Debug.LogError("Error in gods/building in BuildingsManager: GetGodsBuilding");
            return null;
        }

        public GodType GetRandomGodInBuildings()
        {
            var filteredBuildings = CurrentBuildings.Where(building => building.BuildingMainData.Type != BuildingType.Cottage).ToList();

            if (filteredBuildings.Count == 0)
            {
                return GodType.Noone;
            }

            int randomIndex = Random.Range(0, filteredBuildings.Count);
            return filteredBuildings[randomIndex].BuildingMainData.GodType;
        }

        public bool CheckIfGodsBuildingIsBuilt(GodType p_godName)
        {
            foreach (var building in CurrentBuildings)
            {
                if (building.BuildingMainData.GodType != p_godName)
                    continue;

                return true;
            }

            return false;
        }

        private bool ShouldStartTutorial()
        {
            return CurrentBuildings.Count == 1 && CurrentBuildings
                .First(x => x.BuildingMainData.Type == BuildingType.Cottage).IsDamaged;
        }

        public string GetLocalizedName(BuildingType p_building)
        {
            return _buildingsDatabase.allBuildings.Find(x => x.Type == p_building).BuildingName.GetLocalizedString();
        }

        public Sprite GetBuildingIcon(BuildingType p_building)
        {
            return _buildingsDatabase.allBuildings.Find(x => x.Type == p_building).Icon;
        }

        private AudioClip GetRightSoundEffect(BuildingType p_type)
        {
            switch (p_type)
            {
                case BuildingType.Cottage:
                    return CottageSoundEffect;
                case BuildingType.Farm:
                    return FarmSoundEffect;
                case BuildingType.GuardTower:
                    return GuardTowerSoundEffect;
                case BuildingType.Woodcutter:
                    return WoodcutterSoundEffect;
                case BuildingType.Alchemical_Hut:
                    return Alchemical_HutSoundEffect;
                case BuildingType.Mining_Shaft:
                    return Mining_ShaftSoundEffect;
                case BuildingType.Ritual_Circle:
                    return Ritual_CircleSoundEffect;
                case BuildingType.Peat_Excavation:
                    return Peat_ExcavationSoundEffect;
                case BuildingType.Charcoal_Pile:
                    return Charcoal_PileSoundEffect;
                case BuildingType.Herbs_Garden:
                    return Herbs_GardenSoundEffect;
                case BuildingType.Apiary:
                    return ApiarySoundEffect;
                case BuildingType.Workshop:
                    return Woodworking_StationSoundEffect;
                case BuildingType.Sacrificial_Altar:
                    return Sacrificial_AltarSoundEffect;
            }

            return null;
        }

        #region PointsManipulation

        public void HandlePointsManipulation(PointsType p_pointsType, int p_pointsNumber, bool p_add,
            bool p_createEffect = false)
        {
            var specificValue = p_pointsNumber;

            if (!p_add)
                specificValue = 0 - p_pointsNumber;

            switch (p_pointsType)
            {
                case PointsType.Resource:
                    ManipulateResourcePoints(specificValue, p_createEffect);
                    break;
                case PointsType.Defense:
                    ManipulateDefencePoints(specificValue, p_createEffect);
                    break;
                case PointsType.ResourcesAndDefense:
                    ManipulateResourcePoints(specificValue, p_createEffect);
                    ManipulateDefencePoints(specificValue / 2, p_createEffect);
                    break;
                case PointsType.DefenseAndResources:
                    ManipulateDefencePoints(specificValue, p_createEffect);
                    ManipulateResourcePoints(specificValue / 2, p_createEffect);
                    break;
                case PointsType.StarDust:
                    ManipulateShardsOfDestiny(specificValue, p_createEffect);
                    break;
            }
        }

        private void ManipulateDefencePoints(int p_amountOfResources, bool p_createEffect = false)
        {
            CurrentDefensePoints += p_amountOfResources;
            if (CurrentDefensePoints < 0)
                CurrentDefensePoints = 0;

            OnDefensePointsChange?.Invoke(p_amountOfResources, p_createEffect);
        }

        private void ManipulateResourcePoints(int p_amountOfResources, bool p_createEffect = false)
        {
            CurrentResourcePoints += p_amountOfResources;
            if (CurrentResourcePoints < 0)
                CurrentResourcePoints = 0;

            OnResourcePointsChange?.Invoke(p_amountOfResources, p_createEffect);
        }

        private void ManipulateShardsOfDestiny(int p_amountOfResources, bool p_createEffect = false)
        {
            CurrentDestinyShards += p_amountOfResources;
            if (CurrentDestinyShards < 0)
                CurrentDestinyShards = 0;

            OnDestinyShardsPointsChange?.Invoke(p_amountOfResources, p_createEffect);
        }

        #endregion

        #region Saving

        public BuildingManagerSavedData GetSavedData()
        {
            var savedBuildings = new List<BuildingSavedData>();

            foreach (var building in CurrentBuildings)
            {
                var buildingData = new BuildingSavedData
                {
                    TypeOfBuilding = building.BuildingMainData.Type,
                    CurrentLevel = building.CurrentLevel,
                    HaveWorker = building.HaveWorker,
                    IsDamaged = building.IsDamaged,
                    PlayedMinigame = building.PlayedMinigame,
                    CurrentDayOnQueue = building.CurrentDayOnQueue,
                    CurrentTechnologyDayOnQueue = building.CurrentTechnologyDayOnQueue,
                    HaveSomethingToCollect = building.HaveSomethingToCollect,
                    CanEndBuildingSequence = building.CanEndBuildingSequence,
                    IsBeeingUpgradedOrBuilded = building.IsBeeingUpgradedOrBuilded,
                    IsProtected = building.IsProtected,
                    CurrentTechnologyLvl = building.CurrentTechnologyLvl
                    // Add other fields here
                };

                savedBuildings.Add(buildingData);
            }

            return new BuildingManagerSavedData
            {
                Buildings = savedBuildings,
                BonusPerMission = _bonus,
                ResourcesStoredInBasement = _resourcesStoredInBasement,
                CurrentResourcePoints = CurrentResourcePoints,
                CurrentDefensePoints = CurrentDefensePoints,
                ShardsOfDestinyAmount = CurrentDestinyShards
            };
        }

        public void LoadSavedData(BuildingManagerSavedData p_data)
        {
            _resourcesStoredInBasement = p_data.ResourcesStoredInBasement;
            _bonus = p_data.BonusPerMission;
            CurrentResourcePoints = p_data.CurrentResourcePoints;
            CurrentDefensePoints = p_data.CurrentDefensePoints;
            CurrentDestinyShards = p_data.ShardsOfDestinyAmount;

            foreach (var savedBuilding in p_data.Buildings)
            {
                var probableBuilding = CurrentBuildings.Find(x => (int)
                    x.BuildingMainData.Type == (int)savedBuilding.TypeOfBuilding);

                if (probableBuilding != null)
                {
                    probableBuilding.CurrentLevel = savedBuilding.CurrentLevel;
                    probableBuilding.HaveWorker = savedBuilding.HaveWorker;
                    probableBuilding.IsDamaged = savedBuilding.IsDamaged;
                    probableBuilding.PlayedMinigame = savedBuilding.PlayedMinigame;
                    probableBuilding.CurrentDayOnQueue = savedBuilding.CurrentDayOnQueue;
                    probableBuilding.CurrentTechnologyDayOnQueue = savedBuilding.CurrentTechnologyDayOnQueue;
                    probableBuilding.HaveSomethingToCollect = savedBuilding.HaveSomethingToCollect;
                    probableBuilding.CanEndBuildingSequence = savedBuilding.CanEndBuildingSequence;
                    probableBuilding.IsBeeingUpgradedOrBuilded = savedBuilding.IsBeeingUpgradedOrBuilded;
                    probableBuilding.IsProtected = savedBuilding.IsProtected;
                    probableBuilding.CurrentTechnologyLvl = savedBuilding.CurrentTechnologyLvl;
                }
                else
                {
                    HandleBuiltOfBuilding(_buildingsDatabase.allBuildings.Find(
                        x => x.Type == savedBuilding.TypeOfBuilding), true);

                    probableBuilding = CurrentBuildings.Find(x =>
                        x.BuildingMainData.Type == savedBuilding.TypeOfBuilding);

                    probableBuilding.CurrentLevel = savedBuilding.CurrentLevel;
                    probableBuilding.HaveWorker = savedBuilding.HaveWorker;
                    probableBuilding.IsDamaged = savedBuilding.IsDamaged;
                    probableBuilding.PlayedMinigame = savedBuilding.PlayedMinigame;
                    probableBuilding.CurrentDayOnQueue = savedBuilding.CurrentDayOnQueue;
                    probableBuilding.CurrentTechnologyDayOnQueue = savedBuilding.CurrentTechnologyDayOnQueue;
                    probableBuilding.HaveSomethingToCollect = savedBuilding.HaveSomethingToCollect;
                    probableBuilding.CanEndBuildingSequence = savedBuilding.CanEndBuildingSequence;
                    probableBuilding.IsBeeingUpgradedOrBuilded = savedBuilding.IsBeeingUpgradedOrBuilded;
                    probableBuilding.IsProtected = savedBuilding.IsProtected;
                    probableBuilding.CurrentTechnologyLvl = savedBuilding.CurrentTechnologyLvl;

                    if (probableBuilding.IsDamaged)
                    {
                        probableBuilding.SetDestroyStage();
                    }
                    else if (probableBuilding.IsBeeingUpgradedOrBuilded)
                    {
                        if (probableBuilding.CurrentLevel == 0)
                        {
                            probableBuilding.SetFirstStage();
                        }
                        else
                        {
                            probableBuilding.SetUpgradeStage();
                        }
                    }
                }

                // postawić jeśli nie istnieje, wczytać dane
                // jeszcze trzeba zapisać questy
            }

            OnResourcePointsChange?.Invoke(0, false);
            OnDestinyShardsPointsChange?.Invoke(0, false);
            OnDefensePointsChange?.Invoke(0, false);
        }

        #endregion

        private float[] _bonuses = { .05f, .1f, .15f, .2f, .25f, .3f, .35f, .4f, .5f };

        public void TryToActivateBonus()
        {
            var index = Random.Range(0, CurrentBuildings.Count);
            var bonus = _bonuses[Random.Range(0, _bonuses.Length)];

            for (int i = 0; i < 50; i++)
            {
                if (CurrentBuildings[index].BuildingMainData.Type == BuildingType.Cottage)
                {
                    index = Random.Range(0, CurrentBuildings.Count);
                    continue;
                }

                _bonus = new CurrentMissionBonus
                {
                    Building = BuildingType.Farm,
                    BonusInPercents = bonus
                };

                break;
            }
        }

        public void SetZeroResourcePoints()
        {
            HandlePointsManipulation(PointsType.Resource, CurrentResourcePoints, false);
        }
    }

    [Serializable]
    public class CurrentMissionBonus
    {
        public BuildingType Building;
        public float BonusInPercents;
    }

    [Serializable]
    public struct BuildingSavedData
    {
        public BuildingType TypeOfBuilding;
        public int CurrentLevel;
        public bool HaveWorker;
        public bool IsDamaged;
        public bool PlayedMinigame;
        public int CurrentDayOnQueue;
        public int CurrentTechnologyDayOnQueue;
        public bool HaveSomethingToCollect;
        public bool CanEndBuildingSequence;
        public bool IsBeeingUpgradedOrBuilded;
        public bool IsProtected;
        public int CurrentTechnologyLvl;
    }

    [Serializable]
    public struct BuildingManagerSavedData
    {
        public List<BuildingSavedData> Buildings;
        public CurrentMissionBonus BonusPerMission;
        public int ResourcesStoredInBasement;
        public int CurrentResourcePoints;
        public int CurrentDefensePoints;
        public int ShardsOfDestinyAmount;
    }

    [Serializable]
    public class BuildingTransforms
    {
        public Transform SiteForBuilding;
        public BuildingData BuildingData;
    }
}