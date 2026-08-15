using System;
using System.Collections.Generic;
using Core;
using Model;
using DG.Tweening;
using UnityEngine;
using R3;

namespace View // 名前空間は大文字から始めるのがC#の一般的な規約です
{
    public class AudioView : MonoBehaviour
    {
        [Header("Audio Sources")]
        [SerializeField] private AudioSource musicSource;
        [SerializeField] private AudioSource footstepSource;
        [SerializeField] private AudioSource oneShotSource;
        
        [Header("Settings")]
        [SerializeField] private float loopFadeDuration = 0.5f;
        [SerializeField] private float footstepInterval = 0.4f;
        [SerializeField] private float musicVolume = 0.5f;
        [SerializeField] private float footstepVolume = 1f;
        [SerializeField] private float oneShotVolume = 1f;
        [SerializeField] private float loopVolume = 0.5f;

        private Dictionary<LoopId, AudioSource> _loopSources = new Dictionary<LoopId, AudioSource>();
        
        private AudioDataSO _audioData;
        private List<MusicId> _trackList;
        private float _lastFootstepTime;
        // フェードアウト中
        public bool IsMusicSourceFadeOut;
        private AudioClip _footstepClip;
        
        public void Initialize(AudioDataSO audioData) 
        {
            _audioData = audioData;
            _lastFootstepTime = Time.time;
            _trackList = _audioData.GetMusicIdList();
            if(_trackList.Count == 0) Debug.LogError("[AudioView] AudioDataSOのトラックリストが空です");
        }

        public bool IsMusicSourcePlaying()
        {
            return  musicSource.isPlaying;
        }

        public MusicId PlayMusic(MusicId currentId, float fadeDuration = 1f) 
        {
            var nextId = GetNextMusicId(currentId);
            var nextMusicClip = _audioData.GetMusicClip(nextId);
            IsMusicSourceFadeOut = false;
            musicSource.DOKill();
            {
                musicSource.Stop();
                musicSource.clip = nextMusicClip;
                musicSource.volume = musicVolume;
                musicSource.Play();
            }
            return nextId;
            // 次の曲を取得する
            MusicId GetNextMusicId(MusicId currentId)
            {
                if(!_trackList.Contains(currentId))
                {
                    Debug.LogWarning($"[AudioView] トラックID {currentId} がトラックリストに存在しません");
                    return _trackList[0];
                }
                int currentIndex = _trackList.IndexOf(currentId);
                int nextIndex = (currentIndex + 1) % _trackList.Count;
                var nextId = _trackList[nextIndex];
                return nextId;
            }
        }
        
        public void StopMusic(float fadeDuration = 1f)
        {
            musicSource.DOKill();
            IsMusicSourceFadeOut = true;
            musicSource.DOFade(0f, fadeDuration)
                .OnComplete(() =>
                {
                    musicSource.Stop();
                    IsMusicSourceFadeOut = false;
                });
        }

        public void PauseMusic()
        {
            if (musicSource.isPlaying)
            {
                musicSource.Pause();
            }
        }

        public void ResumeMusic()
        {
            if (!musicSource.isPlaying && musicSource.clip != null)
            {
                musicSource.UnPause();
            }
        }
        
        public void PlayLoop(LoopId id) 
        {
            if (_loopSources.TryGetValue(id, out var source))
            {
                if (!source.isPlaying)
                {
                    source.volume = loopVolume;
                    source.Play();
                }
            }
            else
            {
                var newSource = gameObject.AddComponent<AudioSource>();
                newSource.clip = _audioData.GetLoopClip(id);
                newSource.loop = true;
                newSource.volume = loopVolume;
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
            if (Time.time - _lastFootstepTime >= footstepInterval)
            {
                AudioClip footstepClip = _audioData.GetFootstepClip(id);
                footstepSource.PlayOneShot(footstepClip);
                _lastFootstepTime = Time.time;
            }
        }
        
        public void PlayOneShot(OneShotId id) 
        {
            var clip = _audioData.GetOneShotClip(id);
            if (clip != null)
            {
                oneShotSource.PlayOneShot(clip);
            }
        }
    }
}