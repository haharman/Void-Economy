namespace Core
{
    public enum SceneType
    {
        None,
        Title,
        Orrery
    }
    
    public static class SceneCore
    {
        public static string GetSceneName(this SceneType sceneType)
        {
            return sceneType switch
            {
                SceneType.Title  => "TitleScene",
                SceneType.Orrery => "OrreryScene",
                _ => throw new System.ArgumentOutOfRangeException(nameof(sceneType), $"未定義のシーンです: {sceneType}")
            };
        }
        
        public static SceneType GetSceneType(string sceneName)
        {
            return sceneName switch
            {
                "TitleScene" => SceneType.Title,
                "OrreryScene" => SceneType.Orrery,
                _ => throw new System.ArgumentOutOfRangeException(nameof(sceneName), $"未定義のシーン名です: {sceneName}")
            };
        }
    }
}