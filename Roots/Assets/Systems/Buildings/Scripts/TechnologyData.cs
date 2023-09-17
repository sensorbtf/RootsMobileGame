using System;
using UnityEngine;
using UnityEngine.Serialization;

namespace Buildings
{
    [CreateAssetMenu(fileName = "TechnologyData", menuName = "ScriptableObjects/TechnologyData", order = 2)]
    public class TechnologyData : ScriptableObject
    {
        public TechnologyDataPerLevel[] DataPerTechnologyLevel;
    }

    [Serializable]
    public class TechnologyDataPerLevel
    {
        public int RequiredBuildingLevel;
        public OtherBuildingsRequirements OtherBuildingsRequirements;
        public int WorksDayToAchieve;
        public float MinigameDuration;
        public float Effciency;
    }

    [Serializable]
    public class OtherBuildingsRequirements
    {
        public BuildingType NeededBuilding;
        public int NeededBuildingLevel;
    }
}