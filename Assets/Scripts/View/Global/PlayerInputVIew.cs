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
    public class PlayerInputView : MonoBehaviour
    {
        // Presenterが購読するイベント群
        public event Action OnInteractPressed;
        public event Action OnMenuPressed;
        public event Action OnForceExitPressed;
        public event Action<Vector2> OnMoveChanged;
        
        [Header("参照")]
        [SerializeField] private YarnView yarnView;

        [Header("Input Action References")]
        [Tooltip("Interact アクション (例: Eキー / Gamepad South)")]
        [SerializeField] private InputActionReference interactAction;
        [Tooltip("Move アクション (例: WASD / Gamepad Left Stick)")]
        [SerializeField] private InputActionReference moveAction;
        [Tooltip("Menu アクション (例: Escキー / Gamepad Start)")]
        [SerializeField] private InputActionReference menuAction;
        [Tooltip("ForceExit アクション (例: @キー / Gamepad Select)")]
        [SerializeField] private InputActionReference forceExitAction;
        
        private Vector2 _lastSentMoveInput;
        private const float SqrChangeThreshold = 0.0001f;

        private void OnEnable()
        {
            // 各アクションのコールバック登録と有効化
            RegisterAction(interactAction, HandleInteract);
            RegisterAction(menuAction, HandleMenu);
            RegisterAction(forceExitAction, HandleForceExit);

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
            UnregisterAction(forceExitAction, HandleForceExit);

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
                Vector2 rawMoveInput = moveAction.action.ReadValue<Vector2>();
                Vector2 currentMoveInput = (yarnView != null && yarnView.isPlaying) 
                    ? Vector2.zero 
                    : rawMoveInput;
                if((currentMoveInput - _lastSentMoveInput).sqrMagnitude > SqrChangeThreshold){
                    _lastSentMoveInput = currentMoveInput;
                    OnMoveChanged?.Invoke(currentMoveInput);
                }
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
            if (yarnView.isPlaying) return;
            OnInteractPressed?.Invoke();
        }

        private void HandleMenu(InputAction.CallbackContext context)
        {
            if (yarnView.isPlaying) return;
            OnMenuPressed?.Invoke();
        }
        
        private void HandleForceExit(InputAction.CallbackContext context) => OnForceExitPressed?.Invoke();
    }
}
