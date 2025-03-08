using System.Collections.Generic;
using UnityEngine;

namespace Volpi.Entertaiment.SDK.Utilities
{
    public class AudioManager : Singleton<AudioManager>
    {
        [SerializeField] private AudioSource _bgmSource;
        [SerializeField] private int _poolSize = 10;
        
        private Queue<AudioSource> _sfxPool;
        private List<AudioSource> _activeSfx;
        private GameObject _sfxContainer;
        
        [SerializeField, Range(0f, 1f)] private float _masterVolume = 1f;
        [SerializeField, Range(0f, 1f)] private float _sfxVolume = 1f;
        [SerializeField, Range(0f, 1f)] private float _bgmVolume = 1f;

        public void InitializeSfxPool()
        {
            _sfxPool = new Queue<AudioSource>();
            _activeSfx = new List<AudioSource>();
            
            _sfxContainer = new GameObject("SFX Container");
            _sfxContainer.transform.SetParent(transform);

            for (int i = 0; i < _poolSize; i++)
            {
                AudioSource source = CreateAudioSource();
                _sfxPool.Enqueue(source);
            }
        }

        private AudioSource CreateAudioSource()
        {
            GameObject sfxObject = new("SFX Source");
            sfxObject.transform.SetParent(_sfxContainer.transform);
            AudioSource source = sfxObject.AddComponent<AudioSource>();
            source.playOnAwake = false;
            
            SfxAutoReturn autoReturn = sfxObject.AddComponent<SfxAutoReturn>();
            autoReturn.Initialize(this);
            
            return source;
        }

        public void PlaySfx(Sfx sfx)
        {
            if (sfx == null || sfx.Clip == null)
            {
                return;
            }
            
            AudioSource source = _sfxPool.Count > 0 ? _sfxPool.Dequeue() : CreateAudioSource();
            source.clip = sfx.Clip;
            source.volume = sfx.Volume * _sfxVolume * _masterVolume;
            source.pitch = sfx.Pitch;
            source.loop = sfx.Loop;
            source.Play();
            _activeSfx.Add(source);
            
            if (!sfx.Loop && source.TryGetComponent(out SfxAutoReturn autoReturn))
            {
                autoReturn.SetReturnTime(sfx.Clip.length);
            }
        }

        public void PlayBGM(BGM bgm)
        {
            if (bgm == null || bgm.Clip == null)
            {
                return;
            }
            
            _bgmSource.clip = bgm.Clip;
            _bgmSource.volume = bgm.Volume * _bgmVolume * _masterVolume;
            _bgmSource.loop = bgm.Loop;
            _bgmSource.Play();
        }

        public void StopBGM()
        {
            _bgmSource.Stop();
        }
        
        public void ReturnToPool(AudioSource source)
        {
            source.Stop();
            _activeSfx.Remove(source);
            _sfxPool.Enqueue(source);
        }
        
        public void SetMasterVolume(float volume)
        {
            _masterVolume = Mathf.Clamp01(volume);
            UpdateVolumes();
        }

        public void SetSfxVolume(float volume)
        {
            _sfxVolume = Mathf.Clamp01(volume);
        }
        
        public void SetBGMVolume(float volume)
        {
            _bgmVolume = Mathf.Clamp01(volume);
            _bgmSource.volume = _bgmVolume * _masterVolume;
        }
        
        private void UpdateVolumes()
        {
            foreach (AudioSource source in _activeSfx)
            {
                source.volume = _sfxVolume * _masterVolume;
            }
            
            _bgmSource.volume = _bgmVolume * _masterVolume;
        }
    }
}
