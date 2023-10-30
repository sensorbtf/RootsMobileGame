using UnityEngine;
using Buildings;

namespace Gods
{
    [CreateAssetMenu(fileName = "GodDataSO", menuName = "ScriptableObjects/GodData", order = 3)]
    public class GodDataSO : ScriptableObject
    {
        public GodType GodName;
        public Sprite GodImage;
        public BuildingType AffectedBuilding;
    }

    public enum GodType
    {
        Veles = 0,
        Perun = 1,
        Svarog = 2,
        Zirnitra = 3,
        Lada = 4,
        Dziewona = 5,
        Radegast = 6,
        Morana = 7,
        Svetovid = 8
    }

    public enum BlessingLevel
    { 
        Noone = 0,
        Small = 1,
        Medium = 2,
        Big = 3,
    }
}