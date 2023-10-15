using UnityEngine;
using UnityEngine.EventSystems;

public class PullableObject : MonoBehaviour, IDragHandler, IBeginDragHandler, IEndDragHandler
{
    private bool _isDragging = false;
    private Vector3 _offset;
    private RectTransform _initialPosition;

    public void SetPosition(RectTransform p_initialPosition)
    {
        _initialPosition = p_initialPosition;
    }
    public void OnBeginDrag(PointerEventData eventData)
    {
        _isDragging = true;
        _offset = transform.position - Input.mousePosition;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (_isDragging)
        {
            // Snap the UI element to the touch position (or mouse position).
            transform.position = Input.mousePosition + _offset;
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        _isDragging = false;
        transform.position = _initialPosition.position;
    }
}
