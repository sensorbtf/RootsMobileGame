using System;
using AudioSystem;
using Buildings;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

namespace Minigames
{
    public class RandomRightLeftClickingMinigame : Minigame
    {
        [SerializeField] private Button _leftSideButton;
        [SerializeField] private Button _rightSideButton;

        [SerializeField] private AudioClip[] _hitSounds;
        
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
                _timeText.text = $"Collect: {_score:F0} resource points";
            }
        }

        public override void SetupGame(Building p_building)
        {
            base.SetupGame(p_building);
            
            _leftSideButton.onClick.AddListener(AddScore);
            _rightSideButton.onClick.AddListener(AddScore);
            _leftSideButton.interactable = false;
            _rightSideButton.interactable = false;
        }

        public override void AddScore()
        {
            _audioManager.CreateNewAudioSource(_hitSounds[Random.Range(0, _hitSounds.Length)]);
            
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