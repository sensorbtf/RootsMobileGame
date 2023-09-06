using System;
using GeneralSystems;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;

namespace Buildings
{
    [Serializable]
    public class Building : MonoBehaviour, IPointerClickHandler
    {
        public BuildingData BuildingMainData;
        public GameObject BuildingInBuildStage;
        public int CurrentDayOnQueue;
        private int _currentLevel;
        private bool _hasWorker = false;
        public bool IsBeeingUpgradedOrBuilded = false;

        public int CurrentLevel
        {
            get => _currentLevel;
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

        public void OnPointerClick(PointerEventData eventData)
        {
            if (!CameraController.isDragging)
            {
                OnBuildingClicked?.Invoke(BuildingMainData, CurrentLevel);
            }
        }

        public void HandleLevelUp()
        {
            _currentLevel++;
            IsBeeingUpgradedOrBuilded = false;
            CurrentDayOnQueue = 0;
            // prefab upgrade
        }
        
        public void InitiateBuildingSequence()
        {
            _currentLevel = 0;
            CurrentDayOnQueue = 0;
            IsBeeingUpgradedOrBuilded = true;
            BuildingInBuildStage.SetActive(true);
        }

        public void InitiateUpgradeSequence()
        {
            CurrentDayOnQueue = 0;
            IsBeeingUpgradedOrBuilded = true;
            Destroy(BuildingMainData.PerLevelData[_currentLevel].FinalPrefab);
            BuildingInBuildStage.SetActive(true);
        }
        
        public void FinishBuildingSequence()
        {
            _currentLevel++;
            IsBeeingUpgradedOrBuilded = false;
            Instantiate(BuildingMainData.PerLevelData[_currentLevel].FinalPrefab, gameObject.transform);
            BuildingInBuildStage.SetActive(false);
        }
    }
}
