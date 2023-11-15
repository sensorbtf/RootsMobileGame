using System;
using System.Globalization;
using System.IO;
using Buildings;
using Gods;
using UnityEngine;
using World;
using GooglePlayServices;
using UnityEngine.Serialization;

namespace Saving
{
    // WCZYTANIE GRY -> PORÓWNANIE CZASU GRANIA (SEJVY W CHMURZE VS LOKALNE)
    // ROZPOCZĘCIE WCZYTYWANIA
    public class SavingManager : MonoBehaviour
    {
        [SerializeField] private GodsManager _godsManager;
        [SerializeField] private WorldManager _worldManager;
        [SerializeField] private BuildingsManager _buildingsManager;
        [SerializeField] private GPGSManager _gpgsManager;

        private float _sessionStartTime;
        private string _path;
        public event Action<MainGameManagerSavedData> OnLoad;

        private void Awake()
        {
            _path = Application.persistentDataPath + "/gameData.json";
            Debug.Log(_path);
            _sessionStartTime = Time.time;
        }
        

        public void SaveMainGame(MainGameManagerSavedData p_data)
        {
            var path = Application.persistentDataPath + "/gameData.json";

            p_data.TimeOfWorkersSetISO8601 = p_data.TimeOfWorkersSet.ToString("o");
            p_data.TimeOfGiftTakenISO8601 = p_data.TimeOfGiftTaken.ToString("o");

            var gameData = new GameData
            {
                WorldManagerSavedData = _worldManager.GetSavedData(),
                BuildingManagerSavedData = _buildingsManager.GetSavedData(),
                GodsManagerSavedData = _godsManager.GetSavedData(),
                MainSavedData = p_data
            };

            var json = JsonUtility.ToJson(gameData);
            TimeSpan playTime = TimeSpan.FromSeconds(Time.time - _sessionStartTime);
            _gpgsManager.TryToSaveGame(json, playTime);

            File.WriteAllText(path, json);
            Debug.Log($"Saved locally to: {path}");
        }

        public void LoadMainGame(bool p_cloudSave)
        {
            if (p_cloudSave)
            {
                _gpgsManager.OnCloudDataRead += HandleCloudSaveData;
                _gpgsManager.TryToReadGame();
            }
            else
            {
                if (!File.Exists(_path))
                    return;

                string json = File.ReadAllText(_path);
                GameData gameData = JsonUtility.FromJson<GameData>(json);

                HandleLoadingOfData(gameData);
            }
        }

        private void HandleCloudSaveData(byte[] p_gameData)
        {
            _gpgsManager.OnCloudDataRead -= HandleCloudSaveData;
            
            var jsonString = System.Text.Encoding.UTF8.GetString(p_gameData);
            var gameData = JsonUtility.FromJson<GameData>(jsonString);
            Debug.Log($"Save got from cloud:");

            HandleLoadingOfData(gameData);
        }

        private void HandleLoadingOfData(GameData p_gameData)
        {
            DateTime timeOfWorkersSet = DateTime.ParseExact(p_gameData.MainSavedData.TimeOfWorkersSetISO8601,
                "o", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind);
            
            DateTime timeOfGiftTaken = DateTime.ParseExact(p_gameData.MainSavedData.TimeOfGiftTakenISO8601,
                "o", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind);

            p_gameData.MainSavedData.TimeOfWorkersSet = timeOfWorkersSet;
            p_gameData.MainSavedData.TimeOfGiftTaken = timeOfGiftTaken;
            _worldManager.LoadSavedData(p_gameData.WorldManagerSavedData);
            _buildingsManager.LoadSavedData(p_gameData.BuildingManagerSavedData);
            _godsManager.LoadSavedData(p_gameData.GodsManagerSavedData);

            OnLoad?.Invoke(p_gameData.MainSavedData);
        }

        public void ResetSave()
        {
            Debug.Log("Save Deleted");
            File.Delete(_path);
        }
        
        public void ChooseProperSave(Action<bool> p_onDecisionMade)
        {
            TimeSpan localPlayedTime = GetLocalOverallPlaytime();

            GetCloudOverallPlaytime(cloudPlayedTime =>
            {
                bool fromCloud = false;

                if (!(localPlayedTime == TimeSpan.Zero && cloudPlayedTime == TimeSpan.Zero))
                {
                    fromCloud = cloudPlayedTime > localPlayedTime;
                }

                p_onDecisionMade(fromCloud);
            });
        }

        public void CheckSave(Action<bool> p_isSaveAvaiable)
        {
            TimeSpan localPlayedTime = GetLocalOverallPlaytime();

            GetCloudOverallPlaytime(cloudPlayedTime =>
            {
                Debug.Log("CheckSave");

                var isAnySaveGood = !(localPlayedTime == TimeSpan.Zero && cloudPlayedTime == TimeSpan.Zero);
                p_isSaveAvaiable(isAnySaveGood);
            });
        }

        private TimeSpan GetLocalOverallPlaytime()
        {
            if (!File.Exists(_path))
                return TimeSpan.Zero;

            var json = File.ReadAllText(_path);
            var gameData = JsonUtility.FromJson<GameData>(json);

            return gameData.MainSavedData.TotalTimePlayed != null
                ? TimeSpan.Parse(gameData.MainSavedData.TotalTimePlayed)
                : TimeSpan.Zero;
        }

        private void GetCloudOverallPlaytime(Action<TimeSpan> onPlaytimeRetrieved)
        {
            Debug.Log("GetCloudOverallPlaytime");
            
            Action<byte[]> cloudDataHandler = null;
            cloudDataHandler = (byte[] data) =>
            {
                _gpgsManager.OnCloudDataRead -= cloudDataHandler;
                
                if (data == null)
                {
                    onPlaytimeRetrieved(TimeSpan.Zero);
                }
                else
                {
                    var jsonString = System.Text.Encoding.UTF8.GetString(data);
                    var gameData = JsonUtility.FromJson<GameData>(jsonString);
                    onPlaytimeRetrieved(TimeSpan.Parse(gameData.MainSavedData.TotalTimePlayed));
                }
            };
    
            _gpgsManager.OnCloudDataRead += cloudDataHandler;
            _gpgsManager.TryToReadGame();
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
        public int LoginDay;
        public int FreeDaysSkipAmount;
        public int CurrentPlayerState;
        public string TotalTimePlayed;
        public DateTime TimeOfWorkersSet;
        public string TimeOfWorkersSetISO8601; 
        public DateTime TimeOfGiftTaken;
        public string TimeOfGiftTakenISO8601; 
    }
}