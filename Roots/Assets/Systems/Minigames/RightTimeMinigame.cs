using Buildings;
using UnityEngine;
using UnityEngine.UI;

namespace Minigames
{
    public class RightTimeMinigame : Minigame
    {
        [SerializeField] private GameObject _movingObject; 
        [SerializeField] private int _moveSpeed;
        [SerializeField] private float _tolerance = 0.5f;
        [SerializeField] private float _blockingDuration = 200f;
        [SerializeField] private Transform _startPosition;
        [SerializeField] private Transform _endPosition;
        [SerializeField] private GameObject _targetPosition; 
        [SerializeField] private Button _buttonToClick; 

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
            
            _timer = 0;
            _isGameActive = false;
            _timeText.text = $"Click to collect: {_score:F1} resource points";
        }

        public override void StartTheGame(Building p_building)
        {
            base.StartTheGame(p_building);
            
            _currentMovementSpeed = _moveSpeed;
            var currentScale = _targetPosition.transform.localScale;
            _targetPosition.transform.localScale = new Vector3(currentScale.x * (1 + p_building.CurrentTechnologyLvl * 0.1f), currentScale.y, currentScale.z);
            
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
            float distance = Mathf.Abs(_movingObject.transform.position.x - _targetPosition.transform.position.x);
            Debug.Log("Distance: " + distance + ", Tolerance: " + _tolerance);

            if (distance < _tolerance)
            {
                AddScore();
            }
            else
            {
                _isBlocked = true;
                _buttonToClick.interactable = false;
            }
        }

        public override void StartInteractableMinigame()
        {
            _collectPointsButton.interactable = true;
        }
    }
}
