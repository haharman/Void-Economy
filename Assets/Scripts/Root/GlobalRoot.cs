using System;
using System.Collections.Generic;
using UnityEngine;
using Model;
using Model.Physics;
using Model.Economy;
using Presenter;
using View;
using Core;
using Root;
using UnityEditor;
using UnityEngine.SceneManagement;
using Yarn.Unity;

public class GlobalRoot : MonoBehaviour, IDataService, ISceneService, IQuitService
{
    // Root
    [Header("Root")]
    private TitleRoot _titleRoot;
    private SurfaceRoot _surfaceRoot;
    private SpaceRoot _spaceRoot;
    private DataPersister _dataPersister;
    private SceneSwitcher _sceneSwitcher;
    
    
    // Model
    private GlobalStateModel _globalStateModel;
    private QuestModel _questModel;
    
    private PhysicsEngine _physicsEngine;
    private EconomyEngine _economyEngine;
    private EntityModel _entityModel;
    private YarnModel _yarnModel;
    
    private PlayerModel _playerModel;
    private InventoryModel _inventoryModel;
    
    private SaveData _loadedData;
    
    // Presenter
    private InputPresenter _inputPresenter;
    private AudioPresenter _audioPresenter;
    
    // View
    [Header("View")]
    [SerializeField] private InputView inputView;
    [SerializeField] private YarnView yarnView;
    [SerializeField] private AudioView audioView;
    
    [Header("ScriptableObject")]
    [SerializeField] private AudioDataSO audioData;
    [SerializeField] private DefaultSaveDataSO defaultSaveData;
    
    [SerializeField] private DialogueRunner dialogueRunner;
    [SerializeField] private InMemoryVariableStorage yarnVariableStorage;

    private ISceneRoot _currentSceneRoot;

    #region 初期化
    
    // 初期化の起点
    private void Awake()
    {
        var titleScene = SceneCore.GetSceneName(SceneType.Title);
        if(SceneManager.GetActiveScene().name != titleScene)
        {
            Debug.LogError($"[GlobalRoot] Titleシーンをロードするよう設定してください 現在のシーン: {SceneManager.GetActiveScene().name}");
        }
        // インスタンスの重複を解除
        var existing = FindObjectsOfType<GlobalRoot>();
        if (existing.Length > 1)
        {
            Destroy(gameObject);
            Debug.Log("[GlobalRoot] Awake 重複");
            return;
        }
        DontDestroyOnLoad(gameObject);
        Init();
        InitScene(SceneType.Title);
    }

    // GlobalRootの初期化
    private void Init()
    {
        Debug.Log("[GlobalRoot] Init 開始");
        
        // Root層 インスタンスの作成
        _dataPersister = new DataPersister(defaultSaveData.data.DeepCopy());
        _sceneSwitcher = new SceneSwitcher();
        // ISceneService
        _sceneSwitcher.OnLoadStart += HandleOnLoadStart;
        _sceneSwitcher.OnLoadComplete += HandleOnLoadComplete;
        
        // Model層 インスタンスの作成
        _globalStateModel = new GlobalStateModel(this);
        _economyEngine = new EconomyEngine();
        _physicsEngine = new PhysicsEngine();
        _yarnModel = new YarnModel(dialogueRunner ,yarnVariableStorage, _globalStateModel);
        _entityModel = new EntityModel(_yarnModel);
       _inventoryModel = new InventoryModel();
        _playerModel = new PlayerModel(_globalStateModel, _entityModel, _physicsEngine);
        _audioPresenter = new AudioPresenter(audioView, _playerModel, this.destroyCancellationToken);
        _questModel = new QuestModel();
        
        // Presenter層 インスタンスの作成
        _inputPresenter = new InputPresenter(inputView, _playerModel, _yarnModel, _globalStateModel);
        
        // View層の初期化
        audioView.Initialize(audioData);
        
        Debug.Log("[GlobalRoot] Init終了");
    }
    
