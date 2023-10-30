using System;
using System.Collections;
using Buildings;using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Minigames
{
    public abstract class Minigame : MonoBehaviour
    {
        [HideInInspector] public float _timer;
        [HideInInspector] public float _efficiency;
        [HideInInspector] public bool _isGameActive;
        [HideInInspector] public bool _givesResources;
        [HideInInspector] public float _score;
        [HideInInspector] public PointsType _type;
        public Button _collectPointsButton;
        public TextMeshProUGUI _coutdownText;
        public TextMeshProUGUI _scoreText;

        public TextMeshProUGUI _timeText;

        public virtual void Update()
        {
            if (!_isGameActive)
                return;

            UpdateTimerText();
        }

        public event Action OnMinigameEnded;
        public event Action<PointsType, int> OnMiniGamePointsCollected;

        public virtual void StartTheGame(Building p_building)
        {
            _score = 0;

            _timer = p_building.BuildingMainData.Technology.DataPerTechnologyLevel[p_building.CurrentTechnologyLvl]
                .MinigameDuration;
            _efficiency = p_building.BuildingMainData.Technology.DataPerTechnologyLevel[p_building.CurrentTechnologyLvl]
                .Efficiency;
            _type = p_building.ProductionType;

            _collectPointsButton.onClick.AddListener(EndMinigame);
            _collectPointsButton.interactable = false;

            if (p_building.BuildingMainData.Type == BuildingType.GuardTower)
                _givesResources = false;
            else
                _givesResources = true;

            StartCoroutine(StartCountdown());
        }

        private void EndMinigame()
        {
            if (_givesResources)
                OnMiniGamePointsCollected?.Invoke(_type, (int)_score);

            OnMinigameEnded?.Invoke();
        }

        private void UpdateTimerText()
        {
            _timer -= Time.deltaTime;
            _timeText.text = Mathf.FloorToInt(_timer).ToString();
        }

        private IEnumerator StartCountdown()
        {
            var count = 3;

            while (count > 0)
            {
                _coutdownText.text = count + "...";
                yield return new WaitForSeconds(1);
                count--;
            }

            _coutdownText.text = "Start!";
            StartInteractableMinigame();
            _isGameActive = true;
            yield return new WaitForSeconds(0.5f);
            _coutdownText.enabled = false;
        }

        public abstract void AddScore();

        public abstract void StartInteractableMinigame();
    }
}