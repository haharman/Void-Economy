using System;
using Core;
using Model;
using Presenter;
using R3;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Service
{
    public class SceneService : ISceneState, ISceneLoader
    {
        // public SceneType CurrentSceneType { get; }
        public ReactiveProperty<SceneState> CurrentSceneState { get; }
        public event Action<SceneType> LoadingStarted;
        public event Action<SceneType> LoadingCompleted;
        private bool _isLoading = false;
        
        public string ActiveSceneName => SceneManager.GetActiveScene().name;

        public SceneService(SceneType sceneType)
        {
            CurrentSceneState = new ReactiveProperty<SceneState>();
            // CurrentSceneType = sceneType;
        }
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
            if(SceneManager.GetActiveScene().name == sceneName)
            {
                Debug.LogWarning($"[SceneSwitcher] 既に {sceneName} がアクティブです。ロードをキャンセルしました。");
                return;
            }

            _isLoading = true;
            LoadingStarted?.Invoke(sceneType);
            
            Debug.Log($"[SceneSwitcher] {sceneName} のロードを開始します...");

            // 非同期ロードを開始（画面は固まりません）
            AsyncOperation asyncOp = SceneManager.LoadSceneAsync(sceneName);

            // ロード完了時に呼ばれるコールバックを登録
            if (asyncOp != null)
                asyncOp.completed += (op) =>
                {
                    _isLoading = false;
                    Debug.Log($"[SceneSwitcher] {sceneName} のロードが完了しました。");
                    LoadingCompleted?.Invoke(sceneType);
                };
        }
    }
}