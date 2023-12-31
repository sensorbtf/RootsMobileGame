using UnityEngine;

namespace World
{
    [CreateAssetMenu(fileName = "MissionsData", menuName = "ScriptableObjects/MissionsData", order = 3)]
    public class MissionsSO : ScriptableObject
    {
        public Mission[] AllMissions;
    }
}