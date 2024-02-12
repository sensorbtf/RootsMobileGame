using System;
using System.Collections.Generic;
using Buildings;
using UnityEngine;
using UnityEngine.UI;

namespace Minigames
{
    public class AlchemicalHutMinigame : Minigame
    {
        [SerializeField] private GameObject _movingObject; 
        [SerializeField] private RectTransform _movingObjectRect; 
        [SerializeField] private int _moveSpeed;
        [SerializeField] private float _blockingDuration = 200f;
        [SerializeField] private RectTransform _startPosition;
        [SerializeField] private RectTransform _endPosition;
        [SerializeField] private RectTransform _targetPosition; 
        [SerializeField] private Button _buttonToClick; 
        [SerializeField] private BoxCollider2D _movingObjectCollider;
        [SerializeField] private BoxCollider2D _targetPositionCollider;
        [SerializeField] private BoxCollider2D _caldurionCollider;
        
        [SerializeField] private AudioClip _intoCaldurion;
        [SerializeField] private AudioClip _failedToClick;

        private bool _isMovingToEnd = true;
        private bool _isBlocked;
        private float _toBlockTimer;
        private int _bonusPerClick;
        private int _currentMovementSpeed;
        private GameObject _herbsToMove;

        private new void Update()   
        {
            base.Update();

            if (!_isGameActive)
                return;

            MoveObject();

            if (_isBlocked)
            {
                HandleBlocking();
            }

            if (_timer <= 0)
            {
                EndGame();
            }
        }

        private void MoveObject()
        {
            Vector2 targetAnchoredPosition = _isMovingToEnd ? _endPosition.anchoredPosition : _startPosition.anchoredPosition;

            float step = _currentMovementSpeed * Time.deltaTime;

            _movingObjectRect.anchoredPosition = Vector2.MoveTowards(_movingObjectRect.anchoredPosition,
                targetAnchoredPosition, step);

            if (Vector2.Distance(_movingObjectRect.anchoredPosition, targetAnchoredPosition) < 0.1f)
            {
                _isMovingToEnd = !_isMovingToEnd;
            }

            if (_herbsToMove != null)
            {
                _herbsToMove.transform.position = Vector2.MoveTowards(_herbsToMove.transform.position,
                    _caldurionCollider.gameObject.transform.position, step * 4);
                
                Vector2 boxSize = _herbsToMove.GetComponent<BoxCollider2D>().size;
                var transform1 = _herbsToMove.transform;
                Vector2 boxPosition = transform1.position;
                float angle = transform1.eulerAngles.z;

                ContactFilter2D filter = new ContactFilter2D().NoFilter();

                Collider2D[] results = Physics2D.OverlapBoxAll(boxPosition, boxSize, angle, filter.layerMask);

                if (Array.Exists(results, collider => collider == _caldurionCollider))
                {
                    _audioManager.CreateNewAudioSource(_intoCaldurion);
                    Destroy(_herbsToMove);
                }
            }
        }

        private void HandleBlocking()
        {
            if (_toBlockTimer < _blockingDuration)
            {
                _toBlockTimer += Time.deltaTime;
            }
            else
            {
                _buttonToClick.interactable = true;
                _isBlocked = false;
                _toBlockTimer = 0;
                _movingObject.GetComponent<Image>().color = Color.white;
            }
        }

        private void EndGame()
        {
            _buttonToClick.interactable = false;
            _collectPointsButton.interactable = true;
            if (_herbsToMove != null)
            {
                Destroy(_herbsToMove);
            }
            _timer = 0;
            _isGameActive = false;
        }

        public override void SetupGame(Building p_building)
        {
            base.SetupGame(p_building);
    
            _currentMovementSpeed = _moveSpeed;

            float newWidth = _targetPosition.rect.width * (1 + p_building.CurrentTechnologyLvl * 0.1f);

            Vector2 newSizeDelta = _targetPosition.sizeDelta;
            newSizeDelta.x = newWidth;
            _targetPosition.sizeDelta = newSizeDelta;

            Vector2 colliderSize = _targetPositionCollider.size;
            colliderSize.x = newWidth;
            _targetPositionCollider.size = colliderSize;

            _buttonToClick.onClick.AddListener(TryToGetPoints);
            _score = 0;
        }
        
        private void TryToGetPoints()
        {
            Vector2 boxSize = _movingObjectCollider.size;
            var transform1 = _movingObjectCollider.transform;
            Vector2 boxPosition = transform1.position;

            float angle = transform1.eulerAngles.z;

            ContactFilter2D filter = new ContactFilter2D().NoFilter();

            Collider2D[] results = Physics2D.OverlapBoxAll(boxPosition, boxSize, angle, filter.layerMask);

            if (Array.Exists(results, collider => collider == _targetPositionCollider))
            {
                AddScore();
            }
            else
            {
                _isBlocked = true;
                _movingObject.GetComponent<Image>().color = Color.red;
                _buttonToClick.interactable = false;
                _audioManager.CreateNewAudioSource(_failedToClick);
            }
        }
        
        public override void AddScore()
        {
            _currentMovementSpeed += _bonusPerClick;
            _score += _efficiency;
            _herbsToMove = Instantiate(_movingObject, _movingObjectRect);
            _herbsToMove.transform.position = _movingObjectRect.position;
            base.AddScore();
        }

        public override void StartMinigame()
        {
            _collectPointsButton.interactable = false;
        }
    }
}
