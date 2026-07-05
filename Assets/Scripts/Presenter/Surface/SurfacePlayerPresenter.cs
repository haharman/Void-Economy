using System.Threading;
using Core;
using UnityEngine;
using View;
using Model;
using R3;

namespace Presenter
{
    public class SurfacePlayerPresenter : IUpdatable, ISaveDataReader, ISaveDataWriter
    {
        // Model
        private SurfacePlayerModel _model;

        // View
        private SurfacePlayerView _view;
        private ParallaxView _parallaxView;

        // Presenter
        private AudioPresenter _audioPresenter;

        public SurfacePlayerPresenter(SurfacePlayerModel playerModel, SurfacePlayerView playerView,
            ParallaxView parallaxView, AudioPresenter audioPresenter, CancellationToken cancellationToken)
        {
            _model = playerModel;
            _view = playerView;
            _parallaxView = parallaxView;
            _audioPresenter = audioPresenter;

            _model.Position
                .Subscribe(pos => _view.OnUpdatePosition(pos))
                .RegisterTo(cancellationToken);

            _model.Velocity
                .Subscribe(vel => _view.OnUpdateVelocity(vel.x))
                .RegisterTo(cancellationToken);
        }

        public void OnUpdate(float deltaTime)
        {
            _model.Tick(deltaTime);
        }

        public void ReadFrom(SaveData data)
        {
            PlayerSaveData d = data.player;
            _model.IsSurface = d.isSurface;
            _model.Position.Value = d.position;
            _model.Velocity.Value = d.velocity;
        }

        public void WriteTo(SaveData data)
        {
            PlayerSaveData d = new PlayerSaveData();
            d.isSurface = _model.IsSurface;
            d.position = _model.Position.Value;
            d.velocity = _model.Velocity.Value;
            data.player = d;
        }
    }
}