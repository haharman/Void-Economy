using System;
using Core;
using R3;

namespace Model
{
    public class GlobalStateModel
    {
        // privateフィールド
        private readonly ISceneState _sceneState;
        
        // publicフィールド
        public bool IsPaused { get; set; } // Pauseがないので不要では？
        public ReactiveProperty<SceneState> CurrentSceneState { get; } = new(SceneState.Title);
        public Surface CurrentSurface { get; set; }
        public ReactiveProperty<InputState> CurrentInputState { get; set; } = new(InputState.Disable);
        
        // イベント
        public event Action OnDialogueStarted;
        public event Action OnDialogueCompleted;
        
        public GlobalStateModel(ISceneState sceneService)
        {
            _sceneState = sceneService;
            _sceneState.OnLoadStart += HandleOnLoadStart;
            _sceneState.OnLoadComplete += HandleOnLoadComplete;
        }

        public void Init()
        {
            CurrentInputState.Value = InputState.UI;
        }

        private void HandleOnLoadStart(SceneType sceneType)
        {
            switch (sceneType)
            {
                case SceneType.Title:
                    CurrentSceneState.Value = SceneState.ToTitle;
                    break;
                case SceneType.Surface:
                    CurrentSceneState.Value = SceneState.ToSurface;
                    break;
                case SceneType.Space:
                    CurrentSceneState.Value = SceneState.ToSpace;
                    break;
            }
            CurrentInputState.Value = InputState.Disable;
        }
        
        private void HandleOnLoadComplete(SceneType sceneType)
        {
            switch (sceneType)
            {
                case SceneType.Title:
                    CurrentSceneState.Value = SceneState.Title;
                    CurrentInputState.Value = InputState.UI;
                    break;
                case SceneType.Surface:
                    CurrentSceneState.Value = SceneState.Surface;
                    CurrentSurface = Surface.Mine;
                    CurrentInputState.Value = InputState.Surface;
                    break;
                case SceneType.Space:
                    CurrentSceneState.Value = SceneState.Space;
                    CurrentInputState.Value = InputState.Space;
                    break;
            }
        }
        
        public void InvokeOnDialogueStarted()
        {
            OnDialogueStarted?.Invoke();
        }
        
        public void InvokeOnDialogueCompleted()
        {
            OnDialogueCompleted?.Invoke();
        }
    }
}