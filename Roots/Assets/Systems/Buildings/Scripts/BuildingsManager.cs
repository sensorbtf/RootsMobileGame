using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System;
using UnityEngine.Serialization;

namespace Buildings
{
    public class BuildingManager : MonoBehaviour
    {
        [SerializeField] private WorkersManager _workersManager; // need to extend
        [SerializeField] private BuildingTransforms[] _placesForBuildings; // need to extend
        [SerializeField] private BuildingDatabase _buildingsDatabase;
        [SerializeField] private Sprite _buildingInBuildStage;

        private List<Building> _currentlyBuildBuildings;
        private int _resourcesStoredInBasement = 0;
        private int _currentResourcePoints;
        private int _currentDefensePoints;
        private int _shardsOfDestinyAmount;

        public Sprite DefenseAndResourcesPointsIcon;
        public Sprite ResourcesPointsIcon;
        public Sprite DefensePointsIcon;
        public Sprite ShardsOfDestinyIcon;

        public event Action<Building> OnBuildingClicked;
        public event Action<Building> OnBuildingStateChanged;
        public event Action<Building> OnBuildingTechnologyLvlUp;
        public event Action<Building> OnBuildingDestroyed;
        public event Action<PointsType, int> OnPointsGathered;

        public event Action<int, bool> OnResourcePointsChange;
        public event Action<int, bool> OnDefensePointsChange;
        public event Action<int, bool> OnDestinyShardsPointsChange;
        public List<Building> CurrentBuildings => _currentlyBuildBuildings;
        public BuildingDatabase AllBuildingsDatabase => _buildingsDatabase;

        public int GetFarmProductionAmount
        {
            get
            {
                var building = _currentlyBuildBuildings.Find(x => x.BuildingMainData.Type == BuildingType.Farm);
                return building.BuildingMainData.PerLevelData[building.CurrentLevel].ProductionAmountPerDay;
            }
        }

        public int GetBaseCottageBasementDeep
        {
            get
            {
                var building = _currentlyBuildBuildings.Find(x => x.BuildingMainData.Type == BuildingType.Cottage);
                return building.BuildingMainData.PerLevelData[building.CurrentLevel].ProductionAmountPerDay;
            }
        }

        public int ResourcesInBasement
        {
            set => _resourcesStoredInBasement =
                GetBaseCottageBasementDeep <= value ? value : GetBaseCottageBasementDeep;
            get => _resourcesStoredInBasement;
        }

        public int CurrentResourcePoints => _currentResourcePoints;

        public int CurrentDefensePoints => _currentDefensePoints;

        public int ShardsOfDestinyAmount => _shardsOfDestinyAmount;
        
        public void StartOnWorld()
        {
            HandlePointsManipulation(PointsType.ShardsOfDestiny, 250, true);

            _currentlyBuildBuildings = new List<Building>();

            foreach (var buildingToBuild in _buildingsDatabase.allBuildings)
            {
                if (buildingToBuild.Type is BuildingType.Cottage or BuildingType.Farm or BuildingType.Woodcutter)
                {
                    HandleBuiltOfBuilding(buildingToBuild, true);
                }
            }
        }

        public Building GetSpecificBuilding(BuildingType p_data)
        {
            foreach (var building in _currentlyBuildBuildings)
            {
                if (building.BuildingMainData.Type == p_data)
                {
                    return building;
                }
            }

            return null;
        }

        public void PutBuildingOnQueue(BuildingData p_buildingData)
        {
            var building = GetSpecificBuilding(p_buildingData.Type);

            if (building == null)
            {
                HandleBuiltOfBuilding(p_buildingData, false);
            }
            else
            {
                HandleUpgradeOfBuilding(p_buildingData.Type, false);
            }

            AssignWorker(GetSpecificBuilding(p_buildingData.Type), true);
        }

        public void HandleBuildingsModifications(Building p_building)
        {
            AssignWorker(p_building, !p_building.HaveWorker);
        }

        public void RefreshBuildingsOnNewDay()
        {
            foreach (var building in _currentlyBuildBuildings)
            {
                if (building.IsBeeingUpgradedOrBuilded || !building.HaveWorker || building.IsDamaged)
                    continue;

                building.CurrentTechnologyDayOnQueue++;

                switch (building.BuildingMainData.PerLevelData[building.CurrentLevel].ProductionType)
                {
                    case PointsType.Nothing:
                        break;
                    case PointsType.Resource:
                        building.SetCollectionIcon(ResourcesPointsIcon);
                        break;
                    case PointsType.Defense:
                        building.SetCollectionIcon(DefensePointsIcon);
                        break;
                    case PointsType.ResourcesAndDefense:
                        building.SetCollectionIcon(DefenseAndResourcesPointsIcon);
                        break;
                    case PointsType.ShardsOfDestiny:
                        break;
                    default:
                        break;
                }

                // add some sort of bonus here to avoid saving up data in Buildingscript
            }

            foreach (var building in _currentlyBuildBuildings)
            {
                if (!building.HaveWorker)
                    continue;

                building.HandleNewDay();
            }
        }

        public bool CanBuildBuilding(BuildingData p_building)
        {
            if (!_workersManager.IsAnyWorkerFree())
                return false;

            foreach (var building in _currentlyBuildBuildings)
            {
                if (building.BuildingMainData.Type != p_building.Type)
                    continue;

                if (building.IsBeeingUpgradedOrBuilded)
                    return false;
            }

            if (CurrentResourcePoints < p_building.PerLevelData[0].Requirements.ResourcePoints)
            {
                return false;
            }

            return true;
        }

