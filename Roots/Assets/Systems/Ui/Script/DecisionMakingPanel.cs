using System.Collections.Generic;
using Buildings;
using GameManager;
using GeneralSystems;
using UnityEngine;
using World;

namespace InGameUi
{
    public class DecisionMakingPanel : MonoBehaviour
    {
        [SerializeField] private MainGameManager _gameManager;
        [SerializeField] private WorldManager _worldManager;
        private DecisionMakingRefs _uiReferences;

        private void Start()
        {
            _worldManager.OnResourcesRequirementsMeet += ViewResourcesMetPanel;
            _worldManager.OnLeaveDecision += ViewLeavePanel;
            _worldManager.OnStormCame += ViewStormConsequencesPanel;
            _uiReferences = gameObject.GetComponent<DecisionMakingRefs>();
            _gameManager.OnPlayerCameBack += ViewWelcomeBackPanel;
            gameObject.SetActive(false);
        }
        
        private void ViewWelcomeBackPanel(bool p_shouldOfferLoginGift)
        {
            HandleTurnOnOff(true);
            
            _uiReferences.Title.text = "Welcome back";
            _uiReferences.Description.text =
                $"Absence time: {_gameManager.HoursOfAbstence} hours. You were granted with {_gameManager.FreeSkipsGotten}/{_gameManager.MaxFreeSkipsAmount} free skips \n Now you have {_gameManager.FreeSkipsLeft} free skips left";

            _uiReferences.NoButtonGo.gameObject.SetActive(false);
            _uiReferences.YesButtonGo.gameObject.SetActive(true);

            if (p_shouldOfferLoginGift)
            {
                _uiReferences.YesButton.onClick.AddListener(() => HandleDestinyShardsGift());
                _uiReferences.YesButtonText.text = $"Collect {_gameManager.GetDailyReward} Destiny Shards";
            }
            else
            {
                _uiReferences.YesButton.onClick.AddListener(() => HandleTurnOnOff(false));
                _uiReferences.YesButtonText.text = "Continue";
            }
        }

        private void ViewStormConsequencesPanel(List<BuildingType> p_destroyedBuildings, bool p_won)
        {
            HandleTurnOnOff(true);
            
            _uiReferences.Title.text = "Destroyed Buildings";
            _uiReferences.Description.text = "";

            foreach (var buildingType in p_destroyedBuildings) _uiReferences.Description.text += buildingType + "\n";

            _uiReferences.YesButtonText.text = p_won ? "Start New Mission" : "Try Again";
            _uiReferences.YesButton.onClick.AddListener(() => DealWithStormEffects(p_won));
            _uiReferences.YesButton.interactable = true;
            
            _uiReferences.YesButtonGo.gameObject.SetActive(true);
            _uiReferences.NoButtonGo.gameObject.SetActive(false);
        }

        private void ViewLeavePanel()
        {
            HandleTurnOnOff(true);

            _uiReferences.Title.text = "You left your settlement unprotected";
            _uiReferences.Description.text =
                "As usual, monsters came with storm and started to demolish everything on their way";
            
            _uiReferences.NoButtonGo.gameObject.SetActive(true);
            _uiReferences.YesButtonGo.gameObject.SetActive(true);
            
            _uiReferences.YesButton.onClick.AddListener(() => HandleLeaveEffects(true));
            _uiReferences.YesButtonText.text = "Continue";
            _uiReferences.NoButton.onClick.AddListener(() => HandleLeaveEffects(false));
            _uiReferences.NoButtonText.text = "Offer Destiny Shards for least damages";
        }

        private void ViewResourcesMetPanel()
        {
            HandleTurnOnOff(true);

            _uiReferences.Title.text = "You gathered enough resources";
            _uiReferences.Description.text = "Do you want to leave earlier?";

            _uiReferences.NoButtonGo.gameObject.SetActive(true);
            _uiReferences.YesButtonGo.gameObject.SetActive(true);
            
            _uiReferences.YesButton.onClick.AddListener(() => HandleLeaveDecision(true));
            _uiReferences.YesButtonText.text = "Yes, leave everything for monsters";
            _uiReferences.NoButton.onClick.AddListener(() => HandleLeaveDecision(false));
            _uiReferences.NoButtonText.text = "No, keep working.";
        }

        private void HandleLeaveDecision(bool p_wantToLeave)
        {
            if (p_wantToLeave)
                _worldManager.LeaveMission();
            else
                _worldManager.HandleNewDayStarted(false);

            HandleTurnOnOff(false);
        }

        private void HandleLeaveEffects(bool p_continueWithoutDSSpent)
        {
            if (p_continueWithoutDSSpent)
                _worldManager.EndMission(true, false);
            else
                _worldManager.EndMission(true, false);

            HandleTurnOnOff(false);
        }

        private void DealWithStormEffects(bool p_won)
        {
            _worldManager.StartMission(p_won);

            HandleTurnOnOff(false);
        }

        private void HandleTurnOnOff(bool p_setting)
        {
            gameObject.SetActive(p_setting);
            GameplayHud.BlockHud = p_setting;
            CameraController.IsUiOpen = p_setting;
        }
        
        private void HandleDestinyShardsGift()
        {
            _gameManager.HandleLoginReward();
            _uiReferences.YesButton.onClick.AddListener(() => HandleTurnOnOff(false));
            _uiReferences.YesButtonText.text = "Continue";
        }
    }
}