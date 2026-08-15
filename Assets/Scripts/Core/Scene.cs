namespace Core
{
    
    public static class SceneCore
    {
        public static string GetSceneName(this SceneId sceneType)
        {
            return sceneType switch
            {
                SceneId.Title  => "TitleScene",
                SceneId.Orrery => "OrreryScene",
                _ => throw new System.ArgumentOutOfRangeException(nameof(sceneType), $"未定義のシーンです: {sceneType}")
            };
        }
        
        public static SceneId GetSceneType(string sceneName)
        {
            return sceneName switch
            {
                "TitleScene" => SceneId.Title,
                "OrreryScene" => SceneId.Orrery,
                _ => throw new System.ArgumentOutOfRangeException(nameof(sceneName), $"未定義のシーン名です: {sceneName}")
            };
        }
    }
}