using System.Threading;
using Core;
using Model;
using R3;
using UnityEngine;
using View;

namespace Presenter
{
    public class PlanetPresenter : IUpdatable
    {
        private readonly CoordSystemId _id;
        private readonly PlanetView _view;
        private readonly GameObject _coordObj;
        private readonly IJetpackSource _jetpackSource;
        private readonly IPlanetModel _planetModel;
        private readonly CoordView _coordView;

        private bool _loaded = true;

        public PlanetPresenter(CoordSystemId id, PlanetView view, CoordView coordView, IJetpackSource jetpackSource, 
            IPlanetModel planetModel, CancellationToken cancellationToken)
        {
            _id = id;
            _view = view;
            _coordObj = coordView.gameObject;
            _coordView = coordView;
            _planetModel = planetModel;
            _jetpackSource = jetpackSource;
            jetpackSource.LoadSurfaceRequested
                .Where(receivedId => receivedId == _id)
                .Subscribe(_ => LoadSurface())
                .RegisterTo(cancellationToken);
            jetpackSource.UnloadSurfaceRequested
                .Where(receivedId => receivedId == _id)
                .Subscribe(_ => UnloadSurface())
                .RegisterTo(cancellationToken);
        }

        public void Initialize()
        {
            Debug.Log("[PlanetPresenter] Initialize()");
            if(_jetpackSource.CoordPos.CurrentValue.CoordSystem == _id)
                LoadSurface();
            else
                UnloadSurface();
        }

        public void OnUpdate(float deltaTime)
        {
            Vector2 pos = _planetModel.Position;
            _view.UpdatePosition(pos);
            if(_loaded)
                _coordView.UpdatePosition(_jetpackSource.CoordPos.CurrentValue.Position.x, _planetModel.Radius);
        }
        
        private void LoadSurface()
        {
            //_coordObj.SetActive(true);
            //_loaded = true;
        }

        private void UnloadSurface()
        {
            //_coordObj.SetActive(false);
            //_loaded = false;
        }
    }
}