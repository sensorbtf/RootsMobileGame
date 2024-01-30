using System;
using System.Collections.Generic;
using AudioSystem;
using Buildings;
using GeneralSystems;
using Narrator;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Serialization;
using World;

namespace InGameUi
{
    public class BuildingDefendPanel : MonoBehaviour
    {
        [Header("System Refs")] 
        [SerializeField] private BuildingsManager buildingsManager;
        [SerializeField] private AudioManager _audioManager;
        [SerializeField] private NarratorManager _narratorManager;
        [SerializeField] private WorkersManager _workersManager;
        [SerializeField] private WorldManager _worldManager;
        
        [Header("Object Refs")] 
        [SerializeField] private GameObject _buildingEntryPrefab;
        [SerializeField] private GameObject _endAssigningGo;
        [SerializeField] private Transform contentTransform;
        [SerializeField] private TextMeshProUGUI _numberOfWorkers;
        [SerializeField] private TextMeshProUGUI _panelTitle;

        [Header("Sounds")] 
        [SerializeField] private AudioClip _defendBuildingSound;
        [SerializeField] private AudioClip _undefendBuilding;
        
        private Button _endAssigmentButton;
        
        private List<GameObject> _runtimeBuildingsUiToDestroy;

        private void Start()
        {
            _runtimeBuildingsUiToDestroy = new List<GameObject>();

            _endAssigmentButton = _endAssigningGo.GetComponent<Button>();
            _endAssigmentButton.onClick.AddListener(ClosePanel);

            _worldManager.OnDefendingVillage += OpenPanel;
            gameObject.SetActive(false);
        }

        private void OnDestroy()
        {
            _worldManager.OnDefendingVillage -= OpenPanel;
        }

        private void ClosePanel()
        {
            _audioManager.PlayButtonSoundEffect(_endAssigmentButton.interactable);
            
            foreach (var createdUiElement in _runtimeBuildingsUiToDestroy) 
                Destroy(createdUiElement);

            CameraController.IsUiOpen = false;
            GameplayHud.BlockHud = false;

            _runtimeBuildingsUiToDestroy.Clear();

            _worldManager.HandleEndMissionConsequences(true, true);
            gameObject.SetActive(false);
        }

        private void OpenPanel()
        {
            gameObject.SetActive(true);
            CameraController.IsUiOpen = true;
            GameplayHud.BlockHud = true;

            _narratorManager.TryToActivateNarrator(TutorialStep.OnDefendPanelOpened_Q18);
            
            UpdatePanelState();
            HandleBuildingsCreation();
        }

        private void HandleBuildingsCreation()
        {
            foreach (var building in buildingsManager.CurrentBuildings)
            {
                var newBuilding = Instantiate(_buildingEntryPrefab, contentTransform);
                _runtimeBuildingsUiToDestroy.Add(newBuilding);

                var script = newBuilding.GetComponent<BuildingWorkerRefs>();
                script.BuildingImage.sprite = building.BuildingMainData.Icon;
                script.BuildingName.text = building.BuildingMainData.Type.ToString();
                script.DefendToggle.interactable = true;

                script.DefendToggle.onValueChanged.AddListener(delegate
                {
                    HandleToggleValueChanged(building, script.DefendToggle);
                });
            }
        }

        private void UpdatePanelState()
        {
            _numberOfWorkers.text =
                $"Workers: {_workersManager.BaseWorkersAmounts.ToString()}/{_workersManager.OverallAssignedWorkers}";

            _endAssigningGo.SetActive(!_workersManager.IsAnyWorkerFree());
            RefreshToggles();
        }

        private void RefreshToggles()
        {
            if (_workersManager.IsAnyWorkerFree())
                foreach (var building in _runtimeBuildingsUiToDestroy)
                    building.GetComponent<BuildingWorkerRefs>().DefendToggle.interactable = true;
            else
                foreach (var building in _runtimeBuildingsUiToDestroy)
                {
                    var script = building.GetComponent<BuildingWorkerRefs>();

                    if (!script.DefendToggle.isOn) script.DefendToggle.interactable = false;
                }
        }

        private void HandleToggleValueChanged(Building p_building, Toggle p_scriptDefendToggle)
        {
            if (p_scriptDefendToggle.isOn)
            {
                _audioManager.CreateNewAudioSource(_defendBuildingSound);
                _workersManager.WorkersDefending++;
                p_building.IsProtected = true;
            }
            else
            {
                _audioManager.CreateNewAudioSource(_undefendBuilding);
                _workersManager.WorkersDefending--;
                p_building.IsProtected = false;
            }

            UpdatePanelState();
        }
    }
}