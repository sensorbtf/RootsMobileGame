using System;
using UnityEngine;
using UnityEngine.EventSystems;

public class PullableObject : MonoBehaviour, IDragHandler, IBeginDragHandler, IEndDragHandler
{
    [SerializeField] private RectTransform _initialPosition;
    [SerializeField] private Vector3 _initialPos;
    private bool _isDragging;
    private bool _isGameOn;
    private Vector3 _offset;

    public event Action OnBeginDraw;
    
    public void SetPosition(RectTransform p_initialPosition)
    {
        _isGameOn = true;
        _initialPosition.position = p_initialPosition.position;
        _initialPos = p_initialPosition.position;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (_isGameOn)
        {
            _isDragging = true;
            OnBeginDraw.Invoke();
            _offset = transform.position - Input.mousePosition;
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (_isDragging && _isGameOn) transform.position = Input.mousePosition + _offset;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (_isGameOn)
        {
            _isDragging = false;
            transform.position = _initialPos;
        }
    }

    public void EndMinigame()
    {
        _isGameOn = false;
        transform.position = _initialPos;
        _isDragging = false;
    }
}