using UnityEngine;
using UnityEngine.Localization;

namespace Gods
{
    [CreateAssetMenu(fileName = "GodDataSO", menuName = "ScriptableObjects/GodData", order = 3)]
    public class GodDataSO : ScriptableObject
    {
        public GodType GodName;
        public LocalizedString GodLocalizedName;
        public Sprite GodImage;
    }

    public enum GodType
    {
        Noone = 0,
        Veles = 1,
        Perun = 2,
        Svarog = 3,
        Zirnitra = 4,
        Lada = 5,
        Dziewona = 6,
        Radegast = 7,
        Morana = 8,
        Svetovid = 9,
        Ziva = 10,
        Mokosh = 11,
        Kresnik = 12
    }

    public enum BlessingLevel
    {
        Noone = 0,
        Small = 1,
        Medium = 2,
        Big = 3
    }
}