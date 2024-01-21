using Buildings;
using UnityEngine;
using UnityEngine.UI;

namespace Minigames
{
    public class MiningShaftMinigame : Minigame
    {
        [SerializeField] private Button _buttonToMash;

        private new void Update()
        {
            base.Update();

            if (_timer <= 0)
            {
                _timer = 0;
                _isGameActive = false;
                _collectPointsButton.interactable = true;
                _buttonToMash.interactable = false;
            }
        }

        public override void SetupGame(Building p_building)
        {
            base.SetupGame(p_building);

            _score = 0;
            _buttonToMash.gameObject.GetComponent<Image>().sprite = 
                p_building.BuildingMainData.Technology.SpriteOfObject;
            _buttonToMash.onClick.AddListener(AddScore);
            _buttonToMash.interactable = false;
            _collectPointsButton.interactable = false;
        }

        public override void AddScore()
        {
            _buttonToMash.interactable = false;
            _score += _efficiency;
            StartMinigame();
            
            base.AddScore();
        }

        public override void StartMinigame()
        {
            _buttonToMash.interactable = true;
        }
    }
}