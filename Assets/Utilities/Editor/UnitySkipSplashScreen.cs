namespace Volpi.Entertaiment.SDK.Utilities.Editor
{
    #if !UNITY_EDITOR
    using UnityEngine;
    using UnityEngine.Rendering;

    public sealed class UnitySkipSplashScreen
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSplashScreen)]
        private static void BeforeSplashScreen()
        {
            System.Threading.Tasks.Task.Run(AsyncSkip);
        }

        private static void AsyncSkip()
        {
    #if UNITY_ANDROID || UNITY_IOS
            SplashScreen.Stop(SplashScreen.StopBehavior.StopImmediate);
    #endif
        }
    }
    #endif
}