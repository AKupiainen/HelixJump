namespace Volpi.Entertainment.SDK.Utilities
{
    using UnityEngine;
    using UnityEngine.Rendering;

    public class SpeedLineController : MonoBehaviour
    {
        private Volume _volume;
        private SpeedLinesEffect _speedLinesEffect;

        [Range(0f, 10f)]
        private float _speed = 1f;

        void Start()
        {
            _volume = FindObjectOfType<Volume>();

            if (_volume != null && _volume.profile.TryGet(out _speedLinesEffect))
            {
                Debug.Log("SpeedLinesEffect found in the Volume.");
            }
            else
            {
                Debug.LogError("No SpeedLinesEffect found in the Volume Profile!");
            }
        }

        private void Update()
        {
            if (_speedLinesEffect != null)
            {
                _speedLinesEffect.Speed.value = _speed;
            }
        }

        public void SetSpeed(float speed)
        {
            _speed = speed;
        }
    }
}