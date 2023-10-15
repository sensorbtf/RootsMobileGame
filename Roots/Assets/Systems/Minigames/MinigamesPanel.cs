using System;
using Buildings;
using InGameUi;
using UnityEngine;
using World;

namespace Minigames
{
    public class MinigamesPanel : MonoBehaviour
    {
        [SerializeField] private BuildingManager _buildingsManager;
        [SerializeField] private WorldManager _worldManager;
        [SerializeField] private SpecificBuildingPanel _specificBuildingPanel;
        [SerializeField] private GameObject _minigamesPanelGo;
        [SerializeField] private MinigamesPerBuildings[] _minigamesPerBuilding;
        private Building _currentBuilding;
        private GameObject _currentMinigame;
        private Minigame _currentMinigameScript;

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
                        _currentMinigame = Instantiate(rightMinigame, _minigamesPanelGo.transform);
                        _currentMinigameScript = _currentMinigame.GetComponent<Minigame>();
                        _currentMinigameScript.StartTheGame(p_building);
                        _currentMinigameScript.OnMiniGamePointsCollected += GoBackToSpecificPanel;

                        if (_currentMinigameScript is RightLeftClickingMinigame)
                        {
                            var watchTowerMinigame = _currentMinigameScript as RightLeftClickingMinigame;
                            watchTowerMinigame.OnStormReveal += RevealStorm;
                        }

                        break;
                    }
                }
            }
        }

        private void RevealStorm(int p_daysToSee)
        {
            _worldManager.RevealStorm(p_daysToSee);

            var watchTowerMinigame = _currentMinigameScript as RightLeftClickingMinigame;
            watchTowerMinigame.OnStormReveal -= RevealStorm;
        }

        private void GoBackToSpecificPanel(PointsType p_pointsType, int p_pointsNumber)
        {
            _minigamesPanelGo.SetActive(false);
            gameObject.SetActive(false);
            _specificBuildingPanel.ActivateOnClick(_currentBuilding);
            _buildingsManager.HandlePointsManipulation(p_pointsType, p_pointsNumber, true, true);
            _worldManager.HandleMinigamesResourcesQuests(p_pointsType, p_pointsNumber);


            Destroy(_currentMinigame);
        }

        [Serializable]
        public class MinigamesPerBuildings
        {
            public GameObject _minigame;
            public BuildingType[] _buildings;
        }
    }
}