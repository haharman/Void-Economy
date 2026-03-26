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
using UnityEngine.SceneManagement;

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
    private PlayerInputPresenter _playerInputPresenter;
    private YarnPresenter _yarnPresenter;
    private AudioPresenter _audioPresenter;
    
    // View
    [Header("View")]
    [SerializeField] private PlayerInputView playerInputView;
    [SerializeField] private YarnView yarnView;
    [SerializeField] private AudioView audioView;
    
    [Header("ScriptableObject")]
    [SerializeField] private AudioDataSO audioData;
    [SerializeField] private DefaultSaveDataSO defaultSaveData;

    private ISceneRoot _currentSceneRoot;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void RuntimeInit()
    {
        var titleScene = SceneCore.GetSceneName(SceneType.Title);
        if (SceneManager.GetActiveScene().name != titleScene)
        {
            SceneManager.LoadScene(titleScene);
        }
    }

    private void Awake()
    {
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
        _entityModel = new EntityModel();
        _physicsEngine = new PhysicsEngine();
        _yarnModel = new YarnModel(yarnView.variableStorage);
        _audioPresenter = new AudioPresenter();
        _inventoryModel = new InventoryModel();
        _playerModel = new PlayerModel(_globalStateModel, _entityModel, _physicsEngine);
        _questModel = new QuestModel();
        
        // Presenter層 インスタンスの作成
        _yarnPresenter = new YarnPresenter(yarnView, _inventoryModel);
        _playerInputPresenter = new PlayerInputPresenter(playerInputView, _playerModel);
        
        // View層の初期化
        audioView.Initialize(audioData);
        
        Debug.Log("[GlobalRoot] Init終了");
    }

    private void Update()
    {
        var deltaTime = Time.deltaTime;
        OnUpdate(deltaTime);
        _currentSceneRoot?.OnUpdate(deltaTime);
    }

    private void OnUpdate(float deltaTime)
    {
        _playerModel.OnUpdate(deltaTime);
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
        
        private void HandleOnLoadStart(SceneType sceneType)
        {
            OnLoadStart?.Invoke(sceneType);
        }

        private void HandleOnLoadComplete(SceneType sceneType)
        {
            OnLoadComplete?.Invoke(sceneType);
            InitScene(sceneType);
        }

        private void InitScene(SceneType sceneType)
        {
            switch (sceneType)
            {
                case SceneType.Title:
                    _titleRoot = FindFirstObjectByType<TitleRoot>();
                    _titleRoot.Init(this, this, this);
                    _currentSceneRoot = _titleRoot;
                    break;
                case SceneType.Surface:
                    var surface = _globalStateModel.currentSurface;
                    _surfaceRoot = FindFirstObjectByType<SurfaceRoot>();
                    _surfaceRoot.Init(surface, _playerModel, _entityModel);
                    _currentSceneRoot = _surfaceRoot;
                    break;
                case SceneType.Space:
                    _spaceRoot = FindFirstObjectByType<SpaceRoot>();
                    _spaceRoot.Init();
                    _currentSceneRoot = _spaceRoot;
                    break;
                default :
                    Debug.LogWarning($"[GlobalRoot] 未定義のシーン: {sceneType}");
                    break;
            }
        }

        #endregion
    #region IQuitService
    
        public void QuitGame()
        {
            // Dispose処理
            _sceneSwitcher.OnLoadStart -= HandleOnLoadStart;
            _sceneSwitcher.OnLoadComplete -= HandleOnLoadComplete;
                
            UnityEngine.Application.Quit();
        }
    
    #endregion
}

