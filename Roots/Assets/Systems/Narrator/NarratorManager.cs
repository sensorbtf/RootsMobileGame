using System;
using Buildings;
using UnityEngine;

namespace Narrator
{
    public class NarratorManager: MonoBehaviour
    {
        [SerializeField] private BuildingsManager _buildingsManager;
        
        private TutorialStep _currentTutorialStep;
        private int _currentSubText = 0;

        public event Action<bool> OnTutorialAdvancement;
        public TutorialStep CurrentTutorialStep => _currentTutorialStep;
        public int CurrentSubText => _currentSubText;
        
        private void Start()
        {
            _currentTutorialStep = TutorialStep.Start;
            _buildingsManager.OnBuildingRepaired += CheckBuildingRepaired;
            _buildingsManager.OnTutorialStart += StartTutorial;
            _buildingsManager.OnBuildingStateChanged += CheckBuildingBuilt;
            _buildingsManager.OnBuildingTechnologyLvlUp += CheckBuildingTechLevelUp;
        }

        public void TryToActivateNarrator(TutorialStep p_step)
        {
            if ((int)p_step == (int)_currentTutorialStep + 1)
            {
                _currentTutorialStep = p_step;
                _currentSubText = 0;
                OnTutorialAdvancement?.Invoke(true);
            }
        }

        public void AddToSubtext()
        {
            _currentSubText++;
        }

        private void StartTutorial()
        {
            _currentTutorialStep = TutorialStep.GameStarted_Q1;
            OnTutorialAdvancement?.Invoke(true);
            
            _buildingsManager.OnTutorialStart -= StartTutorial;
        }
        
        private void CheckBuildingRepaired(Building p_building)
        {
            _currentTutorialStep = TutorialStep.OnCottageRepaired_Q6;
            OnTutorialAdvancement?.Invoke(true);
            
            _buildingsManager.OnBuildingRepaired -= CheckBuildingRepaired;
        }
        
        private void CheckBuildingBuilt(Building p_building)
        {
            if (_currentTutorialStep == TutorialStep.NextDayWithFarmFinished_Q8 && p_building.BuildingMainData.Type == BuildingType.Farm)
            {
                TryToActivateNarrator(TutorialStep.AfterFarmFinishBuild_Q9);
            }
            _buildingsManager.OnBuildingStateChanged -= CheckBuildingBuilt;
        }
        
        private void CheckBuildingTechLevelUp(Building p_building)
        {
            if (_currentTutorialStep == TutorialStep.OnFarmPanelWithTechnology1_Q11 && p_building.BuildingMainData.Type == BuildingType.Farm)
            {
                TryToActivateNarrator(TutorialStep.OnTechnologyInFarmLvlUp_Q12);
            }
            
            _buildingsManager.OnBuildingTechnologyLvlUp -= CheckBuildingTechLevelUp;
        }

        public NarratorManagerSavedData GetSavedData()
        {
            return new NarratorManagerSavedData
            {
                CurrentTutorialStep = (int)_currentTutorialStep,
                CurrentSubText = _currentSubText,
            };
        }
        
        public void LoadSavedData(NarratorManagerSavedData p_data)
        {
            _currentTutorialStep = (TutorialStep)p_data.CurrentTutorialStep; 
            _currentSubText = p_data.CurrentSubText;

            OnTutorialAdvancement?.Invoke(false);
        }
    }
    
    [Serializable]
    public struct NarratorManagerSavedData
    {
        public int CurrentTutorialStep;
        public int CurrentSubText;
    }

    public enum TutorialStep
    {
        Start = -1,
        GameStarted_Q1 = 0,
        FirstWorkingPanelOpen_Q2 = 1,
        SecondWorkingPanel_Q3 = 2,
        FirstDayStarted_Q4 = 3,
        OnDaySkip_Q5 = 4,
        OnCottageRepaired_Q6 = 5,
        ThirdWorkingPanelOpened_Q7 = 6,
        NextDayWithFarmFinished_Q8 = 7,
        AfterFarmFinishBuild_Q9 = 8,
        OnFarmPanelOpen_Q10 = 9,
        OnFarmPanelWithTechnology1_Q11 = 10,
        OnTechnologyInFarmLvlUp_Q12 = 11,
        OnFarmMinigameEnded_Q13 = 12,
        OnFarmPanelClosed_Q14 = 13,
        OnGuardTowerMinigameEnded_Q15 = 14,
        OnDefendPanelOpened_Q16 = 15,
        OnAfterDefendPanel_Q17 = 16,
        OnMissionRestart_Q18 = 17,
        OnWorkersPanelOpenAfterRestart_Q19 = 18,
    }
}