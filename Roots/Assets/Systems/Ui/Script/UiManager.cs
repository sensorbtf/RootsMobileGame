using System;
using InGameUi;
using Narrator;
using UnityEngine;

namespace InGameUi
{
    public class UiManager: MonoBehaviour
    {
        [SerializeField] private BuildingDefendPanel _buildingDefendPanel;
        [SerializeField] private BuildingPanel _buildingPanel;
        [SerializeField] private DecisionMakingPanel _decisionMakingPanel;
        [SerializeField] private GatheringDefensePanel _gatheringDefensePanel;
        [SerializeField] private GodsPanel _godsPanel;
        [SerializeField] private MinigamesPanel _minigamesPanel;
        [SerializeField] private NewDaySummary _newDaySummaryPanel;
        [SerializeField] private SettingsPanel _settingsPanel;
        [SerializeField] private SpecificBuildingPanel _specificBuildingPanel;
        [SerializeField] private WorkersPanel _workersPanel;

        public bool IsAnyPanelOpen()
        {
            return _buildingDefendPanel.isActiveAndEnabled ||
                   _buildingPanel.isActiveAndEnabled ||
                   _decisionMakingPanel.isActiveAndEnabled ||
                   _gatheringDefensePanel.isActiveAndEnabled ||
                   _godsPanel.isActiveAndEnabled ||
                   _minigamesPanel.isActiveAndEnabled ||
                   _newDaySummaryPanel.isActiveAndEnabled ||
                   _settingsPanel.isActiveAndEnabled ||
                   _specificBuildingPanel.isActiveAndEnabled ||
                   _workersPanel.isActiveAndEnabled;
        }
    }
}