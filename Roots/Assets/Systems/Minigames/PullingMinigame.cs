using Buildings;
using UnityEngine;

namespace Minigames

{
    public class PullingMinigame : Minigame
    {
        [SerializeField] private GameObject _destructionRegionGo;
        [SerializeField] private GameObject _prefabToInstantiate;
        [SerializeField] private RectTransform _placeToInstantiate;

        private GameObject _currentPrefab;
        private Collider2D _currentPrefabCollider;
        private Collider2D _destructionRegion;

        private new void Update()
        {
            base.Update();

            if (!_isGameActive)
                return;

            if (_destructionRegion.bounds.Intersects(_currentPrefabCollider.bounds)) 
                OnGameobjectIntersect();

            if (_timer <= 0)
            {
                _currentPrefab.GetComponent<PullableObject>().EndMinigame();
                _timer = 0;
                _isGameActive = false;
                _collectPointsButton.interactable = true;
                _timeText.text = $"Click to collect: {_score:F0} resource points";
            }
        }

        public override void SetupGame(Building p_building)
        {
            base.SetupGame(p_building);

            _destructionRegion = _destructionRegionGo.GetComponent<Collider2D>();
            _score = 0;
        }

        private void OnGameobjectIntersect()
        {
            Destroy(_currentPrefab);
            AddScore();
            CreatePrefab();
        }

        private void CreatePrefab()
        {
            _currentPrefab = Instantiate(_prefabToInstantiate, _placeToInstantiate);
            _currentPrefab.GetComponent<PullableObject>().SetPosition(_placeToInstantiate);
            _currentPrefabCollider = _currentPrefab.GetComponent<Collider2D>();
        }

        public override void AddScore()
        {
            _score += _efficiency;
            _scoreText.text = $"Score: {_score:F1}";
        }

        public override void StartMinigame()
        {
            CreatePrefab();
        }
    }
}