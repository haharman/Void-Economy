using Core;
using System;
using R3;

namespace Model
{
    public interface ISceneState
    {
        // public SceneType CurrentSceneType { get; }
        public ReactiveProperty<SceneId> CurrentSceneState { get; }
        public event Action<SceneId> LoadingStarted;
        public event Action<SceneId> LoadingCompleted;
    }
}