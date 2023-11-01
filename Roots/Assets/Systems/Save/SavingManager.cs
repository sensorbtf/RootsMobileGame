using System;
using System.Globalization;
using System.IO;
using Buildings;
using Gods;
using UnityEngine;
using World;

namespace Saving
{
    public class SavingManager : MonoBehaviour
    {
        [SerializeField] private GodsManager _godsManager;
        [SerializeField] private WorldManager _worldManager;
        [SerializeField] private BuildingsManager _buildingsManager;

        private string _path;

        private void Awake()
        {
            _path = Application.persistentDataPath + "/gameData.json";

            // _mainGameManager.OnSaveTrigger += SaveMainGame;
            // _mainGameManager.OnLoadTrigger += LoadMainGame;
            // _mainGameManager.OnResetProgress += ResetSave;
        }

        public event Action<MainGameManagerSavedData> OnLoad;


        public void SaveMainGame(MainGameManagerSavedData p_data)
        {
            var path = Application.persistentDataPath + "/gameData.json";

            p_data.TimeOfWorkersSetISO8601 = p_data.TimeOfWorkersSet.ToString("o");

            var gameData = new GameData
            {
                WorldManagerSavedData = _worldManager.GetSavedData(),
                BuildingManagerSavedData = _buildingsManager.GetSavedData(),
                GodsManagerSavedData = _godsManager.GetSavedData(),
                MainSavedData = p_data
            };

            var json = JsonUtility.ToJson(gameData);
            File.WriteAllText(path, json);

            Debug.Log($"Saved to: {path}");
        }

        public void LoadMainGame()
        {
            if (!File.Exists(_path))
                return;

            var json = File.ReadAllText(_path);
            var gameData = JsonUtility.FromJson<GameData>(json);

            DateTime timeOfWorkersSet = DateTime.ParseExact(gameData.MainSavedData.TimeOfWorkersSetISO8601, 
                "o", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind);

            gameData.MainSavedData.TimeOfWorkersSet = timeOfWorkersSet;
            _worldManager.LoadSavedData(gameData.WorldManagerSavedData);
            _buildingsManager.LoadSavedData(gameData.BuildingManagerSavedData);
            _godsManager.LoadSavedData(gameData.GodsManagerSavedData);

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
        public GodsManagerSavedData GodsManagerSavedData;
    }

    [Serializable]
    public struct MainGameManagerSavedData
    {
        public int FreeDaysSkipAmount;
        public int CurrentPlayerState;
        public float TimeLeftInSeconds;
        public DateTime TimeOfWorkersSet;
        public string TimeOfWorkersSetISO8601; // Add this field to store the DateTime as a string
    }

}