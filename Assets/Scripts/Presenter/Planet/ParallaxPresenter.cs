using Core;
using Model;
using UnityEngine;
using View;

namespace Presenter
{
    public class ParallaxPresenter : IUpdatable
    {
        private CoordSystemId _coord;
        
        private ParallaxLoopView _view;
        private IJetpackSource _jetpackSource;
        private ICameraSource _cameraSource;

        public ParallaxPresenter(ParallaxLoopView parallaxView, ICameraSource cameraSource, CoordSystemId coord)
        {
            _view = parallaxView;
            _cameraSource = cameraSource;
            _coord = coord;
        }
        
        public void OnUpdate(float deltaTime)
        {
            if(_coord == _cameraSource.Coord)
                _view.UpdateLoopLayers(_cameraSource.Position.x, _cameraSource.Size.x);
            else
                Debug.Log("描画されないときはOnUpdate配信を止めよう");
        }
    }
}