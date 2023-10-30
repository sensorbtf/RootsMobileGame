using System.Collections.Generic;
using Buildings;
using UnityEngine;
using World;

namespace InGameUi
{
    public class DecisionMakingPanel : MonoBehaviour
    {
        [SerializeField] private WorldManager _worldManager;
        private DecisionMakingRefs _uiReferences;

        private void Start()
        {
            _worldManager.OnResourcesRequirementsMeet += ViewResourcesMetPanel;
            _worldManager.OnLeaveDecision += ViewLeavePanel;
            _worldManager.OnStormCame += ViewStormConsequencesPanel;
            _uiReferences = gameObject.GetComponent<DecisionMakingRefs>();

            gameObject.SetActive(false);
        }

        private void ViewStormConsequencesPanel(List<BuildingType> p_destroyedBuildings, bool p_won)
        {
            gameObject.SetActive(true);
            GameplayHud.BlockHud = true;

            _uiReferences.Title.text = "Destroyed Buildings";
            _uiReferences.Description.text = "";

            foreach (var buildingType in p_destroyedBuildings) _uiReferences.Description.text += buildingType + "\n";

            _uiReferences.YesButtonText.text = p_won ? "Start New Mission" : "Try Again";

            _uiReferences.YesButton.onClick.AddListener(() => DealWithStormEffects(p_won));
            _uiReferences.NoButton.interactable = false;
            _uiReferences.NoButtonText.text = "Not Available";
        }

        private void ViewLeavePanel()
        {
            gameObject.SetActive(true);
            GameplayHud.BlockHud = true;

            _uiReferences.Title.text = "You left your settlement unprotected";
            _uiReferences.Description.text =
                "As usual, monsters came with storm and started to demolish everything on their way";

            _uiReferences.YesButton.onClick.AddListener(() => HandleLeaveEffects(true));
            _uiReferences.YesButtonText.text = "Continue";
            _uiReferences.NoButton.onClick.AddListener(() => HandleLeaveEffects(false));
            _uiReferences.NoButtonText.text = "Offer Destiny Shards for least damages";
        }

        private void ViewResourcesMetPanel()
        {
            gameObject.SetActive(true);
            GameplayHud.BlockHud = true;

            _uiReferences.Title.text = "You gathered enough resources";
            _uiReferences.Description.text = "Do you want to leave earlier?";

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

            gameObject.SetActive(false);
            GameplayHud.BlockHud = false;
        }

        private void HandleLeaveEffects(bool p_continueWithoutDSSpent)
        {
            if (p_continueWithoutDSSpent)
                _worldManager.EndMission(true, false);
            else
                _worldManager.EndMission(true, false);

            gameObject.SetActive(false);
            GameplayHud.BlockHud = false;
        }

        private void DealWithStormEffects(bool p_won)
        {
            _worldManager.StartMission(p_won);
            gameObject.SetActive(false);
            GameplayHud.BlockHud = false;
        }
    }
}