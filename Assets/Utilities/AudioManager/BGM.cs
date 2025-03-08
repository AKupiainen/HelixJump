using UnityEngine;

namespace Volpi.Entertaiment.SDK.Utilities
{
    [CreateAssetMenu(fileName = "NewBGM", menuName = "Audio/BGM")]
    public class BGM : ScriptableObject
    {
        [SerializeField] private AudioClip _clip;
        [SerializeField, Range(0f, 1f)] private float _volume = 1f;
        [SerializeField] private bool _loop = true;

        public AudioClip Clip => _clip;
        public float Volume => _volume;
        public bool Loop => _loop;
    }
}