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
        
        public SurfacePlayerPresenter(PlayerModel playerModel, SurfacePlayerView playerView, ParallaxView parallaxView, CancellationToken cancellationToken)
        {
            _playerModel = playerModel;
            _playerView = playerView;
            _parallaxView = parallaxView;
            
            _playerModel.position
                .Subscribe(pos => _playerView.OnUpdatePosition(pos))
                .RegisterTo(cancellationToken);

            _playerModel.velocity
                .Subscribe(vel => _playerView.OnUpdateVelocity(vel.x))
                .RegisterTo(cancellationToken);
        }
    }
}