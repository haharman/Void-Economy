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
    public class SurfaceRoot : MonoBehaviour
    {
        // Service
        private ISceneService _sceneService;
        private IUpdatableService _updatableService;
        
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
        private InteractionGuidePresenter _interactionGuidePresenter;
        private AudioPresenter _audioPresenter;
        
        public void Init(ISceneService sceneService, IUpdatableService updatableService, Surface surface, SurfacePlayerModel playerModel, EntityModel entityModel, AudioPresenter audioPresenter)
        {
            _sceneService = sceneService;
            _updatableService = updatableService;
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
            _interactionGuidePresenter = new InteractionGuidePresenter(interactionGuideView, _interactionGuideModel);
            
            // Presenter層 IUpdatable登録
            updatableService.Register(_interactionGuidePresenter);
            updatableService.Register(_surfacePlayerPresenter);
            
            // Viewの初期化
            surfacePlayerView.Initialize();
            interactionGuideView.Initialize(_interactionGuideModel);
        }

        public void LoadTitle()
        {
            _updatableService.ClearUpdatables();
            _sceneService.LoadScene(SceneType.Title);
        }
    }
}