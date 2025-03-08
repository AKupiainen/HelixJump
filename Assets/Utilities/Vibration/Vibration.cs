namespace Volpi.Entertainment.SDK.Utilities
{
    using UnityEngine;
    using System;

#if UNITY_IOS
    using System.Collections;
    using System.Runtime.InteropServices;
#endif

    public static class Vibration
    {
#if UNITY_IOS
        [DllImport ( "__Internal" )]
        private static extern bool _HasVibrator ();

        [DllImport ( "__Internal" )]
        private static extern void _Vibrate ();

        [DllImport ( "__Internal" )]
        private static extern void _VibratePop ();

        [DllImport ( "__Internal" )]
        private static extern void _VibratePeek ();

        [DllImport ( "__Internal" )]
        private static extern void _VibrateNope ();

        [DllImport("__Internal")]
        private static extern void _impactOccurred(string style);

        [DllImport("__Internal")]
        private static extern void _notificationOccurred(string style);

        [DllImport("__Internal")]
        private static extern void _selectionChanged();
#endif

#if UNITY_ANDROID
        private static AndroidJavaClass _unityPlayer;
        private static AndroidJavaObject _currentActivity;
        private static AndroidJavaObject _vibrator;
        private static AndroidJavaObject _context;
        private static AndroidJavaClass VibrationEffect;

#endif
        private static bool _initialized = false;

        public static void Init()
        {
            if (_initialized)
            {
                return;
            }

#if UNITY_ANDROID

            if (Application.isMobilePlatform)
            {
                _unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
                _currentActivity = _unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
                _vibrator = _currentActivity.Call<AndroidJavaObject>("getSystemService", "vibrator");
                _context = _currentActivity.Call<AndroidJavaObject>("getApplicationContext");

                if (AndroidVersion >= 26)
                {
                    VibrationEffect = new AndroidJavaClass("android.os.VibrationEffect");
                }
            }
#endif

            _initialized = true;
        }


        public static void VibrateIOS(ImpactFeedbackStyle style)
        {
#if UNITY_IOS
            _impactOccurred(style.ToString());
#endif
        }

        public static void VibrateIOS(NotificationFeedbackStyle style)
        {
#if UNITY_IOS
            _notificationOccurred(style.ToString());
#endif
        }

        public static void VibrateIOS_SelectionChanged()

        {
#if UNITY_IOS
        _selectionChanged();
#endif
        }
        
        public static void VibratePop()
        {
            if (Application.isMobilePlatform)
            {
#if UNITY_IOS
                _VibratePop ();
#elif UNITY_ANDROID
                VibrateAndroid(50);
#endif
            }
        }
        
        public static void VibratePeek()
        {
            if (Application.isMobilePlatform)
            {
#if UNITY_IOS
                _VibratePeek ();
#elif UNITY_ANDROID
                VibrateAndroid(100);
#endif
            }
        }
        
        public static void VibrateNope()
        {
            if (Application.isMobilePlatform)
            {
#if UNITY_IOS
                _VibrateNope ();
#elif UNITY_ANDROID
                long[] pattern = { 0, 50, 50, 50 };
                VibrateAndroid(pattern, -1);
#endif
            }
        }


#if UNITY_ANDROID

        public static void VibrateAndroid(long milliseconds)
        {
            if (Application.isMobilePlatform)
            {
                if (AndroidVersion >= 26)
                {
                    AndroidJavaObject createOneShot =
                        VibrationEffect.CallStatic<AndroidJavaObject>("createOneShot", milliseconds, -1);
                    _vibrator.Call("vibrate", createOneShot);

                }
                else
                {
                    _vibrator.Call("vibrate", milliseconds);
                }
            }
        }
        
        public static void VibrateAndroid(long[] pattern, int repeat)
        {
            if (Application.isMobilePlatform)
            {
                if (AndroidVersion >= 26)
                {
                    AndroidJavaObject createWaveform =
                        VibrationEffect.CallStatic<AndroidJavaObject>("createWaveform", pattern, repeat);
                    _vibrator.Call("vibrate", createWaveform);

                }
                else
                {
                    _vibrator.Call("vibrate", pattern, repeat);
                }
            }
        }
#endif
        
        public static void CancelAndroid()
        {
            if (Application.isMobilePlatform)
            {
#if UNITY_ANDROID
                _vibrator.Call("cancel");
#endif
            }
        }

        public static bool HasVibrator()
        {
            if (Application.isMobilePlatform)
            {
#if UNITY_ANDROID
                AndroidJavaClass contextClass = new("android.content.Context");
                string contextVibratorService = contextClass.GetStatic<string>("VIBRATOR_SERVICE");
                AndroidJavaObject systemService =
                    _context.Call<AndroidJavaObject>("getSystemService", contextVibratorService);
                
                if (systemService.Call<bool>("hasVibrator"))
                {
                    return true;
                }

                return false;

#elif UNITY_IOS
                return _HasVibrator ();
#else
                return false;
#endif
            }
            else
            {
                return false;
            }
        }
        
        public static void Vibrate()
        {
#if UNITY_ANDROID || UNITY_IOS

            if (Application.isMobilePlatform)
            {
                Handheld.Vibrate();
            }
#endif
        }

        private static int AndroidVersion
        {
            get
            {
                int versionNumber = 0;
                
                if (Application.platform == RuntimePlatform.Android)
                {
                    string androidVersion = SystemInfo.operatingSystem;
                    int sdkPos = androidVersion.IndexOf("API-", StringComparison.Ordinal);
                    versionNumber = int.Parse(androidVersion.Substring(sdkPos + 4, 2));
                }

                return versionNumber;
            }
        }
    }

    public enum ImpactFeedbackStyle
    {
        Heavy,
        Medium,
        Light,
        Rigid,
        Soft
    }

    public enum NotificationFeedbackStyle
    {
        Error,
        Success,
        Warning
    }
}