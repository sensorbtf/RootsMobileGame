using System;
using System.Collections.Generic;
using AudioSystem;
using Buildings;
using GameManager;
using GeneralSystems;
using Narrator;
using UnityEngine;
using World;

namespace InGameUi
{
    public class DecisionMakingPanel : MonoBehaviour
    {
        [SerializeField] private AudioManager _audioManager;
        [SerializeField] private NarratorManager _narratorManager;
        [SerializeField] private MainGameManager _gameManager;
        [SerializeField] private WorldManager _worldManager;
        private DecisionMakingRefs _uiReferences;

        private void Start()
        {
            _worldManager.OnResourcesRequirementsMeet += ViewResourcesMetPanel;
            _worldManager.OnLeaveDecision += ViewLeavePanel;
            _worldManager.OnStormCame += ViewStormConsequencesPanel;
            _gameManager.OnPlayerCameBack += ViewWelcomeBackPanel;
            
            _uiReferences = gameObject.GetComponent<DecisionMakingRefs>();
            gameObject.SetActive(false);
        }

        private void OnDestroy()
        {
            _worldManager.OnResourcesRequirementsMeet -= ViewResourcesMetPanel;
            _worldManager.OnLeaveDecision -= ViewLeavePanel;
            _worldManager.OnStormCame -= ViewStormConsequencesPanel;
            _gameManager.OnPlayerCameBack -= ViewWelcomeBackPanel;
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
                _uiReferences.YesButton.onClick.AddListener(delegate
                {
                    _audioManager.PlayButtonSoundEffect(_uiReferences.YesButton.interactable);
                    HandleDestinyShardsGift();
                });
                _uiReferences.YesButtonText.text = $"Collect {_gameManager.GetDailyReward} Destiny Shards";
            }
            else
            {
                _uiReferences.YesButton.onClick.AddListener(delegate
                {
                    _audioManager.PlayButtonSoundEffect(_uiReferences.YesButton.interactable);
                    HandleTurnOnOff(false);
                });
                _uiReferences.YesButtonText.text = "Continue";
            }
        }

        private void ViewStormConsequencesPanel(List<BuildingType> p_destroyedBuildings, bool p_won)
        {
            HandleTurnOnOff(true);
            
            _uiReferences.Title.text = "Destroyed Buildings";
            _uiReferences.Description.text = "";

            foreach (var buildingType in p_destroyedBuildings) 
                _uiReferences.Description.text += buildingType + "\n";

            var hasCompletedMission = p_won && _worldManager.AreResourcesEnough();

            _uiReferences.YesButtonText.text = hasCompletedMission ? "Start New Mission" : "Try Again";
            _uiReferences.YesButton.onClick.AddListener(delegate
            {
                _audioManager.PlayButtonSoundEffect(_uiReferences.YesButton.interactable);
                DealWithStormEffects(hasCompletedMission);
            });
            _uiReferences.YesButton.interactable = true;
            
            _uiReferences.YesButtonGo.gameObject.SetActive(true);
            _uiReferences.NoButtonGo.gameObject.SetActive(false);
            
            _narratorManager.TryToActivateNarrator(TutorialStep.OnAfterDefendPanel_Q19);
        }

        private void ViewLeavePanel()
        {
            HandleTurnOnOff(true);

            _uiReferences.Title.text = "You left your settlement unprotected";
            _uiReferences.Description.text =
                "As usual, monsters came with storm and started to demolish everything on their way";
            
            _uiReferences.NoButtonGo.gameObject.SetActive(true);
            _uiReferences.YesButtonGo.gameObject.SetActive(true);
            
            _uiReferences.YesButton.onClick.AddListener(delegate
            {
                _audioManager.PlayButtonSoundEffect(_uiReferences.YesButton.interactable);
                HandleLeaveEffects(true);
            });
            _uiReferences.YesButtonText.text = "Continue";
            _uiReferences.NoButton.onClick.AddListener(delegate
            {
                _audioManager.PlayButtonSoundEffect(_uiReferences.NoButton.interactable);
                HandleLeaveEffects(false);
            });
            _uiReferences.NoButtonText.text = "Offer Destiny Shards for least damages";
        }

        private void ViewResourcesMetPanel()
        {
            HandleTurnOnOff(true);

            _uiReferences.Title.text = "You gathered enough resources";
            _uiReferences.Description.text = "Do you want to leave earlier?";

            _uiReferences.NoButtonGo.gameObject.SetActive(true);
            _uiReferences.YesButtonGo.gameObject.SetActive(true);
            
            _uiReferences.YesButton.onClick.AddListener(delegate
            {
                _audioManager.PlayButtonSoundEffect(_uiReferences.YesButton.interactable);
                HandleLeaveDecision(true);
            });
            _uiReferences.YesButtonText.text = "Yes, leave everything for monsters";
            _uiReferences.NoButton.onClick.AddListener(delegate
            {
                _audioManager.PlayButtonSoundEffect(_uiReferences.NoButton.interactable);
                HandleLeaveDecision(false);
            });
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
            _narratorManager.TryToActivateNarrator(TutorialStep.OnMissionRestart_Q20);
            HandleTurnOnOff(false);
        }
        private void HandleDestinyShardsGift()
        {
            _gameManager.HandleLoginReward();
            _uiReferences.YesButton.onClick.RemoveAllListeners();
            _uiReferences.YesButton.onClick.AddListener(delegate
            {
                _audioManager.PlayButtonSoundEffect(_uiReferences.YesButton.interactable);
                HandleTurnOnOff(false);
            });
            _uiReferences.YesButtonText.text = "Continue";
        }

        private void HandleTurnOnOff(bool p_setting)
        {
            gameObject.SetActive(p_setting);
            GameplayHud.BlockHud = p_setting;
            CameraController.IsUiOpen = p_setting;
        }
        
    }
}