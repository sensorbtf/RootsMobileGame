using Buildings;
using TMPro;
using UnityEngine;
using World;
using UnityEngine.UI;

namespace InGameUi
{
    public class GameplayHud: MonoBehaviour
    {
        public static bool BlockHud;
        
        [SerializeField] private WorldManager _worldManager;
        [SerializeField] private BuildingManager _buildingManager;
        
        [SerializeField] private TextMeshProUGUI CurrentDay;
        [SerializeField] private TextMeshProUGUI DayToStorm;
        [SerializeField] private TextMeshProUGUI ResourcePoints;
        [SerializeField] private TextMeshProUGUI DefensePoints;
        [SerializeField] private TextMeshProUGUI ShardsOfDestiny;
        
        [SerializeField] private Button SkipDayButton;

        private void Update() // Better way to do it?
        {
            CurrentDay.text = $"Current day: {_worldManager.CurrentDay.ToString()}";
            DayToStorm.text = $"Storm in: {_worldManager.StormDaysRange.ToString()}";
            ResourcePoints.text = $"Resource Points: {_buildingManager.CurrentResourcePoints.ToString()} / " +
                                  $"{_worldManager.NeededResourcePoints}";
            DefensePoints.text = $"Defense Points: {_buildingManager.CurrentDefensePoints.ToString()}";
            ShardsOfDestiny.text = $"Shards Of Destiny: {_buildingManager.ShardsOfDestinyAmount.ToString()}";

            if (BlockHud)
            {
                SkipDayButton.interactable = false;
            }
            else
            {
                SkipDayButton.interactable = true;
            }
        }

        public void SkipDayOnClick()
        {
            _worldManager.SkipDay();
            Debug.Log("Skipped Day");
        }
    }
}