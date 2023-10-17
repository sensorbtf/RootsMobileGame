using UnityEngine;
using UnityEngine.EventSystems;

public class PullableObject : MonoBehaviour, IDragHandler, IBeginDragHandler, IEndDragHandler
{
    private bool _isGameOn = false;
    private bool _isDragging = false;
    private Vector3 _offset;
    private RectTransform _initialPosition;

    public void SetPosition(RectTransform p_initialPosition)
    {
        _isGameOn = true;
        _initialPosition = p_initialPosition;
    }
    public void OnBeginDrag(PointerEventData eventData)
    {
        if (_isGameOn)
        {
            _isDragging = true;
            _offset = transform.position - Input.mousePosition;
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (_isDragging && _isGameOn)
        {
            transform.position = Input.mousePosition + _offset;
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (_isGameOn)
        {
            _isDragging = false;
            transform.position = _initialPosition.position;
        }
    }

    public void EndMinigame()
    {
        _isGameOn = false;
        transform.position = _initialPosition.position;
        _isDragging = false;
    }
}
