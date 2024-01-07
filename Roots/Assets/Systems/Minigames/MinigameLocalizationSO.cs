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
    }
}