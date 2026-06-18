using System.Threading;
using UnityEngine;
using View;
using Model;
using R3;

namespace Presenter
{
    public class SurfacePlayerPresenter
    {
        // Model
        private PlayerModel _playerModel;
        
        // View
        private SurfacePlayerView _playerView;
        private ParallaxView _parallaxView;
        
        // Presenter
        private AudioPresenter _audioPresenter;
        
        public SurfacePlayerPresenter(PlayerModel playerModel, SurfacePlayerView playerView, ParallaxView parallaxView, AudioPresenter audioPresenter, CancellationToken cancellationToken)
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
    }
}