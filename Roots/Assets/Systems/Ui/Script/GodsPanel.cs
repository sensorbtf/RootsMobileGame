using System.Collections.Generic;
using System.Linq;
using Buildings;
using Gods;
using GameManager;
using GeneralSystems;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System;

namespace InGameUi
{
    public class GodsPanel : MonoBehaviour
    {
        [SerializeField] private BuildingsManager _buildingsManager;
        [SerializeField] private GodsManager _godsManager;
        [SerializeField] private WorkersManager _workersManager;
        [SerializeField] private MainGameManager _gameManager;
        [SerializeField] private GameObject _godPrefab;

        [SerializeField] private TextMeshProUGUI _tabName;
        [SerializeField] private Transform _contentTransform;
        [SerializeField] private Button _goBackButton;

        private List<GameObject> _createdGodsInstances;
        public event Action OnBackToWorkersPanel;

        private void Start()
        {
            _createdGodsInstances = new List<GameObject>();
            _goBackButton.onClick.AddListener(BackToWorkerTab);
            gameObject.SetActive(false);
        }

        public void BackToWorkerTab()
        {
            ClosePanel();
            OnBackToWorkersPanel?.Invoke();
        }

        public void ActivatePanel()
        {
            gameObject.SetActive(true);
            GameplayHud.BlockHud = true;
            CameraController.IsUiOpen = true;

            foreach (var god in _godsManager.PlayerCurrentBlessings)
            {
                var newGod = Instantiate(_godPrefab, _contentTransform);
                var newGodRef = newGod.GetComponent<GodInstance>();

                newGodRef.GodName.text = god.Key.GodName.ToString();
                newGodRef.GodImage.sprite = god.Key.GodImage;
                newGodRef.AffectedBuilding.sprite = _buildingsManager.GetBuildingIcon(god.Key.AffectedBuilding);
                newGodRef.Slider.value = (int)_godsManager.GetCurrentBlessingLevel(god.Key.GodName);
                OnSliderValueChange(newGodRef, god.Key.GodName);
                newGodRef.Slider.onValueChanged.AddListener(delegate { OnSliderValueChange(newGodRef, god.Key.GodName); });
                
            }
        }

        private void ClosePanel()
        {
            foreach (var createdUiElement in _createdGodsInstances)
            {
                Destroy(createdUiElement);
            }

            _createdGodsInstances.Clear();
            CameraController.IsUiOpen = false;
            GameplayHud.BlockHud = false;
            gameObject.SetActive(false);
        }

        private void RefereshActivationButton(GodInstance p_newGodRef)
        { 
        
        }

        private void OnSliderValueChange(GodInstance p_newGodRef, GodType p_godType)
        {
            var value = Math.Ceiling(p_newGodRef.Slider.value);
            var currentBlessingLevel = _godsManager.GetCurrentBlessingLevel(p_godType);
            var amountOfBlessings = _godsManager.GetAmountOfAviableBlessings(p_godType, currentBlessingLevel);

            if (amountOfBlessings <= 0)
            {
                p_newGodRef.ActivationButton.onClick.AddListener();
                // activate buying visuals
                p_newGodRef.ActivationButtonText.text = $"Activate {}";
            }
            else
            { 
                p_newGodRef.ActivationButton.onClick.AddListener();
                // activate activation things
                p_newGodRef.ActivationButtonText.text = $"Activate {}";
            }

            SwitchSliderValue(p_newGodRef);
        }

        private void SwitchSliderValue(GodInstance p_newGodRef)
        {
            switch (p_newGodRef.Slider.value)
            {
                case 0:
                    p_newGodRef.BlessingChooserText.text = "Noone blessing";
                    break;
                case 1:
                    p_newGodRef.BlessingChooserText.text = "Small Blessing \n +10% efficiency";
                    break;
                case 2:
                    p_newGodRef.BlessingChooserText.text = "Medium Blessing \n +25% efficiency";
                    break;
                case 3:
                    p_newGodRef.BlessingChooserText.text = "Big Blessing \n +50% efficiency";
                    break;
                default:
                    break;
            }
        }
    }
}