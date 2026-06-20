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
        private ISceneService _sceneService;
        private IQuitService _quitService;
        private readonly InputView _inputView;
        private readonly SurfacePlayerModel _playerModel;
        private readonly SpacePlayerModel _spacePlayerModel;
        private readonly YarnModel _yarnModel;
        private readonly GlobalStateModel _globalStateModel;

        private Vector2 _moveInputInDialogue; // ダイアログ中の移動入力を一時的に保存する変数

        public InputPresenter(ISceneService sceneService, IQuitService quitService, InputView inputView, SurfacePlayerModel playerModel, YarnModel yarnModel, GlobalStateModel globalStateModel)
        {
            Debug.Log("[InputPresenter] Init");
            _sceneService = sceneService;
            _quitService = quitService;
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
            
            _globalStateModel.OnDialogueStarted += HandleDialogueStarted;
            _globalStateModel.OnDialogueCompleted += HandleDialogueCompleted;
        }

        private void UnsubscribeEvents()
        {
            _inputView.OnMoveChanged -= HandleMove;
            _inputView.OnInteractPressed -= HandleInteract;
            _inputView.OnMenuPressed -= HandleMenu;
            
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
                    _spacePlayerModel.Fly(input);
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
            Debug.Log("[InputPresenter] Open Menu");
            _sceneService.LoadScene(SceneType.Title);
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