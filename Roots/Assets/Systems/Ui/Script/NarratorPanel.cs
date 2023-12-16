using System;
using System.Collections;
using AudioSystem;
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
        [SerializeField] private AudioManager _audioManager;
        [SerializeField] private NarratorManager _narratorManager;
        [SerializeField] private GameObject _viniete;

        [SerializeField] private TextMeshProUGUI Text;
        [SerializeField] private Button OnOffButton;
        [SerializeField] private Image ButtonImage;
        
        [SerializeField] private Sprite HideUiSprite;
        [SerializeField] private Sprite ShowUiSprite;
        [SerializeField] private Sprite MoreSprite;
        [SerializeField] private AudioClip _soundEffect;
        
        [SerializeField] private Vector3 _startPosition;
        [SerializeField] private Vector3 _endPosition;
        [SerializeField] private float _duration = 1.0f;
        [SerializeField] private float _typingSpeed = 0.05f;

        [SerializeField] private TutorialTexts[] TutorialTexts;
        
        private Coroutine _typingCoroutine;

        private void Start()
        {
            LocalizationSettings.SelectedLocaleChanged += OnLocaleChanged;
            _narratorManager.OnTutorialAdvancement += ActivateNarrator;
            
            OnOffButton.onClick.RemoveAllListeners();
            OnOffButton.onClick.AddListener(delegate { ShowAndMovePanel(false); });
        }

        public void ActivateNarrator(bool p_show)
        {
            if (p_show)
            {
                ShowAndMovePanel(true);
            }
        }

        private void ShowAndMovePanel(bool p_shouldType)
        {
            StartCoroutine(MoveUI(_startPosition, p_shouldType));

            if (GetCurrentText().Text.Length == _narratorManager.CurrentSubText + 1)
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
            if (_typingCoroutine != null) 
            {
                SkipTyping();
            }
            else
            {
                StartCoroutine(MoveUI(_endPosition, false));
                OnOffButton.onClick.RemoveAllListeners();
                OnOffButton.onClick.AddListener(delegate { ShowAndMovePanel(false); });
                ButtonImage.sprite = ShowUiSprite;

                GameplayHud.BlockHud = false;
                _viniete.SetActive(false);
            }
        }

        private void GetNextText()
        {
            if (_typingCoroutine != null) 
            {
                SkipTyping();
            }
            else
            {
                _narratorManager.AddToSubtext();
                _typingCoroutine = StartCoroutine(TypeText(GetCurrentText().Text[_narratorManager.CurrentSubText].GetLocalizedString()));
            }
            
            if (GetCurrentText().Text.Length == _narratorManager.CurrentSubText + 1)
            {
                OnOffButton.onClick.RemoveAllListeners();
                OnOffButton.onClick.AddListener(HideAndMovePanel);
                ButtonImage.sprite = HideUiSprite;
            }
        }

        private IEnumerator MoveUI(Vector3 p_targetPosition, bool p_shouldStartTyping)
        {
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
                Text.text = "";

                _typingCoroutine =
                    StartCoroutine(
                        TypeText(GetCurrentText().Text[_narratorManager.CurrentSubText].GetLocalizedString()));
            }
            else
            {
                SkipTyping();
            }
        }

        private IEnumerator TypeText(string p_text)
        {
            Text.text = "";
            GameplayHud.BlockHud = true;
            _viniete.SetActive(true);
            
            foreach (char c in p_text)
            {
                _audioManager.TryToPlayWritingEffect(_soundEffect);
                Text.text += c;
                yield return new WaitForSeconds(_typingSpeed);
            }
            
            _audioManager.MuteWritingEffect();
        }

        private void SkipTyping()
        {
            if (_typingCoroutine == null)
                return;

            StopCoroutine(_typingCoroutine);
            _audioManager.MuteWritingEffect();
            _typingCoroutine = null;

            Text.text = GetCurrentText().Text[_narratorManager.CurrentSubText].GetLocalizedString();
        }

        private void OnLocaleChanged(Locale p_locale)
        {
            if (_narratorManager.CurrentTutorialStep != TutorialStep.Start)
            {
                Text.text = GetCurrentText().Text[_narratorManager.CurrentSubText].GetLocalizedString();
            }
        }

        private TutorialTexts GetCurrentText()
        {
            return TutorialTexts[(int)_narratorManager.CurrentTutorialStep];
        }
    }

    [Serializable]
    public struct TutorialTexts
    {
        public LocalizedString[] Text;
    }
}