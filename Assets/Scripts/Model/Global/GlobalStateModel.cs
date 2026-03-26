using Core;

namespace Model
{
    public class GlobalStateModel
    {
        public GlobalStateModel(ISceneService sceneService)
        {
            _sceneService = sceneService;
            _sceneService.OnLoadStart += HandleOnLoadStart;
            _sceneService.OnLoadComplete += HandleOnLoadComplete;
            currentSceneState = SceneState.Title;
        }
        private readonly ISceneService _sceneService;
        public enum SceneState
        {
            ToTitle,
            Title,
            ToSurface,
            Surface,
            ToSpace,
            Space
        }
        
        public bool isPaused { get; set; }
        public SceneState currentSceneState { get; private set; }
        public Surface currentSurface { get; set; } = Surface.Mine;
        
        private void HandleOnLoadStart(SceneType sceneType)
        {
            switch (sceneType)
            {
                case SceneType.Title:
                    currentSceneState = SceneState.ToTitle;
                    break;
                case SceneType.Surface:
                    currentSceneState = SceneState.ToSurface;
                    break;
                case SceneType.Space:
                    currentSceneState = SceneState.ToSpace;
                    break;
            }
        }
        
        private void HandleOnLoadComplete(SceneType sceneType)
        {
            switch (sceneType)
            {
                case SceneType.Title:
                    currentSceneState = SceneState.Title;
                    break;
                case SceneType.Surface:
                    currentSceneState = SceneState.Surface;
                    break;
                case SceneType.Space:
                    currentSceneState = SceneState.Space;
                    break;
            }
        }
    }
}