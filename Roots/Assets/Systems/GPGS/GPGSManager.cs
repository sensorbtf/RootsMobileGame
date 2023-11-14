using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GooglePlayGames;
using GooglePlayGames.BasicApi;
using GooglePlayGames.BasicApi.SavedGame;
using TMPro;

namespace GooglePlayServices
{
    public class GPGSManager : MonoBehaviour
    {
        private const string SAVE_FILENAME = "Roots_SaveData";
        [SerializeField] private TextMeshProUGUI _infoText;
        
        private byte[] _savedData;

        public event Action<byte[]> OnCloudDataRead;
        
        void Start()
        {
            SignIn();
        }

        public void SignIn()
        {
            PlayGamesPlatform.Instance.Authenticate(ProcessAuthentication);
        }

        private void ProcessAuthentication(SignInStatus status)
        {
            if (status == SignInStatus.Success)
            {
                var playerName = PlayGamesPlatform.Instance.GetUserDisplayName();
                var id = PlayGamesPlatform.Instance.GetUserId();
                var url = PlayGamesPlatform.Instance.GetUserImageUrl();
                _infoText.text = $"NAME: {playerName}. ID {id}. URL {url}";
            }
            else
            {
                _infoText.text = $"Failed";
                // local save?
            }
        }

        #region Saving

        public void TryToReadGame()
        {
            ISavedGameClient savedGameClient = PlayGamesPlatform.Instance.SavedGame;

            if (savedGameClient == null)
            {
                OnCloudDataRead?.Invoke(null);
                return;
            }
    
            savedGameClient.OpenWithAutomaticConflictResolution(
                SAVE_FILENAME,
                DataSource.ReadCacheOrNetwork,
                ConflictResolutionStrategy.UseLongestPlaytime,
                (status, game) =>
                {
                    if (status == SavedGameRequestStatus.Success)
                    {
                        _infoText.text = "Save Opened Successfully";
                        savedGameClient.ReadBinaryData(game, OnSavedGameDataRead);
                    }
                    else
                    {
                        // Handle error
                    }
                });
        }
        
        private void OnSavedGameDataRead(SavedGameRequestStatus status, byte[] data)
        {
            if (status == SavedGameRequestStatus.Success)
            {
                _infoText.text = "OnSavedGameDataRead GIt";
                OnCloudDataRead?.Invoke(data);
            }
            else
            {
                // Handle error, possibly try to load local data
            }
        }

        public void TryToSaveGame(string savedData, TimeSpan totalPlaytime)
        {
            ISavedGameClient savedGameClient = PlayGamesPlatform.Instance.SavedGame;
            
            if (savedGameClient == null)
                return;
            
            savedGameClient.OpenWithAutomaticConflictResolution(
                SAVE_FILENAME,
                DataSource.ReadCacheOrNetwork,
                ConflictResolutionStrategy.UseLongestPlaytime,
                (status, game) =>
                {
                    if (status == SavedGameRequestStatus.Success)
                    {
                        _infoText.text = "Save Opened Successfully";
                        Debug.Log($"Save Opened Successfully Now Reading");
                        
                        SaveGame(game, savedData, totalPlaytime);
                        // The saved game has been successfully opened or created.
                        // Proceed with reading from or writing to the file.
                    }
                    else
                    {
                        // The saved game failed to open. You should handle the error appropriately.
                        // This could involve retrying the open or informing the user of the issue.
                    }
                });
        }

        public void SaveGame(ISavedGameMetadata game, string savedData, TimeSpan totalPlaytime)
        {
            ISavedGameClient savedGameClient = PlayGamesPlatform.Instance.SavedGame;
            
            if (savedGameClient == null)
                return;
            
            SavedGameMetadataUpdate.Builder builder = new SavedGameMetadataUpdate.Builder();
            byte[] data = System.Text.Encoding.UTF8.GetBytes(savedData);

            builder = builder
                .WithUpdatedPlayedTime(totalPlaytime)
                .WithUpdatedDescription("Saved game at " + DateTime.Now);

            Debug.Log($"Saved game at" + DateTime.Now);
            
            SavedGameMetadataUpdate updatedMetadata = builder.Build();
            savedGameClient.CommitUpdate(game, updatedMetadata, data, OnSavedGameWritten);
        }

        public void OnSavedGameWritten(SavedGameRequestStatus status, ISavedGameMetadata game)
        {
            if (status == SavedGameRequestStatus.Success)
            {
                _infoText.text = "Save Written Successfully";
                Debug.Log($"Save Written Successfully");

            }
            else
            {
                _infoText.text = "Save Readed Fail";
                Debug.Log($"Save Readed Fail");
            }
        }

        #endregion
    }
}