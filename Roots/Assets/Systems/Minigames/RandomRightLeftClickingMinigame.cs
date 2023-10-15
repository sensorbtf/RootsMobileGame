using Buildings;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;
namespace Minigames
{
    public class RandomRightLeftClickingMinigame :Minigame
    {
        [SerializeField] private Button _leftSideButton;
        [SerializeField] private Button _rightSideButton;

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
            base.Update();

            if (_timer <= 0)
            {
                _timer = 0;
                _isGameActive = false;
                _collectPointsButton.interactable = true;
                _leftSideButton.interactable = false;
                _rightSideButton.interactable = false;
                _timeText.text = $"Click to collect: {_score} resource points";
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