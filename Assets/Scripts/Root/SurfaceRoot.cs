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
        // Model
        private EntityModel _entityModel;
        
        // View
        [SerializeField] private SurfacePlayerView surfacePlayerView;
        [SerializeField] private ParallaxView parallaxView;
        [SerializeField] private List<SurfaceView> surfaceViewList;
        private SurfaceView _currentSurfaceView;
        
        // Presenter
        private SurfacePlayerPresenter _surfacePlayerPresenter;
        private EntityPresenter _entityPresenter;
        
        public void Init(Surface surface, PlayerModel playerModel, EntityModel entityModel)
        {
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
            var entityConfigList = _currentSurfaceView.GetEntityConfigList();
            var entityViewList = _currentSurfaceView.GetEntityViewList();
            var entityModelList = _entityModel.Init(entityConfigList);
            
            // Presenterのインスタンスを作成
            _entityPresenter = new EntityPresenter(entityModelList, entityViewList, this.destroyCancellationToken);
            _surfacePlayerPresenter = new SurfacePlayerPresenter(playerModel, surfacePlayerView, parallaxView,
                this.destroyCancellationToken);
            
            // Viewの初期化
            surfacePlayerView.Initialize();
        }

        public void OnUpdate(float deltaTime)
        {
            
        }
    }
}