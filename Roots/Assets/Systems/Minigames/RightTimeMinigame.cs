using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RightTimeMinigame : MonoBehaviour // slider going from left to right. Player need to click in right moment (time => points possibility, efficiency == wider 
{
    [SerializeField] private Button _gameModeButton;
    [SerializeField] private TextMeshProUGUI _time;
    [SerializeField] private TextMeshProUGUI _score;
    
    private float timer;        
    private bool isGameActive;

    private void Start()// simple clicker
    {
    }

    private void Update()
    {
        if (isGameActive)
        {
            timer -= Time.deltaTime;
            //UpdateTimerText();

            if (timer <= 0)
            {
                timer = 0;
                isGameActive = false;
                // Optional: Disable the button when time is up.
                //clickButton.interactable = false;
            }
        }
    }

    private int score = 0;
    private void AddScore()
    {
        score++;
        Debug.Log("Score: " + score);
    }
}
