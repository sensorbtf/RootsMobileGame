using UnityEngine;
using System.Collections.Generic;

namespace Systems
{
    [CreateAssetMenu(fileName = "New Building Database", menuName = "Building Database")]
    public class BuildingDatabase : ScriptableObject
    {
        public List<BuildingData> allBuildings;
    }
}