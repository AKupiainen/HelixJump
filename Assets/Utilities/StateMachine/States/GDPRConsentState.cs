using System.Threading.Tasks;
using UnityEngine;

namespace Volpi.Entertaiment.SDK.Utilities
{
    [CreateAssetMenu(fileName = "GDPRConsentState", menuName = "FSM/States/GDPRConsent")]
    public class GdprConsentState : AsyncState
    {
        [SerializeField] private string _privacyLinkUrl;
        
        private const string AdsPersonalizationConsent = "Ads";

        public override async Task EnterState()
        {
            Debug.Log("[FSM] Entering GDPR Consent State...");

            if (SimpleGDPR.GetConsentState(AdsPersonalizationConsent) == SimpleGDPR.ConsentState.Unknown)
            {
                Debug.Log("[FSM] Showing GDPR Consent Dialog...");
                await ShowGdprConsentDialogAsync();
            }
            else
            {
                Debug.Log($"[FSM] GDPR Consent Already Given: {SimpleGDPR.GetConsentState(AdsPersonalizationConsent)}");
            }
        }

        private  async Task ShowGdprConsentDialogAsync()
        {
            TaskCompletionSource<bool> tcs = new();
            
            GameObject tempObject = new("GDPRDialogHandler");
            GdprDialogHandler handler = tempObject.AddComponent<GdprDialogHandler>();

            handler.StartCoroutine(handler.ShowDialogCoroutine(_privacyLinkUrl, tcs));
            await tcs.Task;
            
            Destroy(tempObject);
        }

        public override Task ExecuteState()
        {
            return Task.CompletedTask;
        }

        public override Task ExitState()
        {
            Debug.Log("[FSM] Exiting GDPR Consent State...");
            return Task.CompletedTask;
        }
    }
    
    public class GdprDialogHandler : MonoBehaviour
    {
        private const string AdsPersonalizationConsent = "Ads";

        public System.Collections.IEnumerator ShowDialogCoroutine(string privacyLink,  TaskCompletionSource<bool> tcs)
        {
            GDPRConsentDialog consentDialog = new GDPRConsentDialog()
                .AddSectionWithToggle(AdsPersonalizationConsent, "Personalized Ads", "Enable personalized ads for a better experience.")
                .AddPrivacyPolicies(privacyLink);

            yield return consentDialog.WaitForDialog();
            
            tcs.SetResult(true);
        }
    }
}