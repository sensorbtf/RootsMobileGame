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
        private List<BuildingData> _buildingsOnInPanelQueue;
        //private Dictionary<BuildingData, bool> _buildingsOnCurrentlyBuildingQueue;

        [HideInInspector] public Dictionary<BuildingData, bool> BuildingsToShow;
        public event Action OnBackToWorkersPanel;

        private void Start()
        {
            //_buildingManager.OnBuildingClicked += ActivateOnClick;
            _runtimeBuildingsUiToDestroy = new List<GameObject>();
            _buildingsOnInPanelQueue = new List<BuildingData>();
            //_buildingsOnCurrentlyBuildingQueue = new Dictionary<BuildingData, bool>();
            BuildingsToShow = new Dictionary<BuildingData, bool>();
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
            
            foreach (var buildingData in _buildingsOnInPanelQueue)
            {
                BuildingsToShow.TryAdd(buildingData, true);
            }

            _runtimeBuildingsUiToDestroy.Clear();
            gameObject.SetActive(false);
        }

        // private void ActivateOnClick(BuildingData p_specificBuilding, int p_level)
        // {
        //     _currentBuildingData = p_specificBuilding;
        //     _currentBuildingLevel = p_level;
        //
        //     HandleView();
        // }

        public void BackToWorkerTab()
        {
            ClosePanel();
            OnBackToWorkersPanel?.Invoke();
        }

        public void HandleView(bool p_fromWorkerPanel = false)
        {
            gameObject.SetActive(true);
            CameraController.IsUiOpen = true;
            GameplayHud.BlockHud = true;
            
            RefreshWorkersAmount();
            
            _buildingName.text = "Start Building";
            UpdateWorkersText();

            if (p_fromWorkerPanel)
            {
                _endBuildingButton.SetActive(true);
            }
            else
            {
                _endBuildingButton.SetActive(false);
            }

            CreateBuildings();
        }

        private void CreateBuildings()
        {
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
                newTierPanel.GetComponentInChildren<TextMeshProUGUI>().text = "Tier: " + tier;
                _runtimeBuildingsUiToDestroy.Add(newTierPanel);

                foreach (BuildingData buildingData in buildingsByTier[tier])
                {
                    GameObject newBuildingUi = Instantiate(buildingEntryPrefab, contentTransform);
                    
                    SingleBuildingUi script = newBuildingUi.GetComponent<SingleBuildingUi>();
                    script.BuildingName.GetComponent<TextMeshProUGUI>().text = buildingData.Type.ToString();
                    script.BuildingInfo.GetComponent<TextMeshProUGUI>().text = "";

                    var builtBuilding = _buildingManager.GetSpecificBuilding(buildingData);

                    if (builtBuilding != null) // is builded or building is in progress
                    {
                        script.BuildingIcon.GetComponent<Image>().sprite =
                            buildingData.PerLevelData[builtBuilding.CurrentLevel].Icon;

                        if (!builtBuilding.IsBeeingUpgradedOrBuilded &&
                            !_buildingsOnInPanelQueue.Contains(builtBuilding.BuildingMainData)) // open way to upgrade
                        {
                            var nextLevel = builtBuilding.CurrentLevel;
                            nextLevel++;

                            script.LevelInfo.GetComponent<TextMeshProUGUI>().text =
                                $"{builtBuilding.CurrentLevel} >> {nextLevel}";

                            if (_buildingManager.CanUpgradeBuilding(builtBuilding))
                            {
                                script.CreateOrUpgradeBuilding.interactable = true;
                                script.CreateOrUpgradeBuilding.image.color = Color.green;

                                script.CreateOrUpgradeBuilding.onClick.AddListener(() =>
                                    AssigningWorkerHandler(builtBuilding.BuildingMainData, builtBuilding.CurrentLevel,
                                        script, true)); // unabling usual working for defense/resources + kicking out worker
                            }
                            else
                            {
                                script.CreateOrUpgradeBuilding.interactable = false;
                                script.CreateOrUpgradeBuilding.image.color = Color.red;
                            }

                            UpdateRequirementsText(script,
                                builtBuilding.BuildingMainData.PerLevelData[builtBuilding.CurrentLevel].Requirements);
                        }
                        else if (_buildingsOnInPanelQueue.Contains(builtBuilding.BuildingMainData))
                        {
                            script.CreateOrUpgradeBuilding.onClick.AddListener(() =>
                                AssigningWorkerHandler(builtBuilding.BuildingMainData, builtBuilding.CurrentLevel,
                                    script, false));

                            script.BuildingInfo.GetComponent<TextMeshProUGUI>().text = "In Progress";
                            script.LevelInfo.GetComponent<TextMeshProUGUI>().text = "Cancel";

                            script.CreateOrUpgradeBuilding.interactable = true;
                            script.CreateOrUpgradeBuilding.image.color = Color.yellow;
                        }
                        // else if (_buildingsOnCurrentlyBuildingQueue.ContainsKey(buildingData))
                        // {
                        //     if (_buildingsOnCurrentlyBuildingQueue[buildingData]) 
                        //     {
                        //         script.BuildingInfo.GetComponent<TextMeshProUGUI>().text = "In Progress";
                        //         script.LevelInfo.GetComponent<TextMeshProUGUI>().text = "Cancel";
                        //
                        //         script.CreateOrUpgradeBuilding.interactable = true;
                        //         script.CreateOrUpgradeBuilding.image.color = Color.yellow;
                        //
                        //         script.CreateOrUpgradeBuilding.onClick.AddListener(() =>
                        //             AffectWorkingProcess(builtBuilding.BuildingMainData, script, false));
                        //     }
                        //     else 
                        //     {
                        //         script.BuildingInfo.GetComponent<TextMeshProUGUI>().text = "Cancelled";
                        //         script.LevelInfo.GetComponent<TextMeshProUGUI>().text = "Back to Work";
                        //
                        //         script.CreateOrUpgradeBuilding.image.color = Color.blue;
                        //         script.CreateOrUpgradeBuilding.interactable = true;
                        //
                        //         script.CreateOrUpgradeBuilding.onClick.AddListener(() =>
                        //             AffectWorkingProcess(builtBuilding.BuildingMainData, script, true));
                        //     }
                        // }
                        // else
                        // {
                        //     if (builtBuilding.HaveWorker ||
                        //         _buildingsOnInPanelQueue.Contains(builtBuilding
                        //             .BuildingMainData)) // ability to break building process
                        //     {
                        //         script.BuildingInfo.GetComponent<TextMeshProUGUI>().text = "In Progress";
                        //         script.LevelInfo.GetComponent<TextMeshProUGUI>().text = "Cancel";
                        //
                        //         script.CreateOrUpgradeBuilding.interactable = true;
                        //         script.CreateOrUpgradeBuilding.image.color = Color.yellow;
                        //
                        //         script.CreateOrUpgradeBuilding.onClick.AddListener(() =>
                        //             AffectWorkingProcess(builtBuilding.BuildingMainData, script, false));
                        //     }
                        //     else if (!builtBuilding.HaveWorker)
                        //     {
                        //         script.BuildingInfo.GetComponent<TextMeshProUGUI>().text = "Cancelled";
                        //         script.LevelInfo.GetComponent<TextMeshProUGUI>().text = "Back to Work";
                        //
                        //         script.CreateOrUpgradeBuilding.image.color = Color.blue;
                        //         script.CreateOrUpgradeBuilding.interactable = true;
                        //
                        //         script.CreateOrUpgradeBuilding.onClick.AddListener(() =>
                        //             AffectWorkingProcess(builtBuilding.BuildingMainData, script, true));
                        //     }
                        // }
                    }
                    else // is completly not builded - even in building stage
                    {
                        script.BuildingIcon.GetComponent<Image>().sprite = buildingData.PerLevelData[0].Icon;
                        
                        if (_buildingsOnInPanelQueue.Contains(buildingData))
                        {
                            script.BuildingInfo.GetComponent<TextMeshProUGUI>().text = "In Progress";
                            script.LevelInfo.GetComponent<TextMeshProUGUI>().text = "Cancel";

                            script.CreateOrUpgradeBuilding.interactable = true;
                            script.CreateOrUpgradeBuilding.image.color = Color.yellow;  
                            
                            script.CreateOrUpgradeBuilding.onClick.AddListener(() =>
                                AssigningWorkerHandler(buildingData, 0,
                                    script, false));
                        }
                        else
                        {
                            script.LevelInfo.GetComponent<TextMeshProUGUI>().text = "Build";
                            UpdateRequirementsText(script, buildingData.PerLevelData[0].Requirements);

                            if (_buildingManager.CanBuildBuilding(buildingData))
                            {
                                script.CreateOrUpgradeBuilding.image.color = Color.green;
                                script.CreateOrUpgradeBuilding.interactable = true;
                                script.CreateOrUpgradeBuilding.onClick.AddListener(() =>
                                    AssigningWorkerHandler(buildingData, 0, script, true));
                            }
                            else
                            {
                                script.CreateOrUpgradeBuilding.image.color = Color.red;
                                script.CreateOrUpgradeBuilding.interactable = false;
                            }  
                        }
                    }

                    _runtimeBuildingsUiToDestroy.Add(newBuildingUi);
                }
            }
        }

        private void AssigningWorkerHandler(BuildingData p_buildingData, int p_buildingLevel,
            SingleBuildingUi p_newGathering, bool p_assign)
        {
            p_newGathering.CreateOrUpgradeBuilding.onClick.RemoveAllListeners();

            if (p_assign)
            {
                p_newGathering.CreateOrUpgradeBuilding.image.color = Color.cyan; // in progress
                p_newGathering.CreateOrUpgradeBuilding.interactable = true;

                p_newGathering.BuildingInfo.GetComponent<TextMeshProUGUI>().text = "Started Working";
                p_newGathering.LevelInfo.GetComponent<TextMeshProUGUI>().text = "Cancel";

                p_newGathering.CreateOrUpgradeBuilding.onClick.AddListener(() => AssigningWorkerHandler(p_buildingData,
                    p_buildingLevel, p_newGathering, false));
            }
            else
            {
                p_newGathering.CreateOrUpgradeBuilding.image.color = Color.green;
                p_newGathering.CreateOrUpgradeBuilding.interactable = _workersManager.BaseWorkersAmounts > 0;
                var building = _buildingManager.GetSpecificBuilding(p_buildingData);

                if (building == null)
                {
                    UpdateRequirementsText(p_newGathering, p_buildingData.PerLevelData[0].Requirements);
                    p_newGathering.LevelInfo.GetComponent<TextMeshProUGUI>().text = "Build";
                }
                else
                {
                    var nextLevel = building.CurrentLevel;
                    nextLevel++;

                    p_newGathering.LevelInfo.GetComponent<TextMeshProUGUI>().text = $"{building.CurrentLevel} >> {nextLevel}";
                    UpdateRequirementsText(p_newGathering, p_buildingData.PerLevelData[building.CurrentLevel].Requirements);
                }
                
                if (BuildingsToShow.ContainsKey(p_buildingData))
                {
                    BuildingsToShow.Remove(p_buildingData);
                }

                p_newGathering.CreateOrUpgradeBuilding.onClick.AddListener(() => AssigningWorkerHandler(p_buildingData,
                    p_buildingLevel, p_newGathering, true));
            }

            OnButtonClicked(p_buildingData, p_buildingLevel, p_assign);
        }

        // private void AffectWorkingProcess(BuildingData p_buildingData, SingleBuildingUi p_newGathering, bool p_assign)
        // {
        //     p_newGathering.CreateOrUpgradeBuilding.onClick.RemoveAllListeners();
        //
        //     if (p_assign)
        //     {
        //         p_newGathering.CreateOrUpgradeBuilding.image.color = Color.cyan; // in progress
        //         p_newGathering.BuildingInfo.GetComponent<TextMeshProUGUI>().text = "In Progress";
        //         p_newGathering.LevelInfo.GetComponent<TextMeshProUGUI>().text = "Cancel";
        //         p_newGathering.CreateOrUpgradeBuilding.interactable = true;
        //         p_newGathering.CreateOrUpgradeBuilding.onClick.AddListener(() =>
        //             AffectWorkingProcess(p_buildingData, p_newGathering, false));
        //     }
        //     else
        //     {
        //         p_newGathering.CreateOrUpgradeBuilding.image.color = Color.green;
        //         p_newGathering.LevelInfo.GetComponent<TextMeshProUGUI>().text = "Back To Work";
        //         p_newGathering.BuildingInfo.GetComponent<TextMeshProUGUI>().text = "Cancelled";
        //         p_newGathering.CreateOrUpgradeBuilding.interactable = true; //AssignedWorkers < 0;
        //         p_newGathering.CreateOrUpgradeBuilding.onClick.AddListener(() =>
        //             AffectWorkingProcess(p_buildingData, p_newGathering, true));
        //     }
        //
        //     OnAffectingWorkButtonClicked(p_buildingData, p_assign);
        // }

        private void OnButtonClicked(BuildingData p_buildingData, int p_buildingLevel, bool p_assign)
        {
            if (p_assign)
            {
                _workersManager.WorkersInBuilding++;
                _buildingsOnInPanelQueue.Add(p_buildingData);
                _buildingManager.RemoveResourcePoints(p_buildingData, p_buildingLevel);
            }
            else
            {
                _workersManager.WorkersInBuilding--;
                _buildingsOnInPanelQueue.Remove(p_buildingData);
                _buildingManager.AddResourcePoints(p_buildingData, p_buildingLevel);
            }

            UpdateWorkersText();
        }

        // private void OnAffectingWorkButtonClicked(BuildingData p_buildingData, bool p_assign)
        // {
        //     // var building = _buildingManager.CurrentBuildings.Find(x => x.BuildingMainData == p_buildingData);
        //     //
        //     // if (building.IsBeeingUpgradedOrBuilded) // is process already in
        //     // {
        //     //     if (p_assign)
        //     //     {
        //     //         _buildingsOnCurrentlyBuildingQueue.TryAdd(p_buildingData, true);
        //     //         _workersManager.WorkersInBuilding++;
        //     //     }
        //     //     else
        //     //     {
        //     //         _buildingsOnCurrentlyBuildingQueue.Remove(p_buildingData);
        //     //         _workersManager.WorkersInBuilding--;
        //     //     }
        //     // }
        //     // else
        //     // {
        //     if (p_assign)
        //     {
        //         _workersManager.WorkersInBuilding++;
        //     }
        //     else
        //     {
        //         _workersManager.WorkersInBuilding--;
        //     }
        //
        //     if (_buildingsOnCurrentlyBuildingQueue.ContainsKey(p_buildingData))
        //     {
        //         _buildingsOnCurrentlyBuildingQueue[p_buildingData] = p_assign;
        //     }
        //     else
        //     {
        //         _buildingsOnCurrentlyBuildingQueue.Add(p_buildingData, p_assign);
        //     }
        //
        //     UpdateWorkersText();
        // }

        public void ConfirmWorkersAssigment()
        {
            foreach (var buildingData in _buildingsOnInPanelQueue)
            {
                _buildingManager.PutBuildingOnQueue(buildingData);
            }

            // foreach (var buildingData in _buildingsOnCurrentlyBuildingQueue)
            // {
            //     _buildingManager.ModifyBuildingOnQueue(buildingData.Key, buildingData.Value);
            // }

            _buildingsOnInPanelQueue.Clear();
            //_buildingsOnCurrentlyBuildingQueue.Clear();
        }

        private void UpdateWorkersText()
        {
            _numberOfWorkers.text =
                $"Workers: {_workersManager.BaseWorkersAmounts.ToString()}/{_workersManager.WorkersInBuilding}";
        }

        private void UpdateRequirementsText(SingleBuildingUi p_script, Requirements p_requirements)
        {
            p_script.BuildingInfo.GetComponent<TextMeshProUGUI>().text =
                $"Resource Points: {p_requirements.ResourcePoints}\n" +
                $"Days To Complete: {p_requirements.DaysToComplete}\n";
            // +
            //     $"Buildings needed: {p_requirements.OtherBuildingRequirements[0]}\n" +
            //     $"Researches needed: {p_requirements.ResearchRequirements[0]}";
        }

        public void RefreshWorkersAmount()
        {
            _workersManager.WorkersInBuilding = 0;
            
            foreach (var building in _buildingManager.CurrentBuildings)
            {
                if (building.HaveWorker && building.IsBeeingUpgradedOrBuilded)
                {
                    _workersManager.WorkersInBuilding++;
                }
            }

            _workersManager.WorkersInBuilding += _buildingsOnInPanelQueue.Count;
        }
    }
}