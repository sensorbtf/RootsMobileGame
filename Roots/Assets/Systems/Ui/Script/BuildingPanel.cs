using System;
using System.Collections.Generic;
using Buildings;
using GeneralSystems;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace InGameUi
{
    public class BuildingPanel : MonoBehaviour
    {
        [SerializeField] private BuildingManager _buildingManager;

        [SerializeField] private TextMeshProUGUI _buildingName;
        [SerializeField] private GameObject buildingEntryPrefab;
        [SerializeField] private GameObject tierPanelPrefab;
        [SerializeField] private Transform contentTransform;

        private BuildingData _currentBuildingData;
        private List<GameObject> _runtimeBuildingsUiToDestroy;

        private void Start()
        {
            gameObject.SetActive(false);
            _buildingManager.OnBuildingClicked += ActivateOnClick;
            _runtimeBuildingsUiToDestroy = new List<GameObject>();
        }

        private void ActivateOnClick(BuildingData p_buildingName, int p_level)
        {
            gameObject.SetActive(true);
            CameraController.IsUiOpen = true;

            _currentBuildingData = p_buildingName;
            _buildingName.text = p_buildingName.type.ToString();

            if (p_buildingName.type == BuildingType.Cottage)
            {
                HandleCottageView();
            }

            // View all buildings in cottage as it is like centrum dowodzenia for fast building
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
            gameObject.SetActive(false);
        }

        public void UpgradeBuilding()
        {
            _buildingManager.HandleBuildingUpgrade(_currentBuildingData);
        }

        private void OnBuildOrUpgradeButtonClicked(BuildingData p_buildingData, bool p_upgrade)
        {
            if (p_upgrade)
            {
                _buildingManager.HandleBuildingUpgrade(p_buildingData);
            }
            else
            {
                _buildingManager.HandleBuildingBuilt(p_buildingData);
            }
        }

        private void HandleCottageView()
        {
            // Temporary storage to organize buildings by tier
            Dictionary<int, List<BuildingData>> buildingsByTier = new Dictionary<int, List<BuildingData>>();

            foreach (BuildingData building in _buildingManager.AllBuildingsDatabase.allBuildings)
            {
                if (building.type == BuildingType.Cottage)
                    continue;
                
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
                var isNewBuilding = true;
                _runtimeBuildingsUiToDestroy.Add(newTierPanel);

                foreach (BuildingData building in buildingsByTier[tier])
                {
                    GameObject newBuildingUi = Instantiate(buildingEntryPrefab, contentTransform);

                    SingleButtonUi script = newBuildingUi.GetComponent<SingleButtonUi>();
                    script.BuildingName.GetComponent<TextMeshProUGUI>().text = building.type.ToString();

                    foreach (var builtBuilding in _buildingManager.CurrentBuildings)
                    {
                        if (building.type != builtBuilding.BuildingMainData.type)
                            continue;

                        script.BuildingIcon.GetComponent<Image>().sprite =
                            building.PerLevelData[builtBuilding.CurrentLevel].Icon;

                        var nextLevel = builtBuilding.CurrentLevel;
                        nextLevel++;
                        
                        script.LevelInfo.GetComponent<TextMeshProUGUI>().text =
                            $"{builtBuilding.CurrentLevel} >> {nextLevel}";
                        
                        script.CreateBuilding.onClick.AddListener(() => OnBuildOrUpgradeButtonClicked(building, true));
                        script.CreateBuilding.image.color = Color.yellow;
                        isNewBuilding = false;
                    }

                    if (isNewBuilding)
                    {
                        script.BuildingIcon.GetComponent<Image>().sprite = building.PerLevelData[0].Icon;
                        script.LevelInfo.GetComponent<TextMeshProUGUI>().text = $"{0} >> {1}";
                        script.CreateBuilding.onClick.AddListener(() => OnBuildOrUpgradeButtonClicked(building, false));
                        script.CreateBuilding.image.color = Color.green;
                    }
                    
                    _runtimeBuildingsUiToDestroy.Add(newBuildingUi);
                }
            }
        }
    }
}