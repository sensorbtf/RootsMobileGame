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
        [HideInInspector] public int CurrentTechnologyDayOnQueue;
        [HideInInspector] public bool HaveSomethingToCollect = false;
        [HideInInspector] public bool IsBeeingUpgradedOrBuilded = false;
        [HideInInspector] public bool IsProtected = false;
        [HideInInspector] public int CurrentTechnologyLvl = 0;

        private BaseDataPerLevel _currentLevelData => BuildingMainData.PerLevelData[_currentLevel];
        
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
                    CurrentDayOnQueue = 0;
                    OnBuildingDamaged?.Invoke(this);
                }
                else
                {
                    InGameIcon.color = Color.white;
                    CurrentDayOnQueue = 0;
                    OnWorkDone?.Invoke(this, false);
                }
                
                _isDamaged = value;
            }
        }

        public bool ProducesDefensePoints => _currentLevelData.CanRiseDefenses;
        public bool ProducesResourcePoints => _currentLevelData.CanProduce;
        public bool ProducesDefenseAndResourcePoints => _currentLevelData.CanRiseDefenses && _currentLevelData.CanProduce;

        public bool PlayedMinigame;

        public event Action<Building> OnBuildingClicked; 
        public event Action<PointsType, int> OnPointsGathered; 
        public event Action<Building, bool> OnWorkDone; 
        public event Action<Building> OnBuildingDamaged;
        public event Action<Building> OnBuildingDestroyed;

        public void OnPointerClick(PointerEventData p_eventData)
        {
            if (HaveSomethingToCollect)
            {
                if (_currentLevelData.CanRiseDefenses)
                {
                    OnPointsGathered?.Invoke(PointsType.Defense, _currentLevelData.DefensePointsPerDay);
                }
                else if (_currentLevelData.CanProduce)
                {
                    OnPointsGathered?.Invoke(PointsType.Resource, _currentLevelData.ProductionPerDay);
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
            InGameIcon.sprite = _currentLevelData.InGameSprite;
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
            InGameIcon.sprite = _currentLevelData.InGameSprite;
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

        public void UpgradeTechnologyLevel()
        {
            CurrentTechnologyLvl++;
            CurrentTechnologyDayOnQueue = 0;
        }

        public bool CanPlayMinigame()
        {
            return _haveWorker && !PlayedMinigame && !_isDamaged && !IsBeeingUpgradedOrBuilded;
        }

        public void HandleNewDay()
        {
            PlayedMinigame = false;
            CurrentDayOnQueue++;
            
            if (_isDamaged)
            {
                if (CurrentDayOnQueue < 1)
                    return;

                IsDamaged = false;
            }
            
            if (IsBeeingUpgradedOrBuilded)
            {
                if (CurrentDayOnQueue < _currentLevelData.Requirements.DaysToComplete)
                    return;
                    
                if (CurrentLevel == 0)
                {
                    FinishBuildingSequence();
                }
                else
                {
                    HandleLevelUp();
                }
            }
        }
    }
}
