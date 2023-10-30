using Buildings;
using UnityEngine;

namespace World
{
    [CreateAssetMenu(fileName = "QuestData", menuName = "ScriptableObjects/QuestData", order = 2)]
    public class QuestSO : ScriptableObject
    {
        public QuestType QuestKind;
        public BuildingType TargetName;
        public int TargetAmount;
        public int ShardsOfDestinyReward;
    }

    public enum QuestType
    {
        AchieveBuildingLvl,
        AchieveTechnologyLvl,
        MinigameResourcePoints,
        MinigameDefensePoints,
        ResourcePoints,
        DefensePoints,
        RepairBuilding,
        DoMinigame
    }
}