using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class SingleBuildingUi : MonoBehaviour
{
    [FormerlySerializedAs("CreateBuilding")] public Button CreateOrUpgradeBuilding;
    public Image BuildingIcon;
    public TextMeshProUGUI BuildingName;
    public TextMeshProUGUI LevelInfo;
    public TextMeshProUGUI BuildingInfo;
}