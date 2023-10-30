using GameManager;
using GeneralSystems;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace InGameUi
{
    public class SettingsPanel : MonoBehaviour
    {
        [SerializeField] private MainGameManager _gameManager;
        [SerializeField] private Button _backButton;
        [SerializeField] private Button _resetGameButton;

        private void Start()
        {
            gameObject.SetActive(false);

            _resetGameButton.onClick.AddListener(ResetGame);
            _backButton.onClick.AddListener(ClosePanel);
        }

        public void OpenPanel()
        {
            CameraController.IsUiOpen = true;
            GameplayHud.BlockHud = true;

            gameObject.SetActive(true);
        }

        private void ClosePanel()
        {
            CameraController.IsUiOpen = false;
            GameplayHud.BlockHud = false;

            gameObject.SetActive(false);
        }

        private void ResetGame()
        {
            _gameManager.ResetSave();
        }
    }
}