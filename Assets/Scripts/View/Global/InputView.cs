using System;
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
        public event Action OnInteractPressed;
        public event Action OnMenuPressed;
        public event Action OnForceExitPressed;
        public event Action<Vector2> OnMoveChanged;
        

        [Header("Input Action References")]
        [Tooltip("Interact アクション (例: Eキー / Gamepad South)")]
        [SerializeField] private InputActionReference interactAction;
        [Tooltip("Move アクション (例: WASD / Gamepad Left Stick)")]
        [SerializeField] private InputActionReference moveAction;
        [Tooltip("Menu アクション (例: Escキー / Gamepad Start)")]
        [SerializeField] private InputActionReference menuAction;
        
        private Vector2 _lastSentMoveInput;
        private const float SqrChangeThreshold = 0.0001f;

        private void OnEnable()
        {
            // 各アクションのコールバック登録と有効化
            RegisterAction(interactAction, HandleInteract);
            RegisterAction(menuAction, HandleMenu);

            // Moveは継続的な値の取得を行うため、有効化のみ行う
            if (moveAction != null)
            {
                moveAction.action.Enable();
            }
        }

        private void OnDisable()
        {
            // 各アクションのコールバック解除と無効化
            UnregisterAction(interactAction, HandleInteract);
            UnregisterAction(menuAction, HandleMenu);

            if (moveAction != null)
            {
                moveAction.action.Disable();
            }
        }

        private void Update()
        {
            // 移動値の読み取りとイベント発行
            // 毎フレーム入力をチェックし、入力がある（ゼロベクトルではない）場合、または
            // キャラクターを停止させるためにゼロベクトルを送る必要がある場合の処理はPresenterで行う前提
            if (moveAction != null && moveAction.action.enabled)
            {
                Vector2 currentMoveInput = moveAction.action.ReadValue<Vector2>();
                if((currentMoveInput - _lastSentMoveInput).sqrMagnitude > SqrChangeThreshold){
                    _lastSentMoveInput = currentMoveInput;
                    OnMoveChanged?.Invoke(currentMoveInput);
                }
            }
            else
            {
                Debug.LogWarning($"[PlayerInputView] Moveアクションが未設定または無効", this);
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
        private void HandleInteract(InputAction.CallbackContext context)
        {
            OnInteractPressed?.Invoke();
        }

        private void HandleMenu(InputAction.CallbackContext context)
        {
            OnMenuPressed?.Invoke();
        }
        
        private void HandleForceExit(InputAction.CallbackContext context) => OnForceExitPressed?.Invoke();
    }
}
