namespace Volpi.Entertaiment.SDK.MobileNotifications
{
    using UnityEngine;

    [CreateAssetMenu(fileName = "NotificationData", menuName = "Notification Data", order = 1)]
    public class NotificationDataContainer : ScriptableObject
    {
        [SerializeField] private NotificationData[] _notificationDatas;

        public NotificationData[] NotificationDatas => _notificationDatas;
    }
}