using System.Collections.Generic;
using UnityEngine;

namespace Buildings
{
    [CreateAssetMenu(fileName = "New Building Database", menuName = "Building Database")]
    public class BuildingDatabase : ScriptableObject
    {
        public List<BuildingData> allBuildings;
    }
}