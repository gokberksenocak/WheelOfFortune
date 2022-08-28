using UnityEngine;
using GoogleMobileAds.Api;

public class AdManager : MonoBehaviour
{
    private RewardedAd rewardedAd;
    [SerializeField] private GameManager gameManager;

    public void Start()
    {
        RequestRewardedAd();
    }
    void Update()
    {
        if (Input.touchCount > 0)
        {
            OnValidate();
        }
    }
        public void RequestRewardedAd()
    {
        string adUnitId;
#if UNITY_ANDROID
        adUnitId = "ca-app-pub-3940256099942544/5224354917";
#elif UNITY_IPHONE
            adUnitId = "ca-app-pub-3940256099942544/1712485313";
#else
        adUnitId = "unexpected_platform";
#endif

        this.rewardedAd = new RewardedAd(adUnitId);

        // Called when the user should be rewarded for interacting with the ad.
        this.rewardedAd.OnUserEarnedReward += HandleUserEarnedReward;

        // Create an empty ad request.
        AdRequest request = new AdRequest.Builder().Build();
        // Load the rewarded ad with the request.
        this.rewardedAd.LoadAd(request);
    }

    public void HandleUserEarnedReward(object sender, Reward args)
    {
        gameManager.RestartPanel.SetActive(false);
        gameManager.ReviveButton.interactable = false;
        gameManager.finish = false;
    }
    public void ShowRewardedAd()
    {
        if (this.rewardedAd.IsLoaded())
        {
            this.rewardedAd.Show();
        }
    }
    public void OnValidate()
    {
        gameManager.ReviveButton.onClick.AddListener(ShowRewardedAd);
    }
}