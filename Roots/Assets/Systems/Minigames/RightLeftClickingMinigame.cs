using Buildings;
using System;
using UnityEngine;
using UnityEngine.UI;

namespace Minigames
{
    public class RightLeftClickingMinigame: Minigame
    {
        [SerializeField] private Button _leftSideButton;
        [SerializeField] private Button _rightSideButton;

        public event Action<int> OnStormReveal;

        public override void StartTheGame(Building p_building)
        {
            base.StartTheGame(p_building);

            _score = 0;

            _leftSideButton.onClick.AddListener(AddScore);
            _rightSideButton.onClick.AddListener(AddScore);
            _leftSideButton.interactable = false;
            _rightSideButton.interactable = false;

        }

        private new void Update()
        {
            _timeText.text = $"Achieve {_timer} points";

            if (_score >= _timer) // needed points
            {
                _timer = 0;
                _isGameActive = false;
                _collectPointsButton.interactable = true;
                _leftSideButton.interactable = false;
                _rightSideButton.interactable = false;
                _timeText.text = $"Check storm in 2 days";
                OnStormReveal?.Invoke((int)_efficiency);
            }
        }

        public override void AddScore()
        {
            _score += _efficiency;
            StartInteractableMinigame();
            _scoreText.text = $"Score: {_score}";
        }

        public override void StartInteractableMinigame()
        {
            if (_leftSideButton.interactable)
            {
                _leftSideButton.interactable = false;
                _rightSideButton.interactable = true;
            }
            else
            {
                _leftSideButton.interactable = true;
                _rightSideButton.interactable = false;
            }
        }
    }
}