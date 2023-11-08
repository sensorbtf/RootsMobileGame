using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GooglePlayGames;
using GooglePlayGames.BasicApi;
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
        var test = PlayGamesPlatform.Instance;
        _infoText.text = "Trying" + test;
        Debug.Log(test);
        if (test == null)
        {
            Debug.Log(test + " IS NULL");
        }
        test.Authenticate(ProcessAuthentication);
    }

    internal void ProcessAuthentication(SignInStatus status)
    {
        if (status == SignInStatus.Success)
        {
            var name = PlayGamesPlatform.Instance.GetUserDisplayName();
            var id = PlayGamesPlatform.Instance.GetUserId();
            var url = PlayGamesPlatform.Instance.GetUserImageUrl();
            _infoText.text = $"NAME: {name}. ID {id}. URL {url}";
        }
        else
        {
            _infoText.text = $"Failed";
        }
    }
}