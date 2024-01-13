using AudioSystem;
using GeneralSystems;
using InGameUi;
using TMPro;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class InfoPanel : MonoBehaviour
{
    [Header("System Refs")]
    [SerializeField] private AudioManager _audioManager;

    [Header("Refs")]
    [SerializeField] private Button _button;
    [SerializeField] private TextMeshProUGUI _title;
    [SerializeField] private TextMeshProUGUI _buttonText;
    
    [Header("Info Prefabs")]
    [SerializeField] private GameObject _resourcesInfoGo;
    [SerializeField] private GameObject _stormMeter;

    [Header("Localization")] 
    [SerializeField] LocalizedString _resourcePanelTitle;
    [SerializeField] LocalizedString _stormPanelTitle;
    [SerializeField] LocalizedString _buttonName;

    private void Start()
    {
        LocalizationSettings.SelectedLocaleChanged += OnLocaleChanged;

        gameObject.SetActive(false);
        _button.onClick.AddListener(delegate 
        { 
            _audioManager.PlayButtonSoundEffect(true);
            HandleTurnOnOff(false); 
        });
    }
    
    private void OnDestroy()
    {
        LocalizationSettings.SelectedLocaleChanged -= OnLocaleChanged;
    }

    public void ShowResourcesInfo()
    {
        HandleTurnOnOff(true);
        _title.text = _resourcePanelTitle.GetLocalizedString();
        _resourcesInfoGo.SetActive(true);
        _stormMeter.SetActive(false);
    }
    
    public void ShowStormInfo()
    {
        HandleTurnOnOff(true);
        _title.text = _stormPanelTitle.GetLocalizedString();
        _resourcesInfoGo.SetActive(false);
        _stormMeter.SetActive(true);
    }

    private void HandleTurnOnOff(bool p_turnOffPanel)
    {
        gameObject.SetActive(p_turnOffPanel);
        GameplayHud.BlockHud = p_turnOffPanel;
        CameraController.IsUiOpen = p_turnOffPanel;
        
        _resourcesInfoGo.SetActive(false);
        _stormMeter.SetActive(false);
    }
    
    private void OnLocaleChanged(Locale p_locale)
    {
        if (_resourcesInfoGo.activeSelf)
        {
            _title.text = _resourcePanelTitle.GetLocalizedString();
        }
        else if (_stormMeter.activeSelf)
        {
            _title.text = _stormPanelTitle.GetLocalizedString();
        }
        _buttonText.text = _buttonName.GetLocalizedString();
    }
}