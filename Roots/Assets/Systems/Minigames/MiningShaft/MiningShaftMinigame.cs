using Buildings;
using UnityEngine;
using UnityEngine.UI;

namespace Minigames
{
    public class MiningShaftMinigame : Minigame
    {
        [SerializeField] private Button _buttonToMash;
        [SerializeField] private GameObject _pickaxe;
        [SerializeField] private AudioClip _pickaxeSound;

        private GameObject _newPickaxe;
        private GameObject _mainCanvas;

        private new void Update()
        {
            if (!_isGameActive)
                return;
            
            base.Update();

            if (_timer <= 0)
            {
                _timer = 0;
                _isGameActive = false;
                _collectPointsButton.interactable = true;
                _buttonToMash.interactable = false;
                
                if (_newPickaxe != null)
                {
                    Destroy(_newPickaxe);
                }
            }
        }

        public override void SetupGame(Building p_building)
        {
            base.SetupGame(p_building);

            _mainCanvas = GameObject.FindWithTag("MainCanvas");
            _score = 0;
            _buttonToMash.onClick.AddListener(AddScore);
            _buttonToMash.interactable = false;
            _collectPointsButton.interactable = false;
        }

        public override void AddScore()
        {
            ButtonClicked();
            
            _buttonToMash.interactable = false;
            _score += _efficiency;
            StartMinigame();
            
            base.AddScore();
        }

        public override void StartMinigame()
        {
            _buttonToMash.interactable = true;
        }
        
        private void ButtonClicked()
        {
            _audioManager.CreateNewAudioSource(_pickaxeSound);

            InstantiatePickaxeAtPosition(Input.mousePosition);
        }

        private void InstantiatePickaxeAtPosition(Vector2 position)
        {
            _audioManager.CreateNewAudioSource(_pickaxeSound);
            
            if (_newPickaxe != null)
            {
                Destroy(_newPickaxe);
            }
            
            _newPickaxe = Instantiate(_pickaxe, _mainCanvas.transform);
            var val = _newPickaxe.GetComponent<RectTransform>().sizeDelta / 2;
            var newPos = new Vector2(position.x + val.x, position.y);
            _newPickaxe.transform.position = newPos;
        }
    }
}