using System;
using Core;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Model
{
    public class SceneSwitcher
    {
        public event Action<SceneType> OnLoadStart;
        public event Action<SceneType> OnLoadComplete;

        private bool _isLoading = false;

        public string ActiveSceneName => SceneManager.GetActiveScene().name;

        /// <summary>
        /// 非同期でシーンをロードします。
        /// </summary>
        public void LoadScene(SceneType sceneType)
        {
            string sceneName = SceneCore.GetSceneName(sceneType);
            if (_isLoading)
            {
                Debug.LogWarning($"[SceneSwitcher] 既にロード中です。({sceneName} への遷移をキャンセルしました)");
                return;
            }

            _isLoading = true;
            OnLoadStart?.Invoke(sceneType);
            
            Debug.Log($"[SceneSwitcher] {sceneName} のロードを開始します...");

            // 非同期ロードを開始（画面は固まりません）
            AsyncOperation asyncOp = SceneManager.LoadSceneAsync(sceneName);

            // ロード完了時に呼ばれるコールバックを登録
            if (asyncOp != null)
                asyncOp.completed += (op) =>
                {
                    _isLoading = false;
                    Debug.Log($"[SceneSwitcher] {sceneName} のロードが完了しました。");
                    OnLoadComplete?.Invoke(sceneType);
                };
        }
    }
}