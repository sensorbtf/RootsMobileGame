using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Buildings;
using InGameUi;
using UnityEngine;
using UnityEngine.UI;

public class MinigamesPanel : MonoBehaviour
{
    [SerializeField] private BuildingManager _buildingsManager;
    [SerializeField] private SpecificBuildingPanel _specificBuildingPanel;
    [SerializeField] private GameObject _minigamesPanelGo;
    [SerializeField] private MinigamesPerBuildings[] _minigamesPerBuilding;
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
        GameObject rightMinigame = null;

        foreach (var minigame in _minigamesPerBuilding)
        {
            if (rightMinigame != null)
                break;
            
            foreach (var building in minigame._buildings)
            {
                if (building == p_building.BuildingMainData.Type)
                {
                    rightMinigame = minigame._minigame;
                    break;
                }
            }
        }

        // check what sort of minigame should be activated
        // check what will I get from specific building resources/defense points/both
        // select right minigame and type
        // dziedzictwo dla minigierek, okreœlanie typu, co zapewniaj¹

        _currentMinigame = Instantiate(rightMinigame, _minigamesPanelGo.transform);
        var script = _currentMinigame.GetComponent<ClickerMinigame>();
            
        script.StartTheGame(p_building);
        script.OnResourcesCollected += GoBackToSpecificPanel;
    }

    private void GoBackToSpecificPanel(PointsType p_pointsType, int p_pointsNumber)
    {
        _minigamesPanelGo.SetActive(false);
        gameObject.SetActive(false);
        _specificBuildingPanel.ActivateOnClick(_currentBuilding);
        _buildingsManager.HandlePointsManipulation(p_pointsType, p_pointsNumber, true);
        Destroy(_currentMinigame);
    }


    [Serializable]
    public class MinigamesPerBuildings
    {
        public GameObject _minigame;
        public BuildingType[] _buildings;
    }
}
