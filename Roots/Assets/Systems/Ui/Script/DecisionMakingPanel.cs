using System;
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
            _worldManager.OnResourcesRequirementsMeet += ViewResourcesMetCondition;
            _uiReferences = gameObject.GetComponent<DecisionMakingRefs>();
            
            gameObject.SetActive(false);
        }

        private void ViewResourcesMetCondition()
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
                _worldManager.EndMission();
            }
            else
            {
                _worldManager.HandleNewDayStarted();
            }
            
            gameObject.SetActive(false);
        }
    }
}