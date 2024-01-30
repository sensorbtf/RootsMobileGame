using System;
using System.Collections.Generic;
using System.Linq;
using AudioSystem;
using Buildings;
using GameManager;
using GeneralSystems;
using Gods;
using TMPro;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.UI;

namespace InGameUi
{
    public class GodsPanel : MonoBehaviour
    {
        [Header("System Refs")]
        [SerializeField] private BuildingsManager _buildingsManager;
        [SerializeField] private GodsManager _godsManager;
        [SerializeField] private AudioManager _audioManager;
        [SerializeField] private MainGameManager _gameManager; 
        
        [Header("Refs")]
        [SerializeField] private GameObject _godPrefab;
        [SerializeField] private Transform _contentTransform;
        [SerializeField] private Button _goBackButton;
        [SerializeField] private TextMeshProUGUI _tabName;
        
        [Header("Audio Refs")]
        [SerializeField] private AudioClip _boughtEffect;
        [SerializeField] private AudioClip _deactivateEffect;
        [SerializeField] private AudioClip _nooneEffectActivation;
        [SerializeField] private AudioClip _smallEffectActivation;
        [SerializeField] private AudioClip _mediumEffectActivation;
        [SerializeField] private AudioClip _bigEffectActivation;

        [Header("Localization Refs")] 
        [SerializeField] private LocalizedString _deactivateBlessing;
        [SerializeField] private LocalizedString _activateBlessing;
        [SerializeField] private LocalizedString _deactivateOtherBlessings;
        [SerializeField] private LocalizedString _buyForText;
        [SerializeField] private LocalizedString _chooseBlessingLevel;
        [SerializeField] private LocalizedString _smallBlessingEfficiency;
        [SerializeField] private LocalizedString _mediumBlessingEfficiency;
        [SerializeField] private LocalizedString _bigBlessingEfficiency;
        
        
        private List<GameObject> _createdGodsInstances;
        
        public event Action OnGodsPanelOpened;
        public event Action OnBackToWorkersPanel;

        private void Start()
        {
            _createdGodsInstances = new List<GameObject>();
            _goBackButton.onClick.AddListener(BackToWorkerTab);
            gameObject.SetActive(false);
        }

        public void BackToWorkerTab()
        {
            _audioManager.PlayButtonSoundEffect(_goBackButton.interactable);
            
            ClosePanel();
            OnBackToWorkersPanel?.Invoke();
        }

        public void ActivatePanel()
        {
            OnGodsPanelOpened?.Invoke();
            gameObject.SetActive(true);
            GameplayHud.BlockHud = true;
            CameraController.IsUiOpen = true;

            foreach (var god in _godsManager.PlayerCurrentBlessings.Keys.ToList())
            {
                if (!_buildingsManager.CheckIfGodsBuildingIsBuilt(god.GodName))
                  continue;  
                
                var newGod = Instantiate(_godPrefab, _contentTransform);
                var newGodRef = newGod.GetComponent<GodInstanceUI>();
                var buildingToInfluence = _buildingsManager.GetGodsBuilding(god.GodName);

                newGodRef.GodName.text = god.GodLocalizedName.GetLocalizedString();
                newGodRef.GodImage.sprite = god.GodImage;
                newGodRef.AffectedBuilding.sprite = _buildingsManager.GetBuildingIcon(buildingToInfluence.Type);
                newGodRef.AffectedBuildingText.text = buildingToInfluence.BuildingName.GetLocalizedString();
                newGodRef.Slider.value = (int)_godsManager.GetCurrentBlessingLevel(god.GodName);
                OnSliderValueChange(newGodRef, god.GodName);
                newGodRef.Slider.onValueChanged.AddListener(delegate { OnSliderValueChange(newGodRef, god.GodName); });

                _createdGodsInstances.Add(newGod);
            }
        }

        private void ClosePanel()
        {
            foreach (var createdUiElement in _createdGodsInstances) Destroy(createdUiElement);

            _createdGodsInstances.Clear();
            CameraController.IsUiOpen = false;
            GameplayHud.BlockHud = false;
            gameObject.SetActive(false);
        }

