using System;
using System.Collections.Generic;
using AudioSystem;
using Buildings;
using GameManager;
using GeneralSystems;
using Narrator;
using UnityEngine;
using UnityEngine.Localization;
using World;

namespace InGameUi
{
    public class DecisionMakingPanel : MonoBehaviour
    {
        [Header("System Refs")]
        [SerializeField] private AudioManager _audioManager;
        [SerializeField] private BuildingsManager _buildingManager;
        [SerializeField] private NarratorManager _narratorManager;
        [SerializeField] private MainGameManager _gameManager;
        [SerializeField] private WorldManager _worldManager;
        [SerializeField] private AdsForRewards _rewardingAdsManager;
        [SerializeField] private int _amountOfStarForLeaving;

        [Header("Localization")] 
        [SerializeField] private LocalizedString _advertisement;
        [SerializeField] private LocalizedString _advertInfo;
        [SerializeField] private LocalizedString _advertWatch;
        [SerializeField] private LocalizedString _advertBack;
        [SerializeField] private LocalizedString _welcomeBack;
        [SerializeField] private LocalizedString _abstenceText;
        [SerializeField] private LocalizedString _collectStarDust;
        [SerializeField] private LocalizedString _continue;
        [SerializeField] private LocalizedString _destroyedBuildings;
        [SerializeField] private LocalizedString _newMission;
        [SerializeField] private LocalizedString _tryAgain;
        [SerializeField] private LocalizedString _leftSettlementUnprotected;
        [SerializeField] private LocalizedString _monstersCameAndDestroyed;
        [SerializeField] private LocalizedString _starDustLoweringDamage;
        [SerializeField] private LocalizedString _enoughResourcesGathered;
        [SerializeField] private LocalizedString _leavingEalierText;
        [SerializeField] private LocalizedString _leavingEalierTextDesc;
        [SerializeField] private LocalizedString _leavingEalierConfirm;
        [SerializeField] private LocalizedString _leavingEalierRefuse;
        
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

        public void AdvertisementAlert()
        {
            HandleTurnOnOff(true);

            _uiReferences.Title.text = _advertisement.GetLocalizedString();
            _uiReferences.Description.text = _advertInfo.GetLocalizedString();

            _uiReferences.NoButtonGo.gameObject.SetActive(true);
            _uiReferences.YesButtonGo.gameObject.SetActive(true);
            
            _uiReferences.YesButtonText.text = _advertWatch.GetLocalizedString();
            _uiReferences.NoButtonText.text = _advertBack.GetLocalizedString();

            _uiReferences.YesButton.onClick.AddListener(delegate
            {
                _audioManager.PlayButtonSoundEffect(_uiReferences.YesButton.interactable);
                _rewardingAdsManager.ShowRewardedAdd();
                HandleTurnOnOff(false);
            });
            
            _uiReferences.NoButton.onClick.AddListener(delegate
            {
                _audioManager.PlayButtonSoundEffect(_uiReferences.YesButton.interactable);
                HandleTurnOnOff(false);
            });
        }

        private void ViewWelcomeBackPanel(bool p_shouldOfferLoginGift)
        {
            HandleTurnOnOff(true);
            
            _uiReferences.Title.text = _welcomeBack.GetLocalizedString();
            var text = string.Format(_abstenceText.GetLocalizedString(), _gameManager.HoursOfAbstence,
                _gameManager.FreeSkipsGotten, _gameManager.FreeSkipsLeft, _gameManager.MaxFreeSkipsAmount);
            _uiReferences.Description.text = text;
            
            _uiReferences.NoButtonGo.gameObject.SetActive(false);
            _uiReferences.YesButtonGo.gameObject.SetActive(true);

            if (p_shouldOfferLoginGift)
            {
                _uiReferences.YesButton.onClick.AddListener(delegate
                {
                    _audioManager.PlayButtonSoundEffect(_uiReferences.YesButton.interactable);
                    HandleDestinyShardsGift();
                });

                _uiReferences.YesButtonText.text = string.Format(_collectStarDust.GetLocalizedString(), _gameManager.GetDailyReward);
            }
            else
            {
                _uiReferences.YesButton.onClick.AddListener(delegate
                {
                    _audioManager.PlayButtonSoundEffect(_uiReferences.YesButton.interactable);
                    HandleTurnOnOff(false);
                });
                _uiReferences.YesButtonText.text = _continue.GetLocalizedString();
            }
        }

