using Model;
using UnityEngine;
using View;

namespace Presenter
{
    public class ParallaxPresenter : IUpdatable
    {
        private ParallaxView _view;
        private IJetpackSource _jetpackSource;

        public ParallaxPresenter(ParallaxView parallaxView, IJetpackSource jetpackSource)
        {
            _view = parallaxView;
        }
        
        public void OnUpdate(float deltaTime)
        {
            // カメラのX座標を渡さなければいけないが、とりあえずJetpack座標
            _view.UpdateLayers(_jetpackSource.CoordPos.CurrentValue.Position.x);
        }
    }
}