        private void OnSliderValueChange(GodInstanceUI p_newGodRef, GodType p_godType)
        {
            var value = (int)Math.Ceiling(p_newGodRef.Slider.value);
            var blessingOnSlider = (BlessingLevel)value;
            var amountOfBlessings = _godsManager.GetAmountOfAvaiableBlessings(p_godType, blessingOnSlider);
            p_newGodRef.ActivationButton.onClick.RemoveAllListeners();

            if (blessingOnSlider == 0)
            {
                p_newGodRef.ActivationButton.gameObject.SetActive(false);
                p_newGodRef.GlowEffect.color = _godsManager.NoEffect;
                //p_newGodRef.SliderHandleImage.color = _godsManager.NoEffect;
                p_newGodRef.SliderFillImage.color = _godsManager.NoEffect;
                p_newGodRef.SliderFillImage.color = _godsManager.NoEffect;
            }
            else if (_godsManager.IsGodBlessingOnLevelActivated(p_godType, blessingOnSlider))
            {
                p_newGodRef.ActivationButton.interactable = true;

                p_newGodRef.ActivationButton.onClick.AddListener(delegate
                {
                    DeactivateSpecificBlessing(p_godType, blessingOnSlider);
                    OnSliderValueChange(p_newGodRef, p_godType);
                });

                p_newGodRef.ActivationButtonText.text = _deactivateBlessing.GetLocalizedString();
                SwitchEffectColor(p_newGodRef);
                p_newGodRef.ActivationButton.gameObject.SetActive(true);
            }
            else if (amountOfBlessings <= 0)
            {
                p_newGodRef.ActivationButton.interactable = _godsManager.BlessingPrices[blessingOnSlider] <=
                                                            _buildingsManager.CurrentDestinyShards;

                p_newGodRef.ActivationButton.onClick.AddListener(delegate
                {
                    BuySpecificBlessing(p_godType, blessingOnSlider);
                    OnSliderValueChange(p_newGodRef, p_godType);
                });

                p_newGodRef.ActivationButtonText.text = string.Format(_buyForText.GetLocalizedString(),
                    _godsManager.BlessingPrices[blessingOnSlider]);
                p_newGodRef.GlowEffect.color = _godsManager.NoEffect;
                //p_newGodRef.SliderHandleImage.color = _godsManager.NoEffect;
                p_newGodRef.SliderFillImage.color = _godsManager.NoEffect;
                p_newGodRef.ActivationButton.gameObject.SetActive(true);
            }
            else
            {
                var currentBlessingLvl = _godsManager.GetCurrentBlessingLevel(p_godType);
                if (currentBlessingLvl != 0 && currentBlessingLvl != blessingOnSlider)
                {
                    p_newGodRef.BlessingChooserText.text = _deactivateOtherBlessings.GetLocalizedString();
                    p_newGodRef.ActivationButton.gameObject.SetActive(false);
                    return;
                }

                p_newGodRef.ActivationButton.interactable = true;
                p_newGodRef.ActivationButton.onClick.AddListener(delegate
                {
                    ActivateSpecificBlessing(p_godType, blessingOnSlider);
                    OnSliderValueChange(p_newGodRef, p_godType);
                });

                p_newGodRef.ActivationButtonText.text = string.Format(_activateBlessing.GetLocalizedString(), amountOfBlessings);
                p_newGodRef.GlowEffect.color = _godsManager.NoEffect;
                p_newGodRef.ActivationButton.gameObject.SetActive(true);
            }

            SwitchDescText(p_newGodRef, blessingOnSlider);
        }

        private void DeactivateSpecificBlessing(GodType p_godType, BlessingLevel p_currentBlessingLevel)
        {
            _audioManager.CreateNewAudioSource(_deactivateEffect);

            _godsManager.DeactivateSpecificBlessing(p_godType, p_currentBlessingLevel);
        }

        private void ActivateSpecificBlessing(GodType p_godType, BlessingLevel p_currentBlessingLevel)
        {
            switch (p_currentBlessingLevel)
            {
                case BlessingLevel.Noone:
                    _audioManager.CreateNewAudioSource(_nooneEffectActivation);
                    break;
                case BlessingLevel.Small:
                    _audioManager.CreateNewAudioSource(_smallEffectActivation);
                    break;
                case BlessingLevel.Medium:
                    _audioManager.CreateNewAudioSource(_mediumEffectActivation);
                    break;
                case BlessingLevel.Big:
                    _audioManager.CreateNewAudioSource(_bigEffectActivation);
                    break;
            }

            _godsManager.ActivateSpecificBlessing(p_godType, p_currentBlessingLevel);
        }

        private void BuySpecificBlessing(GodType p_godType, BlessingLevel p_blessingLevel)
        {
            _audioManager.CreateNewAudioSource(_boughtEffect);
            
            _godsManager.BuySpecificBlessing(p_godType, p_blessingLevel);
            _buildingsManager.HandlePointsManipulation(PointsType.StarDust,
                _godsManager.BlessingPrices[p_blessingLevel], false);
        }

        private void SwitchDescText(GodInstanceUI p_newGodRef, BlessingLevel p_blessingOnSlider)
        {
            switch (p_newGodRef.Slider.value)
            {
                case 0:
                    p_newGodRef.BlessingChooserText.text = _chooseBlessingLevel.GetLocalizedString();
                    break;
                case 1:
                    p_newGodRef.BlessingChooserText.text = string.Format(_smallBlessingEfficiency.GetLocalizedString(),
                        Mathf.CeilToInt(_godsManager.GetBlessingValue(p_blessingOnSlider) * 100));
                    break;
                case 2:
                    p_newGodRef.BlessingChooserText.text = string.Format(_mediumBlessingEfficiency.GetLocalizedString(),
                        Mathf.CeilToInt(_godsManager.GetBlessingValue(p_blessingOnSlider) * 100));
                    break;
                case 3:
                    p_newGodRef.BlessingChooserText.text = string.Format(_bigBlessingEfficiency.GetLocalizedString(),
                        Mathf.CeilToInt(_godsManager.GetBlessingValue(p_blessingOnSlider) * 100));
                    break;
            }
        }

        private void SwitchEffectColor(GodInstanceUI p_newGodRef)
        {
            var color = p_newGodRef.Slider.value switch
            {
                1 => _godsManager.SmallEffect,
                2 => _godsManager.MediumEffect,
                3 => _godsManager.BigEffect,
                _ => p_newGodRef.GlowEffect.color
            };

            p_newGodRef.GlowEffect.color = color;
            //p_newGodRef.SliderHandleImage.color = color;
            p_newGodRef.SliderFillImage.color = color;
            p_newGodRef.GlowEffect.color = color;
        }
    }
}