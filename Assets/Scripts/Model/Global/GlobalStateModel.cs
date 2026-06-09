using System;
using Core;

namespace Model
{
    public class GlobalStateModel
    {
        // フィールド
        private readonly ISceneService _sceneService;
        public bool isPaused { get; set; }
        public SceneState sceneState { get; private set; }
        public Surface surface { get; set; }
        public InputState inputState { get; set; }
        
        // イベント
        public event Action OnDialogueStarted;
        public event Action OnDialogueCompleted;
        
        public GlobalStateModel(ISceneService sceneService)
        {
            _sceneService = sceneService;
            _sceneService.OnLoadStart += HandleOnLoadStart;
            _sceneService.OnLoadComplete += HandleOnLoadComplete;
            sceneState = SceneState.Title;
        }
        
        private void HandleOnLoadStart(SceneType sceneType)
        {
            switch (sceneType)
            {
                case SceneType.Title:
                    sceneState = SceneState.ToTitle;
                    break;
                case SceneType.Surface:
                    sceneState = SceneState.ToSurface;
                    break;
                case SceneType.Space:
                    sceneState = SceneState.ToSpace;
                    break;
            }
            inputState = InputState.Disable;
        }
        
        private void HandleOnLoadComplete(SceneType sceneType)
        {
            switch (sceneType)
            {
                case SceneType.Title:
                    sceneState = SceneState.Title;
                    inputState = InputState.UI;
                    break;
                case SceneType.Surface:
                    sceneState = SceneState.Surface;
                    surface = Surface.Mine;
                    inputState = InputState.Surface;
                    break;
                case SceneType.Space:
                    sceneState = SceneState.Space;
                    inputState = InputState.Space;
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