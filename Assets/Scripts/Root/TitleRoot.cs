using Core;
using UnityEngine;
using View;
using Presenter;
using Model;
using UnityEngine.SceneManagement;

namespace Root
{
    public class TitleRoot : MonoBehaviour
    {
        private ISceneService _sceneService;
        private IDataService _dataService;
        private IQuitService _quitService;
        
        [SerializeField] private TitleView view;
        
        private TitlePresenter _presenter;

        public void Init(IDataService dataService, IQuitService quitService, ISceneService sceneService, IUpdatableService updatableService)
        {
            Debug.Log("[TitleRoot] Init");
            _dataService = dataService;
            _quitService = quitService;
            _sceneService = sceneService;
            _presenter = new TitlePresenter(view, _sceneService, _dataService, _quitService);
        }

        public void LoadSurface() => _sceneService.LoadScene(SceneType.Surface);
        public void QuitGame() => _quitService.QuitGame();
    }
}