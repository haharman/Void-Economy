using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public class PlayModeStartSceneSetter
{
    const string Path = "Assets/Scenes/TitleScene.unity";
    [MenuItem("Tools/Set Play Mode Start Scene")]
    private static void SetStartScene()
    {
        Debug.Log("[PlayModeStartSceneSetter] Play Mode Start Scene を設定します...");
        var sceneAsset = AssetDatabase.LoadAssetAtPath<SceneAsset>(Path);
        if (sceneAsset == null)
        {
            Debug.LogError($"シーンが見つかりません: {Path}");
            return;
        }
        EditorSceneManager.playModeStartScene = sceneAsset;
    }
}