using System;
using UnityEngine;
using UnityEngine.Serialization;

namespace Buildings
{
    [CreateAssetMenu(fileName = "BuildingData", menuName = "ScriptableObjects/BuildingData", order = 1)]
    public class BuildingData : ScriptableObject
    {
        public BuildingType Type;
        public GameObject MainPrefab;
        public int BaseCottageLevelNeeded;
        [SerializeField] public BaseDataPerLevel[] PerLevelData;
        [SerializeField] public TechnologyData Technology;
    }

    [Serializable]
    public class BaseDataPerLevel
    {
        public Sprite Icon;
        public Sprite InGameSprite;
        public Requirements Requirements;
        public PointsType ProductionType;
        public int ProductionAmountPerDay;
    }
    
    [Serializable]
    public class Requirements
    {
        public int ResourcePoints;
        public int DaysToComplete;
        //public BuildingRequirements[] OtherBuildingRequirements;
    }
    
    [Serializable]
    public class BuildingRequirements
    {
        public BuildingType SpecificBuilding;
        public int LevelOfBuilding;
    }
    
    public enum BuildingType
    {
        Cottage = 0,
        Farm = 1,
        GuardTower = 2,
        Woodcutter = 3,
        Alchemical_Hut = 4,
        Mining_Shaft = 5,
        Ritual_Circle = 6,
    }
    
    public enum PointsType
    {
        Nothing = 0,
        Resource = 1,
        Defense = 2,
        ResourcesAndDefense = 3,
        ShardsOfDestiny = 4
    }
}