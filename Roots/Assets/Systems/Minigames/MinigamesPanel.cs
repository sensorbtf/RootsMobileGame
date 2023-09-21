using System;
using System.Collections;
using System.Collections.Generic;
using Buildings;
using InGameUi;
using UnityEngine;
using UnityEngine.UI;

public class MinigamesPanel : MonoBehaviour
{
    [SerializeField] private SpecificBuildingPanel _specificBuildingPanel;
    [SerializeField] private GameObject _minigamesPanelGo;
    [SerializeField] private GameObject _woodcutterMinigame;

    private Building _currentBuilding;
    
    private void Start()
    {
        _specificBuildingPanel.OpenMiniGameOfType += OpenRightMinigame;
        
        _minigamesPanelGo.SetActive(false);
        gameObject.SetActive(false);
    }

    private void OpenRightMinigame(Building p_building)
    {
        _minigamesPanelGo.SetActive(true);
        gameObject.SetActive(true);
        _currentBuilding = p_building;
        
        if (p_building.BuildingMainData.Type == BuildingType.Woodcutter)
        {
            var minigame = Instantiate(_woodcutterMinigame, _minigamesPanelGo.transform);
            var script = minigame.GetComponent<ClickerMinigame>();
            
            script.StartTheGame(_specificBuildingPanel.TechnologyData);

            script.OnResourcesCollected += GoBackToSpecificPanel;
        }
    }

    private void GoBackToSpecificPanel()
    {
        _minigamesPanelGo.SetActive(false);
        gameObject.SetActive(false);
        _specificBuildingPanel.ActivateOnClick(_currentBuilding);
    }
}
