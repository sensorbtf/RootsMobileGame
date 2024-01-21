using System;
using System.Collections;
using System.Linq;
using AudioSystem;
using Buildings;
using GeneralSystems;
using Narrator;
using TMPro;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using UnityEngine.UI;

namespace InGameUi
{
    public class NarratorPanel : MonoBehaviour
    {
        [Header("System refs")]
        [SerializeField] private UiManager _uiManager;
        [SerializeField] private BuildingsManager _buildingsManager;
        [SerializeField] private AudioManager _audioManager;
        [SerializeField] private NarratorManager _narratorManager;
        [SerializeField] private GameObject _viniete;

        [SerializeField] private TextMeshProUGUI Text;
        [SerializeField] private Button OnOffButton;
        [SerializeField] private Image ButtonImage;
        
        [Header("Sprites")]
        [SerializeField] private Sprite HideUiSprite;
        [SerializeField] private Sprite ShowUiSprite;
        [SerializeField] private Sprite MoreSprite;
        
        [Header("Audio Clips")]
        [SerializeField] private AudioClip _soundEffect;
        [SerializeField] private AudioClip _unrollingPaper;
        
        [SerializeField] private RectTransform _start;
        [SerializeField] private RectTransform _end;
        [SerializeField] private float _duration = 1.0f;
        [SerializeField] private float _typingSpeed = 0.05f;

        [SerializeField] private TutorialTexts[] TutorialTexts;

        private Coroutine _typingCoroutine;

        private void Awake()
        {
            LocalizationSettings.SelectedLocaleChanged += OnLocaleChanged;
            _narratorManager.OnTutorialAdvancement += ActivateNarrator;
            
            OnOffButton.onClick.RemoveAllListeners();
            OnOffButton.onClick.AddListener(delegate
            {
                _audioManager.PlayButtonSoundEffect(OnOffButton.interactable);
                ShowAndMovePanel(false);
            });
        }

        private void OnDestroy()
        {
            LocalizationSettings.SelectedLocaleChanged -= OnLocaleChanged;
            _narratorManager.OnTutorialAdvancement -= ActivateNarrator;
        }

        private void ActivateNarrator(bool p_show)
        {
            if (_narratorManager.CurrentTutorialStep == TutorialStep.Quests_End)
            {
                if (_buildingsManager.Bonus != null && _buildingsManager.Bonus.Building != BuildingType.Cottage)
                {
                    ShowAndMovePanel(true);
                }
                else
                {
                    Text.text = TutorialTexts[(int)TutorialStep.Quests_End].Text[_narratorManager.CurrentSubText].GetLocalizedString();
                }
            }
            else
            {
                if (p_show)
                {
                    ShowAndMovePanel(true);
                }
                else
                {
                    Text.text = GetCurrentText();
                }
            }
        }

        private void ShowAndMovePanel(bool p_shouldType)
        {
            CameraController.IsUiOpen = true;
            _viniete.SetActive(true);
            
            StartCoroutine(MoveUI(_start.localPosition, p_shouldType));
            
            if (TutorialTexts[(int)_narratorManager.CurrentTutorialStep].Text.Length == _narratorManager.CurrentSubText + 1)
            {
                OnOffButton.onClick.RemoveAllListeners();
                OnOffButton.onClick.AddListener(HideAndMovePanel);
                ButtonImage.sprite = HideUiSprite;
            }
            else
            {
                OnOffButton.onClick.RemoveAllListeners();
                OnOffButton.onClick.AddListener(GetNextText);
                ButtonImage.sprite = MoreSprite;
            }
        }

        private void HideAndMovePanel()
        {
            _audioManager.PlayButtonSoundEffect(OnOffButton.interactable);
            
            if (_typingCoroutine != null) 
            {
                SkipTyping();
            }
            else
            {
                StartCoroutine(MoveUI(_end.localPosition, false));
                OnOffButton.onClick.RemoveAllListeners();
                OnOffButton.onClick.AddListener(delegate { ShowAndMovePanel(false); });
                ButtonImage.sprite = ShowUiSprite;
                _viniete.SetActive(false);
                CameraController.IsUiOpen = _uiManager.IsAnyPanelOpen();
            }
        }

        private void GetNextText()
        {
            _audioManager.PlayButtonSoundEffect(OnOffButton.interactable);
            
            if (_typingCoroutine != null) 
            {
                SkipTyping();
            }
            else
            {
                _narratorManager.AddToSubtext();
                _typingCoroutine = StartCoroutine(TypeText(GetCurrentText()));
            }
            
            if (TutorialTexts[(int)_narratorManager.CurrentTutorialStep].Text.Length == _narratorManager.CurrentSubText + 1)
            {
                OnOffButton.onClick.RemoveAllListeners();
                OnOffButton.onClick.AddListener(HideAndMovePanel);
                ButtonImage.sprite = HideUiSprite;
            }
        }

        private IEnumerator MoveUI(Vector3 p_targetPosition, bool p_shouldStartTyping)
        {
            if (p_shouldStartTyping)
            {
                _viniete.SetActive(true);
                Text.text = "";
            }
            
            _audioManager.PlaySpecificSoundEffect(_unrollingPaper);
            float elapsed = 0;
            Vector3 initialPosition = gameObject.transform.localPosition;

            while (elapsed < _duration)
            {
                gameObject.transform.localPosition =
                    Vector3.Lerp(initialPosition, p_targetPosition, elapsed / _duration);
                elapsed += Time.deltaTime;
                yield return null;
            }

            gameObject.transform.localPosition = p_targetPosition;

            if (p_shouldStartTyping)
            {
                _typingCoroutine = StartCoroutine(TypeText(GetCurrentText()));
            }
            else
            {
                SkipTyping();
            }
        }

        private IEnumerator TypeText(string p_text)
        {
            Text.text = "";
            _viniete.SetActive(true);

            foreach (char c in p_text)
            {
                _audioManager.TryToPlayWritingEffect(_soundEffect);
                Text.text += c;
                yield return new WaitForSeconds(_typingSpeed);
            }

            _typingCoroutine = null;
            _audioManager.MuteWritingEffect();
        }

        private void SkipTyping()
        {
            if (_typingCoroutine == null)
                return;

            StopCoroutine(_typingCoroutine);
            _audioManager.MuteWritingEffect();
            _typingCoroutine = null;

            Text.text = GetCurrentText();
        }

        private void OnLocaleChanged(Locale p_locale)
        {
            if (_narratorManager.CurrentTutorialStep != TutorialStep.Start)
            {
                Text.text = GetCurrentText();
            }
        }

        private string GetCurrentText()
        {
            if (_narratorManager.CurrentTutorialStep == TutorialStep.Quests_End && _buildingsManager.Bonus != null)
            {
                var text = TutorialTexts.Last().Text[0].GetLocalizedString();
                return string.Format(text, _buildingsManager.GetSpecificBuilding(_buildingsManager.Bonus.Building)
                    .BuildingMainData.BuildingName.GetLocalizedString(), _buildingsManager.Bonus.BonusInPercents);
            }

            return TutorialTexts[(int)_narratorManager.CurrentTutorialStep].Text[_narratorManager.CurrentSubText].GetLocalizedString();
        }
    }

    [Serializable]
    public struct TutorialTexts
    {
        public LocalizedString[] Text;
    }
}