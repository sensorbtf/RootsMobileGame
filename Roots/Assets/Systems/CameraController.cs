using System;
using UnityEngine;

namespace Systems
{
    public class CameraController : MonoBehaviour
    {
        public static bool isDragging = false; 
        public static bool IsUiOpen = false; 

        private Vector3 _touchStart;
        public float zoomOutMin = 1;
        public float zoomOutMax = 8;

        public Transform leftBoundaryObject;
        public Transform rightBoundaryObject;
        public Transform topBoundaryObject;
        public Transform bottomBoundaryObject;

        private float _leftBoundary;
        private float _rightBoundary;
        private float _topBoundary;
        private float _bottomBoundary;
        private Camera _camera;

        private void Start()
        {
            _camera = Camera.main;
            if (Camera.main != null)
                Camera.main.aspect = 9f / 16f; 

            if (leftBoundaryObject != null) _leftBoundary = leftBoundaryObject.position.x;
            if (rightBoundaryObject != null) _rightBoundary = rightBoundaryObject.position.x;
            if (topBoundaryObject != null) _topBoundary = topBoundaryObject.position.y;
            if (bottomBoundaryObject != null) _bottomBoundary = bottomBoundaryObject.position.y;
        }

        void Update()
        {
            if (IsUiOpen)
                return;
            
            // Camera Movement
            if (Input.GetMouseButtonDown(0))
            {
                isDragging = false;

                _touchStart = _camera.ScreenToWorldPoint(Input.mousePosition);
            }

            if (Input.GetMouseButton(0))
            {
                Vector3 direction = _touchStart - _camera.ScreenToWorldPoint(Input.mousePosition);
                
                if (direction.magnitude > 0.1f) // Change this threshold as needed
                {
                    isDragging = true;
                }

                if (isDragging)
                {
                    _camera.transform.position += direction;
                }
            }

            if (Input.GetMouseButtonUp(0))
            {
                isDragging = false; 
            }

            // Restrict the camera within a boundary box
            float camHeight = _camera.orthographicSize;
            float camWidth = _camera.aspect * camHeight;

            float xMin = _leftBoundary + camWidth;
            float xMax = _rightBoundary - camWidth;
            float yMin = _bottomBoundary + camHeight;
            float yMax = _topBoundary - camHeight;

            var position = _camera.transform.position;
            Vector3 clampedPosition =position;
            clampedPosition.x = Mathf.Clamp(position.x, xMin, xMax);
            clampedPosition.y = Mathf.Clamp(position.y, yMin, yMax);
            position = clampedPosition;
            _camera.transform.position = position;

            // Camera Zoom
            float zoom = Mathf.Clamp(_camera.orthographicSize, zoomOutMin, zoomOutMax);
            _camera.orthographicSize = Mathf.Lerp(_camera.orthographicSize, zoom, Time.deltaTime);

            // Zoom with Pinch
            if (Input.touchCount == 2)
            {
                Touch touchZero = Input.GetTouch(0);
                Touch touchOne = Input.GetTouch(1);

                Vector2 touchZeroPrevPos = touchZero.position - touchZero.deltaPosition;
                Vector2 touchOnePrevPos = touchOne.position - touchOne.deltaPosition;

                float prevMagnitude = (touchZeroPrevPos - touchOnePrevPos).magnitude;
                float currentMagnitude = (touchZero.position - touchOne.position).magnitude;

                float difference = currentMagnitude - prevMagnitude;

                Zoom(difference * 0.01f);
            }

            // Zoom with Mouse Wheel
            float scrollData;
            scrollData = Input.GetAxis("Mouse ScrollWheel");

            Zoom(scrollData);
        }

        void Zoom(float p_increment)
        {
            Camera.main.orthographicSize =
                Mathf.Clamp(_camera.orthographicSize - p_increment, zoomOutMin, zoomOutMax);
        }
    }
}