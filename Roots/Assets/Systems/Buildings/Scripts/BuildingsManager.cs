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
        [SerializeField] private Sprite _resourcesPointsIcon;
        [SerializeField] private Sprite _defensePointsIcon;
        [SerializeField] private Sprite _defenseAndResourcesPointsIcon;
        
        private List<Building> _currentlyBuildBuildings;

        public event Action<BuildingData, int> OnBuildingClicked;
        public List<Building> CurrentBuildings => _currentlyBuildBuildings;
        public BuildingDatabase AllBuildingsDatabase => _buildingsDatabase;

        public int GetFarmProductionAmount
        {
            get
            {
                var building = _currentlyBuildBuildings.Find(x => x.BuildingMainData.Type == BuildingType.Farm);

                return building.BuildingMainData.PerLevelData[building.CurrentLevel].ProductionPerDay;
            }
        }

        public int CurrentResourcePoints = 0;
        public int CurrentDefensePoints = 0;
        public int ShardsOfDestinyAmount = 999;

        public void StartOnWorld()
        {
            ShardsOfDestinyAmount = 999;

            _currentlyBuildBuildings = new List<Building>();

            foreach (var buildingToBuild in _buildingsDatabase.allBuildings)
            {
                if (buildingToBuild.Type is BuildingType.Cottage or BuildingType.Farm)
                {
                    HandleBuiltOfBuilding(buildingToBuild, true);
                }
            }
        }

        private void OnEnable()
        {
            Building.OnBuildingClicked += HandleBuildingClicked;
        }

        private void OnDisable()
        {
            Building.OnBuildingClicked -= HandleBuildingClicked;
        }

        public Building GetSpecificBuilding(BuildingData p_data)
        {
            foreach (var building in _currentlyBuildBuildings)
            {
                if (building.BuildingMainData == p_data)
                {
                    return building;
                }
            }

            return null;
        }

        public void PutBuildingOnQueue(BuildingData p_buildingData)
        {
            var building = _currentlyBuildBuildings.Find(x => x.BuildingMainData == p_buildingData);
            
            if (building == null)
            {
                HandleBuiltOfBuilding(p_buildingData, false);
            }
            else
            {
                HandleUpgradeOfBuilding(p_buildingData, false);
            }
            
            AssignWorker(_currentlyBuildBuildings.Find(x => x.BuildingMainData == p_buildingData), true);
        }
        
        public void ModifyBuildingOnQueue(BuildingData p_buildingData, bool p_assign)
        {
            var building = _currentlyBuildBuildings.Find(x => x.BuildingMainData == p_buildingData);

            if (building.HaveWorker)
            {
                AssignWorker(building, false);
            }
            else
            {
                AssignWorker(building, true);
            }
        }

        public void RefreshBuildingsOnNewDay()
        {
            foreach (var building in _currentlyBuildBuildings)
            {
                if (!building.HaveWorker || building.IsBeeingUpgradedOrBuilded)
                    continue;

                if (building.BuildingMainData.PerLevelData[building.CurrentLevel].CanProduce &&
                    building.BuildingMainData.PerLevelData[building.CurrentLevel].CanRiseDefenses)
                {
                    building.SetCollectionIcon(_defenseAndResourcesPointsIcon);
                }
                
                if (building.BuildingMainData.PerLevelData[building.CurrentLevel].CanProduce)
                {
                    building.SetCollectionIcon(_resourcesPointsIcon);
                }

                if ( building.BuildingMainData.PerLevelData[building.CurrentLevel].CanRiseDefenses)
                {
                    building.SetCollectionIcon(_defensePointsIcon);
                }

                // add some sort of bonus here to avoid saving up data in Buildingscript
            }
            
            foreach (var building in _currentlyBuildBuildings)
            {
                if (!building.IsBeeingUpgradedOrBuilded || !building.HaveWorker)
                    continue;

                building.CurrentDayOnQueue++;

                if (building.CurrentDayOnQueue < building.BuildingMainData.PerLevelData[building.CurrentLevel].Requirements.DaysToComplete)
                    continue;

                if (building.CurrentLevel == 0)
                {
                    building.FinishBuildingSequence();
                }
                else
                {
                    building.HandleLevelUp();
                }
            }
        }

        public bool CanBuildBuilding(BuildingData p_building)
        {
            if (_workersManager.IsAnyWorkerFree())
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
            if (_workersManager.BaseWorkersAmounts - _workersManager.OverallAssignedWorkers == 0)
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
                newBuilding.OnWorkDone += AssignWorker;
            }
        }

        private void HandleUpgradeOfBuilding(BuildingData p_buildingData, bool p_instant)
        {
            for (int i = 0; i < _currentlyBuildBuildings.Count; i++)
            {
                if (_currentlyBuildBuildings[i].BuildingMainData.Type != p_buildingData.Type)
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
            switch (p_type)
            {
                case PointsType.Resource:
                    CurrentResourcePoints += p_amount;
                    break;
                
                case PointsType.Defense:
                    CurrentDefensePoints += p_amount;
                    break;
            }
        }

        private void HandleBuildingClicked(BuildingData p_buildingData, int p_level)
        {
            Debug.Log($"Building clicked: {p_buildingData}, Level: {p_level}");

            OnBuildingClicked?.Invoke(p_buildingData, p_level);
        }

        public void AssignWorker(Building p_building, bool p_assign)
        {
            if (p_assign && p_building.HaveWorker)
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
            
            p_building.OnWorkDone -= AssignWorker;
        }

        public bool CanAssignWorker(Building p_building)
        {
            foreach (var building in _currentlyBuildBuildings)
            {
                if (p_building.BuildingMainData.Type == building.BuildingMainData.Type)
                {
                    return !building.HaveWorker;
                }
            }

            return false;
        }

        public void RemoveResourcePoints(BuildingData p_buildingData, int p_buildingLevel)
        {
            CurrentResourcePoints -= p_buildingData.PerLevelData[p_buildingLevel].Requirements.ResourcePoints;
        }
        
        public void AddResourcePoints(BuildingData p_buildingData, int p_buildingLevel)
        {
            CurrentResourcePoints += p_buildingData.PerLevelData[p_buildingLevel].Requirements.ResourcePoints;
        }

        public int GetProductionDataOfBuilding(Building p_building)
        {
            return p_building.BuildingMainData.PerLevelData[p_building.CurrentLevel]
                .ProductionPerDay; // * _bonusesManager.GetBonusForBuilding(p_building)
        }

        public int GetDefenseRisingDataOfBuilding(Building p_building)
        {
            return p_building.BuildingMainData.PerLevelData[p_building.CurrentLevel]
                .DefensePointsPerDay; // * _bonusesManager.GetBonusForBuilding(p_building)
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
    }

    [Serializable]
    public class BuildingTransforms
    {
        public Transform SiteForBuilding;
        public BuildingData BuildingData;
    }
}