using System.Globalization;
using AudioSystem;
using GameManager;
using GeneralSystems;
using TMPro;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using UnityEngine.UI;

namespace InGameUi
{
    public class SettingsPanel : MonoBehaviour
    {
        [SerializeField] private AudioManager _audioManager;
        [SerializeField] private MainGameManager _gameManager;
        [SerializeField] private Button _backButton;
        [SerializeField] private Button _resetGameButton;
        [SerializeField] private Toggle _muteMusic;
        [SerializeField] private Toggle _muteEffect;
        
        [SerializeField] private Button _englishLanguage;
        [SerializeField] private Button _polishLanguage;

        [SerializeField] private TextMeshProUGUI _muteMusicText;
        [SerializeField] private TextMeshProUGUI _muteEffectText;
        [SerializeField] private TextMeshProUGUI _resetWorldText;
        
        [SerializeField] private LocalizedString _resetWorld;
        [SerializeField] private LocalizedString _resetWorldConfirm;
        
        private bool _enableReset;

        private void Start()
        {
            LocalizationSettings.SelectedLocaleChanged += OnLocaleChanged;
            gameObject.SetActive(false);

            HandleSoundSettings();

            _resetGameButton.onClick.AddListener(delegate
            {
                _audioManager.PlayButtonSoundEffect(_resetGameButton.interactable);
                ChangeResetButton(true);
            });
            
            _backButton.onClick.AddListener(delegate
            {
                _audioManager.PlayButtonSoundEffect(_backButton.interactable);
                ClosePanel();
            });
            _muteMusic.onValueChanged.AddListener(MuteMusic);
            _muteEffect.onValueChanged.AddListener(MuteEffects);
            _englishLanguage.onClick.AddListener(delegate
            {
                _audioManager.PlayButtonSoundEffect(true);
                ChangeLanguage(Languages.English);
            });
            _polishLanguage.onClick.AddListener(delegate
            {
                _audioManager.PlayButtonSoundEffect(true);
                ChangeLanguage(Languages.Polish);
            });
            
            ChangeResetButton(false);

            if (PlayerPrefs.GetInt("Setting_Language", 0) == 0)
            {
                ChangeLanguage(DetectLanguage());
            }
            else
            {
                ChangeLanguage((Languages)PlayerPrefs.GetInt("Setting_Language", 0));
            }
        }
        
        private void OnDestroy()
        {
            LocalizationSettings.SelectedLocaleChanged -= OnLocaleChanged;
        }

        public void OpenPanel()
        {
            CameraController.IsUiOpen = true;
            GameplayHud.BlockHud = true;

            gameObject.SetActive(true);
        }

        private void ClosePanel()
        {
            CameraController.IsUiOpen = false;
            GameplayHud.BlockHud = false;

            gameObject.SetActive(false);
        }

        
        private void ChangeResetButton(bool p_enableReset)
        {
            _enableReset = p_enableReset;
            if (p_enableReset)
            {
                _resetWorldText.text = _resetWorldConfirm.GetLocalizedString();
                
                _resetGameButton.onClick.RemoveAllListeners();
                _resetGameButton.onClick.AddListener(delegate
                {
                    _audioManager.PlayButtonSoundEffect(_resetGameButton.interactable);
                    ResetGame();
                });
            }
            else
            {
                _resetWorldText.text = _resetWorld.GetLocalizedString();
                
                _resetGameButton.onClick.RemoveAllListeners();
                _resetGameButton.onClick.AddListener(delegate
                {
                    _audioManager.PlayButtonSoundEffect(_resetGameButton.interactable);
                    ChangeResetButton(true);
                });
            }
        }

        private void MuteMusic(bool p_isToggleOn)
        {
            _audioManager.PlayButtonSoundEffect(_muteMusic.interactable);
            
            _audioManager.MuteMusic(p_isToggleOn);
            _muteMusicText.text = p_isToggleOn ? "On" : "Off";

            PlayerPrefs.SetInt("Setting_MuteMusic", p_isToggleOn ? 0 : 1); // 0 == umuted, 1 == muted
            PlayerPrefs.Save();
        }

        private void MuteEffects(bool p_isToggleOn)
        {
            _audioManager.PlayButtonSoundEffect(_muteEffect.interactable);
            
            _audioManager.MuteEffects(p_isToggleOn);
            _muteEffectText.text = p_isToggleOn ? "On" : "Off";

            PlayerPrefs.SetInt("Setting_MuteEffects", p_isToggleOn ? 0 : 1);
            PlayerPrefs.Save();
        }

        private void ResetGame()
        {
            PlayerPrefs.DeleteAll();
            _gameManager.ResetSave();
        }

        private void HandleSoundSettings()
        {
            var muteSettingInt = PlayerPrefs.GetInt("Setting_MuteMusic", 0);
            var muteSettingBool = muteSettingInt == 0;
            _muteMusic.isOn = muteSettingBool;
            MuteMusic(muteSettingBool);

            muteSettingInt = PlayerPrefs.GetInt("Setting_MuteEffects", 0);
            muteSettingBool = muteSettingInt == 0;
            _muteEffect.isOn = muteSettingBool;
            MuteEffects(muteSettingBool);
        }
        
        private void ChangeLanguage(Languages p_language)
        {
            _gameManager.ChangeLocale(p_language);
            PlayerPrefs.SetInt("Setting_Language", (int)p_language);
        }

        private Languages DetectLanguage()
        {
            CultureInfo cultureInfo = CultureInfo.CurrentCulture;
            string isoLanguageName = cultureInfo.TwoLetterISOLanguageName;

            switch (isoLanguageName)
            {
                case "en":
                    return Languages.English;
                case "pl":
                    return Languages.Polish;
                // Add more cases as needed for supported languages
                default:
                    return Languages.English; // Default language if not supported
            }
        }

        private void OnLocaleChanged(Locale p_locale)
        {
            if (_enableReset)
            {
                _resetWorldText.text = _resetWorldConfirm.GetLocalizedString();
            }
            else
            {
                _resetWorldText.text = _resetWorld.GetLocalizedString();
            }
        }
    }

    // foreach (Languages day in Enum.GetValues(typeof(Languages)))
    // {
    //     // CREATE PREFAB LANGUAGE
    // }
}