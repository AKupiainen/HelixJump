using System;
using UnityEngine;
using Volpi.Entertaiment.SDK.Localization;

namespace Volpi.Entertaiment.SDK.MobileNotifications
{
    [Serializable]
    public class NotificationData 
    {
        [SerializeField] private string _title;
        [SerializeField] private string _text;
        [SerializeField] private float _timeDelayFromNowHours; 
        [SerializeField] private string _smallIcon;
        [SerializeField] private string _largeIcon;
        [SerializeField] private string _customData;
        [SerializeField] private float _repeatIntervalHours; 
        [SerializeField] private NotificationSafeZoneData[] _notificationSafeZoneData = Array.Empty<NotificationSafeZoneData>();

        public string Title => LocalizationService.Instance.GetLocalizedValue(_title);
        public string Text => LocalizationService.Instance.GetLocalizedValue(_text);
        public TimeSpan TimeDelayFromNow => TimeSpan.FromHours(_timeDelayFromNowHours);
        public string SmallIcon => _smallIcon;
        public string LargeIcon => _largeIcon;
        public string CustomData => _customData;
        public TimeSpan RepeatInterval => TimeSpan.FromHours(_repeatIntervalHours);
        public NotificationSafeZoneData[] NotificationSafeZoneData => _notificationSafeZoneData;
    }
}