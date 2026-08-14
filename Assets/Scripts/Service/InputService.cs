using System;
using Core;
using UnityEngine;
using View;
using Presenter;
using R3;

namespace Service
{
    /// <summary>
    /// 入力View(PlayerInputView)からのイベントを受け取り、
    /// Modelの更新や他のPresenter/Viewへの命令を行う仲介役(Presenter)。
    /// </summary>
    public class InputService : IDisposable, IInputService, IUpdatable
    {
        // IInputService
        public ReactiveProperty<InputState> CurrentInputState { get; set; }
        public ReactiveProperty<Vector2> CurrentMoveInput { get; }
        public event Action OnPlayerSubmitPressed;
        public event Action OnPlayerCancelPressed;
        public event Action OnUISubmitPressed;
        public event Action OnUICancelPressed;

        // Field
        private readonly InputView _view;
        private readonly IDisposable _inputStateSubscription;
        private Vector2 _moveInputInDialogue; // ダイアログ中の移動入力を一時的に保存する変数

        public InputService(InputView inputView)
        {
            Debug.Log("[InputPresenter] Init");
            _view = inputView;
            CurrentInputState = new ReactiveProperty<InputState>();
            CurrentMoveInput = new ReactiveProperty<Vector2>();
            _inputStateSubscription = this.CurrentInputState
                .Subscribe(HandleInputStateChanged);

            SubscribeEvents();
        }

        public void Initialize()
        {
            CurrentInputState.Value = InputState.UI;
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
            _view.OnPlayerMoveChanged += HandleMoveChanged;
            _view.OnPlayerSubmitPressed += HandleSubmitPressed;
            _view.OnPlayerCancelPressed += HandlePlayerCancelPressed;
            _view.OnUIMoveChanged += HandleOnUIMoveChanged;
            _view.OnUISubmitPressed += HandleUISubmitPressed;
            _view.OnUICancelPressed += HandleUICancelPressed;
        }

        private void UnsubscribeEvents()
        {
            _view.OnPlayerMoveChanged -= HandleMoveChanged;
            _view.OnPlayerSubmitPressed -= HandleSubmitPressed;
            _view.OnPlayerCancelPressed -= HandlePlayerCancelPressed;
            _view.OnUIMoveChanged -= HandleOnUIMoveChanged;
            _view.OnUISubmitPressed -= HandleUISubmitPressed;
            _view.OnUICancelPressed -= HandleUICancelPressed;
        }

        private void HandleMoveChanged(Vector2 input)
        {
            if (CurrentInputState.Value == InputState.Dialogue)
            {
                _moveInputInDialogue = input;
            }
            CurrentMoveInput.Value = input;
        }

        private void HandleSubmitPressed()
        {
            OnPlayerSubmitPressed?.Invoke();
        }

        // 現在はメニューを開く動作にそのままバイパスされている。変更の可能性あり
        private void HandlePlayerCancelPressed()
        {
            OnPlayerCancelPressed?.Invoke();
            CurrentMoveInput.Value = Vector2.zero; // 移動入力をリセット
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
            switch (currentInputState)
            {
                case InputState.Player:
                    CurrentMoveInput.Value = _moveInputInDialogue;
                    _moveInputInDialogue = Vector2.zero;
                    break;
                case InputState.UI:
                    CurrentMoveInput.Value = Vector2.zero;
                    break;
                case InputState.Dialogue:
                    CurrentMoveInput.Value = Vector2.zero;
                    break;
                default:
                    break;
            }
        }
        
        // InputServiceに含まれる。MenuPresenterが実行する
        public void OnMenuClosed(InputState state)
        {
            _view.currentInputState = state;
        }

        // OnUpdatable
        public void OnUpdate(float deltaTime)
        {
            _view.OnUpdate();
        }
    }
}