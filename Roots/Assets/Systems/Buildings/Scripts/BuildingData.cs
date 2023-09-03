using System;
using UnityEngine;
using UnityEngine.Serialization;

namespace Buildings
{
    [CreateAssetMenu(fileName = "BuildingData", menuName = "ScriptableObjects/BuildingData", order = 1)]
    public class BuildingData : ScriptableObject
    {
        public BuildingType Type;
        public int BaseCottageLevelNeeded;
        [SerializeField] public BaseDataPerLevel[] PerLevelData;
    }

    [Serializable]
    public class BaseDataPerLevel
    {
        public Sprite Icon;
        public GameObject Prefab;
        public int CottageLevelNeeded;
        public Requirements Requirements;
        public Researches[] AvaiableResearches;
        public bool CanProduce;
        public int ProductionPerDay;
        public bool CanRiseDefenses;
        public int DefencePointsPerDay;
    }
    
    [Serializable]
    public class Requirements
    {
        public int ResourcePoints;
        public int DaysToComplete;
        public BuildingRequirements[] BuildingRequirements;
        public Researches[] ResearchRequirements;
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
    }
    
    public enum Researches
    {
        Cottage,
        Farm,
        GuardTower,
    }
}