using System;
using UnityEngine;
using Model;

using Model.Economy;
using Presenter;
using View;
using Core;
using Service;
using UnityEngine.SceneManagement;
using Yarn.Unity;

public class SystemRoot : MonoBehaviour, IQuitService
{
    
    // Service
    private SaveDataService _saveDataService;
    private SceneService _sceneService;
    private UpdateService _updateService;
    private UpdateService _systemUpdateService;
    private InputService _inputService;
    
    // Presenter
    private MenuPresenter _menuPresenter;
    
    // View
    [Header("View")]
    [SerializeField] private InputView inputView;
    [SerializeField] private AudioView audioView;
    [SerializeField] private MenuView menuView;
    
    [Header("ScriptableObject")]
    [SerializeField] private AudioDataSO audioData;
    [SerializeField] private DefaultSaveDataSo defaultSaveData;

    #region 初期化
    
    // 初期化の起点
    private void Awake()
    {
        InitializeApp();
    }

    private void InitializeApp()
    {
        // タイトルシーンかチェック
        var titleScene = SceneCore.GetSceneName(SceneId.Title);
        if(SceneManager.GetActiveScene().name != titleScene)
        {
            Debug.LogError($"[GlobalRoot] Titleシーンをロードするよう設定してください 現在のシーン: {SceneManager.GetActiveScene().name}");
        }
        
        // インスタンスの重複を解除
        var existing = FindObjectsOfType<SystemRoot>();
        if (existing.Length > 1)
        {
            Destroy(gameObject);
            Debug.Log("[GlobalRoot] Awake 重複");
            return;
        }
        
        // システムルートを破棄しない設定に
        DontDestroyOnLoad(gameObject);
        
        Bootstrap();
        Initialize();
    }

    // SystemRootの初期化
    private void Bootstrap()
    {
        Debug.Log("[GlobalRoot] Bootstrap()");
        
        // ここでConfigDataServiceの初期化
        // _configDataService = new ConfigDataService();
        
        // Service層 インスタンスの作成
        _saveDataService = new SaveDataService(defaultSaveData.data.DeepCopy());
        _sceneService = new SceneService(SceneId.Title);
        _updateService = new UpdateService();
        _systemUpdateService = new UpdateService();
        _inputService = new InputService(inputView);
        
        // ISceneService
        _sceneService.LoadingStarted += OnLoadingStarted;
        _sceneService.LoadingCompleted += OnLoadingCompleted;
        
        // Presenter層 インスタンスの作成
        _menuPresenter = new MenuPresenter(_sceneService , _sceneService, _saveDataService , this,_inputService ,menuView, this.destroyCancellationToken);

    }

    private void Initialize()
    {
        Debug.Log("[GlobalRoot] Initialize()");
        
        _inputService.Initialize();
        
        // 例外的：AudioPresenterはOrreryスコープ、Menuは直接操作するため。
        audioView.Initialize(audioData);
        
        // IUpdatable登録
        _systemUpdateService.Register(_inputService);
        _updateService.Register(_systemUpdateService);
    }

    private void OnLoadingStarted(SceneId sceneType)
    {
        _updateService.Unregister(_systemUpdateService);
    }
    
    private void OnLoadingCompleted(SceneId sceneType)
    {
        if (sceneType == SceneId.Orrery)
        {
            // OrreryRootと接続
            var orreryRoot = FindFirstObjectByType<OrreryRoot>();
            if (orreryRoot == null)
            {
                Debug.LogError("[GlobalRoot] OrreryRootが見つかりませんでした");
                return;
            }

            orreryRoot.Bootstrap(audioView, _inputService, _saveDataService);
            
            _saveDataService.Load(newGame:true);
            
            orreryRoot.Initialize(_systemUpdateService);
            _updateService.Register(_systemUpdateService);
        }
        else if (sceneType == SceneId.Title)
        {
            _updateService.Register(_systemUpdateService);
        }
    }
    
    # endregion
    
    private void Update()
    {
        if(_updateService != null)
            _updateService.OnUpdate(Time.deltaTime);
    }
    #region IQuitService
    
        public void QuitGame()
        {
            Debug.Log("[SystemRoot] QuitGame");
            // Dispose処理
            _sceneService.LoadingCompleted -= OnLoadingCompleted;
            #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
            #else
            UnityEngine.Application.Quit();
            #endif
        }
    
    #endregion
}

