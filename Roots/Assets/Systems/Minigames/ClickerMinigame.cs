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
    private float _efficiency;        
    private bool _isGameActive;
    private float _score = 0;
    private PointsType _type;

    public event Action<PointsType, int> OnResourcesCollected;

    public void StartTheGame(Building p_building)
    {
        _score = 0;

        _timer = p_building.BuildingMainData.Technology.DataPerTechnologyLevel[p_building.CurrentTechnologyLvl].MinigameDuration;
        _efficiency = p_building.BuildingMainData.Technology.DataPerTechnologyLevel[p_building.CurrentTechnologyLvl].Efficiency;
        _type = p_building.ProductionType;

        _leftSideButton.onClick.AddListener(AddScore);
        _rightSideButton.onClick.AddListener(AddScore);
        _leftSideButton.interactable = false;
        _rightSideButton.interactable = false;
        
        _collectPointsButton.onClick.AddListener(EndMinigame);
        _collectPointsButton.interactable = false;
        
        StartCoroutine(StartCountdown());
    }

    private void EndMinigame()
    {
        OnResourcesCollected?.Invoke(_type, (int)_score);
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
            
            _collectPointsButton.interactable = true;
            _timeText.text = $"Click to collect: {_score} resource points"; // some sort of przelicznik
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
