using System;
using UnityEngine;

namespace World
{
    [Serializable]
    public class CurrentQuests
    {
        [SerializeField] public Quest[] CurrentLevelQuests;
        [SerializeField] public int NeededMissionToRankUp;
    }

    [Serializable]
    public class Quest
    {
        public QuestSO SpecificQuest;

        private int _achievedTargetAmount;

        private bool _isCompleted;
        private bool _isRedeemed;

        public Action<Quest> OnCompletion;

        public bool IsCompleted
        {
            get => _isCompleted;
            set
            {
                _isCompleted = value;
                if (_isCompleted) OnCompletion?.Invoke(this);
            }
        }

        public bool IsRedeemed
        {
            get => _isRedeemed;
            set => _isRedeemed = value;
        }

        public int AchievedTargetAmount
        {
            get => _achievedTargetAmount;
            set
            {
                _achievedTargetAmount = value;

                if (_achievedTargetAmount >= SpecificQuest.TargetAmount) IsCompleted = true;
            }
        }
    }
}