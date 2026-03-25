using System.Collections.Generic;
using Core;
using Model;
using DG.Tweening;
using UnityEngine;

namespace View // 名前空間は大文字から始めるのがC#の一般的な規約です
{
    public class AudioView : MonoBehaviour
    {
        [Header("Audio Sources")]
        [SerializeField] private AudioSource musicSource;
        [SerializeField] private AudioSource footstepSource;
        [SerializeField] private AudioSource oneShotSource;
        
        [Header("Settings")]
        [SerializeField] private float musicChangeFadeDuration = 1f;
        [SerializeField] private float musicStopFadeDuration = 0.5f;
        [SerializeField] private float loopFadeDuration = 0.5f;
        [SerializeField] private float footstepInterval = 0.4f;

        private Dictionary<LoopId, AudioSource> _loopSources = new Dictionary<LoopId, AudioSource>();
        
        private AudioDataSO _audioData;
        
        private float _lastFootstepTime;
        private bool _footstepPlaying;
        private AudioClip _footstepClip;
        
        public void Initialize(AudioDataSO audioData) 
        {
            _audioData = audioData;
            _footstepPlaying = false;
            _lastFootstepTime = Time.time;
        }

        public void PlayMusic(MusicId id) 
        {
            var nextMusicClip = _audioData.GetMusicClip(id);
            musicSource.DOKill();
            
            if (musicSource.isPlaying)
            {
                musicSource.DOFade(0f, musicChangeFadeDuration)
                    .OnComplete(() => 
                    {
                        musicSource.Stop();
                        musicSource.clip = nextMusicClip;
                        musicSource.volume = 1f;
                        musicSource.Play();
                    });
            }
            else
            {
                musicSource.Stop();
                musicSource.clip = nextMusicClip;
                musicSource.volume = 1f;
                musicSource.Play();
            }
        }
        
        public void StopMusic() 
        {
            musicSource.DOKill();
            musicSource.DOFade(0f, musicStopFadeDuration)
                .OnComplete(() => musicSource.Stop());
        }
        
        public void PlayLoop(LoopId id) 
        {
            if (_loopSources.TryGetValue(id, out var source))
            {
                if (!source.isPlaying)
                {
                    source.volume = 1f;
                    source.Play();
                }
            }
            else
            {
                var newSource = gameObject.AddComponent<AudioSource>();
                newSource.clip = _audioData.GetLoopClip(id);
                newSource.loop = true;
                newSource.volume = 1f;
                newSource.Play();
                _loopSources[id] = newSource;
            }
        }
        
        public void StopLoop(LoopId id) 
        {
            if (_loopSources.TryGetValue(id, out var source))
            {
                _loopSources.Remove(id);
                source.DOKill();
                source.DOFade(0f, loopFadeDuration)
                    .OnComplete(() =>
                    {
                        source.Stop();
                        Destroy(source);
                    });
            }
        }
        
        public void StopAllLoops() 
        {
            foreach (var source in _loopSources.Values)
            {
                var s = source;
                s.DOKill();
                s.DOFade(0f, loopFadeDuration)
                    .OnComplete(() =>
                    {
                        s.Stop();
                        Destroy(s);
                    });
            }
            _loopSources.Clear();
        }
        
        public void PlayFootstep(FootstepId id) 
        {
            _footstepPlaying = true;
            _footstepClip = _audioData.GetFootstepClip(id);
        }
        
        public void StopFootstep() 
        {
            _footstepPlaying = false;
        }
        
        public void PlayOneShot(OneShotId id) 
        {
            var clip = _audioData.GetOneShotClip(id);
            if (clip != null)
            {
                oneShotSource.PlayOneShot(clip);
            }
        }
        
        private void Update() 
        {
            if (_footstepPlaying && Time.time - _lastFootstepTime >= footstepInterval)
            {
                footstepSource.PlayOneShot(_footstepClip);
                _lastFootstepTime = Time.time;
            }
        }
    }
}