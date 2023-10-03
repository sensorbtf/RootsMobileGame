using System;
using Buildings;
using UnityEngine;

namespace World
{
    [CreateAssetMenu(fileName = "QuestData", menuName = "ScriptableObjects/QuestData", order = 2)]
    public class QuestSO : ScriptableObject
    {
        public int ShardsOfDestinyReward;
        public Quest[] Quests;
    }

    [Serializable]
    public class Quest
    {
        private bool _isCompleted = false;
        
        public Action<Quest> OnCompletion;
        
        public QuestType QuestKind;
        public BuildingType TargetName;  
        public int TargetAmount;  
        public int ShardsOfDestinyReward;
        public bool IsCompleted {
            get
            {
                return _isCompleted;
            }
            set
            {
                _isCompleted = value;
                if (_isCompleted)
                {
                    OnCompletion?.Invoke(this);
                }
            }
        }
    }
    
    public enum QuestType
    {
        AchieveBuildingLvl,
        AchieveTechnologyLvl,
        MinigameResourcePoints,
        MinigameDefensePoints,
    }
}