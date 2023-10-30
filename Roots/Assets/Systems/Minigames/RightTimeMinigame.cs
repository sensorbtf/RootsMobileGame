using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Minigames
{
    public class
        RightTimeMinigame : MonoBehaviour // slider going from left to right. Player need to click in right moment (time => points possibility, efficiency == wider 
    {
        [SerializeField] private Button _gameModeButton;
        [SerializeField] private TextMeshProUGUI _score;
        [SerializeField] private TextMeshProUGUI _time;
        private bool isGameActive;

        private int score;

        private float timer;

        private void Start() // simple clicker
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

        private void AddScore()
        {
            score++;
            Debug.Log("Score: " + score);
        }
    }
}