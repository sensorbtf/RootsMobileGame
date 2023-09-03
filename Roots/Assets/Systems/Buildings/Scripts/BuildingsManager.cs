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
        private Dictionary<BuildingData, int> _buildingsInQueue;

        public event Action<BuildingData, int> OnBuildingClicked;
        public List<Building> CurrentBuildings => _currentlyBuildBuildings;
        public Dictionary<BuildingData, int> BuildingsInQueue => _buildingsInQueue;
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
            _buildingsInQueue = new Dictionary<BuildingData, int>();

            foreach (var buildingToBuild in _buildingsDatabase.allBuildings)
            {
                if (buildingToBuild.Type is BuildingType.Cottage or BuildingType.Farm)
                {
                    HandleBuildingBuilt(buildingToBuild);
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
            _buildingsInQueue.Add(p_buildingData, p_buildingData.PerLevelData[p_buildingLevel].Requirements.DaysToComplete);
        }
        
        public void RefreshQueue()
        {
            foreach (var building in _buildingsInQueue.ToList())
            {
                _buildingsInQueue[building.Key]--;

                if (_buildingsInQueue[building.Key] > 0) 
                    continue;
                
                if (!HandleBuildingBuilt(building.Key))
                {
                    HandleBuildingUpgrade(building.Key);
                }
                
                _buildingsInQueue.Remove(building.Key);
            }
        }
        
        public int GatherProductionPointsFromBuildings()
        {
            int resourcePoints = 0;

            foreach (var building in _currentlyBuildBuildings)
            {
                if (!building.BuildingMainData.PerLevelData[building.CurrentLevel].CanProduce || !building.HasWorker)
                    continue;

                resourcePoints += building.BuildingMainData.PerLevelData[building.CurrentLevel].ProductionPerDay;
            }

            return resourcePoints;
        }
        
        public int GatherDefensePointsFromBuildings()
        {
            int resourcePoints = 0;

            // foreach (var building in _currentlyBuildBuildings)
            // {
            //     if (!building.BuildingMainData.PerLevelData[building.CurrentLevel] || !building.hasWorker)
            //         continue;
            //
            //     resourcePoints += building.BuildingMainData.PerLevelData[building.CurrentLevel].ProductionPerDay;
            // }

            return resourcePoints;
        }
        
        public bool CanUpgradeOrBuildBuilding(BuildingData p_building, int p_currentLevel = 0)
        {
            if (_buildingsInQueue.ContainsKey(p_building))
            {
                //set upgragdingUi
                return false;
            }

            if (_workersManager.WorkersAmount <= 0)
            {
                return false;
            }

            if (CurrentResourcePoints < p_building.PerLevelData[p_currentLevel].Requirements.ResourcePoints)
            {
                return false;
            }
            
            return true;
        }
        
        private bool HandleBuildingBuilt(BuildingData p_buildingData)
        {
            if (_currentlyBuildBuildings.Any(x => x.BuildingMainData.Type == p_buildingData.Type))
                return false;
            
            foreach (var building in _placesForBuildings)
            {
                if (building.BuildingData != p_buildingData) 
                    continue;
                
                var newBuilding = Instantiate(p_buildingData.PerLevelData[0].Prefab, 
                    building.SiteForBuilding.position, Quaternion.identity);
                
                _currentlyBuildBuildings.Add(newBuilding.GetComponent<Building>());
                return true;
            }

            return false;
        }

        private bool HandleBuildingUpgrade(BuildingData p_buildingData)
        {
            for (int i = 0; i < _currentlyBuildBuildings.Count; i++)
            {
                if (_currentlyBuildBuildings[i].BuildingMainData.Type != p_buildingData.Type)
                    continue;

                _currentlyBuildBuildings[i].CurrentLevel++;
                return true;
            }

            Debug.LogError($"Trying to upgrade non-existing building: {p_buildingData}");
            return false;
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
    }

    [Serializable]
    public class BuildingTransforms
    {
        public Transform SiteForBuilding;
        public BuildingData BuildingData;
    }
}