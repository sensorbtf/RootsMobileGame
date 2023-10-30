using System.Collections.Generic;
using UnityEngine;

namespace Gods
{
    [CreateAssetMenu(fileName = "New Gods Database", menuName = "Gods Database")]
    public class GodsDatabase : ScriptableObject
    {
        public List<GodDataSO> AllGods;
    }
}