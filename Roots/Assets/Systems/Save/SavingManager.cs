using System;
using System.IO;
using Buildings;
using UnityEngine;
using UnityEngine.Serialization;
using World;

namespace Saving
{
    public class SavingManager : MonoBehaviour
    {
        [SerializeField] private WorldManager _worldManager;
        [SerializeField] private BuildingsManager _buildingsManager;
        
        private string _path;

        public event Action<MainGameManagerSavedData> OnLoad;

        void Awake()
        {
            _path = Application.persistentDataPath + "/gameData.json";

            // _mainGameManager.OnSaveTrigger += SaveMainGame;
            // _mainGameManager.OnLoadTrigger += LoadMainGame;
            // _mainGameManager.OnResetProgress += ResetSave;
        }

        public void SaveMainGame(MainGameManagerSavedData p_data)
        {
            var path = Application.persistentDataPath + "/gameData.json";
            
            GameData gameData = new GameData
            {
                WorldManagerSavedData = _worldManager.GetSavedData(),
                BuildingManagerSavedData = _buildingsManager.GetSavedData(),
                MainSavedData = p_data,
            };
            
            string json = JsonUtility.ToJson(gameData);
            File.WriteAllText(path, json);
            
            Debug.Log($"Saved to: {path}");
        }

        public void LoadMainGame()
        {
            if (!File.Exists(_path))
                return;

            var json = File.ReadAllText(_path);
            var gameData = JsonUtility.FromJson<GameData>(json);
            
            _worldManager.LoadSavedData(gameData.WorldManagerSavedData);
            _buildingsManager.LoadSavedData(gameData.BuildingManagerSavedData);
            
            OnLoad?.Invoke(gameData.MainSavedData);
        }
        
        public void ResetSave()
        {
            Debug.Log("Save Deleted");
            File.Delete(_path);
        }

        public bool IsSaveAvaiable()
        {
            return File.Exists(_path);
        }
    }
    
    [Serializable]
    public struct GameData
    {
        public MainGameManagerSavedData MainSavedData;
        public WorldManagerSavedData WorldManagerSavedData;
        public BuildingManagerSavedData BuildingManagerSavedData;
    }
    
    [Serializable]
    public struct MainGameManagerSavedData
    {
        public DateTime TimeOfWorkersSet;
        public int FreeDaysSkipAmount;
        public int CurrentPlayerState;
        public float TimeLeftInSeconds;
    }
}