        public bool CanUpgradeBuilding(Building p_building)
        {
            if (!_workersManager.IsAnyWorkerFree())
                return false;

            if (CurrentResourcePoints < p_building.BuildingMainData.PerLevelData
                    [p_building.CurrentLevel].Requirements.ResourcePoints)
            {
                return false;
            }

            return true;
        }

        private void HandleBuiltOfBuilding(BuildingData p_buildingData, bool p_instant)
        {
            if (_currentlyBuildBuildings.Any(x => x.BuildingMainData.Type == p_buildingData.Type))
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
                    newBuilding.InGameIcon.sprite = _buildingInBuildStage;

                    newBuilding.InitiateBuildingSequence();
                }

                _currentlyBuildBuildings.Add(newBuilding);
                newBuilding.OnPointsGathered += GatherPoints;
                newBuilding.OnWorkDone += PublishBuildingBuiltEvent;
                newBuilding.OnTechnologyUpgrade += PublishBuildingTechnologyEvent;
                newBuilding.OnBuildingClicked += HandleBuildingClicked;
                newBuilding.OnBuildingDamaged += HandleBuildingDamaged;
            }
        }

        private void PublishBuildingTechnologyEvent(Building p_building)
        {
            OnBuildingTechnologyLvlUp?.Invoke(p_building);
        }
        
        private void PublishBuildingBuiltEvent(Building p_building, bool p_unassignWorkers)
        {
            AssignWorker(p_building, p_unassignWorkers);
            OnBuildingStateChanged?.Invoke(p_building);
        }

        private void HandleBuildingDamaged(Building p_building)
        {
            OnBuildingDestroyed?.Invoke(p_building);
        }

        public void HandleUpgradeOfBuilding(BuildingType p_buildingType, bool p_instant)
        {
            for (int i = 0; i < _currentlyBuildBuildings.Count; i++)
            {
                if (_currentlyBuildBuildings[i].BuildingMainData.Type != p_buildingType)
                    continue;

                if (p_instant)
                {
                    _currentlyBuildBuildings[i].HandleLevelUp();
                }
                else
                {
                    _currentlyBuildBuildings[i].InGameIcon.sprite = _buildingInBuildStage;
                    _currentlyBuildBuildings[i].InitiateUpgradeSequence();
                }
            }
        }

        private void GatherPoints(PointsType p_type, int p_amount)
        {
            HandlePointsManipulation(p_type, p_amount, true, true);
            OnPointsGathered?.Invoke(p_type, p_amount);
        }

        private void HandleBuildingClicked(Building p_building)
        {
            OnBuildingClicked?.Invoke(p_building);
        }

        public void AssignWorker(Building p_building, bool p_assign)
        {
            if (p_assign && p_building.HaveWorker || !p_assign && !p_building.HaveWorker)
                return;

            p_building.HaveWorker = p_assign;

            if (p_assign)
            {
                Debug.Log($"Worker added to: " + p_building);
                _workersManager.WorkersInBuilding++;
            }
            else
            {
                Debug.Log($"Worker Removed from: " + p_building);
                _workersManager.WorkersInBuilding--;
            }
        }

        public int GetProductionDataOfBuilding(Building p_building)
        {
            return p_building.BuildingMainData.PerLevelData[p_building.CurrentLevel]
                .ProductionAmountPerDay; // * _bonusesManager.GetBonusForBuilding(p_building)
        }

        public bool IsAnyBuildingNonGathered()
        {
            foreach (var building in _currentlyBuildBuildings)
            {
                if (building.HaveSomethingToCollect)
                {
                    return true;
                }
            }

            return false;
        }

        public void EndMissionHandler()
        {
            foreach (var building in _currentlyBuildBuildings)
            {
                building.HaveWorker = false;
                building.IsProtected = false;

                // any way of attacking 
            }
        }

        public void HandlePointsManipulation(PointsType p_pointsType, int p_pointsNumber, bool p_add, bool p_createEffect = false)
        {
            int specificValue = p_pointsNumber;

            if (!p_add)
            {
                specificValue = 0 - p_pointsNumber;
            }

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
                    ManipulateDefencePoints(specificValue, p_createEffect);
                    break;
                case PointsType.ShardsOfDestiny:
                    ManipulateShardsOfDestiny(specificValue, p_createEffect);
                    break;
                default:
                    break;
            }
        }
        
        private void ManipulateDefencePoints(int p_amountOfResources, bool p_createEffect = false)
        {
            _currentDefensePoints += p_amountOfResources;
            if (_currentDefensePoints < 0)
                _currentDefensePoints = 0;

            OnDefensePointsChange?.Invoke(p_amountOfResources, p_createEffect);
        }

        private void ManipulateResourcePoints(int p_amountOfResources, bool p_createEffect = false)
        {
            _currentResourcePoints += p_amountOfResources;
            if (_currentResourcePoints < 0)
                _currentResourcePoints = 0;

            OnResourcePointsChange?.Invoke(p_amountOfResources, p_createEffect);
        }

        private void ManipulateShardsOfDestiny(int p_amountOfResources, bool p_createEffect = false)
        {
            _shardsOfDestinyAmount += p_amountOfResources;
            if (_shardsOfDestinyAmount < 0)
                _shardsOfDestinyAmount = 0;

            OnDestinyShardsPointsChange?.Invoke(p_amountOfResources, p_createEffect);
        }
    }

    [Serializable]
    public class BuildingTransforms
    {
        public Transform SiteForBuilding;
        public BuildingData BuildingData;
    }
}