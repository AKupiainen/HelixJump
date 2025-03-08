namespace Volpi.Entertaiment.SDK.Utilities
{
    using UnityEngine;
    using GameAnalyticsSDK;

    public class GameAnalyticsManager : Singleton<GameAnalyticsManager>
    {
        private const string LOGTag = "[GameAnalytics]";

        [SerializeField] private bool _isInitialized = false;

        protected override void Awake()
        {
            base.Awake();
            InitializeGameAnalytics();
        }

        private void OnEnable()
        {
            SubscribeToAdEvents();
        }

        private void OnDisable()
        {
            UnsubscribeFromAdEvents();
        }

        private void InitializeGameAnalytics()
        {
            if (_isInitialized)
            {
                Debug.LogWarning($"{LOGTag} Already initialized. Skipping re-initialization.");
                return;
            }

#if UNITY_IOS
            Debug.Log($"{LOGTag} Requesting App Tracking Transparency for iOS...");
            GameAnalytics.RequestTrackingAuthorization();
#endif
            Debug.Log($"{LOGTag} Initializing GameAnalytics...");
            GameAnalytics.Initialize();
            _isInitialized = true;
        }

        private void SubscribeToAdEvents()
        {
            Debug.Log($"{LOGTag} Subscribing to AdMob events...");

            AdMobManager.OnRewardedAdWatched += OnRewardedAdWatched;
            AdMobManager.OnRewardedAdFailed += OnRewardedAdFailed;
            AdMobManager.OnInterstitialAdShown += OnInterstitialAdShown;
            AdMobManager.OnInterstitialAdFailed += OnInterstitialAdFailed;
            AdMobManager.OnInterstitialAdClosed += OnInterstitialAdClosed;
            AdMobManager.OnBannerAdShown += OnBannerAdShown;
        }

        private void UnsubscribeFromAdEvents()
        {
            Debug.Log($"{LOGTag} Unsubscribing from AdMob events...");

            AdMobManager.OnRewardedAdWatched -= OnRewardedAdWatched;
            AdMobManager.OnRewardedAdFailed -= OnRewardedAdFailed;
            AdMobManager.OnInterstitialAdShown -= OnInterstitialAdShown;
            AdMobManager.OnInterstitialAdFailed -= OnInterstitialAdFailed;
            AdMobManager.OnInterstitialAdClosed -= OnInterstitialAdClosed;
            AdMobManager.OnBannerAdShown -= OnBannerAdShown;
        }

        private void OnRewardedAdWatched(string placementId)
        {
            Debug.Log($"{LOGTag} Rewarded ad watched. Placement: {placementId}. Sending analytics event.");
            GameAnalytics.NewAdEvent(GAAdAction.RewardReceived, GAAdType.RewardedVideo, "admob", placementId);
        }

        private void OnRewardedAdFailed(string placementId, int errorReason)
        {
            Debug.Log($"{LOGTag} Rewarded ad failed to show. Placement: {placementId}, Error: {errorReason}");
            GameAnalytics.NewAdEvent(GAAdAction.FailedShow, GAAdType.RewardedVideo, "admob", placementId, MapErrorReason(errorReason));
        }

        private void OnInterstitialAdShown(string placementId)
        {
            Debug.Log($"{LOGTag} Interstitial ad shown. Placement: {placementId}. Sending analytics event.");
            GameAnalytics.NewAdEvent(GAAdAction.Show, GAAdType.Interstitial, "admob", placementId);
        }

        private void OnInterstitialAdFailed(string placementId, int errorReason)
        {
            Debug.Log($"{LOGTag} Interstitial ad failed to show. Placement: {placementId}, Error: {errorReason}");
            GameAnalytics.NewAdEvent(GAAdAction.FailedShow, GAAdType.Interstitial, "admob", placementId, MapErrorReason(errorReason));
        }

        private void OnInterstitialAdClosed(string placementId)
        {
            Debug.Log($"{LOGTag} Interstitial ad closed. Placement: {placementId}. Sending analytics event.");
            GameAnalytics.NewAdEvent(GAAdAction.Clicked, GAAdType.Interstitial, "admob", placementId);
        }

        private void OnBannerAdShown(string placementId)
        {
            Debug.Log($"{LOGTag} Banner ad shown. Placement: {placementId}. Sending analytics event.");
            GameAnalytics.NewAdEvent(GAAdAction.Show, GAAdType.Banner, "admob", placementId);
        }

        private GAAdError MapErrorReason(int errorCode)
        {
            switch (errorCode)
            {
                case 0:  
                    return GAAdError.InternalError;
                case 1:  
                    return GAAdError.InvalidRequest;
                case 2:  
                    return GAAdError.Offline;
                case 3:  
                    return GAAdError.NoFill;
                default:
                    return GAAdError.Unknown;
            }
        }
    }
}
