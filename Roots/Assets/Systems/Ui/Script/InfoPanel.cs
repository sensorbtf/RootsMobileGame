using AudioSystem;
using GeneralSystems;
using InGameUi;
using TMPro;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using UnityEngine.UI;

public class InfoPanel : MonoBehaviour
{
    [Header("System Refs")]
    [SerializeField] private AudioManager _audioManager;

    [Header("Refs")]
    [SerializeField] private Button _button;
    [SerializeField] private TextMeshProUGUI _title;
    [SerializeField] private TextMeshProUGUI _buttonText;
    [SerializeField] private RectTransform _content;
    
    [Header("Info Prefabs")]
    [SerializeField] private GameObject _resourcesInfoGo;
    //[SerializeField] private GameObject _rankAndMission;

    [Header("Localization")] 
    [SerializeField] LocalizedString _panelTitle;
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
        
        _resourcesInfoGo.SetActive(true);
        //_rankAndMission.SetActive(false);
    }

    private void HandleTurnOnOff(bool p_turnOffPanel)
    {
        gameObject.SetActive(p_turnOffPanel);
        GameplayHud.BlockHud = p_turnOffPanel;
        CameraController.IsUiOpen = p_turnOffPanel;
    }
    
    private void OnLocaleChanged(Locale p_locale)
    {
        _title.text = _panelTitle.GetLocalizedString();
        _buttonText.text = _buttonName.GetLocalizedString();
    }
}