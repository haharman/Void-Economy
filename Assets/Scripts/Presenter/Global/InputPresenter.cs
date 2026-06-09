using System;
using Core;
using UnityEngine;
using Model;
using View;

namespace Presenter
{
    /// <summary>
    /// 入力View(PlayerInputView)からのイベントを受け取り、
    /// Modelの更新や他のPresenter/Viewへの命令を行う仲介役(Presenter)。
    /// </summary>
    public class InputPresenter : IDisposable
    {
        private readonly InputView _inputView;
        private readonly PlayerModel _playerModel;
        private readonly YarnModel _yarnModel;
        private readonly GlobalStateModel _globalStateModel;

        private Vector2 _moveInputInDialogue; // ダイアログ中の移動入力を一時的に保存する変数

        public InputPresenter(InputView inputView, PlayerModel playerModel, YarnModel yarnModel, GlobalStateModel globalStateModel)
        {
            Debug.Log("[PlayerInputPresenter] Init");
            _inputView = inputView;
            _playerModel = playerModel;
            _yarnModel = yarnModel;
            _globalStateModel = globalStateModel;

            SubscribeEvents();
        }

        public void Dispose()
        {
            Debug.Log("[PlayerInputPresenter] Dispose");
            UnsubscribeEvents();
        }

        private void SubscribeEvents()
        {
            _inputView.OnMoveChanged += HandleMove;
            _inputView.OnInteractPressed += HandleInteract;
            _inputView.OnMenuPressed += HandleMenu;
            _inputView.OnForceExitPressed += HandleForceExit;
            
            _globalStateModel.OnDialogueStarted += HandleDialogueStarted;
            _globalStateModel.OnDialogueCompleted += HandleDialogueCompleted;
        }

        private void UnsubscribeEvents()
        {
            _inputView.OnMoveChanged -= HandleMove;
            _inputView.OnInteractPressed -= HandleInteract;
            _inputView.OnMenuPressed -= HandleMenu;
            _inputView.OnForceExitPressed -= HandleForceExit;
            
            _globalStateModel.OnDialogueStarted -= HandleDialogueStarted;
            _globalStateModel.OnDialogueCompleted -= HandleDialogueCompleted;
        }

        private void HandleMove(Vector2 input)
        {
            _moveInputInDialogue = input;
            switch (_globalStateModel.inputState)
            {
                case InputState.Surface:
                    _playerModel.moveInput = input;
                    break;
                case InputState.Space:
                    _playerModel.Fly(input);
                    break;
                default:
                    break;
            }
        }

        private void HandleInteract()
        {
            switch (_globalStateModel.inputState)
            {
                case InputState.Surface:
                    _playerModel.Interact();
                    break;
                default:
                    break;
            }
        }

        private void HandleMenu()
        {
            _playerModel.OpenMenu();
            Debug.Log("[PlayerInputPresenter] Open Menu");
        }

        private void HandleForceExit()
        {
            _playerModel.ForceExit();
            Debug.LogWarning("[PlayerInputPresenter] Force Exit");
        }
        
        private void HandleDialogueStarted()
        {
            _playerModel.moveInput = Vector2.zero; // 移動入力をリセット
        }
        
        private void HandleDialogueCompleted()
        {
            _playerModel.moveInput = _moveInputInDialogue; // ダイアログ中に保存していた移動入力を復元
        }
            
    }
}