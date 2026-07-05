using System;
using Core;
using UnityEngine;
using Model;
using View;
using R3;

namespace Presenter
{
    /// <summary>
    /// 入力View(PlayerInputView)からのイベントを受け取り、
    /// Modelの更新や他のPresenter/Viewへの命令を行う仲介役(Presenter)。
    /// </summary>
    public class InputPresenter : IDisposable, IInputService, IUpdatable
    {
        // IInputService
        public event Action OnPlayerSubmitPressed;
        public event Action OnPlayerCancelPressed;
        public event Action OnUISubmitPressed;
        public event Action OnUICancelPressed;

        // Field
        private ISceneService _sceneService;
        private IQuitService _quitService;
        private readonly InputView _view;
        private readonly SurfacePlayerModel _playerModel;
        private readonly SpacePlayerModel _spacePlayerModel;
        private readonly YarnModel _yarnModel;
        private readonly GlobalStateModel _globalStateModel;
        
        private readonly IDisposable _inputStateSubscription;

        private Vector2 _moveInputInDialogue; // ダイアログ中の移動入力を一時的に保存する変数

        public InputPresenter(ISceneService sceneService, IQuitService quitService, InputView inputView ,SurfacePlayerModel surfacePlayerModel, YarnModel yarnModel, GlobalStateModel globalStateModel)
        {
            Debug.Log("[InputPresenter] Init");
            _sceneService = sceneService;
            _quitService = quitService;
            _view = inputView;
            _playerModel = surfacePlayerModel;
            _yarnModel = yarnModel;
            _globalStateModel = globalStateModel;

            _inputStateSubscription = _globalStateModel.CurrentInputState
                .Subscribe(HandleInputStateChanged);

            SubscribeEvents();
        }

        public void Initialize()
        {
            _globalStateModel.CurrentInputState.Value = InputState.UI;
            _view.Initialize();
        }

        public void Dispose()
        {
            Debug.Log("[InputPresenter] Dispose");
            _inputStateSubscription?.Dispose();
            UnsubscribeEvents();
        }

        private void SubscribeEvents()
        {
            _view.OnPlayerMoveChanged += HandlePlayerMoveChanged;
            _view.OnPlayerSubmitPressed += HandlePlayerSubmitPressed;
            _view.OnPlayerCancelPressed += HandlePlayerCancelPressed;
            _view.OnUIMoveChanged += HandleOnUIMoveChanged;
            _view.OnUISubmitPressed += HandleUISubmitPressed;
            _view.OnUICancelPressed += HandleUICancelPressed;
            
            _globalStateModel.OnDialogueStarted += HandleDialogueStarted;
            _globalStateModel.OnDialogueCompleted += HandleDialogueCompleted;
        }

        private void UnsubscribeEvents()
        {
            _view.OnPlayerMoveChanged -= HandlePlayerMoveChanged;
            _view.OnPlayerSubmitPressed -= HandlePlayerSubmitPressed;
            _view.OnPlayerCancelPressed -= HandlePlayerCancelPressed;
            _view.OnUIMoveChanged -= HandleOnUIMoveChanged;
            _view.OnUISubmitPressed -= HandleUISubmitPressed;
            _view.OnUICancelPressed -= HandleUICancelPressed;
            
            _globalStateModel.OnDialogueStarted -= HandleDialogueStarted;
            _globalStateModel.OnDialogueCompleted -= HandleDialogueCompleted;
        }

        private void HandlePlayerMoveChanged(Vector2 input)
        {
            _moveInputInDialogue = input;
            switch (_globalStateModel.CurrentInputState.Value)
            {
                case InputState.Surface:
                    _playerModel.MoveInput = input;
                    break;
                case InputState.Space:
                    _spacePlayerModel.Fly(input);
                    break;
                default:
                    break;
            }
        }

        private void HandlePlayerSubmitPressed()
        {
            OnPlayerSubmitPressed?.Invoke();
            switch (_globalStateModel.CurrentInputState.Value)
            {
                case InputState.Surface:
                    _playerModel.Interact();
                    break;
                default:
                    break;
            }
        }

        // 現在はメニューを開く動作にそのままバイパスされている。変更の可能性あり
        private void HandlePlayerCancelPressed()
        {
            OnPlayerCancelPressed?.Invoke();
            _playerModel.MoveInput = Vector2.zero; // 移動入力をリセット
        }

        private void HandleOnUIMoveChanged(Vector2 input)
        {
            
        }

        private void HandleUISubmitPressed()
        {
            OnUISubmitPressed?.Invoke();
        }

        private void HandleUICancelPressed()
        {
            OnUICancelPressed?.Invoke();
        }

        // globalStateModel.currentInputStateの変化を受け取る
        private void HandleInputStateChanged(InputState currentInputState)
        {
            _view.currentInputState = currentInputState;
        }
        
        // InputServiceに含まれる。MenuPresenterが実行する
        public void OnMenuClosed(InputState state)
        {
            _view.currentInputState = state;
        }

        private void HandleDialogueStarted()
        {
            _playerModel.MoveInput = Vector2.zero; // 移動入力をリセット
        }
        
        private void HandleDialogueCompleted()
        {
            _playerModel.MoveInput = _moveInputInDialogue; // ダイアログ中に保存していた移動入力を復元
        }

        // OnUpdatable
        public void OnUpdate(float deltaTime)
        {
            _view.OnUpdate();
        }
    }
}