using Systems;
using UnityEngine;

public class BuildingManager : MonoBehaviour
{
    [SerializeField] private BuildingPanel _buildingPanel; 
    
    private void OnEnable()
    {
        Building.OnBuildingClicked += HandleBuildingClicked;
    }

    private void OnDisable()
    {
        Building.OnBuildingClicked -= HandleBuildingClicked;
    }

    private void HandleBuildingClicked(BuildingData p_buildingData, int p_level)
    {
        Debug.Log($"Building clicked: {p_buildingData}, Level: {p_level}");

        _buildingPanel.ActivateOnClick(p_buildingData, p_level);
    }
}