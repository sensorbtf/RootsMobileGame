using UnityEngine;

namespace GeneralSystems
{
    public class CameraController : MonoBehaviour
    {
        public static bool isDragging;
        public static bool IsUiOpen = false;
        public float zoomOutMin = 1;
        public float zoomOutMax = 20;

        public Transform leftBoundaryObject;
        public Transform rightBoundaryObject;
        public Transform topBoundaryObject;
        public Transform bottomBoundaryObject;
        private float _bottomBoundary;
        private Camera _camera;

        private float _leftBoundary;
        private float _rightBoundary;
        private float _topBoundary;

        private Vector3 _touchStart;

        private void Start()
        {
            _camera = Camera.main;

            if (leftBoundaryObject != null) _leftBoundary = leftBoundaryObject.position.x;
            if (rightBoundaryObject != null) _rightBoundary = rightBoundaryObject.position.x;
            if (topBoundaryObject != null) _topBoundary = topBoundaryObject.position.y;
            if (bottomBoundaryObject != null) _bottomBoundary = bottomBoundaryObject.position.y;
        }

        private void Update()
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
                var direction = _touchStart - _camera.ScreenToWorldPoint(Input.mousePosition);

                if (direction.magnitude > 0.1f) // Change this threshold as needed
                    isDragging = true;

                if (isDragging) _camera.transform.position += direction;
            }

            if (Input.GetMouseButtonUp(0)) isDragging = false;

            // Restrict the camera within a boundary box
            var camHeight = _camera.orthographicSize;
            var camWidth = _camera.aspect * camHeight;

            var xMin = _leftBoundary + camWidth;
            var xMax = _rightBoundary - camWidth;
            var yMin = _bottomBoundary + camHeight;
            var yMax = _topBoundary - camHeight;

            var position = _camera.transform.position;
            var clampedPosition = position;
            clampedPosition.x = Mathf.Clamp(position.x, xMin, xMax);
            clampedPosition.y = Mathf.Clamp(position.y, yMin, yMax);
            position = clampedPosition;
            _camera.transform.position = position;

            // Camera Zoom
            var zoom = Mathf.Clamp(_camera.orthographicSize, zoomOutMin, zoomOutMax);
            _camera.orthographicSize = Mathf.Lerp(_camera.orthographicSize, zoom, Time.deltaTime);

            // Zoom with Pinch
            if (Input.touchCount == 2)
            {
                var touchZero = Input.GetTouch(0);
                var touchOne = Input.GetTouch(1);

                var touchZeroPrevPos = touchZero.position - touchZero.deltaPosition;
                var touchOnePrevPos = touchOne.position - touchOne.deltaPosition;

                var prevMagnitude = (touchZeroPrevPos - touchOnePrevPos).magnitude;
                var currentMagnitude = (touchZero.position - touchOne.position).magnitude;

                var difference = currentMagnitude - prevMagnitude;

                Zoom(difference * 0.05f);
            }

            // Zoom with Mouse Wheel
            float scrollData;
            scrollData = Input.GetAxis("Mouse ScrollWheel");

            Zoom(scrollData);
        }

        private void Zoom(float p_increment)
        {
            Camera.main.orthographicSize =
                Mathf.Clamp(_camera.orthographicSize - p_increment, zoomOutMin, zoomOutMax);
        }
    }
}