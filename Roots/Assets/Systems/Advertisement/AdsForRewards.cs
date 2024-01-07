using Buildings;
using UnityEngine;
using UnityEngine.Advertisements;

public class AdsForRewards : MonoBehaviour, IUnityAdsLoadListener, IUnityAdsShowListener
{
    [SerializeField] private BuildingsManager _buildingsManager;
    [SerializeField] private string typeOfAd = "Rewarded_Android";
    [SerializeField] private int _rewardAmount = 10;

    public void LoadRewardedAdd()
    {
        Advertisement.Load(typeOfAd, this);
    }
    
    public void ShowRewardedAdd()
    {
        Advertisement.Show(typeOfAd, this);
        LoadRewardedAdd();
    }

    public void OnUnityAdsAdLoaded(string placementId)
    {
    }

    public void OnUnityAdsFailedToLoad(string placementId, UnityAdsLoadError error, string message)
    {
    }

    #region Show Callbacks
    
    public void OnUnityAdsShowFailure(string placementId, UnityAdsShowError error, string message)
    {
    }

    public void OnUnityAdsShowStart(string placementId)
    {
    }

    public void OnUnityAdsShowClick(string placementId)
    {
    }

    public void OnUnityAdsShowComplete(string placementId, UnityAdsShowCompletionState showCompletionState)
    {
        if (placementId == typeOfAd && showCompletionState.Equals(UnityAdsShowCompletionState.COMPLETED))
        {
            Debug.Log("Rewarded Ad Completed");
            _buildingsManager.HandlePointsManipulation(PointsType.StarDust, _rewardAmount, true, true);
        }
    }
    #endregion
}
