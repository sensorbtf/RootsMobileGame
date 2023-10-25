using System.IO;
using Buildings;
using UnityEngine;
using UnityEngine.Serialization;
using World;

namespace SavingManager
{
    public class SavingManager : MonoBehaviour
    {
        public WorldManager _worldManager;
        public BuildingsManager _buildingsManager;

        private string _path;

        void Awake()
        {
            // 0 for no sync, 1 for panel refresh rate, 2 for 1/2 panel rate
            //QualitySettings.vSyncCount = 2;
            Application.targetFrameRate = 30;
            _path = Application.persistentDataPath + "/gameData.json";

            _worldManager.OnNewDayStarted += SaveGame;
            _worldManager.OnGameStarted += LoadGame;
        }

        public void SaveGame()
        {
            var path = Application.persistentDataPath + "/gameData.json";
            GameData gameData = new GameData
            {
                WorldManagerSavedData = _worldManager.GetSavedData(),
                BuildingManagerSavedData = _buildingsManager.GetSavedData()
            };

            string json = JsonUtility.ToJson(gameData);
            File.WriteAllText(path, json);
            Debug.Log($"Saved to: {path}");
        }

        public void LoadGame()
        {
            if (!File.Exists(_path))
                return;

            string json = File.ReadAllText(_path);
            GameData gameData = JsonUtility.FromJson<GameData>(json);
            _worldManager.LoadSavedData(gameData.WorldManagerSavedData);
            _buildingsManager.LoadSavedData(gameData.BuildingManagerSavedData);
            // You can also access other data from gameData if needed
        }
    }

    [System.Serializable]
    public struct GameData
    {
        public WorldManagerSavedData WorldManagerSavedData;

        public BuildingManagerSavedData BuildingManagerSavedData;
        // Add other relevant data
    }
}