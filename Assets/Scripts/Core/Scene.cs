namespace Core
{
    public enum SceneType
    {
        Title,
        Surface,
        Space
    }
    
    public static class SceneCore
    {
        public static string GetSceneName(this SceneType sceneType)
        {
            return sceneType switch
            {
                SceneType.Title  => "TitleScene",
                SceneType.Space  => "SpaceScene",
                SceneType.Surface => "SurfaceScene",
                _ => throw new System.ArgumentOutOfRangeException(nameof(sceneType), $"未定義のシーンです: {sceneType}")
            };
        }
        
        public static SceneType GetSceneType(string sceneName)
        {
            return sceneName switch
            {
                "TitleScene" => SceneType.Title,
                "SpaceScene" => SceneType.Space,
                "SurfaceScene" => SceneType.Surface,
                _ => throw new System.ArgumentOutOfRangeException(nameof(sceneName), $"未定義のシーン名です: {sceneName}")
            };
        }
    }
}