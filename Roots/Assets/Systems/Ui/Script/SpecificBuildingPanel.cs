using System;
using System.Collections.Generic;
using System.Linq;
using Buildings;
using GeneralSystems;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace InGameUi
{
    public class SpecificBuildingPanel : MonoBehaviour
    {
        [SerializeField] private BuildingManager _buildingManager;
        [SerializeField] private WorkersManager _workersManager;

        [SerializeField] private TextMeshProUGUI _buildingName;
        [SerializeField] private TextMeshProUGUI _description;
        [SerializeField] private Image _buildingIcon;
        [SerializeField] private GameObject _buildingIconPrefab;
        [SerializeField] private GameObject _goBackGo;
        [SerializeField] private GameObject _lvlUpGo;
        [SerializeField] private GameObject _getIntoWorkGo;
        [SerializeField] private Slider _levelUpProgression;
        [SerializeField] private Slider _sliderValue;
        [SerializeField] private Transform _contentTransform;

        private List<GameObject> _runtimeBuildingsUiToDestroy;
        private List<BuildingData> _buildingsOnInPanelQueue;
        private Dictionary<BuildingData, SingleBuildingRefs> _createdUiElements;

        private Button _goBackButton;
        private Button _lvlUpButton;
        private Button _startMiniGameButton;

        private void Start()
        {
            _buildingManager.OnBuildingClicked += ActivateOnClick;
            
            _runtimeBuildingsUiToDestroy = new List<GameObject>();
            _buildingsOnInPanelQueue = new List<BuildingData>();
            _createdUiElements = new Dictionary<BuildingData, SingleBuildingRefs>();

            _goBackButton = _goBackGo.GetComponent<Button>();
            _lvlUpButton = _goBackGo.GetComponent<Button>();
            _startMiniGameButton = _goBackGo.GetComponent<Button>();

            _goBackButton.onClick.AddListener(ClosePanel);
            
            gameObject.SetActive(false);
        }

        private void ClosePanel()
        {
            foreach (var createdUiElement in _runtimeBuildingsUiToDestroy)
            {
                Destroy(createdUiElement);
            }

            CameraController.IsUiOpen = false;
            GameplayHud.BlockHud = false;

            _runtimeBuildingsUiToDestroy.Clear();
            _createdUiElements.Clear();

            gameObject.SetActive(false);
        }

        private void ActivateOnClick(Building p_building)
        {
            _buildingName.text = p_building.BuildingMainData.Type.ToString();
            _description.text = "Level up by placing workers here";
            
            
            HandleView();
        }

        private void HandleView()
        {
            gameObject.SetActive(true);
            CameraController.IsUiOpen = true;
            GameplayHud.BlockHud = true;
            
            CreateTechnologies();
        }

        private void CreateTechnologies()
        {
            
        }
    }
}