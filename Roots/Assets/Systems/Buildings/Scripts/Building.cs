using System;
using GeneralSystems;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace Buildings
{
    public class Building : MonoBehaviour, IPointerClickHandler
    {
        public BuildingData BuildingMainData;
        public SpriteRenderer InGameIcon;
        public SpriteRenderer GatheringIcon;
        private int _currentLevel;
        private bool _haveWorker = false;
        private bool _isDamaged = false;
        [HideInInspector] public int CurrentDayOnQueue;
        [HideInInspector] public bool HaveSomethingToCollect = false;
        [HideInInspector] public bool IsBeeingUpgradedOrBuilded = false;
        [HideInInspector] public bool IsProtected = false;

        public int CurrentLevel
        {
            get => _currentLevel;
        }        
        
        public bool HaveWorker
        {
            get => _haveWorker;
            set => _haveWorker = value;
        }

        public bool IsDamaged
        {
            get => _isDamaged;
            set
            {
                if (value)
                {
                    IsBeeingUpgradedOrBuilded = false;
                    InGameIcon.color = Color.red;
                    OnBuildingDamaged?.Invoke(this);
                }
                else
                {
                    InGameIcon.color = Color.white;
                    OnWorkDone?.Invoke(this, false);
                }
                
                _isDamaged = value;
            }
        }
        
        public bool IsCanceled => IsBeeingUpgradedOrBuilded && !_haveWorker;
        
        public event Action<Building> OnBuildingClicked; 
        public event Action<PointsType, int> OnPointsGathered; 
        public event Action<Building, bool> OnWorkDone; 
        public event Action<Building> OnBuildingDamaged;
        public event Action<Building> OnBuildingDestroyed;

        public void OnPointerClick(PointerEventData p_eventData)
        {
            if (HaveSomethingToCollect)
            {
                if (BuildingMainData.PerLevelData[_currentLevel].CanRiseDefenses)
                {
                    OnPointsGathered?.Invoke(PointsType.Defense, BuildingMainData.PerLevelData[_currentLevel].DefensePointsPerDay);
                }
                else if (BuildingMainData.PerLevelData[_currentLevel].CanProduce)
                {
                    OnPointsGathered?.Invoke(PointsType.Resource, BuildingMainData.PerLevelData[_currentLevel].ProductionPerDay);
                }
                else
                {
                    Debug.LogError("ERROR. Clicking building with points that its not producing!");    
                }

                GatheringIcon.sprite = null;
                HaveSomethingToCollect = false;
                return;
            }
            
            if (!CameraController.isDragging)
            {
                OnBuildingClicked?.Invoke(this);
            }
        }
        
        
        public void FinishBuildingSequence()
        {
            _currentLevel = 1;
            IsBeeingUpgradedOrBuilded = false;
            InGameIcon.sprite = BuildingMainData.PerLevelData[_currentLevel].InGameSprite;
            _haveWorker = false;
            
            OnWorkDone?.Invoke(this, false);
        }
        
        public void InitiateBuildingSequence()
        {
            _currentLevel = 0;
            CurrentDayOnQueue = 0;
            IsBeeingUpgradedOrBuilded = true;
        }
        
        public void HandleLevelUp()
        {
            _currentLevel++;
            IsBeeingUpgradedOrBuilded = false;
            CurrentDayOnQueue = 0;
            InGameIcon.sprite = BuildingMainData.PerLevelData[_currentLevel].InGameSprite;
            _haveWorker = false;
            
            OnWorkDone?.Invoke(this, false);
        }

        public void InitiateUpgradeSequence()
        {
            CurrentDayOnQueue = 0;
            IsBeeingUpgradedOrBuilded = true;
        }
        
        public void SetCollectionIcon(Sprite p_gatheringIcon)
        {
            GatheringIcon.sprite = p_gatheringIcon;
            HaveSomethingToCollect = true;
            _haveWorker = false;
        }
    }
}
