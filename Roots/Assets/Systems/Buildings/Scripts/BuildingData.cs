using System;
using Gods;
using UnityEngine;
using UnityEngine.Localization;

namespace Buildings
{
    [CreateAssetMenu(fileName = "BuildingData", menuName = "ScriptableObjects/BuildingData", order = 1)]
    public class BuildingData : ScriptableObject
    {
        public BuildingType Type;
        public LocalizedString BuildingName;
        public GodType GodType;
        public GameObject MainPrefab;
        public Sprite Icon;
        public Sprite InGameSprite;
        public Sprite FirstStageBuilding;
        public Sprite UpgradeStage;
        public Sprite DestroyedStage;
        public LocalizedString Description;
        public int BaseCottageLevelNeeded;
        public int LevelToEnableMinigame;
        [SerializeField] public BaseDataPerLevel[] PerLevelData;
        [SerializeField] public TechnologyData Technology;
    }

    [Serializable]
    public class BaseDataPerLevel
    {
        public Requirements Requirements;
        public PointsType ProductionType;
        public int ProductionAmountPerDay;
    }

    [Serializable]
    public class Requirements
    {
        public int ResourcePoints;
        public int DaysToComplete;
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
        Peat_Excavation = 7,
        Charcoal_Pile = 8,
        Herbs_Garden = 9,
        Apiary = 10,
        Workshop = 11,
        Sacrificial_Altar = 12,
    }

    public enum PointsType
    {
        Nothing = 0,
        Resource = 1,
        Defense = 2,
        ResourcesAndDefense = 3,
        StarDust = 4,
        DefenseAndResources = 5
    }
}