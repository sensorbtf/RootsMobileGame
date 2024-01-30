using System;
using GeneralSystems;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Rendering.Universal;

namespace Buildings
{
    public class Building : MonoBehaviour, IPointerClickHandler
    {
        public BuildingData BuildingMainData;
        public SpriteRenderer InGameIcon;
        public SpriteRenderer GatheringIcon;
        public Light2D LightOfBuilding;

        [HideInInspector] public int CurrentDayOnQueue;
        [HideInInspector] public int CurrentTechnologyDayOnQueue;
        [HideInInspector] public bool HaveSomethingToCollect;
        [HideInInspector] public bool CanEndBuildingSequence;
        [HideInInspector] public bool IsBeeingUpgradedOrBuilded;
        [HideInInspector] public bool IsProtected;
        [HideInInspector] public int CurrentTechnologyLvl;
        private bool _isDamaged;

        private bool _shouldHighlight = false;
        private bool _reverseLighting = false;
        private GameObject _gatheringIconGo;
        private Vector3 _originalGatheringIconScale;

        private BaseDataPerLevel _currentLevelData => BuildingMainData.PerLevelData[CurrentLevel];

        public int CurrentLevel { get; set; }

        public bool HaveWorker { get; set; }

        public bool IsDamaged
        {
            get => _isDamaged;
            set
            {
                if (_isDamaged)
                {
                    if (!value)
                    {
                        SetInGameStage();
                        CurrentDayOnQueue = 0;
                        OnRepaired?.Invoke(this);
                    }
                }
                else
                {
                    if (value)
                    {
                        IsBeeingUpgradedOrBuilded = false;
                        SetDestroyStage();
                        CurrentDayOnQueue = 0;
                        OnBuildingDamaged?.Invoke(this);
                    }
                }

                _isDamaged = value;
            }
        }

        public PointsType ProductionType => _currentLevelData.ProductionType;

        public bool PlayedMinigame { get; set; } = false;

        public event Action<Building> OnBuildingClicked;
        public event Action<BuildingType, PointsType, int> OnPointsGathered;
        public event Action<Building, bool> OnWorkDone;
        public event Action<Building> OnRepaired;
        public event Action<Building> OnBuildingDamaged;
        public event Action<Building> OnTechnologyUpgrade;
        public event Action<Building> OnBuildingDestroyed;

        private void Start()
        {
            LightOfBuilding.intensity = 0;
            _gatheringIconGo = GatheringIcon.gameObject;
            _originalGatheringIconScale = _gatheringIconGo.transform.localScale;
        }

        public void OnPointerClick(PointerEventData p_eventData)
        {
            if (CameraController.isDragging)
                return;

            if (CanEndBuildingSequence)
            {
                if (CurrentLevel == 0)
                {
                    FinishBuildingSequence();
                }
                else
                {
                    if (_isDamaged)
                        IsDamaged = false;
                    else
                        HandleLevelUp();
                }

                RevokeLighting();

                CanEndBuildingSequence = false;
                return;
            }

            if (HaveSomethingToCollect)
            {
                OnPointsGathered?.Invoke(BuildingMainData.Type, ProductionType,
                    _currentLevelData.ProductionAmountPerDay);

                RevokeLighting();
                HaveSomethingToCollect = false;
                return;
            }

            if (!IsBeeingUpgradedOrBuilded)
                OnBuildingClicked?.Invoke(this);
        }

        public void FinishBuildingSequence()
        {
            CurrentLevel = 1;
            IsBeeingUpgradedOrBuilded = false;
            SetInGameStage();

            HaveWorker = false;
            
            OnWorkDone?.Invoke(this, false);
        }

        public void InitiateBuildingSequence()
        {
            CurrentLevel = 0;
            CurrentDayOnQueue = 0;
            IsBeeingUpgradedOrBuilded = true;
        }

        public void HandleLevelUp()
        {
            CurrentLevel++;
            IsBeeingUpgradedOrBuilded = false;
            CurrentDayOnQueue = 0;
            SetInGameStage();

            HaveWorker = false;

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
            _shouldHighlight = true;

            HaveSomethingToCollect = true;
            HaveWorker = false;
        }

        public void SetBuildingIcon(Sprite p_finishBuildingIcon)
        {
            GatheringIcon.sprite = p_finishBuildingIcon;
            _shouldHighlight = true;
        }

        public void UpgradeTechnologyLevel()
        {
            CurrentTechnologyLvl++;
            CurrentTechnologyDayOnQueue = 0;

            OnTechnologyUpgrade?.Invoke(this);
        }
        
        public bool CanUpgradeTechnology()
        {
            return BuildingMainData.Technology.DataPerTechnologyLevel[CurrentTechnologyLvl].WorksDayToAchieve ==
                   CurrentTechnologyDayOnQueue;
        }

        public bool CanPlayMinigame()
        {
            return  !PlayedMinigame && !_isDamaged && !IsBeeingUpgradedOrBuilded &&
                   CurrentTechnologyLvl > 0; //HaveWorker
        }

        public bool HandleNewDay()
        {
            CurrentDayOnQueue++;

            if (_isDamaged)
            {
                if (CurrentDayOnQueue < 1)
                    return false;

                CanEndBuildingSequence = true;
                return true;
            }

            if (IsBeeingUpgradedOrBuilded)
            {
                if (CurrentDayOnQueue < _currentLevelData.Requirements.DaysToComplete)
                    return false;

                CanEndBuildingSequence = true;
                return true;
            }

            return false;
        }

        public void TryToHighlight()
        {
            if (!_shouldHighlight)
                return;

            // Adjust light intensity
            float intensityChange = _reverseLighting ? -0.02f : 0.02f;
            LightOfBuilding.intensity += intensityChange;

            // Calculate a pulsing scale factor (e.g., oscillating between 0.8 and 1.2 if original scale is 1)
            float scaleMultiplier = 1 + Mathf.Sin(Time.time * 5f) * 0.2f; // Adjust 5f for speed, 0.2f for magnitude
            _gatheringIconGo.transform.localScale = _originalGatheringIconScale * scaleMultiplier;

            // Check and update the state for reversing lighting
            if (_reverseLighting && LightOfBuilding.intensity <= 0f)
            {
                _reverseLighting = false;
            }
            else if (!_reverseLighting && LightOfBuilding.intensity > 0.5f)
            {
                _reverseLighting = true;
            }
        }

        public void SetInGameStage()
        {
            InGameIcon.sprite = BuildingMainData.InGameSprite;
            LightOfBuilding.lightCookieSprite = BuildingMainData.InGameSprite;
        }

        public void SetFirstStage()
        {
            InGameIcon.sprite = BuildingMainData.FirstStageBuilding;
            LightOfBuilding.lightCookieSprite = BuildingMainData.FirstStageBuilding;
        }

        public void SetUpgradeStage()
        {
            InGameIcon.sprite = BuildingMainData.UpgradeStage;
            LightOfBuilding.lightCookieSprite = BuildingMainData.UpgradeStage;
        }

        public void SetDestroyStage()
        {
            InGameIcon.sprite = BuildingMainData.DestroyedStage;
            LightOfBuilding.lightCookieSprite = BuildingMainData.DestroyedStage;
        }
        
        private void RevokeLighting()
        {
            GatheringIcon.sprite = null;

            _shouldHighlight = false;
            _reverseLighting = false;

            LightOfBuilding.intensity = 0;
        }
    }
}