using System;
using UnityEngine;
using Model;
using Model.Physics;
using Model.Economy;
using Presenter;
using View;
using Core;
using Service;
using UnityEngine.SceneManagement;
using Yarn.Unity;

public class GlobalRoot : MonoBehaviour, IQuitService
{
    // Root
    private TitleRoot _titleRoot;
    private SurfaceRoot _surfaceRoot;
    private SpaceRoot _spaceRoot;
    
    // Service
    private DataService _dataService;
    private SceneSwitcher _sceneSwitcher;
    private UpdatableHub _updatableHub;
    
    // Model
    private GlobalStateModel _globalStateModel;
    private QuestModel _questModel;
    
    private PhysicsEngine _physicsEngine;
    private EconomyEngine _economyEngine;
    private EntityModel _entityModel;
    private YarnModel _yarnModel;
    private SurfacePlayerModel _surfacePlayerModel;
    private SpacePlayerModel _spacePlayerModel;

    private InventoryModel _inventoryModel;
    
    // Presenter
    private InputPresenter _inputPresenter;
    private AudioPresenter _audioPresenter;
    private MenuPresenter _menuPresenter;
    
    // View
    [Header("View")]
    [SerializeField] private InputView inputView;
    [SerializeField] private YarnView yarnView;
    [SerializeField] private AudioView audioView;
    [SerializeField] private MenuView menuView;
    
    [Header("ScriptableObject")]
    [SerializeField] private AudioDataSO audioData;
    [SerializeField] private DefaultSaveDataSo defaultSaveData;
    [SerializeField] private DialogueRunner dialogueRunner;
    [SerializeField] private InMemoryVariableStorage yarnVariableStorage;

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
        Bootstrap();
        InitPresenters();
        InitScene(SceneType.Title);
    }

    // GlobalRootの初期化
    private void Bootstrap()
    {
        Debug.Log("[GlobalRoot] Init 開始");
        
        // Service層 インスタンスの作成
        _dataService = new DataService(defaultSaveData.data.DeepCopy());
        _sceneSwitcher = new SceneSwitcher();
        _updatableHub = new UpdatableHub();
        
        // ISceneService
        _sceneSwitcher.OnLoadComplete += InitScene;
        
        // Model層 インスタンスの作成
        _globalStateModel = new GlobalStateModel(_sceneSwitcher);
        _economyEngine = new EconomyEngine();
        _physicsEngine = new PhysicsEngine();
        _yarnModel = new YarnModel(dialogueRunner ,yarnVariableStorage, _globalStateModel);
        _entityModel = new EntityModel(_yarnModel);
        _inventoryModel = new InventoryModel();
        _surfacePlayerModel = new SurfacePlayerModel(_entityModel);
        _spacePlayerModel = new SpacePlayerModel(_entityModel, _physicsEngine);
        
        _questModel = new QuestModel();
        
        // Presenter層 インスタンスの作成
        _audioPresenter = new AudioPresenter(audioView, _surfacePlayerModel, this.destroyCancellationToken);
        _inputPresenter = new InputPresenter(_sceneSwitcher, this, inputView, _surfacePlayerModel, _yarnModel, _globalStateModel);
        _menuPresenter = new MenuPresenter(_sceneSwitcher ,_dataService , this,_inputPresenter ,menuView, _globalStateModel, this.destroyCancellationToken);
        // Presenter層 IUpdatable登録
        _updatableHub.RegisterGlobal(_inputPresenter);
        _updatableHub.RegisterGlobal(_audioPresenter);

    }

    private void InitPresenters()
    {
        _audioPresenter.Initialize(audioData);
        _inputPresenter.Initialize();
        
        Debug.Log("[GlobalRoot] Init終了");
    }

    // シーンのルートクラスを探して、初期化する
    private void InitScene(SceneType sceneType)
    {
        _updatableHub.ClearUpdatables();
        switch (sceneType)
        {
            case SceneType.Title:
                _titleRoot = FindFirstObjectByType<TitleRoot>();
                if(_titleRoot == null)
                {
                    Debug.LogError("[GlobalRoot] TitleRootが見つかりませんでした");
                    return;
                }
                _titleRoot.Init(_updatableHub);
                break;
            case SceneType.Surface:
                var surface = _globalStateModel.CurrentSurface;
                _surfaceRoot = FindFirstObjectByType<SurfaceRoot>();
                if(_surfaceRoot == null)  
                {
                    Debug.LogError("[GlobalRoot] SurfaceRootが見つかりませんでした");
                    return;
                }
                _surfaceRoot.Init(_sceneSwitcher, _updatableHub, surface, _surfacePlayerModel, _entityModel, _audioPresenter);
                break;
            case SceneType.Space:
                _spaceRoot = FindFirstObjectByType<SpaceRoot>();
                if(_spaceRoot == null)
                {
                    Debug.LogError("[GlobalRoot] SpaceRootが見つかりませんでした");
                    return;
                }
                _spaceRoot.Init();
                break;
            default :
                Debug.LogWarning($"[GlobalRoot] 未定義のシーン: {sceneType}");
                return;
        }
    }
    
    # endregion
    
    private void Update()
    {
        _updatableHub.OnUpdate();
    }
    #region IQuitService
    
        public void QuitGame()
        {
            Debug.Log("[GlobalRoot] QuitGame");
            // Dispose処理
            _sceneSwitcher.OnLoadComplete -= InitScene;
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
    UnityEngine.Application.Quit();
#endif
        }
    
    #endregion
}

