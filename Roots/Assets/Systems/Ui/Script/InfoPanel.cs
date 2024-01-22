using AudioSystem;
using Buildings;
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
    [SerializeField] private BuildingsManager _buildingsManager;

    [Header("Refs")]
    [SerializeField] private Button _button;
    [SerializeField] private TextMeshProUGUI _title;
    [SerializeField] private TextMeshProUGUI _buttonText;
    
    [Header("Info Prefabs")]
    [SerializeField] private GameObject _resourcesInfoGo;
    [SerializeField] private GameObject _textInfoGo;
    
    [SerializeField] private TextMeshProUGUI _textGoText;

    [Header("Localization")] 
    [SerializeField] LocalizedString _resourcePanelTitle;
    [SerializeField] LocalizedString _stormPanelTitle;
    [SerializeField] LocalizedString _stormPanelText;
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
            
        _resourcesInfoGo.SetActive(false);
        _textInfoGo.SetActive(false);
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
    }
    
    public void ShowStormInfo()
    {
        HandleTurnOnOff(true);
        _title.text = _stormPanelTitle.GetLocalizedString();
        _textInfoGo.SetActive(true);
    }

    private void HandleTurnOnOff(bool p_turnOn)
    {
        gameObject.SetActive(p_turnOn);
        GameplayHud.BlockHud = p_turnOn;
        CameraController.IsUiOpen = p_turnOn;

        if (!p_turnOn)
        {
            _resourcesInfoGo.SetActive(false);
            _textInfoGo.SetActive(false);
        }
    }
    
    private void OnLocaleChanged(Locale p_locale)
    {
        _buttonText.text = _buttonName.GetLocalizedString();
    }
}