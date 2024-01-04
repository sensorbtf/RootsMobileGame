using System;
using System.Collections.Generic;
using Buildings;
using UnityEngine;
using UnityEngine.UI;

namespace Minigames
{
    public class RightTimeMinigame : Minigame
    {
        [SerializeField] private GameObject _movingObject; 
        [SerializeField] private int _moveSpeed;
        [SerializeField] private float _blockingDuration = 200f;
        [SerializeField] private Transform _startPosition;
        [SerializeField] private Transform _endPosition;
        [SerializeField] private RectTransform _targetPosition; 
        [SerializeField] private Button _buttonToClick; 
        [SerializeField] private BoxCollider2D _movingObjectCollider;
        [SerializeField] private BoxCollider2D _targetPositionCollider;

        private bool _isMovingToEnd = true;
        private bool _isBlocked;
        private float _toBlockTimer;
        private int _bonusPerClick;
        private int _currentMovementSpeed;
        
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
            var target = _isMovingToEnd ? _endPosition.position : _startPosition.position;
            _movingObject.transform.position = Vector2.MoveTowards(_movingObject.transform.position,
                target, _currentMovementSpeed * Time.deltaTime);

            if (Vector2.Distance(_movingObject.transform.position, target) < 0.1f)
            {
                _isMovingToEnd = !_isMovingToEnd;
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
            }
        }

        private void EndGame()
        {
            _buttonToClick.interactable = false;
            _collectPointsButton.interactable = true;

            _timer = 0;
            _isGameActive = false;
            _timeText.text = $"Click to collect: {_score:F0} resource points";
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

        public override void AddScore()
        {
            _currentMovementSpeed += _bonusPerClick;
            _score += _efficiency;
            _scoreText.text = $"Score: {_score:F1}";
        }
        
        private void TryToGetPoints()
        {
            // Define the size and position of the overlap box
            Vector2 boxSize = _targetPositionCollider.size;
            Vector2 boxPosition = _targetPositionCollider.transform.position;

            // Define the angle of the box, usually the same as the target collider
            float angle = _targetPositionCollider.transform.eulerAngles.z;

            // Create a contact filter that doesn't filter out any results
            ContactFilter2D filter = new ContactFilter2D().NoFilter();

            // Perform the overlap box check
            Collider2D[] results = Physics2D.OverlapBoxAll(boxPosition, boxSize, angle, filter.layerMask);

            // Check if the moving object's collider is in the results array
            if (Array.Exists(results, collider => collider == _movingObjectCollider))
            {
                // If the moving object is in the target area, add score
                AddScore();
                Debug.Log("Score added: Moving object is inside the target area.");
            }
            else
            {
                // If not, handle the scenario where the object is outside the target area
                _isBlocked = true;
                _buttonToClick.interactable = false;
                Debug.Log("Moving object is outside the target area.");
            }
        }


        private void TryToGetPo2ints()
        {
            List<Collider2D> results = new List<Collider2D>();
            ContactFilter2D filter = new ContactFilter2D().NoFilter();

            _targetPositionCollider.Overlap(filter, results);

            if (results.Contains(_movingObjectCollider))
            {
                AddScore();
            }
            else
            {
                _isBlocked = true;
                _buttonToClick.interactable = false;
            }
        }
        
        public override void StartMinigame()
        {
            _collectPointsButton.interactable = false;
        }
    }
}
