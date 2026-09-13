using Core;
using Model;
using UnityEngine;
using View;

namespace Presenter
{
    public class ParallaxPresenter : IUpdatable
    {
        private CoordSystemId _coord;
        
        private ParallaxView _view;
        private IJetpackSource _jetpackSource;
        private ICameraSource _cameraSource;

        public ParallaxPresenter(ParallaxView parallaxView, ICameraSource cameraSource, CoordSystemId coord)
        {
            _view = parallaxView;
            _cameraSource = cameraSource;
            _coord = coord;
        }
        
        public void OnUpdate(float deltaTime)
        {
            if (_coord == _cameraSource.Coord)
            {
                Debug.Log(_coord);
                _view.UpdateLoopLayers(_cameraSource.Position.x, _cameraSource.Size.x);
            }
        }
    }
}