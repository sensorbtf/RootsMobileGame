using UnityEngine;
using System;
using System.Collections.Generic;

namespace Buildings
{
    public class BuildingManager : MonoBehaviour
    {
        [SerializeField] private BuildingTransforms[] PlacesForBuildings; // need to extend
        [SerializeField] private BuildingDatabase _buildingsDatabase;
        private List<Building> _currentlyBuildBuildings;

        public event Action<BuildingData, int> OnBuildingClicked;
        public List<Building> CurrentBuildings => _currentlyBuildBuildings;
        public BuildingDatabase AllBuildingsDatabase => _buildingsDatabase;

        private void Start()
        {
            _currentlyBuildBuildings = new List<Building>();
        }

        private void OnEnable()
        {
            Building.OnBuildingClicked += HandleBuildingClicked;
        }

        private void OnDisable()
        {
            Building.OnBuildingClicked -= HandleBuildingClicked;
        }

        public void HandleBuildingBuilt(BuildingData p_buildingData)
        {
            foreach (var building in PlacesForBuildings)
            {
                if (building.BuildingData != p_buildingData) 
                    continue;
                
                var newBuilding = Instantiate(p_buildingData.PerLevelData[0].Prefab, 
                    building.SiteForBuilding.position, Quaternion.identity);
                
                _currentlyBuildBuildings.Add(newBuilding.GetComponent<Building>());
                break;
            }
        }

        public void HandleBuildingUpgrade(BuildingData p_buildingData)
        {
            for (int i = 0; i < _currentlyBuildBuildings.Count; i++)
            {
                if (_currentlyBuildBuildings[i].BuildingMainData.type != p_buildingData.type)
                    continue;

                _currentlyBuildBuildings[i].CurrentLevel++;
                return;
            }

            Debug.LogError($"Trying to upgrade non-existing building: {p_buildingData}");
        }

        public int GatherProductionPointsFromBuildings()
        {
            int resourcePoints = 0;

            foreach (var building in _currentlyBuildBuildings)
            {
                if (!building.BuildingMainData.PerLevelData[building.CurrentLevel].CanProduce)
                    continue;

                resourcePoints += building.BuildingMainData.PerLevelData[building.CurrentLevel].ProductionPerDay;
            }

            return resourcePoints;
        }

        private void HandleBuildingClicked(BuildingData p_buildingData, int p_level)
        {
            Debug.Log($"Building clicked: {p_buildingData}, Level: {p_level}");

            OnBuildingClicked?.Invoke(p_buildingData, p_level);
        }
    }

    [Serializable]
    public class BuildingTransforms
    {
        public Transform SiteForBuilding;
        public BuildingData BuildingData;
    }
}