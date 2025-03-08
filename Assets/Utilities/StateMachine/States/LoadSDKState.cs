using System.Threading.Tasks;
using UnityEngine;
using Volpi.Entertaiment.SDK.Localization;
using Volpi.Entertaiment.SDK.MobileNotifications;
using Volpi.Entertainment.SDK.Utilities;

namespace Volpi.Entertaiment.SDK.Utilities
{
    [CreateAssetMenu(fileName = "LoadSDKState", menuName = "FSM/States/LoadSDK")]
    public class LoadSDKState : AsyncState
    {
        [SerializeField] private float _sdkLoadingDelay = 1f;
        [SerializeField] private bool _cancelPendingNotifications;
        
        public override async Task EnterState()
        {
            Debug.Log("[FSM] Entering Load SDK State...");

            SystemLanguage currentLanguage = Application.systemLanguage;
            LocalizationService.Instance.SetLanguage(currentLanguage);
            
            await AdMobManager.Instance.InitializeAsync();
            Vibration.Init();

            if (await NotificationManager.Instance.RequestNotificationPermission())
            {
                NotificationManager.Instance.Initialize(_cancelPendingNotifications);
            }
            
            await Task.Delay((int)_sdkLoadingDelay * 1000);
        }

        public override Task ExecuteState()
        {
            Debug.Log("[FSM] Exiting Load SDK State...");
            return Task.CompletedTask;
        }

        public override async Task ExitState()
        {
            Debug.Log("[FSM] Exiting Load SDK State...");
            await Task.Yield();
        }
    }
}