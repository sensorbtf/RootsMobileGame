using System;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    private Vector3 _touchStart;
    public float zoomOutMin = 1;
    public float zoomOutMax = 8;

    public Transform leftBoundaryObject;
    public Transform rightBoundaryObject;
    public Transform topBoundaryObject;
    public Transform bottomBoundaryObject;

    public float leftBoundary;
    public float rightBoundary;
    public float topBoundary;
    public float bottomBoundary;

    
    private void Start()
    {
        if (Camera.main != null) 
            Camera.main.aspect = 9f / 16f; // For a 9:16 ratio so will need to dynamically do it in the future
        
        if (leftBoundaryObject != null) leftBoundary = leftBoundaryObject.position.x;
        if (rightBoundaryObject != null) rightBoundary = rightBoundaryObject.position.x;
        if (topBoundaryObject != null) topBoundary = topBoundaryObject.position.y;
        if (bottomBoundaryObject != null) bottomBoundary = bottomBoundaryObject.position.y;
    }

    void Update()
    {
        // Camera Movement
        if (Input.GetMouseButtonDown(0))
        {
            _touchStart = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        }

        if (Input.GetMouseButton(0))
        {
            Vector3 direction = _touchStart - Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Camera.main.transform.position += direction;
        }

        // Restrict the camera within a boundary box
        float camHeight = Camera.main.orthographicSize;
        float camWidth = Camera.main.aspect * camHeight;

        float xMin = leftBoundary + camWidth;
        float xMax = rightBoundary - camWidth;
        float yMin = bottomBoundary + camHeight;
        float yMax = topBoundary - camHeight;

        Vector3 clampedPosition = Camera.main.transform.position;
        clampedPosition.x = Mathf.Clamp(Camera.main.transform.position.x, xMin, xMax);
        clampedPosition.y = Mathf.Clamp(Camera.main.transform.position.y, yMin, yMax);
        Camera.main.transform.position = clampedPosition;

        // Camera Zoom
        float zoom = Mathf.Clamp(Camera.main.orthographicSize, zoomOutMin, zoomOutMax);
        Camera.main.orthographicSize = Mathf.Lerp(Camera.main.orthographicSize, zoom, Time.deltaTime);

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
        Camera.main.orthographicSize = Mathf.Clamp(Camera.main.orthographicSize - p_increment, zoomOutMin, zoomOutMax);
    }
}
