using System;
using System.Collections.Generic;
using AudioSystem;
using UnityEngine;
using UnityEngine.EventSystems;
using Random = UnityEngine.Random;

namespace GeneralSystems
{
    public class CameraController : MonoBehaviour
    {
        [SerializeField] private AudioManager _audioManager;
        [SerializeField] private AudioClip[] _winds;
        
        public static bool isDragging;
        public static bool IsUiOpen = false;
        public static bool WasZoomedIn = false;
        public float zoomOutMin = 1;
        public float zoomOutMax = 20;

        public float zoomSpeed = 5f; 
        public float zoomOutFactor = 20f; 

        private bool shouldRestoreZoom = false;
        private bool _startedOnGo = false;

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

            IsUiOpen = false;
        }

        private void Update()
        {
            HandleZoomRestoration();
            CheckGoDetection();
            
            if (_startedOnGo)
            {
                return;
            }
            
            if (IsUiOpen)
            {
                if (!WasZoomedIn)
                {
                    _camera.orthographicSize = zoomOutMin;
                    WasZoomedIn = true;
                }

                return;
            }

            WasZoomedIn = false;

            HandleCameraMovementAndZoom();
            HandleCameraBoundaries();
            HandleInputBasedZoom();
        }
        
        private bool IsPointerOverUIObjectOnLayer(string layerName)
        {
            Vector2 inputPosition;
            if (Input.GetMouseButton(0)) // If there's a mouse click
            {
                inputPosition = Input.mousePosition;
            }
            else if (Input.touchCount > 0) // If there's a touch input
            {
                inputPosition = Input.GetTouch(0).position;
            }
            else
            {
                return false; // No input detected
            }

            PointerEventData pointerData = new PointerEventData(EventSystem.current)
            {
                position = inputPosition
            };

            List<RaycastResult> results = new List<RaycastResult>();
            EventSystem.current.RaycastAll(pointerData, results);

            foreach (RaycastResult result in results)
            {
                if (result.gameObject.layer == LayerMask.NameToLayer(layerName))
                {
                    return true;
                }
            }

            return false;
        }


        private void CheckGoDetection()
        {
            if (!_startedOnGo)
            {
                if (Input.GetMouseButtonDown(0) || (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began))
                {
                    if (IsPointerOverUIObjectOnLayer("UI"))
                    {
                        _startedOnGo = true;
                        return;
                    }

                    var inputPosition = Input.GetMouseButton(0) ? new Vector2(Input.mousePosition.x, Input.mousePosition.y) : new Vector2(Input.GetTouch(0).position.x, Input.GetTouch(0).position.y);
                    var ray = Camera.main.ScreenPointToRay(inputPosition);

                    RaycastHit2D hit = Physics2D.Raycast(ray.origin, ray.direction);

                    if (hit.collider != null)
                    {
                        if (hit.collider.gameObject.layer == LayerMask.NameToLayer("UI"))
                        {
                            _startedOnGo = true;
                        }
                    }
                }
            }

            if (_startedOnGo)
            {
                if (Input.GetMouseButtonUp(0) || (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Ended))
                {
                    _startedOnGo = false;
                }
            }
        }

        private void HandleCameraMovementAndZoom()
        {
            if (Input.GetMouseButtonDown(0))
            {
                _touchStart = _camera.ScreenToWorldPoint(Input.mousePosition);
            }

            if (Input.GetMouseButton(0))
            {
                Vector3 direction = _touchStart - _camera.ScreenToWorldPoint(Input.mousePosition);
                if (direction.magnitude > 1f) 
                {
                    isDragging = true;
                    ZoomOutWhileMoving();
                    _camera.transform.position += direction;
                }
            }

            if (Input.GetMouseButtonUp(0))
            {
                isDragging = false;
            }
        }

        private void HandleZoomRestoration()
        {
            if (!isDragging && Math.Abs(_camera.orthographicSize - zoomOutMin) > 0.5f)
            {
                _camera.orthographicSize = Mathf.Lerp(_camera.orthographicSize, zoomOutMin, zoomSpeed * Time.deltaTime);
            }
        }

        private void HandleCameraBoundaries()
        {
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
        }

        private void HandleInputBasedZoom()
        {
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

        private void ZoomOutWhileMoving()
        {
            _audioManager.PlayWindEffect(_winds[Random.Range(0, _winds.Length)]);
            
            _camera.orthographicSize = Mathf.Clamp(_camera.orthographicSize + zoomOutFactor, zoomOutMin, zoomOutMax);
        }

        private void Zoom(float p_increment)
        {
            Camera.main.orthographicSize = Mathf.Clamp(_camera.orthographicSize - p_increment, zoomOutMin, zoomOutMax);
        }
    }
}