using System.Threading;
using UnityEngine;
using View;
using Model;
using R3;

namespace Presenter
{
    public class SurfacePlayerPresenter : IUpdatable
    {
        // Model
        private SurfacePlayerModel _playerModel;
        
        // View
        private SurfacePlayerView _playerView;
        private ParallaxView _parallaxView;
        
        // Presenter
        private AudioPresenter _audioPresenter;
        
        public SurfacePlayerPresenter(SurfacePlayerModel playerModel, SurfacePlayerView playerView, ParallaxView parallaxView, AudioPresenter audioPresenter, CancellationToken cancellationToken)
        {
            _playerModel = playerModel;
            _playerView = playerView;
            _parallaxView = parallaxView;
            _audioPresenter = audioPresenter;
            
            _playerModel.position
                .Subscribe(pos => _playerView.OnUpdatePosition(pos))
                .RegisterTo(cancellationToken);

            _playerModel.velocity
                .Subscribe(vel => _playerView.OnUpdateVelocity(vel.x))
                .RegisterTo(cancellationToken);
        }

        public void OnUpdate(float deltaTime)
        {
            _playerModel.Tick(deltaTime);
        }
    }
}