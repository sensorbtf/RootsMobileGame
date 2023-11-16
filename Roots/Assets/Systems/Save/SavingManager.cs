using System;
using System.Collections;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;
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
        private byte[] _savedDataFromCloud;
        private string _path;
        public event Action<MainGameManagerSavedData> OnLoad;
        public event Action OnAuthenticationEnded;

        private void Start()
        {
            StartCoroutine(AuthenticationProcess());
        }

        private IEnumerator AuthenticationProcess()
        {
            yield return StartCoroutine(_gpgsManager.StartAuthentication());

            _path = Application.persistentDataPath + "/gameData.json";
            Debug.Log(_path);
            _sessionStartTime = Time.time;

            OnAuthenticationEnded?.Invoke();
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
                if (_savedDataFromCloud != null)
                {
                    HandleCloudSaveData(_savedDataFromCloud);
                }
                else
                {
                    _gpgsManager.OnCloudDataRead += HandleCloudSaveData;
                    _gpgsManager.TryToReadGame();
                }
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
            _gpgsManager.TryToDeleteSavedGame();
            File.Delete(_path);
        }
        
        public IEnumerator ChooseProperSave()
        {
            TimeSpan localPlayedTime = GetLocalOverallPlaytime();
            yield return StartCoroutine(GetCloudOverallPlaytime(cloudPlayedTime => 
            {
                bool fromCloud = false;

                if (!(localPlayedTime == TimeSpan.Zero && cloudPlayedTime == TimeSpan.Zero))
                {
                    fromCloud = cloudPlayedTime >= localPlayedTime;
                }

                LoadMainGame(fromCloud);
            }));
        }

        public IEnumerator CheckSave(Action<bool> resultCallback)
        {
            TimeSpan localPlayedTime = GetLocalOverallPlaytime();
            yield return StartCoroutine(GetCloudOverallPlaytime(cloudPlayedTime => 
            {
                resultCallback(!(localPlayedTime == TimeSpan.Zero && cloudPlayedTime == TimeSpan.Zero));
            }));
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
        
        private IEnumerator GetCloudOverallPlaytime(Action<TimeSpan> resultCallback)
        {
            if (_savedDataFromCloud != null)
                yield return _savedDataFromCloud;
            
            TimeSpan cloudPlaytime = TimeSpan.Zero;
            
            yield return StartCoroutine(ReadCloudData(data => 
            {
                if (data is { Length: > 1 })
                {
                    var jsonString = System.Text.Encoding.UTF8.GetString(data);
                    var gameData = JsonUtility.FromJson<GameData>(jsonString);
                    cloudPlaytime = TimeSpan.Parse(gameData.MainSavedData.TotalTimePlayed);
                    _savedDataFromCloud = data;
                }

                resultCallback(cloudPlaytime);
            }));
        }

        private IEnumerator ReadCloudData(Action<byte[]> resultCallback)
        {
            bool isDataReceived = false;
            byte[] receivedData = null;

            Action<byte[]> cloudDataHandler = null;
            cloudDataHandler = data =>
            {
                _gpgsManager.OnCloudDataRead -= cloudDataHandler;
                receivedData = data;
                isDataReceived = true;
            };

            _gpgsManager.OnCloudDataRead += cloudDataHandler;
            _gpgsManager.TryToReadGame();

            while (!isDataReceived)
            {
                yield return null;
            }

            resultCallback(receivedData);
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