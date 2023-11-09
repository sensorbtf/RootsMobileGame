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
            (SavedGameRequestStatus status, ISavedGameMetadata game) => {
                // handle opening the saved game
            });
    }

    void SaveGame(ISavedGameMetadata game, string dataToSave)
    {
        byte[] data = System.Text.Encoding.UTF8.GetBytes(dataToSave);
        SavedGameMetadataUpdate.Builder builder = new SavedGameMetadataUpdate.Builder();
        SavedGameMetadataUpdate updatedMetadata = builder.Build();

        ISavedGameClient savedGameClient = PlayGamesPlatform.Instance.SavedGame;
        savedGameClient.CommitUpdate(game, updatedMetadata, data, (SavedGameRequestStatus status, ISavedGameMetadata updatedGame) => {
            // handle the result of the save operation
        });
    }

    void LoadGameData(ISavedGameMetadata game)
    {
        ISavedGameClient savedGameClient = PlayGamesPlatform.Instance.SavedGame;
        savedGameClient.ReadBinaryData(game, (SavedGameRequestStatus status, byte[] data) => {
            if (status == SavedGameRequestStatus.Success)
            {
                // handle data (convert from byte[] to string)
                string savedData = System.Text.Encoding.UTF8.GetString(data);
                // Now you can use JSON to deserialize savedData to your game data
            }
            else
            {
                // handle error
            }
        });
    }

    #endregion
}