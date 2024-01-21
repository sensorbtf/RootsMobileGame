using System;
using Buildings;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Minigames
{
    public class RitualCircleMinigame : Minigame
    {
        [SerializeField] private Button _button;
        [SerializeField] private Slider _slider;

        private new void Update()
        {
            if (_score >= _timer) // needed points
            {
                _timer = 0;
                _isGameActive = false;
                _collectPointsButton.interactable = true;
                _button.interactable = false;
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
        }

        public override void StartMinigame()
        {
            _button.interactable = true;
        }
    }
}