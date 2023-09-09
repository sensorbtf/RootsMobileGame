using System;
using System.Collections.Generic;
using System.Linq;
using Buildings;
using GeneralSystems;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace InGameUi
{
    public class BuildingPanel : MonoBehaviour
    {
        [SerializeField] private BuildingManager _buildingManager;
        [SerializeField] private WorkersManager _workersManager;

        [SerializeField] private TextMeshProUGUI _buildingName;
        [SerializeField] private TextMeshProUGUI _numberOfWorkers;
        [SerializeField] private GameObject buildingEntryPrefab;
        [SerializeField] private GameObject tierPanelPrefab;
        [SerializeField] private GameObject _endBuildingButton;
        [SerializeField] private Transform contentTransform;

        private BuildingData _currentBuildingData;
        private int _currentBuildingLevel;
        private List<GameObject> _runtimeBuildingsUiToDestroy;

        public event Action OnBackToWorkersPanel;

        private void Start()
        {
            _buildingManager.OnBuildingClicked += ActivateOnClick;
            _workersManager.OnWorkersUpdated += UpdateWorkersText;
            _runtimeBuildingsUiToDestroy = new List<GameObject>();
            gameObject.SetActive(false);
        }

        private void OnDisable()
        {
            //_buildingManager.OnBuildingClicked -= ActivateOnClick;
            //_buildingManager.OnWorkersUpdated -= UpdateWorkersText;
        }

        private void ActivateOnClick(BuildingData p_specificBuilding, int p_level)
        {
            _currentBuildingData = p_specificBuilding;
            _currentBuildingLevel = p_level;

            HandleView();

            //_buildingManager.c
            // View all buildings in cottage as it is like centrum dowodzenia for fast building
        }

        public void UpgradeBuilding()
        {
            _buildingManager.PutBuildingOnQueue(_currentBuildingData, _currentBuildingLevel);

            //referesh/make timer for buttons
        }

        public void BackToWorkerTab()
        {
            ClosePanel();
            OnBackToWorkersPanel?.Invoke();
        }

        private void OnBuildOrUpgradeButtonClicked(BuildingData p_buildingData, int p_buildingLevel,
            GameObject p_panelUi)
        {
            _buildingManager.RemoveResourcePoints(p_buildingData, p_buildingLevel);

            _buildingManager.CurrentResourcePoints -=
                p_buildingData.PerLevelData[p_buildingLevel].Requirements.ResourcePoints;
            _workersManager.WorkersAmount--;

            p_panelUi.GetComponent<Image>().color = Color.magenta;
            _buildingManager.PutBuildingOnQueue(p_buildingData, p_buildingLevel);
        }

        private void UpdateWorkersText(int p_workers)
        {
            _numberOfWorkers.text = $"Workers: {p_workers.ToString()}";
        }

        private void ClosePanel()
        {
            foreach (var createdUiElement in _runtimeBuildingsUiToDestroy)
            {
                Destroy(createdUiElement);
            }

            CameraController.IsUiOpen = false;

            _currentBuildingData = null;
            _runtimeBuildingsUiToDestroy.Clear();
            GameplayHud.BlockHud = false;

            gameObject.SetActive(false);
        }

        public void HandleView(bool p_fromWorkerPanel = false)
        {
            gameObject.SetActive(true);
            CameraController.IsUiOpen = true;
            GameplayHud.BlockHud = true;

            _buildingName.text = "Start Building";
            _numberOfWorkers.text = $"Workers: {_workersManager.WorkersAmount.ToString()}";

            if (p_fromWorkerPanel)
            {
                _endBuildingButton.SetActive(true);
            }
            else
            {
                _endBuildingButton.SetActive(false);
            }

            // Temporary storage to organize buildings by tier
            Dictionary<int, List<BuildingData>> buildingsByTier = new Dictionary<int, List<BuildingData>>();

            foreach (BuildingData building in _buildingManager.AllBuildingsDatabase.allBuildings)
            {
                if (!buildingsByTier.ContainsKey(building.BaseCottageLevelNeeded))
                {
                    buildingsByTier[building.BaseCottageLevelNeeded] = new List<BuildingData>();
                }

                buildingsByTier[building.BaseCottageLevelNeeded].Add(building);
            }

            // Now create UI elements by tier
            foreach (int tier in buildingsByTier.Keys)
            {
                var newTierPanel = Instantiate(tierPanelPrefab, contentTransform);
                _runtimeBuildingsUiToDestroy.Add(newTierPanel);

                foreach (BuildingData building in buildingsByTier[tier])
                {
                    GameObject newBuildingUi = Instantiate(buildingEntryPrefab, contentTransform);

                    SingleButtonUi script = newBuildingUi.GetComponent<SingleButtonUi>();
                    script.BuildingName.GetComponent<TextMeshProUGUI>().text = building.Type.ToString();
                    var builtBuilding =
                        _buildingManager.CurrentBuildings.FirstOrDefault(x => x.BuildingMainData.Type == building.Type);
                    
                    if (builtBuilding != null)
                    {
                        script.BuildingIcon.GetComponent<Image>().sprite =
                            building.PerLevelData[builtBuilding.CurrentLevel].Icon;

                        var nextLevel = builtBuilding.CurrentLevel;
                        nextLevel++;

                        script.LevelInfo.GetComponent<TextMeshProUGUI>().text =
                            $"{builtBuilding.CurrentLevel} >> {nextLevel}";
                        script.CreateBuilding.image.color = Color.yellow;
                        script.CreateBuilding.interactable =
                            _buildingManager.CanUpgradeBuilding(builtBuilding);

                        script.CreateBuilding.onClick.AddListener(() =>
                            OnBuildOrUpgradeButtonClicked(building, builtBuilding.CurrentLevel, newBuildingUi));
                    }
                    else
                    {
                        script.BuildingIcon.GetComponent<Image>().sprite = building.PerLevelData[0].Icon;
                        script.LevelInfo.GetComponent<TextMeshProUGUI>().text = $"{0} >> {1}";
                        script.CreateBuilding.image.color = Color.green;
                        script.CreateBuilding.interactable = _buildingManager.CanBuildBuilding(building);

                        script.CreateBuilding.onClick.AddListener(() =>
                            OnBuildOrUpgradeButtonClicked(building, 0, newBuildingUi));  
                    }


                    _runtimeBuildingsUiToDestroy.Add(newBuildingUi);
                }
            }
        }
    }
}