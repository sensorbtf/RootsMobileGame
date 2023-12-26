using Buildings;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Minigames
{
    public class RandomRightLeftClickingMinigame : Minigame
    {
        [SerializeField] private Button _leftSideButton;
        [SerializeField] private Button _rightSideButton;

        private new void Update()
        {
            base.Update();

            if (_timer <= 0)
            {
                _timer = 0;
                _isGameActive = false;
                _collectPointsButton.interactable = true;
                _leftSideButton.interactable = false;
                _rightSideButton.interactable = false;
                _timeText.text = $"Click to collect: {_score:F1} resource points";
            }
        }

        public override void SetupGame(Building p_building)
        {
            base.SetupGame(p_building);

            _score = 0;

            _leftSideButton.onClick.AddListener(AddScore);
            _rightSideButton.onClick.AddListener(AddScore);
            _leftSideButton.interactable = false;
            _rightSideButton.interactable = false;
        }

        public override void AddScore()
        {
            _score += _efficiency;
            StartMinigame();
            _scoreText.text = $"Score: {_score:F1}";
        }

        public override void StartMinigame()
        {
            if (Random.Range(0, 2) == 0)
            {
                _leftSideButton.interactable = true;
                _rightSideButton.interactable = false;
            }
            else
            {
                _rightSideButton.interactable = true;
                _leftSideButton.interactable = false;
            }
        }
    }
}