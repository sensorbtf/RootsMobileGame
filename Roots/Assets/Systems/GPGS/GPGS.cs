using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GooglePlayGames;
using GooglePlayGames.BasicApi;
using GooglePlayGames.BasicApi.SavedGame;
using TMPro;

public class GPGS : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _infoText;
    private const string SAVE_FILENAME = "MyGame_SaveData";
    
    void Start()
    {
        SignIn();
    }

    public void SignIn()
    {
        PlayGamesPlatform.Instance.Authenticate(ProcessAuthentication);
    }

    internal void ProcessAuthentication(SignInStatus status)
    {
        if (status == SignInStatus.Success)
        {
            var playerName = PlayGamesPlatform.Instance.GetUserDisplayName();
            var id = PlayGamesPlatform.Instance.GetUserId();
            var url = PlayGamesPlatform.Instance.GetUserImageUrl();
            _infoText.text = $"NAME: {playerName}. ID {id}. URL {url}";
            
            OpenSavedGame(SAVE_FILENAME);
        }
        else
        {
            _infoText.text = $"Failed";
            // local save?
        }
    }

    #region Saving

    void OpenSavedGame(string filename)
    {
        ISavedGameClient savedGameClient = PlayGamesPlatform.Instance.SavedGame;
        savedGameClient.OpenWithAutomaticConflictResolution(
            filename,
            DataSource.ReadCacheOrNetwork,
            ConflictResolutionStrategy.UseLongestPlaytime,
            (status, game) =>
            {
                if (status == SavedGameRequestStatus.Success)
                {
                    _infoText.text = "Save Opened Successfully";

                    LoadGameData(game);
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

    void SaveGame(ISavedGameMetadata game, string savedData, TimeSpan totalPlaytime)
    {
        ISavedGameClient savedGameClient = PlayGamesPlatform.Instance.SavedGame;
        SavedGameMetadataUpdate.Builder builder = new SavedGameMetadataUpdate.Builder();
        byte[] data = System.Text.Encoding.UTF8.GetBytes(savedData);
        
        builder = builder
            .WithUpdatedPlayedTime(totalPlaytime)
            .WithUpdatedDescription("Saved game at " + DateTime.Now);

        SavedGameMetadataUpdate updatedMetadata = builder.Build();
        savedGameClient.CommitUpdate(game, updatedMetadata, data, OnSavedGameWritten);
    }

    public void OnSavedGameWritten(SavedGameRequestStatus status, ISavedGameMetadata game)
    {
        if (status == SavedGameRequestStatus.Success)
        {
            // handle reading or writing of saved game.
        }
        else
        {
            // handle error
        }
    }

    public void LoadGameData(ISavedGameMetadata game)
    {
        ISavedGameClient savedGameClient = PlayGamesPlatform.Instance.SavedGame;
        savedGameClient.ReadBinaryData(game, OnSavedGameDataRead);
    }

    public void OnSavedGameDataRead(SavedGameRequestStatus status, byte[] data)
    {
        if (status == SavedGameRequestStatus.Success)
        {
            _infoText.text = "Trying to ReadData";
            // handle processing the byte array data
        }
        else
        {
            _infoText.text = "Error in ReadData";
        }
    }

    #endregion
}