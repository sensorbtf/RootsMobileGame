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

        public override void SetupGame(Building p_building)
        {
            base.SetupGame(p_building);

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
            StartMinigame();
            _scoreText.text = $"Score: {_score:F1}";
        }

        public override void StartMinigame()
        {
            _buttonToMash.interactable = true;
        }
    }
}