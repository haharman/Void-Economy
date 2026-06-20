using System;
using System.Collections.Generic;
using Core;
using Model;
using UnityEngine;
using View;
using Presenter;
using UnityEngine.SceneManagement;

namespace Root
{
    public class SurfaceRoot : MonoBehaviour, ISceneRoot
    {
        // Service
        private ISceneService _sceneService;
        
        // Model
        private EntityModel _entityModel;
        private InteractionGuideModel _interactionGuideModel;
        
        // View
        [SerializeField] private SurfacePlayerView surfacePlayerView;
        [SerializeField] private ParallaxView parallaxView;
        [SerializeField] private InteractionGuideView interactionGuideView;
        [SerializeField] private List<SurfaceView> surfaceViewList;
        private SurfaceView _currentSurfaceView;
        
        // Presenter
        private SurfacePlayerPresenter _surfacePlayerPresenter;
        private EntityPresenter _entityPresenter;
        private AudioPresenter _audioPresenter;
        
        // Titleに戻るイベント（仮実装）
        public event Action OnBackToTitle;
        
        public void Init(ISceneService sceneService,Surface surface, PlayerModel playerModel, EntityModel entityModel, AudioPresenter audioPresenter)
        {
            _sceneService = sceneService;
            _currentSurfaceView = null;
            foreach (var surfaceView in surfaceViewList)
            {
                if (surfaceView.surface == surface)
                {
                    _currentSurfaceView = surfaceView;
                    break;
                }
            }
            if (_currentSurfaceView == null)
                Debug.LogError($"[SurfaceRoot] {surface} が見つかりませんでした");
            Debug.Log("[SurfaceRoot] Init");
            
            
            // Modelのインスタンスを作成
            _entityModel = entityModel;
            _interactionGuideModel = new InteractionGuideModel(playerModel);
            var entityConfigList = _currentSurfaceView.GetEntityConfigList();
            var entityViewList = _currentSurfaceView.GetEntityViewList();
            var entityModelList = _entityModel.Init(entityConfigList); // Entity生成に必要な一覧のデータを渡す
            // Presenterのインスタンスを作成
            _audioPresenter = audioPresenter;
            _entityPresenter = new EntityPresenter(entityModelList, entityViewList, this.destroyCancellationToken);
            _surfacePlayerPresenter = new SurfacePlayerPresenter(playerModel, surfacePlayerView, parallaxView,
                _audioPresenter, this.destroyCancellationToken);
            
            // Viewの初期化
            surfacePlayerView.Initialize();
            interactionGuideView.Initialize(_interactionGuideModel);
        }

        public void OnUpdate(float deltaTime)
        {
            _interactionGuideModel.OnUpdate(deltaTime);
        }

        public void LoadTitle() => _sceneService.LoadScene(SceneType.Title);
    }
}