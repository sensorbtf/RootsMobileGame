using GeneralSystems;
using UnityEngine;
using UnityEngine.EventSystems; // Required for Event Systems

namespace InGameUi
{
    public class ButtonPressHandler : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
    {
        public void OnPointerDown(PointerEventData eventData)
        {
            CameraController.IsUiOpen = true;
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            CameraController.IsUiOpen = false;
        }
    }
}