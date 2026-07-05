using System;
using System.Threading;
using Core;
using Model;
using R3;
using UnityEngine;
using View;

namespace Presenter
{
    public class MenuPresenter
    {
        private readonly MenuView _view;
        private readonly ISceneService _sceneService;
        private readonly IDataService _dataService;
        private readonly IQuitService _quitService;
        private readonly IInputService _inputService;
        private readonly GlobalStateModel _globalStateModel;
        private enum PanelType
        {
            Home = 0,
            Load = 1,
            Save = 2,
            Quit = 3,
            Settings = 4
        }
        private PanelType _currentPanel;
        private InputState _previousInputState;

        public MenuPresenter(
            ISceneService sceneService,
            IDataService dataService,
            IQuitService quitService,
            IInputService inputService,
            MenuView view,
            GlobalStateModel globalStateModel,
            CancellationToken cancellationToken)
        {
            _sceneService = sceneService;
            _dataService = dataService;
            _quitService = quitService;
            _inputService = inputService;
            _view = view;
            _globalStateModel = globalStateModel;

            globalStateModel.CurrentSceneState
                .Subscribe(state =>
                {
                    if (state == SceneState.Title)
                    {
                        _view.OnEnterTitle();
                        _currentPanel = PanelType.Home;
                    }
                    else if (state == SceneState.Surface || state == SceneState.Space)
                        _view.OnExitTitle();
                })
                .RegisterTo(cancellationToken);

            _view.OnLoadSlotPressed += HandleLoadSlotPressed;
            _view.OnSaveStarted += HandleSaveStarted;
            _view.OnQuitToTitle += HandleQuitToTitle;
            _view.OnQuitToDesktop += HandleQuitToDesktop;
            _view.OnSettingsChanged += HandleSettingsChanged;
            _view.OnNewGameSlotPressed += HandleNewGameSlotPressed;
            _view.OnMenuPanelChanged += HandleMenuPanelChanged;

            //_inputService.OnUISubmitPressed += view.HandleSubmitPressed;
            _inputService.OnUICancelPressed += HandleUICancelPressed;
            _inputService.OnPlayerCancelPressed += HandlePlayerCancelPressed;
        }

        private void HandleLoadSlotPressed(int slotIndex)
        {
            var slots = _dataService.GetSaveSlots();
            Debug.Log($"Slot[{slotIndex}]が選択されました");
            Debug.Log($"Slot情報：長さ[{slots.Count}]");
            if (slotIndex < slots.Count)
            {
                var sceneType = _dataService.Load(slots[slotIndex].id);
                _sceneService.LoadScene(sceneType);
            }
            else
            {
                Debug.LogError("[MenuPresenter] SlotIndexのデータは存在しません");
            }
            _view.ExitLoad();
        }

        private void HandleNewGameSlotPressed()
        {
            Debug.Log("newGameSlotが選択されました");
            var sceneType = _dataService.LoadNewGame();
            _sceneService.LoadScene(sceneType);
            _view.ExitLoad();
        }

        private void HandleSaveStarted()
        {
            _dataService.Save(false);
            _view.ExitSave();
            _currentPanel = PanelType.Home;
        }

        private void HandleQuitToTitle()
        {
            _view.ExitQuit();
            _currentPanel = PanelType.Home;
            _sceneService.LoadScene(SceneType.Title);
        }

        private void HandleQuitToDesktop()
        {
            _view.ExitQuit();
            _currentPanel = PanelType.Home;
            _quitService.QuitGame();
        }

        private void HandleMenuPanelChanged(int type)
        {
            _currentPanel = (PanelType)type;
        }

        private void HandleSettingsChanged()
        {
        }
        

        private void HandlePlayerCancelPressed()
        {
            Debug.Log("[MenuPresenter] Open Menu");
            _view.OnMenuOpen();
            _currentPanel = PanelType.Home;
            _previousInputState = _globalStateModel.CurrentInputState.Value;
            _globalStateModel.CurrentInputState.Value = InputState.UI;
        }
        
        private void HandleUICancelPressed()
        {
            Debug.Log("[MenuView] HandleCancelPressed");
            switch (_currentPanel)
            {
                case PanelType.Home:
                    _view.OnMenuClose();
                    _globalStateModel.CurrentInputState.Value = _previousInputState;
                    _inputService.OnMenuClosed(_previousInputState);
                    break;
                case PanelType.Load:
                    _view.ExitLoad();
                    _currentPanel = PanelType.Home;
                    break;
                case PanelType.Save:
                    _view.ExitSave();
                    _currentPanel = PanelType.Home;
                    break;
                case PanelType.Quit:
                    _view.ExitQuit();
                    _currentPanel = PanelType.Home;
                    break;
                case PanelType.Settings:
                    _view.ExitSettings();
                    _currentPanel = PanelType.Home;
                    break;
                default:
                    Debug.LogError("[MenuView] 存在しないPanelTypeです currentPanel:"+_currentPanel);
                    break;
            }
        }
    }
}