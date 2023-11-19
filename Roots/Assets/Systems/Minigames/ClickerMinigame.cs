using Buildings;
using UnityEngine;
using UnityEngine.UI;

namespace Minigames
{
    public class ClickerMinigame : Minigame
    {
        [SerializeField] private Button _buttonToMash;

        private new void Update()
        {
            base.Update();

            if (_timer <= 0)
            {
                _timer = 0;
                _isGameActive = false;
                _collectPointsButton.interactable = true;
                _buttonToMash.interactable = false;
                _timeText.text = $"Click to collect: {_score:F1} resource points";
            }
        }

        public override void StartTheGame(Building p_building)
        {
            base.StartTheGame(p_building);

            _score = 0;
            _buttonToMash.gameObject.GetComponent<Image>().sprite = 
                p_building.BuildingMainData.Technology.SpriteOfObject;
            _buttonToMash.onClick.AddListener(AddScore);
            _buttonToMash.interactable = false;
        }

        public override void AddScore()
        {
            _buttonToMash.interactable = false;
            _score += _efficiency;
            StartInteractableMinigame();
            _scoreText.text = $"Score: {_score:F1}";
        }

        public override void StartInteractableMinigame()
        {
            _buttonToMash.interactable = true;
        }
    }
}