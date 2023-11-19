using System;
using UnityEngine;

namespace Buildings
{
    [CreateAssetMenu(fileName = "TechnologyData", menuName = "ScriptableObjects/TechnologyData", order = 2)]
    public class TechnologyData : ScriptableObject
    {
        public TechnologyDataPerLevel[] DataPerTechnologyLevel;
        public Sprite SpriteOfObject;
    }

    [Serializable]
    public class TechnologyDataPerLevel
    {
        public int RequiredBuildingLevel;
        public OtherBuildingsRequirements[] OtherBuildingsRequirements;
        public int WorksDayToAchieve;
        public float MinigameDuration;
        public float Efficiency;
    }

    [Serializable]
    public class OtherBuildingsRequirements
    {
        public BuildingType Building;
        public int Level;
    }
}