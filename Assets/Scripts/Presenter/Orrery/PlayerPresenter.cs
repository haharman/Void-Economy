using System;
using System.Collections.Generic;
using System.Threading;
using Core;
using UnityEngine;
using View;
using Model;
using R3;

namespace Presenter
{
    public class PlayerPresenter : IUpdatable, ISaveDataReader, ISaveDataWriter
    {
        private const float ThresholdY = 20f;
        // Model
        private JetpackModel _model;

        // View
        private PlayerView _view;

        // Presenter
        private AudioPresenter _audioPresenter;
        
        // Service
        private IInputService _inputService;

        public event Action OnEnterSpace;
        
        private CoordSystemId? _previousCoordSystem = null;
        private Vector2 _previousForward = Vector2.zero;

        public PlayerPresenter(JetpackModel jetpackModel, PlayerView playerView, IInputService inputService,
            AudioPresenter audioPresenter, CancellationToken cancellationToken, IReadOnlyDictionary<CoordSystemId, Transform> coordSystemTransformDictionary)
        {
            _model = jetpackModel;
            _view = playerView;
            _inputService = inputService;
            _inputService.CurrentMoveInput
                .Subscribe(vector2 => _model.MoveInput = vector2)
                .RegisterTo(cancellationToken);
            _inputService.OnPlayerSubmitPressed += HandleSubmitPressed;
            _audioPresenter = audioPresenter;
            _model.CoordPos
                .Subscribe(coordPos => HandleUpdateCoordPos(coordPos))
                .RegisterTo(cancellationToken);
            
            _view.Initialize(coordSystemTransformDictionary);
        }

        public void OnUpdate(float deltaTime)
        {
            _model.Tick(deltaTime);
        }

        public void ReadFrom(SaveData data)
        {
            Debug.Log("[PlayerPresenter] ReadFrom()");
            PlayerSaveData d = data.player;
            _model.ReadFrom(new CoordPos(d.coordSystem, d.position, d.velocity, d.forward));
            Debug.Log($"[PlayerPresenter] ReadFrom: {d.position}, {d.velocity}, {d.coordSystem}, {d.forward}");
        }

        public void WriteTo(SaveData data)
        {
            PlayerSaveData d = new PlayerSaveData();
            d.coordSystem = _model.CoordPos.CurrentValue.CoordSystem;
            d.position = _model.CoordPos.CurrentValue.Position;
            d.velocity = _model.CoordPos.CurrentValue.Velocity;
            d.forward = _model.CoordPos.CurrentValue.Forward;
            data.player = d;
        }

        private void HandleSubmitPressed()
        {
            _model.Interact();
        }

        private void HandleUpdateCoordPos(CoordPos coordPos)
        {
            if (coordPos.CoordSystem == _previousCoordSystem)
            {
                _view.OnUpdatePositionVelocity(coordPos.Position, coordPos.Velocity);
                if (coordPos.Forward != _previousForward)
                {
                    _view.OnUpdateRotation(coordPos.Forward);
                    _previousForward = coordPos.Forward;
                }
            }
            else
            {
                _view.OnUpdateCoordPos(coordPos);
                _previousCoordSystem = coordPos.CoordSystem;
            }
        }

        // 未配線
        public void Dispose()
        {
            _inputService.OnPlayerSubmitPressed -= HandleSubmitPressed;
        }
        
    }
}