    // シーンのルートクラスを探して、初期化する
    private void InitScene(SceneType sceneType)
    {
        switch (sceneType)
        {
            case SceneType.Title:
                _titleRoot = FindFirstObjectByType<TitleRoot>();
                if(_titleRoot == null)
                {
                    Debug.LogError("[GlobalRoot] TitleRootが見つかりませんでした");
                    return;
                }
                _titleRoot.Init(this, this, this);
                _currentSceneRoot = _titleRoot;
                break;
            case SceneType.Surface:
                var surface = _globalStateModel.surface;
                _surfaceRoot = FindFirstObjectByType<SurfaceRoot>();
                if(_surfaceRoot == null)  
                {
                    Debug.LogError("[GlobalRoot] SurfaceRootが見つかりませんでした");
                    return;
                }
                _surfaceRoot.Init(this, surface, _playerModel, _entityModel, _audioPresenter);
                _currentSceneRoot = _surfaceRoot;
                break;
            case SceneType.Space:
                _spaceRoot = FindFirstObjectByType<SpaceRoot>();
                if(_spaceRoot == null)
                {
                    Debug.LogError("[GlobalRoot] SpaceRootが見つかりませんでした");
                    return;
                }
                _spaceRoot.Init();
                _currentSceneRoot = _spaceRoot;
                break;
            default :
                Debug.LogWarning($"[GlobalRoot] 未定義のシーン: {sceneType}");
                return;
        }
    }
    
    # endregion
    
    private void Update()
    {
        var deltaTime = Time.deltaTime;
        OnUpdate(deltaTime);
        _currentSceneRoot?.OnUpdate(deltaTime);
    }
    
    private void OnUpdate(float deltaTime)
    {
        if(_playerModel != null) _playerModel.OnUpdate(deltaTime);
        if(_surfaceRoot != null) _surfaceRoot.OnUpdate(deltaTime);
        if(_audioPresenter != null) _audioPresenter.OnUpdate(deltaTime);
    }

    #region IDataService
        // データの集約（Pack）と保存（Save）
        public void SaveGame()
        {
            var globalStateData = PackGlobalState();
            var physicsData = PackPhysics();
            var economyData = PackEconomy();
            var inventoryData = PackInventory();
            var playerData = PackPlayer();
            var yarnData = PackYarn();
            
            
            _dataPersister.Save(new SaveData(globalStateData, physicsData, economyData, inventoryData, playerData, yarnData));
            
            GlobalStateSaveData PackGlobalState()
            {
                var data = new GlobalStateSaveData();
                return data;
            }
            
            PhysicsSaveData PackPhysics()
            {
                var data = new PhysicsSaveData();
                return data;
            }

            EconomySaveData PackEconomy()
            {
                var data = new EconomySaveData();
                return data;
            }

            InventorySaveData PackInventory()
            {
                var data = new InventorySaveData();
                return data;
            }

            PlayerSaveData PackPlayer()
            {
                var data = new PlayerSaveData();
                return data;
            }
            
            List<YarnVariable> PackYarn()
            {
                var data = new List<YarnVariable>();
                return data;
            }
        }

        public SceneType LoadGame()
        {
            _loadedData = _dataPersister.Load();
            Unpack(_loadedData);
            return _loadedData.global.sceneType;
        }
        
        public void NewGame()
        {
            SaveData data = defaultSaveData.data.DeepCopy();
            Unpack(data);
        }
        
        private void Unpack(SaveData data)
        {
            UnpackGlobalState();
            UnpackPhysics();
            UnpackEconomy();
            UnpackInventory();
            UnpackPlayer();
            UnpackYarn();
            void UnpackGlobalState()
            {

            }
            
            void UnpackPhysics()
            {

            }

            void UnpackEconomy()
            {

            }

            void UnpackInventory()
            {

            }

            void UnpackPlayer()
            {

            }
            
            void UnpackYarn()
            {

            }
        }
    
    #endregion
    #region ISceneService
    
        public event Action<SceneType> OnLoadStart;
        public event Action<SceneType> OnLoadComplete;
        public void LoadScene(SceneType sceneType)
        {
            _sceneSwitcher.LoadScene(sceneType);
        }
        
        // SceneSwitcherからロード開始通知を受け取る
        private void HandleOnLoadStart(SceneType sceneType)
        {
            OnLoadStart?.Invoke(sceneType);
        }

        // SceneSwitcherからロード終了通知を受け取る
        private void HandleOnLoadComplete(SceneType sceneType)
        {
            OnLoadComplete?.Invoke(sceneType);
            InitScene(sceneType);
        }
    

        #endregion
    #region IQuitService
    
        public void QuitGame()
        {
            Debug.Log("[GlobalRoot] QuitGame");
            // Dispose処理
            _sceneSwitcher.OnLoadStart -= HandleOnLoadStart;
            _sceneSwitcher.OnLoadComplete -= HandleOnLoadComplete;
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
    UnityEngine.Application.Quit();
#endif
        }
    
    #endregion
}

