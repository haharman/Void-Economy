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
        
        // Presenter
        private SurfacePlayerPresenter _surfacePlayerPresenter;
        
        public void Init(PlayerModel playerModel)
        {
            Debug.Log("[SurfaceRoot] Init");
            // Modelのインスタンスを作成
            _entityModel = new EntityModel();
            
            // Viewの初期化
            surfacePlayerView.Initialize();
            
            // Presenterのインスタンスを作成
            _surfacePlayerPresenter = new SurfacePlayerPresenter(playerModel, surfacePlayerView, parallaxView,
                this.destroyCancellationToken);
        }

        public void OnUpdate(float deltaTime)
        {
            
        }
    }
}