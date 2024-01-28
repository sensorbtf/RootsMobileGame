using System;
using Buildings;
using World;
using UnityEngine;

namespace Narrator
{
    public class NarratorManager: MonoBehaviour
    {
        [SerializeField] private WorldManager _worldManager;
        [SerializeField] private BuildingsManager _buildingsManager;
        [SerializeField] private bool _enableNarrator = true;
        
        private TutorialStep _currentTutorialStep;
        private int _currentSubText = 0;

        public event Action<bool> OnTutorialAdvancement;
        public TutorialStep CurrentTutorialStep => _currentTutorialStep;
        public int CurrentSubText => _currentSubText;
        
        private void Start()
        {
            if (_enableNarrator)
            {
                _currentTutorialStep = TutorialStep.OnGameStarted_Q1;
            }
            else
            {
                _currentTutorialStep = TutorialStep.Quests_End;
            }
            
            _buildingsManager.OnBuildingRepaired += CheckBuildingRepaired;
            _buildingsManager.OnTutorialStart += StartTutorial;
            _buildingsManager.OnBuildingStateChanged += CheckBuildingBuilt;
            _buildingsManager.OnBuildingTechnologyLvlUp += CheckBuildingTechLevelUp;

            _worldManager.OnNewMissionStart += TryToActivateBonus;
            
            _currentSubText = 0;
            OnTutorialAdvancement?.Invoke(true);
        }

        private void OnDestroy()
        {
            _buildingsManager.OnBuildingRepaired -= CheckBuildingRepaired;
            _buildingsManager.OnTutorialStart -= StartTutorial;
            _buildingsManager.OnBuildingStateChanged -= CheckBuildingBuilt;
            _buildingsManager.OnBuildingTechnologyLvlUp -= CheckBuildingTechLevelUp;
            
            _worldManager.OnNewMissionStart -= TryToActivateBonus;
        }

        public void TryToActivateNarrator(TutorialStep p_step)
        {
            if ((int)p_step == (int)_currentTutorialStep + 1)
            {
                _currentTutorialStep = p_step;
                _currentSubText = 0;
                OnTutorialAdvancement?.Invoke(true);
                Debug.Log("Quest: " + _currentTutorialStep);
            }
        }

        public void AddToSubtext()
        {
            _currentSubText++;
        }

        private void StartTutorial()
        {
            if (!_enableNarrator)
                return;
            
            TryToActivateNarrator(TutorialStep.OnGameStarted_Q1);
            
            _buildingsManager.OnTutorialStart -= StartTutorial;
        }
        
        private void CheckBuildingRepaired(Building p_building)
        {
            if (p_building.BuildingMainData.Type == BuildingType.Cottage)
            {
                TryToActivateNarrator(TutorialStep.OnCottageRepaired_Q6);
            
                _buildingsManager.OnBuildingRepaired -= CheckBuildingRepaired;
            }
        }
        
        private void CheckBuildingBuilt(Building p_building)
        {
            if (_currentTutorialStep != TutorialStep.OnFinishingFarmAvaiable_Q8 ||
                p_building.BuildingMainData.Type != BuildingType.Farm) 
                return;
            
            TryToActivateNarrator(TutorialStep.OnFinishedFarm_Q9);
            _buildingsManager.OnBuildingStateChanged -= CheckBuildingBuilt;
        }
        
        private void CheckBuildingTechLevelUp(Building p_building)
        {
            if (_currentTutorialStep != TutorialStep.OnFarmPanelWithTechnology_Q12 ||
                p_building.BuildingMainData.Type != BuildingType.Farm) 
                return;
            
            TryToActivateNarrator(TutorialStep.OnTechnologyInFarmLvlUp_Q13);
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

            if (_enableNarrator)
            {
                _currentTutorialStep = TutorialStep.OnGameStarted_Q1;
                OnTutorialAdvancement?.Invoke(true);
            }
            else
            {
                _currentTutorialStep = TutorialStep.Quests_End;
                OnTutorialAdvancement?.Invoke(false);
            }
        }

        public bool ShouldBlockBuildingTab()
        {
            return _currentTutorialStep is TutorialStep.OnFourthWorkingPanelOpen_Q11 or TutorialStep.OnTechnologyInFarmLvlUp_Q13
                or TutorialStep.OnFarmPanelClosed_Q15;
        }
        
        public bool ShouldBlockResource()
        {
            return _currentTutorialStep is TutorialStep.OnFirstWorkingPanelOpen_Q2 
                or TutorialStep.OnSecondWorkingPanelOpen_Q3 or TutorialStep.OnThirdWorkingPanelOpen_Q7 
                or TutorialStep.AfterRankUp_Q16;
        }
        
        public bool ShouldBlockDefense()
        {
            return _currentTutorialStep is TutorialStep.OnFirstWorkingPanelOpen_Q2
                or TutorialStep.OnSecondWorkingPanelOpen_Q3 or TutorialStep.OnThirdWorkingPanelOpen_Q7 
                or TutorialStep.OnFourthWorkingPanelOpen_Q11 or TutorialStep.OnTechnologyInFarmLvlUp_Q13 
                or TutorialStep.OnFarmPanelClosed_Q15;
        }
        
        public bool ShouldBlockBuildingPanelButton()
        {
            return _currentTutorialStep is TutorialStep.OnDaySkipped_Q5 or TutorialStep.OnFinishingFarmAvaiable_Q8 
                or TutorialStep.OnFinishedFarm_Q9 or TutorialStep.OnFourthWorkingPanelOpen_Q11 or TutorialStep.OnFarmPanelWithTechnology_Q12 
                or TutorialStep.OnFarmPanelClosed_Q15; 
        }
        
        public bool ShouldBlockSkipButton()
        {
            return _currentTutorialStep is TutorialStep.OnTechnologyInFarmLvlUp_Q13 or TutorialStep.OnFarmPanelClosed_Q15;
        }
        
        private void TryToActivateBonus()
        {
            if (_currentTutorialStep < TutorialStep.Quests_End)
                return;

            _buildingsManager.TryToActivateBonus();
            
            OnTutorialAdvancement?.Invoke(true);
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
        OnGameStarted_Q1 = 0,
        OnFirstWorkingPanelOpen_Q2 = 1,
        OnSecondWorkingPanelOpen_Q3 = 2,
        OnFirstDayStarted_Q4 = 3,
        OnDaySkipped_Q5 = 4,
        OnCottageRepaired_Q6 = 5,
        OnThirdWorkingPanelOpen_Q7 = 6,
        OnFinishingFarmAvaiable_Q8 = 7,
        OnFinishedFarm_Q9 = 8,
        OnFarmPanelOpen_Q10 = 9,
        OnFourthWorkingPanelOpen_Q11 = 10,
        OnFarmPanelWithTechnology_Q12 = 11,
        OnTechnologyInFarmLvlUp_Q13 = 12,
        OnFarmMinigameEnded_Q14 = 13,
        OnFarmPanelClosed_Q15 = 14,
        AfterRankUp_Q16 = 15,
        OnGuardTowerMinigameEnded_Q17 = 16,
        OnDefendPanelOpened_Q18 = 17,
        OnAfterDefendPanel_Q19 = 18,
        OnMissionRestart_Q20 = 19,
        OnWorkersPanelOpenAfterRestart_Q21 = 20,
        Quests_End = 21,
    }
}