using System;
using UnityEngine;
using UnityEngine.Advertisements;
using UnityEngine.Serialization;

public class InitializeAds : MonoBehaviour 
{ 
   [SerializeField] private string AndroidGameID = "5508779";
   [SerializeField] private bool testMode = true;
   [SerializeField] private AdsForRewards _rewardAds;
    private void Awake()
    {
        Advertisement.Initialize(AndroidGameID, testMode); // NEED TO DELETE TEST MODE When in development
    }

    private void Start()
    {
        _rewardAds.LoadRewardedAdd();
    }
}
