using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Systems
{
    [Serializable]
    public class Building : MonoBehaviour, IPointerClickHandler
    {
        public BuildingData buildingData;
        public int level;

        public static event Action<BuildingData, int> OnBuildingClicked; 

        private void Start()
        {
            if (buildingData != null)
            {
                // Initialize any properties you want here, such as:
                level = 1; 
            }
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (!CameraController.isDragging)
            {
                // Handle the building click here
                OnBuildingClicked?.Invoke(buildingData, level);
            }
        }
    }
}
