using Core;
using System;
using R3;

namespace Model
{
    public interface ISceneState
    {
        // public SceneType CurrentSceneType { get; }
        public ReactiveProperty<SceneState> CurrentSceneState { get; }
        public event Action<SceneType> LoadingStarted;
        public event Action<SceneType> LoadingCompleted;
    }
}