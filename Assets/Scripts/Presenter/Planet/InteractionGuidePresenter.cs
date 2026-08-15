using Model;
using View;

namespace Presenter
{
    public class InteractionGuidePresenter : IUpdatable
    {
        private readonly InteractionGuideView _view;
        private readonly InteractionGuideModel _model;

        public InteractionGuidePresenter(InteractionGuideView view, InteractionGuideModel model)
        {
            _view = view;
            _model = model;
        }

        public void OnUpdate(float deltaTime)
        {
            _model.Tick(deltaTime);
        }
    }
}