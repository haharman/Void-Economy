using System;
using UnityEngine;
using Model;
using View;

namespace Presenter
{
    /// <summary>
    /// 入力View(PlayerInputView)からのイベントを受け取り、
    /// Modelの更新や他のPresenter/Viewへの命令を行う仲介役(Presenter)。
    /// </summary>
    public class PlayerInputPresenter : IDisposable
    {
        private readonly PlayerInputView _inputView;
        private readonly PlayerModel _playerModel;

        public PlayerInputPresenter(PlayerInputView playerInputView, PlayerModel playerModel)
        {
            Debug.Log("[PlayerInputPresenter] Init");
            _inputView = playerInputView;
            _playerModel = playerModel;

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
        }

        private void UnsubscribeEvents()
        {
            _inputView.OnMoveChanged -= HandleMove;
            _inputView.OnInteractPressed -= HandleInteract;
            _inputView.OnMenuPressed -= HandleMenu;
            _inputView.OnForceExitPressed -= HandleForceExit;
        }

        private void HandleMove(Vector2 input)
        {
            _playerModel.moveInput = input;
        }

        private void HandleInteract()
        {
            _playerModel.interactFlag = true;
            Debug.Log("[PlayerInputPresenter] Interact");
        }

        private void HandleMenu()
        {
            _playerModel.openMenuFlag = true;
            Debug.Log("[PlayerInputPresenter] Open Menu");
        }

        private void HandleForceExit()
        {
            Debug.LogWarning("[PlayerInputPresenter] Force Exit");
        }
    }
}