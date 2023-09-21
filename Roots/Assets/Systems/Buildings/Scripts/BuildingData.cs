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
        [FormerlySerializedAs("TechnologyPerLevel")] [SerializeField] public TechnologyData Technology;
    }

    [Serializable]
    public class BaseDataPerLevel
    {
        public Sprite Icon;
        public Sprite InGameSprite;
        public Requirements Requirements;
        public bool CanProduce;
        public int ProductionPerDay;
        public bool CanRiseDefenses;
        [FormerlySerializedAs("DefencePointsPerDay")] public int DefensePointsPerDay;
    }
    
    [Serializable]
    public class Requirements
    {
        public int ResourcePoints;
        public int DaysToComplete;
        public BuildingRequirements[] OtherBuildingRequirements;
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
        Woodcutter = 3 
        
    }
    
    public enum PointsType
    {
        Resource,
        Defense,
        ShardsOfDestiny,
    }
}