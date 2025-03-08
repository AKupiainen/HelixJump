using System.Threading.Tasks;

namespace Volpi.Entertaiment.SDK.Utilities
{
    using GoogleMobileAds.Api;
    using System;
    using JetBrains.Annotations;
    using UnityEngine;

    public class AdMobManager : Singleton<AdMobManager>
    {
        [Header("Admob Ad Units :")]
        [SerializeField] private string _idBanner = "ca-app-pub-3940256099942544/6300978111";
        [SerializeField] private string _idInterstitial = "ca-app-pub-3940256099942544/1033173712";
        [SerializeField] private string _idReward = "ca-app-pub-3940256099942544/5224354917";

        [Space]
        [SerializeField] private AdPosition _bannerAdPosition = AdPosition.Bottom;

        [Header("Toggle Admob Ads :")]
        [SerializeField] private bool _bannerAdEnabled = true;
        [SerializeField] private bool _interstitialAdEnabled = true;
        [SerializeField] private bool _rewardedAdEnabled = true;

        private BannerView _adBanner;
        private InterstitialAd _adInterstitial;
        private RewardedAd _adReward;
        private bool _isInterstitialLoading;

        public static event Action<string> OnInterstitialAdShown;
        public static event Action<string> OnInterstitialAdClosed;
        public static event Action<string, int> OnInterstitialAdFailed;
        public static event Action<string> OnBannerAdShown;
        public static event Action<string> OnRewardedAdWatched;
        public static event Action<string, int> OnRewardedAdFailed;

        public bool IsRewardedAdLoaded => _rewardedAdEnabled && _adReward != null && _adReward.CanShowAd();

        public Task InitializeAsync()
        {
            TaskCompletionSource<bool> tcs = new();

            MobileAds.Initialize(initStatus =>
            {
                if (initStatus != null)
                {
                    Debug.Log($"[Admob] AdMob initialization status: {initStatus}");
                }
                else
                {
                    Debug.LogWarning("[Admob] AdMob initialization failed (status is null).");
                }

                Debug.Log("[Admob] Admob initialized successfully.");
                tcs.SetResult(true);
                
                RequestRewardAd();
                RequestInterstitialAd();
            });

            return tcs.Task;
        }
        
        private void OnDestroy()
        {
            DestroyBannerAd();
            DestroyInterstitialAd();
            DestroyRewardAd();
        }

        private AdRequest CreateAdRequest()
        {
            return new AdRequest();
        }

        [UsedImplicitly]
        public void ShowBanner()
        {
            if (!_bannerAdEnabled)
            {
                return;
            }

            DestroyBannerAd();

            AdSize adSize = AdSize.GetPortraitAnchoredAdaptiveBannerAdSizeWithWidth(AdSize.FullWidth);
            _adBanner = new BannerView(_idBanner, adSize, _bannerAdPosition);
            _adBanner.LoadAd(CreateAdRequest());

            Debug.Log("[Admob] Banner ad shown.");
            OnBannerAdShown?.Invoke(_idBanner);
        }

        private void DestroyBannerAd()
        {
            if (_adBanner != null)
            {
                _adBanner.Destroy();
                _adBanner = null;
                Debug.Log("[Admob] Banner ad destroyed.");
            }
        }

        [UsedImplicitly]
        public void RequestInterstitialAd()
        {
            if (!_interstitialAdEnabled || _isInterstitialLoading)
            {
                return;
            }

            _isInterstitialLoading = true;

            InterstitialAd.Load(_idInterstitial, CreateAdRequest(),
            (ad, error) =>
            {
                _isInterstitialLoading = false;

                if (error != null || ad == null)
                {
                    Debug.LogWarning($"[Admob] Interstitial ad failed to load: {error?.GetMessage()} (Code: {error?.GetCode()})");
                    OnInterstitialAdFailed?.Invoke(_idInterstitial, error?.GetCode() ?? - 1);
                    return;
                }

                _adInterstitial = ad;
                Debug.Log("[Admob] Interstitial ad loaded successfully.");

                _adInterstitial.OnAdFullScreenContentClosed += () =>
                {
                    Debug.Log("[Admob] Interstitial ad closed.");
                    OnInterstitialAdClosed?.Invoke(_idInterstitial);
                    RequestInterstitialAd();
                };

                _adInterstitial.OnAdFullScreenContentFailed += adError =>
                {
                    Debug.LogWarning($"[Admob] Interstitial ad failed to show: {adError.GetMessage()}");
                    OnInterstitialAdFailed?.Invoke(_idInterstitial, adError.GetCode());
                };
            });
        }

        [UsedImplicitly]
        public void ShowInterstitialAd()
        {
            if (!_interstitialAdEnabled || _adInterstitial == null || !_adInterstitial.CanShowAd())
            {
                Debug.LogWarning("[Admob] Interstitial ad is not ready yet.");
                return;
            }

            Debug.Log("[Admob] Interstitial ad shown.");
            OnInterstitialAdShown?.Invoke(_idInterstitial);

            _adInterstitial.Show();
            RequestInterstitialAd();
        }

        private void DestroyInterstitialAd()
        {
            if (_adInterstitial != null)
            {
                _adInterstitial.Destroy();
                _adInterstitial = null;
                Debug.Log("[Admob] Interstitial ad destroyed.");
            }
        }

        [UsedImplicitly]
        public void RequestRewardAd()
        {
            if (!_rewardedAdEnabled)
            {
                return;
            }

            RewardedAd.Load(_idReward, CreateAdRequest(),
            (ad, error) =>
            {
                if (error != null || ad == null)
                {
                    Debug.LogWarning($"[Admob] Rewarded ad failed to load: {error?.GetMessage()} (Code: {error?.GetCode()})");
                    OnRewardedAdFailed?.Invoke(_idReward, error?.GetCode() ?? - 1);
                    return;
                }

                _adReward = ad;
                Debug.Log("[Admob] Rewarded ad loaded successfully.");

                _adReward.OnAdFullScreenContentClosed += () =>
                {
                    Debug.Log("[Admob] Rewarded ad closed. Requesting a new one...");
                    RequestRewardAd();
                };

                _adReward.OnAdFullScreenContentFailed += adError =>
                {
                    Debug.LogWarning($"[Admob] Rewarded ad failed to show: {adError.GetMessage()}");
                    OnRewardedAdFailed?.Invoke(_idReward, adError.GetCode());
                };
            });
        }

        [UsedImplicitly]
        public void ShowRewardAd()
        {
            if (!_rewardedAdEnabled || _adReward == null || !_adReward.CanShowAd())
            {
                Debug.LogWarning("[Admob] Rewarded ad is not ready yet.");
                return;
            }

            DestroyRewardAd();

            _adReward.Show(reward =>
            {
                Debug.Log($"[Admob] Rewarded ad watched. Rewarding user: {reward.Amount} {reward.Type}");
                OnRewardedAdWatched?.Invoke(_idReward);
            });

            RequestRewardAd();
        }

        private void DestroyRewardAd()
        {
            if (_adReward != null)
            {
                _adReward.Destroy();
                _adReward = null;
                Debug.Log("[Admob] Rewarded ad destroyed.");
            }
        }
    }
}