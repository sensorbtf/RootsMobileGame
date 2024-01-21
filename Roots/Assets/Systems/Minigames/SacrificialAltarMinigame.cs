using System;
using Buildings;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Minigames
{
    public class SacrificialAltarMinigame : Minigame
    {
        [SerializeField] private Button _button;

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

            _button.onClick.AddListener(AddScore);
            _button.interactable = false;
            _collectPointsButton.interactable = false;
        }

        public override void AddScore()
        {
            _button.interactable = false;

            _score += _efficiency;
            
            StartMinigame();
        }

        public override void StartMinigame()
        {
            _button.interactable = true;
        }
    }
}