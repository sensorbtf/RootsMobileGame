using System;
using GeneralSystems;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Buildings
{
    [Serializable]
    public class Building : MonoBehaviour, IPointerClickHandler
    {
        public BuildingData BuildingMainData;
        private int _currentLevel;
        private bool _hasWorker = false;
        private bool _isUpgradedOrInBuilding = false;

        public int CurrentLevel
        {
            get => _currentLevel;
            set => _currentLevel = value;
        }        
        
        public bool HasWorker
        {
            get => _hasWorker;
            set => _hasWorker = value;
        }

        public Building(BuildingData p_buildingData)
        {
            BuildingMainData = p_buildingData;
        }

        public static event Action<BuildingData, int> OnBuildingClicked; 

        private void Start()
        {
            if (BuildingMainData != null)
            {
                CurrentLevel = 1; 
            }
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (!CameraController.isDragging)
            {
                OnBuildingClicked?.Invoke(BuildingMainData, CurrentLevel);
            }
        }
    }
}
