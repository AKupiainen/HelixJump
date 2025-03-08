using System;
using UnityEngine;

namespace Volpi.Entertaiment.SDK.MobileNotifications
{
    [Serializable]
    public class NotificationSafeZoneData
    {
        [SerializeField]
        private float _safeZoneStart;
            
        [SerializeField]
        private float _safeZoneEnd;
            
        public NotificationSafeZoneData(float safeZoneStart, float safeZoneEnd)
        {
            _safeZoneStart = safeZoneStart;
            _safeZoneEnd = safeZoneEnd;
        }

        public float SafeZoneStart => _safeZoneStart;
        public float SafeZoneEnd => _safeZoneEnd;
    }
}