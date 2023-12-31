using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Buildings;
using InGameUi;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(BuildingsManager))]
public class BuildingsDebugger : Editor
{
    BuildingType selectedBuildingType;

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        GUILayout.TextArea("Debugging: ");
        BuildingsManager buildings = (BuildingsManager)target;
        
        selectedBuildingType = (BuildingType)EditorGUILayout.EnumPopup("Select Building Type", selectedBuildingType);

        if (GUILayout.Button("Invoke HandleBuiltOfBuilding"))
        {
            buildings.HandleBuiltOfBuilding(buildings.AllBuildingsDatabase.allBuildings.First(x => x.Type == selectedBuildingType), true);
        }
        
        if (GUILayout.Button("Invoke HandleUpgradeOfBuilding"))
        {
            buildings.HandleUpgradeOfBuilding(selectedBuildingType, true);
        }
        
        if (GUILayout.Button("Invoke UpgradeTechnologyLevel"))
        {
            buildings.GetSpecificBuilding(selectedBuildingType).UpgradeTechnologyLevel();
        }
    }
}