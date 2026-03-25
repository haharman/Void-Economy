using Core;
using Model;
using View;

namespace Presenter
{
    public class TitlePresenter
    {
        private readonly TitleView _view;
        private readonly ISceneService _sceneService;
        private readonly IDataService _dataService;
        private readonly IQuitService _quitService;

        public TitlePresenter(TitleView view, ISceneService sceneService, IDataService dataService, IQuitService quitService)
        {
            _view = view;
            _sceneService = sceneService;
            _dataService = dataService;
            _quitService = quitService;

            // ビューのイベントにロジックを登録
            _view.OnStartClicked += HandleStart;
            _view.OnLoadClicked += HandleLoad;
            _view.OnExitClicked += HandleQuit;

            // セーブデータがなければロードボタンを無効化する等の処理
            _view.SetLoadButtonActive(false); 
        }

        private void HandleStart()
        {
            _dataService.NewGame();
            _sceneService.LoadScene(SceneType.Surface);
        }

        private void HandleLoad()
        {
            SceneType lastScene = _dataService.LoadGame();
            _sceneService.LoadScene(lastScene);
        }

        private void HandleQuit()
        {
            _quitService.QuitGame();
        }
    }
}