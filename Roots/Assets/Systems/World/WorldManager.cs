using Buildings;
using UnityEngine;

namespace World
{
    public class WorldManager : MonoBehaviour
    {
        [SerializeField] private BuildingManager _buildingManager;

        private int _neededResourcePoints = 0;
        private int _resourcePoint = 0;
        private int _currentDay = 0;
        private int _finalHiddenStormDay = 0;
        private Vector2Int _stormPower;
        private Vector2Int _stormDaysRange;
        // nad wszystkimi innymi systemami. Ma info o aktualnie zbudowanych budynkach i na tej podstawie m.in.

        private void EndDay()
        {
            _resourcePoint += _buildingManager.GatherProductionPointsFromBuildings();
            
            //do other thins (days)
        }
        
        private void EndMission()
        {
            _resourcePoint += _buildingManager.GatherProductionPointsFromBuildings();
            
            //get resources from basement
        }
    }
}