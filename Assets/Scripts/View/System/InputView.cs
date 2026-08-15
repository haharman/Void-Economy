using System;
using Core;
using R3;
using UnityEngine;
using UnityEngine.InputSystem;

namespace View
{
    /// <summary>
    /// Unity Input Systemを使用してプレイヤーの入力を検知し、Presenterへイベントを発行するView。
    /// 
    /// 【現在のキーバインド設定想定】
    /// - Move      : WASD (Keyboard) / Left Stick (Gamepad)
    /// - Interact  : E (Keyboard) / Button South (Gamepad)
    /// - Menu      : Escape (Keyboard) / Start (Gamepad)
    /// - ForceExit : Backslash (Keyboard) / Select (Gamepad)
    /// </summary>
    public class InputView : MonoBehaviour
    {
        // Presenterが購読するイベント群
        public event Action OnPlayerSubmitPressed;
        public event Action OnPlayerCancelPressed;
        public event Action OnUISubmitPressed;
        public event Action OnUICancelPressed;
        
        public event Action<Vector2> OnPlayerMoveChanged;
        public event Action<Vector2> OnUIMoveChanged;
        
        public readonly ReactiveProperty<bool> IsPlayerThrustPressed = new(false);
        
        [SerializeField] private InputActionAsset inputActionAsset;
        [SerializeField] private InputActionReference submitAction;
        [SerializeField] private InputActionReference moveAction;
        [SerializeField] private InputActionReference cancelAction;
        [SerializeField] private InputActionReference thrustAction;
        [SerializeField] private InputActionReference uiSubmitAction;
        [SerializeField] private InputActionReference uiMoveAction;
        [SerializeField] private InputActionReference uiCancelAction;

        private InputActionMap _playerMap;
        private InputActionMap _uiMap;
        
        private Vector2 _lastSentMoveInput;
        private const float SqrChangeThreshold = 0.0001f;
        
        // メニューを閉じたときなど、値が変化していないが入力を取得したいときに用いる
        public Vector2 CurrentPlayerMove => moveAction.action.ReadValue<Vector2>();

        
        private InputState _previousInputState = InputState.Disable;
        public InputState currentInputState = InputState.UI;
        public void Initialize()
        {
            _playerMap = inputActionAsset.FindActionMap("Player");
            _uiMap = inputActionAsset.FindActionMap("UI");
            if(_playerMap == null) Debug.LogWarning("[InputView] Player Input Map not found");
            if(_uiMap == null) Debug.LogWarning("[InputView] UI Input Map not found");

            // 各アクションのコールバック登録と有効化
            RegisterAction(submitAction, HandleSubmit);
            RegisterAction(cancelAction, HandleCancel);
            RegisterAction(uiSubmitAction, HandleUISubmit);
            RegisterAction(uiCancelAction, HandleUICancel);
        }

        public void OnUpdate()
        {
            // 移動値の読み取りとイベント発行
            // 毎フレーム入力をチェックし、入力がある（ゼロベクトルではない）場合、または
            // キャラクターを停止させるためにゼロベクトルを送る必要がある場合の処理はPresenterで行う前提
            if (moveAction != null && moveAction.action.enabled)
            {
                Vector2 currentMoveInput = moveAction.action.ReadValue<Vector2>();
                currentMoveInput.x = Mathf.Clamp(currentMoveInput.x, -1f, 1f);
                currentMoveInput.y = Mathf.Clamp(currentMoveInput.y, -1f, 1f);
                if((currentMoveInput - _lastSentMoveInput).sqrMagnitude > SqrChangeThreshold){
                    _lastSentMoveInput = currentMoveInput;
                    OnPlayerMoveChanged?.Invoke(currentMoveInput);
                }
                if (thrustAction != null && thrustAction.action.enabled)
                    IsPlayerThrustPressed.Value = thrustAction.action.IsPressed();
                else
                    IsPlayerThrustPressed.Value = false;
            }
            if (uiMoveAction != null && uiMoveAction.action.enabled)
            {
                Vector2 currentMoveInput = uiMoveAction.action.ReadValue<Vector2>();
                currentMoveInput.x = Mathf.Clamp(currentMoveInput.x, -1f, 1f);
                currentMoveInput.y = Mathf.Clamp(currentMoveInput.y, -1f, 1f);
                if((currentMoveInput - _lastSentMoveInput).sqrMagnitude > SqrChangeThreshold){
                    _lastSentMoveInput = currentMoveInput;
                    OnUIMoveChanged?.Invoke(currentMoveInput);
                }
            }
            if(currentInputState != _previousInputState)
            {
                switch (currentInputState)
                {
                    case InputState.Player:
                        _playerMap.Enable();
                        _uiMap.Disable();
                        break;
                    case InputState.Dialogue:
                        _playerMap.Enable();
                        _uiMap.Disable();
                        break;
                    case InputState.UI:
                        _uiMap.Enable();
                        _playerMap.Disable();
                        break;
                    case InputState.Disable:
                        _playerMap.Disable();
                        _uiMap.Disable();
                        break;
                    default:
                        Debug.LogError("[InputPresenter] 定義されていないInputStateです");
                        break;
                }
                // ActionMapが切り替わるため、入力の履歴をリセット→Moveが通知される
                _lastSentMoveInput = Vector2.zero;
                
                _previousInputState = currentInputState;
            }
        }

        private void OnDisable()
        {
            Debug.Log("[InputView] Disabled");
            // 各アクションのコールバック解除と無効化
            UnregisterAction(submitAction, HandleSubmit);
            UnregisterAction(cancelAction, HandleCancel);
            UnregisterAction(uiSubmitAction, HandleUISubmit);
            UnregisterAction(uiCancelAction, HandleUICancel);

            if (moveAction != null)
            {
                moveAction.action.Disable();
            }
        }
        
        // --- ヘルパーメソッド ---

        /// <summary>
        /// アクションの有効化とコールバックの登録を一括で行う
        /// </summary>
        private void RegisterAction(InputActionReference reference, Action<InputAction.CallbackContext> handler)
        {
            if (reference != null && reference.action != null)
            {
                reference.action.Enable();
                reference.action.performed += handler;
            }
            else
            {
                Debug.LogWarning($"[PlayerInputView] 未設定のInputActionReferenceがあります。インスペクターを確認してください。", this);
            }
        }

        /// <summary>
        /// アクションのコールバック解除と無効化を一括で行う
        /// </summary>
        private void UnregisterAction(InputActionReference reference, Action<InputAction.CallbackContext> handler)
        {
            if (reference != null && reference.action != null)
            {
                reference.action.performed -= handler;
                reference.action.Disable();
            }
        }

        // --- ハンドラー ---
        private void HandleSubmit(InputAction.CallbackContext context)
        {
            Debug.Log("[InputView] Player/Submit");
            OnPlayerSubmitPressed?.Invoke();
        }

        private void HandleCancel(InputAction.CallbackContext context)
        {
            Debug.Log("[InputView] Player/Cancel");
            OnPlayerCancelPressed?.Invoke();
        }

        private void HandleUISubmit(InputAction.CallbackContext context)
        {
            Debug.Log("[InputView] UI/Submit");
            OnUISubmitPressed?.Invoke();
        }

        private void HandleUICancel(InputAction.CallbackContext context)
        {
            Debug.Log("[InputView] UI/Cancel");
            OnUICancelPressed?.Invoke();
        }
    }
}
