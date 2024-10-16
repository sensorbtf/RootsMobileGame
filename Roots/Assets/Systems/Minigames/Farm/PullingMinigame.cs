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
        [SerializeField] private AudioClip _pullingOutSound;

        private GameObject _currentPrefab;
        private Collider2D _currentPrefabCollider;
        private Collider2D _destructionRegion;

        private bool _stopChecking;
        
        private new void Update()
        {
            if (!_isGameActive)
                return;
            
            base.Update();
            
            if (!_stopChecking)
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
            _stopChecking = false;
        }

        private void OnGameobjectIntersect()
        {
            _stopChecking = true;
            _currentPrefab.GetComponent<PullableObject>().OnBeginDraw -= AudioPlay;
            Destroy(_currentPrefab);
            AddScore();
            CreatePrefab();
        }

        private void CreatePrefab()
        {
            _currentPrefab = Instantiate(_prefabToInstantiate, _placeToInstantiate);
            _currentPrefab.GetComponent<PullableObject>().SetPosition(_point);
            _currentPrefab.GetComponent<PullableObject>().OnBeginDraw += AudioPlay;
            _currentPrefabCollider = _currentPrefab.GetComponent<Collider2D>();
            _stopChecking = false;
        }

        private void AudioPlay()
        {
            _audioManager.CreateNewAudioSource(_pullingOutSound);
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