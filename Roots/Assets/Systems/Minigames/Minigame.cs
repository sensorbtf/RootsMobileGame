using Buildings;
using System;
using System.Collections.Generic;
using System.Linq;
using System;
using System.Collections;
using Buildings;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

namespace Minigames
{
    public abstract class Minigame : MonoBehaviour
    {
        [HideInInspector] public float _timer;
        [HideInInspector] public float _efficiency;
        [HideInInspector] public bool _isGameActive;
        [HideInInspector] public float _score = 0;
        [HideInInspector] public PointsType _type;

        public TextMeshProUGUI _timeText;
        public TextMeshProUGUI _scoreText;
        public TextMeshProUGUI _coutdownText;
        public Button _collectPointsButton;
        
        public event Action<PointsType, int> OnMiniGamePointsCollected;

        public void StartTheGame(Building p_building)
        {
            _score = 0;

            _timer = p_building.BuildingMainData.Technology.DataPerTechnologyLevel[p_building.CurrentTechnologyLvl].MinigameDuration;
            _efficiency = p_building.BuildingMainData.Technology.DataPerTechnologyLevel[p_building.CurrentTechnologyLvl].Efficiency;
            _type = p_building.ProductionType;

            _collectPointsButton.onClick.AddListener(EndMinigame);
            _collectPointsButton.interactable = false;

            StartCoroutine(StartCountdown());
        }

        private void EndMinigame()
        {
            OnMiniGamePointsCollected?.Invoke(_type, (int)_score);
        }

        public virtual void Update()
        {
            if (!_isGameActive)
                return;

            UpdateTimerText();
        }

        private void UpdateTimerText()
        {
            _timer -= Time.deltaTime;
            _timeText.text = Mathf.FloorToInt(_timer).ToString();
        }

        private IEnumerator StartCountdown()
        {
            int count = 3;

            while (count > 0)
            {
                _coutdownText.text = count + "...";
                yield return new WaitForSeconds(1);
                count--;
            }

            _coutdownText.text = "Start!";
            _isGameActive = true;
            StartInteractableMinigame();
            yield return new WaitForSeconds(0.5f);
            _coutdownText.enabled = false;
        }

        public abstract void AddScore();

        public abstract void StartInteractableMinigame();
    }
}
