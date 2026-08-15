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
        private readonly ISceneLoader _sceneService;
        private readonly ISceneState _sceneState;
        private readonly ISaveDataService _saveDataService;
        private readonly IQuitService _quitService;
        private readonly IInputSource _inputSource;
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
            ISceneLoader sceneService,
            ISceneState sceneState,
            ISaveDataService saveDataService,
            IQuitService quitService,
            IInputSource inputSource,
            MenuView view,
            CancellationToken cancellationToken)
        {
            _sceneService = sceneService;
            _sceneState = sceneState;
            _saveDataService = saveDataService;
            _quitService = quitService;
            _inputSource = inputSource;
            _view = view;

            _sceneState.CurrentSceneState
                .Subscribe(state =>
                {
                    if (state == SceneId.Title)
                    {
                        _view.OnEnterTitle();
                        _currentPanel = PanelType.Home;
                    }
                    else if (state == SceneId.Orrery)
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

            //_inputSource.OnUISubmitPressed += view.HandleSubmitPressed;
            _inputSource.OnUICancelPressed += HandleUICancelPressed;
            _inputSource.OnPlayerCancelPressed += HandlePlayerCancelPressed;
        }

        private void HandleLoadSlotPressed(int slotIndex)
        {
            var slots = _saveDataService.GetSaveSlots();
            Debug.Log($"Slot[{slotIndex}]が選択されました");
            Debug.Log($"Slot情報：長さ[{slots.Count}]");
            if (slotIndex < slots.Count)
            {
                _saveDataService.Load(slots[slotIndex].id, false);
                _sceneService.LoadScene(SceneId.Orrery);
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
            _sceneService.LoadScene(SceneId.Orrery);
            _view.ExitLoad();
            _view.OnMenuClose();
            _inputSource.CurrentInputState.Value = InputState.Player;
        }

        private void HandleSaveStarted()
        {
            _saveDataService.Save(false);
            _view.ExitSave();
            _currentPanel = PanelType.Home;
        }

        private void HandleQuitToTitle()
        {
            _view.ExitQuit();
            _currentPanel = PanelType.Home;
            _sceneService.LoadScene(SceneId.Title);
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
            _previousInputState = _inputSource.CurrentInputState.Value;
            _inputSource.CurrentInputState.Value = InputState.UI;
        }
        
        private void HandleUICancelPressed()
        {
            Debug.Log("[MenuView] HandleCancelPressed");
            switch (_currentPanel)
            {
                case PanelType.Home:
                    _view.OnMenuClose();
                    _inputSource.CurrentInputState.Value = _previousInputState;
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