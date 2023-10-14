using System;
using UnityEngine;

namespace World
{
    [Serializable]
    public class CurrentQuests
    {
        [SerializeField] public Quest[] CurrentLevelQuests;
    }

    [Serializable]
    public class Quest
    {
        public QuestSO SpecificQuest;

        public Action<Quest> OnCompletion;

        private bool _isCompleted = false;
        private bool _isRedeemed = false;

        public bool IsCompleted
        {
            get => _isCompleted;
            set
            {
                _isCompleted = value;
                if (_isCompleted)
                {
                    OnCompletion?.Invoke(this);
                }
            }
        }
        
        public bool IsRedeemed
        {
            get => _isRedeemed;
            set => _isRedeemed = value;
        }

        private int _achievedTargetAmount = 0;

        public int AchievedTargetAmount
        {
            get => _achievedTargetAmount;
            set => _achievedTargetAmount = value;
        }
    }
}