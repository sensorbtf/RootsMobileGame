using Buildings;
using UnityEngine;
using UnityEngine.UI;

namespace Minigames
{
    public class RandomPointsToCollectMinigame : Minigame
    {
        [SerializeField] private GameObject _prefabToInstantiate;
        [SerializeField] private RectTransform _placeToInstantiate;

        private int _maxObjectToInstantiateInOnetime;        
        private GameObject _currentPrefab;
        private Button _currentPrefabButton;
        private Vector3 _placeToInstantiateInside;

        private new void Update()
        {
            base.Update();

            if (!_isGameActive)
                return;

            if (_timer <= 0)
            {
                _currentPrefab.GetComponent<Button>().onClick.RemoveAllListeners();
                _currentPrefab.GetComponent<Button>().interactable = false;
                _timer = 0;
                _isGameActive = false;
                _collectPointsButton.interactable = true;
            }
        }

        public override void SetupGame(Building p_building)
        {
            base.SetupGame(p_building);
            _maxObjectToInstantiateInOnetime = Mathf.FloorToInt(p_building.BuildingMainData.Technology
                .DataPerTechnologyLevel[p_building.CurrentTechnologyLvl].Efficiency);
            
            var sizeDelta = _placeToInstantiate.sizeDelta;
            _placeToInstantiateInside.x = sizeDelta.x; 
            _placeToInstantiateInside.y = sizeDelta.y;
            
            _score = 0;
        }

        private void OnCollect()
        {
            Destroy(_currentPrefab);
            AddScore();
            CreatePrefab();
        }

        private void CreatePrefab()
        {
            _currentPrefab = Instantiate(_prefabToInstantiate, _placeToInstantiate);
    
            _currentPrefab.GetComponent<RectTransform>().anchoredPosition = GetRandomPositionWithinBounds();
            //_currentPrefab.GetComponent<ClickableObject>().SetPosition(GetRandomPositionWithinBounds());
            _currentPrefab.GetComponent<Button>().onClick.AddListener(OnCollect);
        }

        private Vector2 GetRandomPositionWithinBounds()
        {
            float randomX = Random.Range(-_placeToInstantiateInside.x / 2, _placeToInstantiateInside.x / 2);
            float randomY = Random.Range(-_placeToInstantiateInside.y / 2, _placeToInstantiateInside.y / 2);
            return new Vector2(randomX, randomY);
        }
        
        public override void AddScore()
        {
            _score += _efficiency;
            
            base.AddScore();
        }

        public override void StartMinigame()
        {
            for (int i = 0; i < _maxObjectToInstantiateInOnetime; i++)
            {
                CreatePrefab();
            }
        }
    }
}