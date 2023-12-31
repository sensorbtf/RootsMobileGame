using System.Collections;
using System.Collections.Generic;
using Buildings;
using InGameUi;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(MinigamesPanel))]
public class MinigamesDebugger : Editor
{
    BuildingType selectedBuildingType;

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        MinigamesPanel minigames = (MinigamesPanel)target;

        // Dropdown to select the building type
        selectedBuildingType = (BuildingType)EditorGUILayout.EnumPopup("Select Building Type", selectedBuildingType);

        if (GUILayout.Button("Invoke DebugOpenMinigame"))
        {
            minigames.DebugOpenMinigame(selectedBuildingType);
        }
    }
}