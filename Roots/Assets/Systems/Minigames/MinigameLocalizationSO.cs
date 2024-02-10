using UnityEngine;
using UnityEngine.Localization;

namespace Minigames
{
    [CreateAssetMenu(fileName = "Minigame Localization", menuName = "ScriptableObjects/MinigameLocalization", order = 5)]
    public class MinigameLocalizationSO: ScriptableObject
    {
        public AudioClip Countdown;
        public AudioClip OnMinigameEnd;
        public AudioClip PointsAdded;
        
        public LocalizedString ScoreText;
        public LocalizedString ResourcePointsCollect;
        public LocalizedString DefensePointsCollect;
        public LocalizedString StarDustPointsCollect;
        public LocalizedString ResourcesAndDefenseCollect;
        public LocalizedString DefenseAndResourcesCollect;
        public LocalizedString GuardTowerStorm;
        public LocalizedString SacrificialAltarReward;
        
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
        
        public LocalizedString FarmInfoBottom;
        public LocalizedString GuardTowerInfoBottom;
        public LocalizedString WoodcutterInfoBottom;
        public LocalizedString Alchemical_HutInfoBottom;
        public LocalizedString Mining_ShaftInfoBottom;
        public LocalizedString Ritual_CircleInfoBottom;
        public LocalizedString Peat_ExcavationInfoBottom;
        public LocalizedString Charcoal_PileInfoBottom;
        public LocalizedString Herbs_GardenInfoBottom;
        public LocalizedString ApiaryInfoBottom;
        public LocalizedString WorkshopInfoBottom;
        public LocalizedString Sacrificial_AltarInfoBottom;
    }
}