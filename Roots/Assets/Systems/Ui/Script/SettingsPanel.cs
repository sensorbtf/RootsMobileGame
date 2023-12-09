using AudioSystem;
using GameManager;
using GeneralSystems;
using TMPro;
using UnityEngine;
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
        
        [SerializeField] private TextMeshProUGUI _muteMusicText;
        [SerializeField] private TextMeshProUGUI _muteEffectText;

        private void Start()
        {
            gameObject.SetActive(false);

            HandleSoundSettings();
            
            _resetGameButton.onClick.AddListener(ResetGame);
            _backButton.onClick.AddListener(ClosePanel);
            _muteMusic.onValueChanged.AddListener(MuteMusic);
            _muteEffect.onValueChanged.AddListener(MuteEffects);
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
        
        private void MuteMusic(bool p_isToggleOn)
        {
            _audioManager.MuteMusic(p_isToggleOn);
            _muteMusicText.text = p_isToggleOn ? "On" : "Off";

            PlayerPrefs.SetInt("Setting_MuteMusic", p_isToggleOn ? 0 : 1);// 0 == umuted, 1 == muted
            PlayerPrefs.Save();
        }
        
        private void MuteEffects(bool p_isToggleOn)
        {
            _audioManager.MuteEffects(p_isToggleOn);
            _muteEffectText.text = p_isToggleOn ? "On" : "Off";
            
            PlayerPrefs.SetInt("Setting_MuteEffects", p_isToggleOn ? 0 : 1);
            PlayerPrefs.Save();
        }

        private void ResetGame() // TODO: Popup confirmation needed
        {
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
    }
}