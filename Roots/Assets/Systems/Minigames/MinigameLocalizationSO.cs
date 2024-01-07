using UnityEngine;
using UnityEngine.Localization;

namespace Minigames
{
    [CreateAssetMenu(fileName = "Minigame Localization", menuName = "ScriptableObjects/MinigameLocalization", order = 5)]
    public class MinigameLocalizationSO: ScriptableObject
    {
        public LocalizedString ScoreText;
        public LocalizedString ResourcePointsCollect;
        public LocalizedString DefensePointsCollect;
        public LocalizedString StarDustPointsCollect;
        public LocalizedString ResourcesAndDefenseCollect;
        public LocalizedString DefenseAndResourcesCollect;
        
        public LocalizedString FarmInfo;
        public LocalizedString GuardTowerInfo;
        public LocalizedString WoodcutterInfo;
        public LocalizedString Alchemical_HutInfo;
        public LocalizedString Mining_ShaftInfo;
        public LocalizedString Ritual_CircleInfo;
        public LocalizedString Peat_ExcavationInfo;
        public LocalizedString Charcoal_PileInfo;
        public LocalizedString Herbs_GardenInfo;
        public LocalizedString ApiaryInfo;
        public LocalizedString WorkshopInfo;
        public LocalizedString Sacrificial_AltarInfo;
    }
}