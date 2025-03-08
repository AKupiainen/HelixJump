using UnityEngine;

[CreateAssetMenu(fileName = "NewSfx", menuName = "Audio/Sfx")]
public class Sfx : ScriptableObject
{
    [SerializeField] private AudioClip _clip;
    [SerializeField, Range(0f, 1f)] private float _volume = 1f;
    [SerializeField, Range(0.1f, 3f)] private float _pitch = 1f;
    [SerializeField] private bool _loop;

    public AudioClip Clip => _clip;
    public float Volume => _volume;
    public float Pitch => _pitch;
    public bool Loop => _loop;
}