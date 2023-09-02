using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Systems
{
    public class BuildingPanel : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _buildingName;
        [SerializeField] private Button _exitButton;
        [SerializeField] private Button _upgradeButton;
        
        [SerializeField] private GameObject buildingEntryPrefab; 
        [SerializeField] private GameObject tierPanelPrefab;
        [SerializeField] private Transform contentTransform;    

        private BuildingName _currentBuilding;
        private List<GameObject> _runtimeBuildingsUiToDestroy;
        
        [SerializeField] private BuildingDatabase buildingDatabase;

        private void Start()
        {
            gameObject.SetActive(false); 

            _runtimeBuildingsUiToDestroy = new List<GameObject>();
        }

       public void ActivateOnClick(BuildingData p_buildingName, int p_level)
        {
            gameObject.SetActive(true);
            CameraController.IsUiOpen = true;

            _currentBuilding = p_buildingName.Name;
            _buildingName.text = p_buildingName.Name.ToString();

            if (p_buildingName.Name == BuildingName.Cottage)
            {
                // Temporary storage to organize buildings by tier
                Dictionary<int, List<BuildingData>> buildingsByTier = new Dictionary<int, List<BuildingData>>();

                foreach (BuildingData building in buildingDatabase.allBuildings)
                {
                    if (!buildingsByTier.ContainsKey(building.UnlockTier))
                    {
                        buildingsByTier[building.UnlockTier] = new List<BuildingData>();
                    }

                    buildingsByTier[building.UnlockTier].Add(building);
                }

                for (int i = 0; i < 3; i++)
                {
                    // Now create UI elements by tier
                    foreach (int tier in buildingsByTier.Keys)
                    {
                        // Instantiate tier panel
                        GameObject newTierPanel = Instantiate(tierPanelPrefab, contentTransform);
                        _runtimeBuildingsUiToDestroy.Add(newTierPanel);
                    
                        foreach (BuildingData building in buildingsByTier[tier])
                        {
                            GameObject newBuildingUi = Instantiate(buildingEntryPrefab, contentTransform);
                        
                            SingleButtonUi script = newBuildingUi.GetComponent<SingleButtonUi>();
                            script.BuildingName.GetComponent<TextMeshProUGUI>().text = building.Name.ToString();
                            script.BuildingIcon.GetComponent<Image>().sprite = building.Icon;
                            script.CreateBuilding.onClick.AddListener(() => OnBuildingButtonClicked(building));

                            _runtimeBuildingsUiToDestroy.Add(newBuildingUi);
                        }
                    }
                }
            }
            
            // View all buildings in cottage as it is like centrum dowodzenia for fast building
        }

        public void ClosePanel()
        {
            // closing logic

            foreach (var createdUiElement in _runtimeBuildingsUiToDestroy)
            {
                Destroy(createdUiElement);
            }
            CameraController.IsUiOpen = false;

            _runtimeBuildingsUiToDestroy.Clear();
            gameObject.SetActive(false);
        }
        
        public void UpgradeBuilding()
        {
            // upgrade logic
        }
        
        private void OnBuildingButtonClicked(BuildingData building)
        {
            Debug.Log("Clicked building of: " + building.Name);
            // Do something
        }

    }
}