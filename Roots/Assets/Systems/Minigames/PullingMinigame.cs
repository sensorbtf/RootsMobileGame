using Buildings;
using UnityEngine;

namespace Minigames

{
    public class PullingMinigame : Minigame
    {
        [SerializeField] private GameObject _destructionRegionGo;
        [SerializeField] private GameObject _prefabToInstantiate;
        [SerializeField] private RectTransform _placeToInstantiate;
        [SerializeField] private RectTransform _point;

        private GameObject _currentPrefab;
        private Collider2D _currentPrefabCollider;
        private Collider2D _destructionRegion;

        private new void Update()
        {
            if (!_isGameActive)
                return;
            
            base.Update();
            
            if (_destructionRegion.bounds.Intersects(_currentPrefabCollider.bounds)) 
                OnGameobjectIntersect();

            if (_timer <= 0)
            {
                _currentPrefab.GetComponent<PullableObject>().EndMinigame();
                _timer = 0;
                _isGameActive = false;
                _collectPointsButton.interactable = true;
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
            _currentPrefab.GetComponent<PullableObject>().SetPosition(_point);
            _currentPrefabCollider = _currentPrefab.GetComponent<Collider2D>();
        }

        public override void AddScore()
        {
            _score += _efficiency;
            base.AddScore();
        }

        public override void StartMinigame()
        {
            CreatePrefab();
        }
    }
}