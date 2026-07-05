using Core;
using Model;
using View;

namespace Presenter
{
    public class TitlePresenter
    {
        private readonly TitleView _view;


        public TitlePresenter(TitleView view)
        {
            _view = view;
        }
    }
}