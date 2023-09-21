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
    
    private void Start()
    {
        _specificBuildingPanel.OpenMiniGameOfType += OpenRightMinigame;
    }

    private void OpenRightMinigame(BuildingType p_obj)
    {
        throw new NotImplementedException();
    }
}
