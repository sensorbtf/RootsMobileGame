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
                
                if (_isCompleted) 
                    OnCompletion?.Invoke(this);
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

                if (_achievedTargetAmount >= SpecificQuest.TargetAmount) 
                    IsCompleted = true;
            }
        }

        public SavedQuestData GetSavedData()
        {
            return new SavedQuestData()
            {
                IsCompleted = _isCompleted,
                AchievedTargetAmount = _achievedTargetAmount,
                IsRedeemed = _isRedeemed
            };
        }
        
        public void LoadSavedData(SavedQuestData p_savedData)
        {
            IsRedeemed = p_savedData.IsRedeemed;
            
            if (IsRedeemed)
                _isCompleted = p_savedData.IsCompleted;
            else
                IsCompleted = p_savedData.IsCompleted;
            
            _achievedTargetAmount = p_savedData.AchievedTargetAmount;
        }
    }

    [Serializable]
    public struct SavedQuestData
    {
        public int AchievedTargetAmount;
        public bool IsCompleted;
        public bool IsRedeemed;
    }
}