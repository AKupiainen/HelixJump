using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Android;
using Volpi.Entertaiment.SDK.Utilities;

namespace Volpi.Entertaiment.SDK.MobileNotifications
{
    using System;
#if UNITY_IOS
    using Unity.Notifications.iOS;
#endif
#if UNITY_ANDROID
    using Unity.Notifications.Android;
#endif

    public class NotificationManager : Singleton<NotificationManager>
    {
        [SerializeField] private bool _cancelPendingNotifications;
        [SerializeField] private NotificationDataContainer _notificationDataContainer;

        private bool _initialized;

        private void OnApplicationFocus(bool focus)
        {
            if (focus == false)
            {
                Debug.Log("App lost focus, sending pending notifications...");
                foreach (NotificationData notification in _notificationDataContainer.NotificationDatas)
                {
                    SendNotification(notification);
                }
            }
        }

        public void Initialize(bool cancelPendingNotifications)
        {
            Debug.Log("Initializing notification system...");

#if UNITY_ANDROID
            const string channelID = "channel_id";

            if (_initialized == false)
            {
                Debug.Log("Registering Android notification channel...");
                _initialized = true;
                AndroidNotificationChannel channel = new()
                {
                    Id = channelID,
                    Name = "Default Channel",
                    Importance = Importance.High,
                    Description = "Generic notifications",
                };

                AndroidNotificationCenter.RegisterNotificationChannel(channel);
            }

            if (cancelPendingNotifications)
            {
                Debug.Log("Canceling all pending Android notifications...");
                AndroidNotificationCenter.CancelAllNotifications();
            }
#endif
#if UNITY_IOS
            if (cancelPendingNotifications)
            {
                Debug.Log("Canceling all pending iOS notifications...");
                iOSNotificationCenter.RemoveAllScheduledNotifications();
                iOSNotificationCenter.RemoveAllDeliveredNotifications();
            }
#endif
        }

        private void SendNotification(NotificationData notificationData)
        {
            Debug.Log($"Preparing to send notification: {notificationData.Title} - {notificationData.Text}");

            TimeSpan adjustedDelay = CalculateAdjustedTimeDelay(notificationData);
            Debug.Log($"Adjusted time delay for notification: {adjustedDelay}");

#if UNITY_ANDROID
            const string channelID = "channel_id";

            AndroidNotification notification = new()
            {
                Title =  notificationData.Title,
                Text = notificationData.Text,
                FireTime = DateTime.UtcNow.Add(adjustedDelay)
            };

            if (notificationData.RepeatInterval.TotalHours > 0)
            {
                notification.RepeatInterval = notificationData.RepeatInterval;
                Debug.Log($"Notification will repeat every {notificationData.RepeatInterval.TotalHours} hours.");
            }
            if (!string.IsNullOrEmpty(notificationData.SmallIcon))
            {
                notification.SmallIcon = notificationData.SmallIcon;
                Debug.Log($"Using small icon: {notificationData.SmallIcon}");
            }
            if (!string.IsNullOrEmpty(notificationData.LargeIcon))
            {
                notification.LargeIcon = notificationData.LargeIcon;
                Debug.Log($"Using large icon: {notificationData.LargeIcon}");
            }
            if (!string.IsNullOrEmpty(notificationData.CustomData))
            {
                notification.IntentData = notificationData.CustomData;
                Debug.Log($"Custom data: {notificationData.CustomData}");
            }

            AndroidNotificationCenter.SendNotification(notification, channelID);
            Debug.Log("Android notification sent.");
#endif

#if UNITY_IOS
            iOSNotificationTimeIntervalTrigger timeTrigger = new()
            {
                TimeInterval = adjustedDelay,
                Repeats = false
            };

            iOSNotification notification = new()
            {
                Title = notificationData.Title,
                Subtitle = "",
                Body = notificationData.Text,
                Data = notificationData.CustomData,
                ShowInForeground = true,
                ForegroundPresentationOption = PresentationOption.Alert | PresentationOption.Sound,
                CategoryIdentifier = "category_a",
                ThreadIdentifier = "thread1",
                Trigger = timeTrigger,
            };

            iOSNotificationCenter.ScheduleNotification(notification);
            Debug.Log("iOS notification scheduled.");
#endif
        }
        
