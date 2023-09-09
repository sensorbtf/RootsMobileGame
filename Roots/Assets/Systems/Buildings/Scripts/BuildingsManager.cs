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

        public void PutBuildingOnQueue(BuildingData p_buildingData, int p_buildingLevel)
        {
            if (p_buildingLevel == 0)
            {
                HandleBuiltOfBuilding(p_buildingData, false);
            }
            else
            {
                HandleBuildingUpgrade(p_buildingData, false);
            }
        }

        public void RefreshBuildingsBuildTimer()
        {
            foreach (var building in _currentlyBuildBuildings)
            {
                if (!building.IsBeeingUpgradedOrBuilded)
                    continue;

                building.CurrentDayOnQueue++;

                if (building.CurrentDayOnQueue < building.BuildingMainData.PerLevelData[building.CurrentLevel].Requirements.DaysToComplete)
                    continue;

                building.FinishBuildingSequence();
            }
        }

        public int GatherProductionPointsFromBuildings()
        {
            int resourcePointsToAdd = 0;

            foreach (var building in _currentlyBuildBuildings)
            {
                if (!building.BuildingMainData.PerLevelData[building.CurrentLevel].CanProduce || !building.HasWorker)
                    continue;

                resourcePointsToAdd += GetProductionDataOfBuilding(building);
                // add some sort of bonus here to avoid saving up data in Buildingscript
            }

            return resourcePointsToAdd;
        }

        public int GatherDefensePointsFromBuildings()
        {
            int defensePointsToAdd = 0;

            foreach (var building in _currentlyBuildBuildings)
            {
                if (!building.BuildingMainData.PerLevelData[building.CurrentLevel].CanRiseDefenses ||
                    !building.HasWorker)
                    continue;

                defensePointsToAdd += GetDefenseRisingDataOfBuilding(building);
                // add some sort of bonus here to avoid saving up data in Buildingscript
            }

            return defensePointsToAdd;
        }

        public bool CanBuildBuilding(BuildingData p_building, int p_currentLevel = 0)
        {
            if (_workersManager.WorkersAmount <= 0)
                return false;

            foreach (var building in _currentlyBuildBuildings)
            {
                if (building.BuildingMainData.Type != p_building.Type)
                    continue;
                
                if (building.IsBeeingUpgradedOrBuilded)
                    return false;
            }
            
            if (CurrentResourcePoints < p_building.PerLevelData[p_currentLevel].Requirements.ResourcePoints)
            {
                return false;
            }

            return true;
        }
        
        public bool CanUpgradeBuilding(Building p_building)
        {
            if (_workersManager.WorkersAmount <= 0)
                return false;
            
            if (p_building.IsBeeingUpgradedOrBuilded)
            {
                //set upgragdingUi
                return false;
            }

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
                    newBuilding.InitiateBuildingSequence();
                }

                _currentlyBuildBuildings.Add(newBuilding);
            }
        }

        private void HandleBuildingUpgrade(BuildingData p_buildingData, bool p_instant)
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
                    _currentlyBuildBuildings[i].InitiateUpgradeSequence();
                }
            }
        }

        private void HandleBuildingClicked(BuildingData p_buildingData, int p_level)
        {
            Debug.Log($"Building clicked: {p_buildingData}, Level: {p_level}");

            OnBuildingClicked?.Invoke(p_buildingData, p_level);
        }

        public void AssignWorker(Building p_building, bool p_assign)
        {
            p_building.HasWorker = p_assign;
            if (p_assign)
            {
                Debug.Log($"Worker added to: " + p_building);
                _workersManager.WorkersAmount--;
            }
            else
            {
                _workersManager.WorkersAmount++;
            }
        }

        public bool CanAssignWorker(Building p_building)
        {
            foreach (var building in _currentlyBuildBuildings)
            {
                if (p_building.BuildingMainData.Type == building.BuildingMainData.Type)
                {
                    return !building.HasWorker;
                }
            }

            return false;
        }

        public void RemoveResourcePoints(BuildingData p_buildingData, int p_buildingLevel)
        {
            CurrentResourcePoints -= p_buildingData.PerLevelData[p_buildingLevel].Requirements.ResourcePoints;
        }

        public int GetProductionDataOfBuilding(Building p_building)
        {
            return p_building.BuildingMainData.PerLevelData[p_building.CurrentLevel]
                .ProductionPerDay; // * _bonusesManager.GetBonusForBuilding(p_building)
        }

        public int GetDefenseRisingDataOfBuilding(Building p_building)
        {
            return p_building.BuildingMainData.PerLevelData[p_building.CurrentLevel]
                .DefencePointsPerDay; // * _bonusesManager.GetBonusForBuilding(p_building)
        }
    }

    [Serializable]
    public class BuildingTransforms
    {
        public Transform SiteForBuilding;
        public BuildingData BuildingData;
    }
}