        private void ViewStormConsequencesPanel(List<BuildingData> p_destroyedBuildings, bool p_won)
        {
            HandleTurnOnOff(true);
            
            _uiReferences.Title.text = _destroyedBuildings.GetLocalizedString();
            _uiReferences.Description.text = "";

            foreach (var building in p_destroyedBuildings) 
                _uiReferences.Description.text += building.BuildingName.GetLocalizedString() + "\n";

            var hasCompletedMission = p_won && _worldManager.AreResourcesEnough();

            _uiReferences.YesButtonText.text = hasCompletedMission ? _newMission.GetLocalizedString() : _tryAgain.GetLocalizedString();
            _uiReferences.YesButton.onClick.AddListener(delegate
            {
                _audioManager.PlayButtonSoundEffect(_uiReferences.YesButton.interactable);
                DealWithStormEffects(hasCompletedMission);
            });
            _uiReferences.YesButton.interactable = true;
            _uiReferences.YesButtonText.text = _continue.GetLocalizedString();
            _uiReferences.YesButtonGo.gameObject.SetActive(true);
            _uiReferences.NoButtonGo.gameObject.SetActive(false);
            
            _narratorManager.TryToActivateNarrator(TutorialStep.OnAfterDefendPanel_Q19);
        }

        private void ViewLeavePanel()
        {
            HandleTurnOnOff(true);

            _uiReferences.Title.text = _leftSettlementUnprotected.GetLocalizedString();
            _uiReferences.Description.text = _monstersCameAndDestroyed.GetLocalizedString();
            
            _uiReferences.NoButtonGo.gameObject.SetActive(true);
            _uiReferences.YesButtonGo.gameObject.SetActive(true);
            
            _uiReferences.YesButton.onClick.AddListener(delegate
            {
                _audioManager.PlayButtonSoundEffect(_uiReferences.YesButton.interactable);
                HandleLeaveEffects(true);
            });
            
            _uiReferences.YesButtonText.text = _continue.GetLocalizedString();
            
            _uiReferences.NoButton.onClick.AddListener(delegate
            {
                _audioManager.PlayButtonSoundEffect(_uiReferences.NoButton.interactable);
                _buildingManager.HandlePointsManipulation(PointsType.StarDust, _amountOfStarForLeaving, false);
                HandleLeaveEffects(false);
            });
            
            _uiReferences.NoButtonText.text = string.Format(_starDustLoweringDamage.GetLocalizedString(), _amountOfStarForLeaving);
        }

        private void ViewResourcesMetPanel()
        {
            HandleTurnOnOff(true);

            _uiReferences.Title.text = _enoughResourcesGathered.GetLocalizedString();
            _uiReferences.Description.text = _leavingEalierTextDesc.GetLocalizedString();

            _uiReferences.NoButtonGo.gameObject.SetActive(true);
            _uiReferences.YesButtonGo.gameObject.SetActive(true);
            
            _uiReferences.YesButton.onClick.AddListener(delegate
            {
                _audioManager.PlayButtonSoundEffect(_uiReferences.YesButton.interactable);
                HandleLeaveDecision(true);
            });
            _uiReferences.YesButtonText.text = _leavingEalierConfirm.GetLocalizedString();
            
            _uiReferences.NoButton.onClick.AddListener(delegate
            {
                _audioManager.PlayButtonSoundEffect(_uiReferences.NoButton.interactable);
                HandleLeaveDecision(false);
            });
            
            _uiReferences.NoButtonText.text = _leavingEalierRefuse.GetLocalizedString();
        }

        private void HandleLeaveDecision(bool p_wantToLeave)
        {
            if (p_wantToLeave)
                _worldManager.LeaveMission();
            else
            {
                HandleTurnOnOff(false);
                _worldManager.HandleNewDayStarted(false);
            }
        }

        private void HandleLeaveEffects(bool p_continueWithoutDSSpent)
        {
            if (p_continueWithoutDSSpent)
                _worldManager.EndMission(true, true);
            else
                _worldManager.EndMission(true, false);
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
            _uiReferences.YesButtonText.text = _continue.GetLocalizedString();
        }

        private void HandleTurnOnOff(bool p_turnOffPanel)
        {
            gameObject.SetActive(p_turnOffPanel);
            GameplayHud.BlockHud = p_turnOffPanel;
            CameraController.IsUiOpen = p_turnOffPanel;
            
            _uiReferences.YesButton.onClick.RemoveAllListeners();
            _uiReferences.NoButton.onClick.RemoveAllListeners();
        }
    }
}