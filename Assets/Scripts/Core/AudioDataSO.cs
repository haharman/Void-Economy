using Yarn.Saliency;

namespace Core
{
    using System;
    using System.Collections.Generic;
    using UnityEngine;
    
    [CreateAssetMenu(fileName = "AudioData", menuName = "ScriptableObjects/AudioData")]
    public class AudioDataSO : ScriptableObject {
        [Serializable]
        public struct MusicConfig {
            public MusicId id;
            public AudioClip clip;
        }
        
        [Serializable]
        public struct LoopConfig {
            public LoopId id;
            public AudioClip clip;
        }
        
        [Serializable]
        public struct FootstepConfig {
            public FootstepId id;
            public List<AudioClip> clips;
        }
        
        [Serializable]
        public struct OneShotConfig {
            public OneShotId id;
            public AudioClip clip;
        }
        [SerializeField] private List<MusicConfig> musicList;
        [SerializeField] private List<LoopConfig> loopList;
        [SerializeField] private List<FootstepConfig> footStepList;
        [SerializeField] private List<OneShotConfig> oneShotList;
        
        public List<MusicId> GetMusicIdList() {
            List<MusicId> idList = new List<MusicId>();
            foreach (var config in musicList) {
                idList.Add(config.id);
            }
            return idList;
        }
        public AudioClip GetMusicClip(MusicId musicId) {
            var config = musicList.Find(x => x.id == musicId);
            return config.clip;
        }
        
        public AudioClip GetLoopClip(LoopId loopId) {
            var config = loopList.Find(x => x.id == loopId);
            return config.clip;
        }
        
        public AudioClip GetFootstepClip(FootstepId footstepId) {
            var config = footStepList.Find(x => x.id == footstepId);
            return config.clips.RandomElement();
        }
        
        public AudioClip GetOneShotClip(OneShotId oneShotId) {
            var config = oneShotList.Find(x => x.id == oneShotId);
            return config.clip;
        }
    }
}