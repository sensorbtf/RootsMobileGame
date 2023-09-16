using System;
using System.Collections.Generic;
using Buildings;
using UnityEngine;
using World;

namespace InGameUi
{
    public class DecisionMakingPanel: MonoBehaviour
    {
        [SerializeField] private WorldManager _worldManager;
        private DecisionMakingRefs _uiReferences;
        
        private void Start()
        {
            _worldManager.OnResourcesRequirementsMeet += ViewResourcesMetPanel;
            _worldManager.OnLeaveDecision += ViewLeavePanel;
            _worldManager.OnStormWon += ViewStormConsequencesPanel;
            _uiReferences = gameObject.GetComponent<DecisionMakingRefs>();
            
            gameObject.SetActive(false);
        }
        
        //Add panel of BeforeStormWorkersAssignig

        private void ViewStormConsequencesPanel(List<BuildingType> p_destroyedBuildings)
        {
            gameObject.SetActive(true);

            _uiReferences.Title.text = "Destroyed Buildings";
            _uiReferences.Description.text = "";
            
            foreach (var buildingType in p_destroyedBuildings)
            {
                _uiReferences.Description.text += buildingType +", ";
            }
            
            _uiReferences.YesButton.onClick.AddListener(DealWithStormEffects);
            _uiReferences.YesButtonText.text = "Start New Mission";
            //_uiReferences.NoButton.onClick.AddListener(() => HandleLeaveEffects(false));
            _uiReferences.NoButtonText.text = "Wut";
        }
        
        private void ViewLeavePanel()
        {
            gameObject.SetActive(true);

            _uiReferences.Title.text = "You left your settlement unprotected";
            _uiReferences.Description.text = "As usual, monsters came with storm and started to demolish everything on their way";
            
            _uiReferences.YesButton.onClick.AddListener(() => HandleLeaveEffects(true));
            _uiReferences.YesButtonText.text = "Continue";
            _uiReferences.NoButton.onClick.AddListener(() => HandleLeaveEffects(false));
            _uiReferences.NoButtonText.text = "Offer Destiny Shards for least damages";
        }

        private void ViewResourcesMetPanel()
        {
            gameObject.SetActive(true);

            _uiReferences.Title.text = "You gathered enough resources";
            _uiReferences.Description.text = "Do you want to leave earlier?";
            
            _uiReferences.YesButton.onClick.AddListener(() => HandleLeaveDecision(true));
            _uiReferences.YesButtonText.text = "Yes, leave base for monsters";
            _uiReferences.NoButton.onClick.AddListener(() => HandleLeaveDecision(false));
            _uiReferences.NoButtonText.text = "No, keep working.";
        }

        private void HandleLeaveDecision(bool p_wantToLeave)
        {
            if (p_wantToLeave)
            {
                _worldManager.LeaveMission();
            }
            else
            {
                _worldManager.HandleNewDayStarted();
            }
            
            gameObject.SetActive(false);
        }
        
        private void HandleLeaveEffects(bool p_continueWithoutDSSpent)
        {
            if (p_continueWithoutDSSpent)
            {
                _worldManager.HandleStormWon(false);
            }
            else
            {
                _worldManager.HandleStormWon(true);
            }
            
            gameObject.SetActive(false);
        }
        
        private void DealWithStormEffects()
        {
            _worldManager.StartMission();
            gameObject.SetActive(false);
        }
    }
}