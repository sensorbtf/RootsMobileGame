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
    private GameObject _currentMinigame;
    
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
        
        // check what sort of minigame should activate
        // check what will I get from specific building resources/defense points/both
        // select right minigame and type
        
        if (p_building.BuildingMainData.Type == BuildingType.Woodcutter)
        {
            _currentMinigame = Instantiate(_woodcutterMinigame, _minigamesPanelGo.transform);
            var script = _currentMinigame.GetComponent<ClickerMinigame>();
            
            script.StartTheGame(p_building);
            script.OnResourcesCollected += GoBackToSpecificPanel;
        }
    }

    private void GoBackToSpecificPanel()
    {
        _minigamesPanelGo.SetActive(false);
        gameObject.SetActive(false);
        _specificBuildingPanel.ActivateOnClick(_currentBuilding);
        Destroy(_currentMinigame);
    }
}
