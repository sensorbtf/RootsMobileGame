using System;
using System.Collections;
using System.Collections.Generic;
using Buildings;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class ClickerMinigame : MonoBehaviour
{
    [SerializeField] private Button _leftSideButton;
    [SerializeField] private Button _rightSideButton;
    [SerializeField] private Button _collectPointsButton;
    [SerializeField] private TextMeshProUGUI _timeText;
    [SerializeField] private TextMeshProUGUI _scoreText;
    [SerializeField] private TextMeshProUGUI _coutdownText;
    
    private float _timer;        
    private float _countdownTimer = 3;   
    private float _efficiency;        
    private bool _isGameActive;
    private bool _countdownEnd;
    private float _score = 0;

    public float Timer => _timer;
    public bool IsGameActive => _isGameActive;
    
    public event Action OnResourcesCollected;

    public void StartTheGame(TechnologyDataPerLevel p_technologyDataPerLevel)
    {
        _score = 0;

        _timer = p_technologyDataPerLevel.MinigameDuration;
        _efficiency = p_technologyDataPerLevel.Efficiency;
        
        _leftSideButton.onClick.AddListener(AddScore);
        _rightSideButton.onClick.AddListener(AddScore);
        _collectPointsButton.onClick.AddListener(EndMinigame);
        
        StartCoroutine(StartCountdown());
    }

    private void EndMinigame()
    {
        OnResourcesCollected?.Invoke();
    }

    private void Update()
    {
        if (!_isGameActive) 
            return;

        UpdateTimerText();

        if (_timer <= 0)
        {
            _timer = 0;
            _isGameActive = false;
            _leftSideButton.interactable = false;
            _rightSideButton.interactable = false;

            _timeText.text = $"Click to collect: {_score} points"; // some sort of przelicznik
        }
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
        RandomizeInteractableButton();
        yield return new WaitForSeconds(0.5f);
        _coutdownText.enabled = false; 
    }

    private void AddScore()
    {
        _score += _efficiency;
        RandomizeInteractableButton();
        _scoreText.text = _score.ToString();
    }

    private void RandomizeInteractableButton()
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
