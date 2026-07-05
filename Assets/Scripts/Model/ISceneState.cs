using Core;
using System;
namespace Model
{
    public interface ISceneState
    {
        public SceneType CurrentSceneType { get; }
        public SceneState CurrentSceneState { get; }
        public event Action<SceneType> OnLoadStart;
        public event Action<SceneType> OnLoadComplete;
    }
}