        public Task<bool> RequestNotificationPermission()
        {
            TaskCompletionSource<bool> tcs = new();

            if (Permission.HasUserAuthorizedPermission("android.permission.POST_NOTIFICATIONS"))
            {
                Debug.Log("Android notification permission already granted.");
                tcs.SetResult(true);
            }
            else
            {
                PermissionCallbacks permissionCallbacks = new();

                permissionCallbacks.PermissionGranted += permission =>
                {
                    if (permission == "android.permission.POST_NOTIFICATIONS")
                    {
                        Debug.Log("Android notification permission granted.");
                        tcs.TrySetResult(true);
                    }
                };

                permissionCallbacks.PermissionDenied += permission =>
                {
                    if (permission == "android.permission.POST_NOTIFICATIONS")
                    {
                        Debug.Log("Android notification permission denied.");
                        tcs.TrySetResult(false);
                    }
                };

                permissionCallbacks.PermissionDeniedAndDontAskAgain += permission =>
                {
                    if (permission == "android.permission.POST_NOTIFICATIONS")
                    {
                        Debug.Log("Android notification permission denied permanently.");
                        tcs.TrySetResult(false);
                    }
                };

                Permission.RequestUserPermissions(new[] { "android.permission.POST_NOTIFICATIONS" }, permissionCallbacks);
            }

            return tcs.Task;
        }

        public string AppWasOpenFromNotification()
        {
            Debug.Log("Checking if app was opened from a notification...");

#if UNITY_ANDROID
            AndroidNotificationIntentData notificationIntentData = AndroidNotificationCenter.GetLastNotificationIntent();

            if (notificationIntentData != null)
            {
                Debug.Log($"Android notification data found: {notificationIntentData.Notification.IntentData}");
                return notificationIntentData.Notification.IntentData;
            }

            Debug.Log("No Android notification data found.");
            return null;
#elif UNITY_IOS
            iOSNotification notificationIntentData = iOSNotificationCenter.GetLastRespondedNotification();

            if (notificationIntentData != null)
            {
                Debug.Log($"iOS notification data found: {notificationIntentData.Data}");
                return notificationIntentData.Data;
            }

            Debug.Log("No iOS notification data found.");
            return null;
#else
            Debug.Log("No notification data for this platform.");
            return null;
#endif
        }

        private static TimeSpan CalculateAdjustedTimeDelay(NotificationData notificationInfo)
        {
            Debug.Log($"Calculating adjusted time delay for notification: {notificationInfo.Title}");

            TimeSpan originalTimeDelay = notificationInfo.TimeDelayFromNow;
            Debug.Log($"Original time delay: {originalTimeDelay}");

            if (notificationInfo.NotificationSafeZoneData.Length == 0)
            {
                Debug.Log("No safe zones defined, returning original time delay.");
                return originalTimeDelay;
            }

            DateTime currentDateTime = DateTime.UtcNow;
            DateTime adjustedTime = currentDateTime.Add(originalTimeDelay);

            foreach (NotificationSafeZoneData safeZone in notificationInfo.NotificationSafeZoneData)
            {
                DateTime safeZoneStart;
                DateTime safeZoneEnd;

                if (safeZone.SafeZoneStart <= safeZone.SafeZoneEnd)
                {
                    safeZoneStart = adjustedTime.Date.AddHours(safeZone.SafeZoneStart);
                    safeZoneEnd = adjustedTime.Date.AddHours(safeZone.SafeZoneEnd);
                }
                else
                {
                    safeZoneStart = adjustedTime.Date.AddHours(safeZone.SafeZoneStart).AddDays(-1);
                    safeZoneEnd = adjustedTime.Date.AddHours(safeZone.SafeZoneEnd);
                }

                DateRange dateRange = new(safeZoneStart, safeZoneEnd);
                bool isWithinSafeZone = IsDateRangeWithinSafeZone(adjustedTime, dateRange);

                if (isWithinSafeZone)
                {
                    adjustedTime = safeZoneEnd;

                    if (adjustedTime - currentDateTime > TimeSpan.Zero)
                    {
                        adjustedTime = adjustedTime.AddHours(24);
                    }
                }
            }

            TimeSpan adjustedTimeDelay = adjustedTime - currentDateTime;
            Debug.Log($"Adjusted time delay: {adjustedTimeDelay}");
            return adjustedTimeDelay;
        }

        private static bool IsDateRangeWithinSafeZone(DateTime adjustedTime, DateRange dateRange)
        {
            bool isWithinRange = dateRange.WithInRange(adjustedTime);
            Debug.Log($"Is adjusted time within safe zone: {isWithinRange}");
            return isWithinRange;
        }
    }

    public class DateRange
    {
        private DateTime Start { get; }
        private DateTime End { get; }

        public DateRange(DateTime start, DateTime end)
        {
            Start = start;
            End = end;
        }

        public bool WithInRange(DateTime value)
        {
            return Start <= value && value <= End;
        }
    }
}
