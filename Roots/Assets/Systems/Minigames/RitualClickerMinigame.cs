using System;
using Buildings;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Minigames
{
    public class RitualClickerMinigame : Minigame
    {
        [SerializeField] private Button _button;
        [SerializeField] private Slider _slider;

        private new void Update()
        {
            _timeText.text = $"Achieve {_timer} points";

            if (_score >= _timer) // needed points
            {
                _timer = 0;
                _isGameActive = false;
                _collectPointsButton.interactable = true;
                _button.interactable = false;
                _timeText.text = $"Collect: {_score:F0} resource points";
            }
        }

        public override void SetupGame(Building p_building)
        {
            base.SetupGame(p_building);

            _score = 0;
            _slider.value = 0;

            _button.onClick.AddListener(AddScore);
            _button.interactable = false;
            _collectPointsButton.interactable = false;
        }

        public override void AddScore()
        {
            _button.interactable = false;
            
            var realEfficiency = _slider.maxValue / _efficiency;
            _slider.value += realEfficiency;
            _score += realEfficiency;
            
            StartMinigame();
            _scoreText.text = $"Score: {_score:F0}";
        }

        public override void StartMinigame()
        {
            _button.interactable = true;
        }
    }
}