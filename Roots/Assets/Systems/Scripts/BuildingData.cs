using UnityEngine;
using UnityEngine.Serialization;

namespace Systems
{
    [CreateAssetMenu(fileName = "BuildingData", menuName = "ScriptableObjects/BuildingData", order = 1)]
    public class BuildingData : ScriptableObject
    {
        public BuildingName Name;
        public Sprite Icon;
        public GameObject Prefab;
        public int Cost; // bound it with tier as well as production
        public int UnlockTier;
        // ... Other attributes
    }

    public enum BuildingName
    {
        Cottage,
        Farm,
        GuardTower,
        // ... other building names
    }
}